using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Calc
{

    public enum ItemType : byte
    {
        Number,
        Operator
    }

    public record RPNItem
    {

        public RPNItem(string item, ItemType type)
        {
            Item = item;
            Type = type;
        }

        public string Item { get; init; }

        public ItemType Type { get; init; }

        public bool IsNumber
        {
            get
            {
                return Type == ItemType.Number;
            }
        }

        public static implicit operator string(RPNItem item)
        {
            return item.Item;
        }

        public static implicit operator BigDecimal(RPNItem item)
        {
            if (item.IsNumber)
            {
                return BigDecimal.Parse(item.Item);
            }
            throw new InvalidCastException("This item not a number");
        }
    }

    public enum OrderType
    {
        Left,
        Right
    }

    public enum OpType
    {
        Op,
        Func,
        CONST
    }

    public record OperatorInfo(int Priority, OrderType Order, OpType Type, int CountOfOp, Func<BigDecimal[], BigDecimal> Func);

    public static class MyMath
    {

        private static readonly BigInteger TAYLOR_SERIES_F = 100;

        public static BigDecimal Factorial(BigDecimal value)
        {
            BigDecimal result = 1;
            if (value <= 0) return 1;
            for (BigDecimal i = 1; i <= value; i++)
            {
                result *= i;
            }
            return result;
        }

        public static BigDecimal Arrangement(BigDecimal n, BigDecimal m)
        {
            if (m > n) throw new InvalidOperationException("输入错误");
            return Factorial(n) / Factorial(n - m);
        }

        public static BigDecimal Combination(BigDecimal n, BigDecimal m)
        {
            if (m > n) throw new InvalidOperationException("输入错误");
            return Factorial(n) / (Factorial(m) * Factorial(n - m));
        }

        public static BigDecimal Pow(BigDecimal a, BigDecimal b)
        {
            if (b == 0) return 1;
            BigDecimal result = 1;
            BigDecimal exp = Abs(b);
            for (BigDecimal i = 1; i <= exp; i++)
            {
                result *= a;
            }
            if (b < 0) return 1 / result;
            return result;
        }

        public static BigDecimal Abs(BigDecimal a)
        {
            return a < 0 ? -a : a;
        }

        public static BigDecimal Max(BigDecimal a, BigDecimal b)
        {
            return a > b ? a : b;
        }

        public static BigDecimal Min(BigDecimal a, BigDecimal b)
        {
            return a <= b ? a : b;
        }

        public static BigDecimal Sin(BigDecimal v)
        {
            BigDecimal s = 0;
            BigDecimal bf = 0;
            BigInteger i = 0;
            while (true)
            {
                BigInteger mx = 2 * i + 1;
                BigDecimal vf = Factorial(mx);
                BigDecimal vt = Pow(v, mx);
                if (i % 2 == 0)
                {
                    s += (vt / vf);
                }
                else
                {
                    s -= (vt / vf);
                }
                if (s.Truncate() == bf.Truncate())
                {
                    break;
                }
                else
                {
                    bf = s;
                }
                i++;
            }
            return s.Truncate();
        }


        public static BigDecimal Cos(BigDecimal v)
        {
            BigDecimal s = 0;
            BigDecimal bf = 0;
            BigInteger i = 0;
            while (true)
            {
                BigInteger mx = 2 * i;
                BigDecimal vf = Factorial(mx);
                BigDecimal vt = Pow(v, mx);
                if (i % 2 == 0)
                {
                    s += (vt / vf);
                }
                else
                {
                    s -= (vt / vf);
                }
                if (s.Truncate() == bf.Truncate())
                {
                    break;
                }
                else
                {
                    bf = s;
                }
                i++;
            }
            return s.Truncate();
        }

        public static BigDecimal Tan(BigDecimal v)
        {
            return Sin(v) / Cos(v);
        }


        public static BigDecimal Ln(BigDecimal v)
        {
            BigDecimal s = 0;
            BigDecimal sv = (v - 1) / (v + 1);
            BigDecimal bf = 0;
            BigInteger i = 0;
            while (true)
            {
                BigDecimal vm = 2 * i + 1;
                BigDecimal vt = Pow(sv, vm);
                BigDecimal vf = vm;
                s += (vt / vf);
                if (s.Truncate() == bf.Truncate())
                {
                    break;
                }
                else
                {
                    bf = s;
                }
                i++;
            }
            return (2 * s).Truncate();
        }

        public static readonly BigDecimal Pi;
        public static readonly BigDecimal E;

        static MyMath()
        {
            Pi = BigDecimal.Parse("3.1415926535897932384626433832795028841971693993751058209749445923078164062862089986280348253421170679");
            E = BigDecimal.Parse("2.7182818284590452353602874713526624977572470936999595749669676277240766303535475945713821785251664274");
        }
    }


    /// <summary>
    /// 中缀表达式装换为后缀表达式
    /// </summary>
    public sealed class Standard2RPN
    {
        private readonly IList<string> _item = new List<string>();

        private readonly IList<RPNItem> _rpn = new List<RPNItem>();

        private readonly string _origin;

        private readonly IDictionary<string, OperatorInfo> _operator;

        public Standard2RPN(string input)
        {
            _operator = new Dictionary<string, OperatorInfo>()
            {
                { "and", new OperatorInfo(0,OrderType.Left, OpType.Op,2, param => ((BigInteger)param[0]) & ((BigInteger)param[1])) },
                { "or", new OperatorInfo(0,OrderType.Left, OpType.Op,2, param => ((BigInteger)param[0]) | ((BigInteger)param[1])) },
                { "xor", new OperatorInfo(0,OrderType.Left, OpType.Op,2, param => ((BigInteger)param[0]) ^ ((BigInteger)param[1])) },
                { "not", new OperatorInfo(0,OrderType.Left, OpType.Op,1, param => ~(BigInteger)param[0]) },
                { "+", new OperatorInfo(1,OrderType.Left, OpType.Op,2, param => param[0] + param[1]) },
                { "-", new OperatorInfo(1,OrderType.Left,OpType.Op,2,param => param[0] - param[1] ) },
                { "*", new OperatorInfo(2,OrderType.Left,OpType.Op,2,param => param[0] * param[1] ) },
                { "/", new OperatorInfo(2,OrderType.Left,OpType.Op,2,param => param[0] / param[1] ) },
                { "%", new OperatorInfo(2,OrderType.Left, OpType.Op,2,param => param[0] % param[1]) },
                { "^", new OperatorInfo(3,OrderType.Right, OpType.Op,2,param => MyMath.Pow(param[0], param[1])) },
                { "!", new OperatorInfo(4,OrderType.Left, OpType.Op,1,param => MyMath.Factorial(param[0])) },
                { "max",new OperatorInfo(int.MaxValue - 1,OrderType.Left,OpType.Func,2,param => MyMath.Max(param[0], param[1])) },
                { "min",new OperatorInfo(int.MaxValue - 1,OrderType.Left,OpType.Func,2, param => MyMath.Min(param[0], param[1])) },
                { "lg",new OperatorInfo(int.MaxValue - 1,OrderType.Left,OpType.Func, 1, param => Math.Log10((double)param[0]))},
                { "ln",new OperatorInfo(int.MaxValue - 1,OrderType.Left,OpType.Func, 1, param => MyMath.Ln(param[0]))},
                { "log",new OperatorInfo(int.MaxValue - 1,OrderType.Left,OpType.Func, 2, param => Math.Log((double)param[1],(double)param[0]))},
                { "sqrt",new OperatorInfo(int.MaxValue - 1,OrderType.Left,OpType.Func, 1, param => Math.Sqrt((double)param[0]))},
                { "sqrtn",new OperatorInfo(int.MaxValue - 1,OrderType.Left,OpType.Func, 2, param => Math.Pow((double)param[0], 1/(double)param[1]))},
                { "sin",new OperatorInfo(int.MaxValue - 1,OrderType.Left,OpType.Func, 1, param => MyMath.Sin(param[0]))},
                { "cos",new OperatorInfo(int.MaxValue - 1,OrderType.Left,OpType.Func, 1, param => MyMath.Cos(param[0]))},
                { "tan",new OperatorInfo(int.MaxValue - 1,OrderType.Left,OpType.Func, 1, param => MyMath.Tan(param[0]))},
                { "abs",new OperatorInfo(int.MaxValue - 1,OrderType.Left,OpType.Func, 1, param => MyMath.Abs(param[0]))},
                { "C",new OperatorInfo(int.MaxValue - 1,OrderType.Left,OpType.Func, 2, param => MyMath.Combination(param[0],param[1]))},
                { "A",new OperatorInfo(int.MaxValue - 1,OrderType.Left,OpType.Func, 2, param => MyMath.Arrangement(param[0],param[1]))},
                { "PI",new OperatorInfo(int.MaxValue,OrderType.Left,OpType.CONST,0, param => MyMath.Pi) },
                { "e",new OperatorInfo(int.MaxValue,OrderType.Left,OpType.CONST,0, param => MyMath.E) },
                { "G",new OperatorInfo(int.MaxValue,OrderType.Left,OpType.CONST,0, param => 6.67259E-11M) },
                { "Na",new OperatorInfo(int.MaxValue,OrderType.Left,OpType.CONST,0, param => 6.02214076E23M) },
                { "c",new OperatorInfo(int.MaxValue,OrderType.Left,OpType.CONST,0, param => 299_792_458M) }
            };
            _origin = input;
            Analyzer();
            Parser();
        }

        public Standard2RPN(string input, IDictionary<string, OperatorInfo> @operator)
        {
            _origin = input;
            _operator = @operator;
            Analyzer();
            Parser();
        }

        private void Analyzer()
        {
            if (string.IsNullOrWhiteSpace(_origin)) return;
            var itor = _origin.GetEnumerator();
            StringBuilder sb = new();
            while (itor.MoveNext())
            {
                char cur = itor.Current;
                if (cur is ' ' or '\n') continue;
                if (sb.Length == 0)
                {
                    sb.Append(cur);
                    continue;
                }
                bool isNumber = IsNumber(sb[^1]) && IsNumber(cur);
                bool isLetter = IsLatter(sb[^1]) && IsLatter(cur);
                if (isNumber || isLetter)
                {
                    sb.Append(cur);
                    continue;
                }
                else
                {
                    _item.Add(sb.ToString());
                    sb.Clear();
                    sb.Append(cur);
                    continue;
                }
            }
            _item.Add(sb.ToString());
        }

        private static bool IsNumber(char c)
        {
            if (c is (>= '0' and <= '9') or '.')
            {
                return true;
            }
            return false;
        }

        private static bool IsLatter(char c)
        {
            if (c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z'))
            {
                return true;
            }
            return false;
        }

        private void Parser()
        {
            // 调度场算法开始
            Stack<string> opt = new();
            int i = 0;
            while (i < _item.Count)
            {
                var token = _item[i];
                if (i == 0 && token == "-")
                {
                    _rpn.Add(new RPNItem("0", ItemType.Number));
                }
                i++;
                if (BigDecimal.TryParse(token, out _))
                {
                    _rpn.Add(new RPNItem(token, ItemType.Number));
                    continue;
                }
                if (_operator.ContainsKey(token))
                {
                    OperatorInfo operatorInfo = _operator[token];
                    if (operatorInfo.Type is OpType.Func or OpType.CONST)
                    {
                        opt.Push(token);
                        continue;
                    }
                    else
                    {
                        while (opt.Count != 0)
                        {
                            string o = opt.Peek();
                            if (o == "(")
                            {
                                break;
                            }
                            OperatorInfo opt2 = _operator[o];
                            if (opt2.Type == OpType.Func)
                            {
                                throw new InvalidOperationException("语法错误");
                            }
                            if ((operatorInfo.Order == OrderType.Left && operatorInfo.Priority <= opt2.Priority) ||
                                (operatorInfo.Order == OrderType.Right && operatorInfo.Priority < opt2.Priority))
                            {
                                string o2 = opt.Pop();
                                _rpn.Add(new RPNItem(o2, ItemType.Operator));
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                        opt.Push(token);
                    }
                    continue;
                }
                if (token == ",")
                {
                    bool findFlag = false;
                    while (opt.Count != 0)
                    {
                        string t = opt.Peek();
                        if (t == "(")
                        {
                            findFlag = true;
                            break;
                        }
                        else
                        {
                            opt.Pop();
                            _rpn.Add(new RPNItem(t, ItemType.Operator));
                        }
                    }
                    if (opt.Count == 0 && !findFlag)
                    {
                        throw new InvalidOperationException("语法错误");
                    }
                    if (i < _item.Count && _item[i] == "-")
                    {
                        // 逗号的下一个也是 - 那也是一个负号
                        _rpn.Add(new RPNItem("0", ItemType.Number));
                    }
                    continue;
                }
                if (token == "(")
                {
                    opt.Push(token);
                    if (i < _item.Count && _item[i] == "-")
                    {
                        // ( 下一个符号就是 - 那就是负数的符号
                        _rpn.Add(new RPNItem("0", ItemType.Number));
                    }
                    continue;
                }
                if (token == ")")
                {
                    bool findFlag = false;
                    while (opt.Count != 0)
                    {
                        string o = opt.Peek();
                        if (o != "(")
                        {
                            _rpn.Add(new RPNItem(o, ItemType.Operator));
                            opt.Pop();
                            continue;
                        }
                        findFlag = true;
                        opt.Pop();
                        if (opt.Count == 0 || opt.Peek() == "(")
                        {
                            break;
                        }
                        o = opt.Peek();
                        OperatorInfo oi = _operator[o];
                        if (oi.Type == OpType.Func)
                        {
                            _rpn.Add(new RPNItem(o, ItemType.Operator));
                            opt.Pop();
                        }
                        break;
                    }
                    if (opt.Count == 0 && !findFlag)
                    {
                        throw new InvalidOperationException("语法错误");
                    }
                    continue;
                }
                throw new InvalidOperationException("发现无法识别的函数或运算符");
            }
            while (opt.Count != 0)
            {
                string t = opt.Pop();
                _rpn.Add(new RPNItem(t, ItemType.Operator));
            }
        }

        public BigDecimal Calc()
        {
            Stack<BigDecimal> dc = new();
            foreach (var item in _rpn)
            {
                if (item.IsNumber)
                {
                    dc.Push(item);
                    continue;
                }
                string op = item;
                OperatorInfo opInfo = _operator[op];
                int c = opInfo.CountOfOp;
                BigDecimal[] opNum = new BigDecimal[c];
                for (int i = opNum.Length - 1; i >= 0; i--)
                {
                    opNum[i] = dc.Pop();
                }
                dc.Push(opInfo.Func(opNum));
            }
            BigDecimal result = dc.Pop();
            if (dc.Count != 0)
            {
                throw new InvalidOperationException("输入有误，结果不可靠");
            }
            return result;
        }

        public static implicit operator Standard2RPN(string input)
        {
            return new Standard2RPN(input);
        }

        public string GetOrigin()
        {
            StringBuilder sb = new();
            foreach (string a in _item)
            {
                sb.Append(a).Append("\t\t");
            }
            return sb.ToString();
        }

        public string GetFormattedOrigin()
        {
            StringBuilder sb = new();
            foreach (string a in _item)
            {
                sb.Append(a).Append(' ');
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            foreach (RPNItem rnt in _rpn)
            {
                sb.Append(rnt.Item).Append("    ");
            }
            return sb.ToString();
        }
    }
}
