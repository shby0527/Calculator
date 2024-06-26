﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;

namespace Calc
{
    public enum ItemType : byte
    {
        Number,
        Operator
    }

    public record RpnItem
    {
        public RpnItem(string item, ItemType type)
        {
            Item = item;
            Type = type;
        }

        public string Item { get; init; }

        private ItemType Type { get; init; }

        public bool IsNumber => Type == ItemType.Number;

        public static implicit operator string(RpnItem item)
        {
            return item.Item;
        }

        public static implicit operator BigDecimal(RpnItem item)
        {
            if (item.IsNumber)
            {
                return BigDecimal.Parse(item.Item);
            }

            throw new InvalidCastException("This item not a number");
        }

        public static implicit operator BigComplex(RpnItem item)
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
        Const
    }

    public record OperatorInfo(
        int Priority,
        OrderType Order,
        OpType Type,
        int CountOfOp,
        Func<BigComplex[], BigComplex> Func);

    public static class MyMath
    {
        public static CancellationToken CancellationToken { private get; set; }

        public static BigDecimal Factorial(BigDecimal value)
        {
            BigDecimal result = 1;
            if (value <= 0) return 1;
            for (BigDecimal i = 1; i <= value; i++)
            {
                result *= i;
                CancellationToken.ThrowIfCancellationRequested();
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
                CancellationToken.ThrowIfCancellationRequested();
            }

            if (b < 0) return 1 / result;
            return result;
        }

        public static BigComplex Pow(BigComplex a, BigDecimal b)
        {
            if (b == 0) return 1;
            BigComplex result = 1;
            var exp = Abs(b);
            for (BigDecimal i = 1; i <= exp; i++)
            {
                result *= a;
                CancellationToken.ThrowIfCancellationRequested();
            }

            if (b < 0) return 1 / result;
            return result;
        }

        private static BigDecimal Abs(BigDecimal a)
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

        public static BigComplex Sqrt(BigDecimal a)
        {
            var origin = Abs(a);
            BigDecimal err = new(1, -150);
            var pre = Abs(a);
            while (Abs(origin - pre * pre) > err)
            {
                pre = (origin / pre + pre) / 2;
                CancellationToken.ThrowIfCancellationRequested();
            }

            return a < 0 ? new BigComplex(0, pre.Truncate(140)) : pre.Truncate(140);
        }


        public static BigDecimal Arctan(BigDecimal x)
        {
            if (Abs(x) > 1)
            {
                return (Pi / 2 - Arctan(1 / x)).Truncate(100);
            }
            else if (Abs(x) >= 0.1)
            {
                return (2 * Arctan((Sqrt(1 + x * x).Real - 1) / x)).Truncate(100);
            }
            else
            {
                BigDecimal err = new(1, -101);
                BigDecimal s = 0;
                BigDecimal bf = 0;
                BigInteger i = 0;
                while (true)
                {
                    CancellationToken.ThrowIfCancellationRequested();
                    BigInteger mx = 2 * i + 1;
                    BigDecimal vf = 2 * i + 1;
                    BigDecimal vt = Pow(x, mx);
                    if (i % 2 == 0)
                    {
                        s += vt / vf;
                        if (Abs(s - bf) < err)
                        {
                            break;
                        }
                        else
                        {
                            bf = s;
                        }
                    }
                    else
                    {
                        s -= vt / vf;
                    }

                    i++;
                }

                return s.Truncate(100);
            }
        }


        public static BigDecimal Arctan2(BigDecimal y, BigDecimal x)
        {
            if (x > 0)
            {
                return Arctan(y / x);
            }

            if (x == 0 && y > 0)
            {
                return (Pi / 2).Truncate(140);
            }

            if (x == 0 && y < 0)
            {
                return (-Pi / 2).Truncate(140);
            }

            if (x < 0 && y >= 0)
            {
                return Arctan(y / x) + Pi.Truncate(140);
            }

            if (x < 0 && y < 0)
            {
                return Arctan(y / x) - Pi.Truncate(140);
            }

            return 0;
        }


        public static BigComplex Sqrt(BigComplex a)
        {
            if (!a.IsComplex) return Sqrt(a.Real);

            return Sqrt(a.Modulus) * new BigComplex(Cos(a.Argument / 2), Sin(a.Argument / 2));
        }

        public static BigDecimal Sin(BigDecimal v)
        {
            BigDecimal err = new(1, -150);
            var sign = 1;
            var arg = v;
            if (arg < 0)
            {
                arg = -arg;
                sign = -sign;
            }

            while (arg >= 2 * Pi)
            {
                // 规约
                arg -= 2 * Pi;
                CancellationToken.ThrowIfCancellationRequested();
            }

            BigDecimal s = 0;
            BigDecimal bf = 0;
            BigInteger i = 0;
            while (true)
            {
                CancellationToken.ThrowIfCancellationRequested();
                BigInteger mx = 2 * i + 1;
                BigDecimal vf = Factorial(mx);
                BigDecimal vt = Pow(arg, mx);
                if (i % 2 == 0)
                {
                    s += vt / vf;
                    if (Abs(s - bf) < err)
                    {
                        break;
                    }
                    else
                    {
                        bf = s;
                    }
                }
                else
                {
                    s -= vt / vf;
                }

                i++;
            }

            return s.Truncate(140) * sign;
        }


        public static BigDecimal Cos(BigDecimal v)
        {
            BigDecimal err = new(1, -150);
            int sign = 1;
            BigDecimal arg = v;
            if (arg < 0)
            {
                arg = -arg;
                sign = -sign;
            }

            while (arg >= 2 * Pi)
            {
                // 规约
                arg -= 2 * Pi;
                CancellationToken.ThrowIfCancellationRequested();
            }

            BigDecimal s = 0;
            BigDecimal bf = 0;
            BigInteger i = 0;
            while (true)
            {
                CancellationToken.ThrowIfCancellationRequested();
                BigInteger mx = 2 * i;
                BigDecimal vf = Factorial(mx);
                BigDecimal vt = Pow(arg, mx);
                if (i % 2 == 0)
                {
                    s += vt / vf;
                    if (Abs(s - bf) < err)
                    {
                        break;
                    }
                    else
                    {
                        bf = s;
                    }
                }
                else
                {
                    s -= vt / vf;
                }

                i++;
            }

            return s.Truncate(140) * sign;
        }

        public static BigDecimal Tan(BigDecimal v)
        {
            return Sin(v) / Cos(v);
        }


        public static BigDecimal Ln(BigDecimal v)
        {
            if (v > 2)
            {
                return Ln(v / 2) + Ln2;
            }

            BigDecimal err = new(1, -101);
            BigDecimal s = 0;
            BigDecimal sv = (v - 1) / (v + 1);
            BigDecimal bf = 0;
            BigInteger i = 0;
            while (true)
            {
                CancellationToken.ThrowIfCancellationRequested();
                BigDecimal vm = 2 * i + 1;
                BigDecimal vt = Pow(sv, vm);
                BigDecimal vf = vm;
                s += vt / vf;
                if (Abs(s - bf) < err)
                {
                    break;
                }
                else
                {
                    bf = s;
                }

                i++;
            }

            return (2 * s).Truncate(100);
        }

        public static BigDecimal Log10(BigDecimal v)
        {
            return Ln(v) / Ln(10);
        }

        public static BigDecimal Log(BigDecimal a, BigDecimal b)
        {
            return Ln(b) / Ln(a);
        }

        public static readonly BigDecimal Pi;
        public static readonly BigDecimal E;

        private static readonly BigDecimal Ln2;

        static MyMath()
        {
            Pi = BigDecimal.Parse(
                "3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117067982148086513282306647093844609550582231725359408128481117450284102701938521105559644622948954930381964428810975665933446128475648233786783165271201909145648566923460348610454326648213393607260249141273724587006606315588174881520920962829254091715364367892590360011330530548820466521384146951941511609433057270365759591953092186117381932611793105118548074462379962749567351885752724891227938183011949129833673362440656643086021394946395224737190702179860943702770539217176293176752384674818467669405132000568127145263560827785771342757789609173637178721468440901224953430146549585371050792279689258923542019956112129021960864034418159813629774771309960518707211349999998372978049951059731732816096318595024459455346908302642522308253344685035261931188171010003137838752886587533208381420617177669147303598253490428755468731159562");
            E = BigDecimal.Parse(
                "2.71828182845904523536028747135266249775724709369995957496696762772407663035354759457138217852516642742746639193200305992181741359662904357290033429526059563073813232862794349076323382988075319525101901157383418793070215408914993488416750924476146066808226480016847741185374234544243710753907774499206955170276183860626133138458300075204493382656029760673711320070932870912744374704723069697720931014169283681902551510865746377211125238978442505695369677078544996996794686445490598793163688923009879312773617821542499922957635148220826989519366803318252886939849646510582093923982948879332036250944311730123819706841614039701983767932068328237646480429531180232878250981945581530175671736133206981125099618188159304169035159888851934580727386673858942287922849989208680582574927961048419844436346324496848756023362482704197862320900216099023530436994184914631409343173814364054625315209618369088870701676839642437814059271456354");
            Ln2 = BigDecimal.Parse(
                "0.6931471805599453094172321214581765680755001343602552541206800094933936219696947156058633269964186875");
        }
    }


    /// <summary>
    /// 中缀表达式装换为后缀表达式
    /// </summary>
    public sealed class Standard2Rpn
    {
        private readonly IList<string> _item = new List<string>();

        private readonly IList<RpnItem> _rpn = new List<RpnItem>();

        private readonly string _origin;

        private readonly IDictionary<string, OperatorInfo> _operator;

        private readonly IDictionary<string, Action<string[]>> _cmd;

        public bool IsCommand { get; private set; }

        public static CancellationToken CancellationToken
        {
            set => MyMath.CancellationToken = value;
        }

        private Standard2Rpn(string input)
        {
            _operator = new Dictionary<string, OperatorInfo>()
            {
                {
                    "and", new OperatorInfo(0, OrderType.Left, OpType.Op, 2, param =>
                    {
                        if (param[0].IsComplex || param[1].IsComplex)
                            throw new InvalidOperationException("Complex Can Not do this");
                        return ((BigInteger)param[0].Real) & ((BigInteger)param[1].Real);
                    })
                },
                {
                    "or", new OperatorInfo(0, OrderType.Left, OpType.Op, 2, param =>
                    {
                        if (param[0].IsComplex || param[1].IsComplex)
                            throw new InvalidOperationException("Complex Can Not do this");
                        return ((BigInteger)param[0].Real) | ((BigInteger)param[1].Real);
                    })
                },
                {
                    "xor", new OperatorInfo(0, OrderType.Left, OpType.Op, 2, param =>
                    {
                        if (param[0].IsComplex || param[1].IsComplex)
                            throw new InvalidOperationException("Complex Can Not do this");
                        return ((BigInteger)param[0].Real) ^ ((BigInteger)param[1].Real);
                    })
                },
                {
                    "not", new OperatorInfo(0, OrderType.Left, OpType.Op, 1, param =>
                    {
                        if (param[0].IsComplex) throw new InvalidOperationException("Complex Can Not do this");
                        return ~(BigInteger)param[0].Real;
                    })
                },
                { "+", new OperatorInfo(1, OrderType.Left, OpType.Op, 2, param => param[0] + param[1]) },
                { "-", new OperatorInfo(1, OrderType.Left, OpType.Op, 2, param => param[0] - param[1]) },
                { "*", new OperatorInfo(2, OrderType.Left, OpType.Op, 2, param => param[0] * param[1]) },
                { "/", new OperatorInfo(2, OrderType.Left, OpType.Op, 2, param => param[0] / param[1]) },
                {
                    "%", new OperatorInfo(2, OrderType.Left, OpType.Op, 2, param =>
                    {
                        if (param[0].IsComplex || param[1].IsComplex)
                            throw new InvalidOperationException("Complex Can Not do this");
                        return param[0].Real % param[1].Real;
                    })
                },
                {
                    "^", new OperatorInfo(3, OrderType.Right, OpType.Op, 2, param =>
                    {
                        if (param[1].IsComplex) throw new InvalidOperationException("Complex Can Not do this");
                        return MyMath.Pow(param[0], param[1].Real);
                    })
                },
                {
                    "!", new OperatorInfo(4, OrderType.Left, OpType.Op, 1, param =>
                    {
                        if (param[0].IsComplex) throw new InvalidOperationException("Complex Can Not do this");
                        return MyMath.Factorial(param[0].Real);
                    })
                },
                {
                    "max", new OperatorInfo(int.MaxValue - 1, OrderType.Left, OpType.Func, 2, param =>
                    {
                        if (param[0].IsComplex || param[1].IsComplex)
                            throw new InvalidOperationException("Complex Can Not do this");
                        return MyMath.Max(param[0].Real, param[1].Real);
                    })
                },
                {
                    "min", new OperatorInfo(int.MaxValue - 1, OrderType.Left, OpType.Func, 2, param =>
                    {
                        if (param[0].IsComplex || param[1].IsComplex)
                            throw new InvalidOperationException("Complex Can Not do this");
                        return MyMath.Min(param[0].Real, param[1].Real);
                    })
                },
                {
                    "lg", new OperatorInfo(int.MaxValue - 1, OrderType.Left, OpType.Func, 1, param =>
                    {
                        if (param[0].IsComplex) throw new InvalidOperationException("Complex Can Not do this");
                        return MyMath.Log10(param[0].Real);
                    })
                },
                {
                    "ln", new OperatorInfo(int.MaxValue - 1, OrderType.Left, OpType.Func, 1, param =>
                    {
                        if (param[0].IsComplex) throw new InvalidOperationException("Complex Can Not do this");
                        return MyMath.Ln(param[0].Real);
                    })
                },
                {
                    "log", new OperatorInfo(int.MaxValue - 1, OrderType.Left, OpType.Func, 2, param =>
                    {
                        if (param[0].IsComplex || param[1].IsComplex)
                            throw new InvalidOperationException("Complex Can Not do this");
                        return MyMath.Log(param[0].Real, param[1].Real);
                    })
                },
                {
                    "sqrt",
                    new OperatorInfo(int.MaxValue - 1, OrderType.Left, OpType.Func, 1,
                        param => MyMath.Sqrt(param[0]))
                },
                {
                    "sqrtn", new OperatorInfo(int.MaxValue - 1, OrderType.Left, OpType.Func, 2, param =>
                    {
                        if (param[0].IsComplex || param[1].IsComplex)
                            throw new InvalidOperationException("Complex Can Not do this");
                        return Math.Pow((double)param[0].Real, 1 / (double)param[1].Real);
                    })
                },
                {
                    "sin", new OperatorInfo(int.MaxValue - 1, OrderType.Left, OpType.Func, 1, param =>
                    {
                        if (param[0].IsComplex) throw new InvalidOperationException("Complex Can Not do this");
                        return MyMath.Sin(param[0].Real);
                    })
                },
                {
                    "cos", new OperatorInfo(int.MaxValue - 1, OrderType.Left, OpType.Func, 1, param =>
                    {
                        if (param[0].IsComplex) throw new InvalidOperationException("Complex Can Not do this");
                        return MyMath.Cos(param[0].Real);
                    })
                },
                {
                    "tan", new OperatorInfo(int.MaxValue - 1, OrderType.Left, OpType.Func, 1, param =>
                    {
                        if (param[0].IsComplex) throw new InvalidOperationException("Complex Can Not do this");
                        return MyMath.Tan(param[0].Real);
                    })
                },
                {
                    "arctan", new OperatorInfo(int.MaxValue - 1, OrderType.Left, OpType.Func, 1, param =>
                    {
                        if (param[0].IsComplex) throw new InvalidOperationException("Complex Can Not do this");
                        return MyMath.Arctan(param[0].Real);
                    })
                },
                {
                    "abs",
                    new OperatorInfo(int.MaxValue - 1, OrderType.Left, OpType.Func, 1,
                        param => BigComplex.Abs(param[0]))
                },
                {
                    "arg",
                    new OperatorInfo(int.MaxValue - 1, OrderType.Left, OpType.Func, 1, param => param[0].Argument)
                },
                {
                    "C", new OperatorInfo(int.MaxValue - 1, OrderType.Left, OpType.Func, 2, param =>
                    {
                        if (param[0].IsComplex || param[1].IsComplex)
                            throw new InvalidOperationException("Complex Can Not do this");
                        return MyMath.Combination(param[0].Real, param[1].Real);
                    })
                },
                {
                    "A", new OperatorInfo(int.MaxValue - 1, OrderType.Left, OpType.Func, 2, param =>
                    {
                        if (param[0].IsComplex || param[1].IsComplex)
                            throw new InvalidOperationException("Complex Can Not do this");
                        return MyMath.Arrangement(param[0].Real, param[1].Real);
                    })
                },
                { "PI", new OperatorInfo(int.MaxValue, OrderType.Left, OpType.Const, 0, _ => MyMath.Pi) },
                { "e", new OperatorInfo(int.MaxValue, OrderType.Left, OpType.Const, 0, _ => MyMath.E) },
                { "G", new OperatorInfo(int.MaxValue, OrderType.Left, OpType.Const, 0, _ => 6.67259E-11M) },
                { "Na", new OperatorInfo(int.MaxValue, OrderType.Left, OpType.Const, 0, _ => 6.02214076E23M) },
                { "c", new OperatorInfo(int.MaxValue, OrderType.Left, OpType.Const, 0, _ => 299_792_458M) },
                { "i", new OperatorInfo(int.MaxValue, OrderType.Left, OpType.Const, 0, _ => new BigComplex(0, 1)) },
            };
            _origin = input;
            Analyzer();
            Parser();
            _cmd = new Dictionary<string, Action<string[]>>()
            {
                {
                    "set", arg =>
                    {
                        if (arg[0] is "modulus" or "mo")
                        {
                            BigComplex.ShowModulus = bool.Parse(arg[2]);
                        }
                        else
                        {
                            throw new InvalidOperationException("参数无法识别");
                        }

                        Console.WriteLine("env changed ok");
                    }
                }
            };
        }

        public Standard2Rpn(string input, IDictionary<string, OperatorInfo> @operator)
        {
            _origin = input;
            _operator = @operator;
            Analyzer();
            Parser();
        }

        private void Analyzer()
        {
            if (string.IsNullOrWhiteSpace(_origin)) return;
            using var itor = _origin.GetEnumerator();
            StringBuilder sb = new();
            while (itor.MoveNext())
            {
                var cur = itor.Current;
                if (cur is ' ' or '\n') continue;
                if (sb.Length == 0)
                {
                    sb.Append(cur);
                    continue;
                }

                var isNumber = IsNumber(sb[^1]) && IsNumber(cur);
                var isLetter = IsLatter(sb[^1]) && IsLatter(cur);
                if (isNumber || isLetter)
                {
                    sb.Append(cur);
                    continue;
                }

                _item.Add(sb.ToString());
                sb.Clear();
                sb.Append(cur);
            }

            _item.Add(sb.ToString());
        }

        private static bool IsNumber(char c)
        {
            return c is >= '0' and <= '9' or '.';
        }

        private static bool IsLatter(char c)
        {
            return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
        }

        private void Parser()
        {
            if (_item.Count > 0 && string.Equals(_item[0], "command", StringComparison.OrdinalIgnoreCase))
            {
                IsCommand = true;
                return;
            }

            IsCommand = false;
            // 调度场算法开始
            Stack<string> opt = new();
            var i = 0;
            while (i < _item.Count)
            {
                var token = _item[i];
                if (i == 0 && token == "-")
                {
                    _rpn.Add(new RpnItem("0", ItemType.Number));
                }

                i++;
                if (BigDecimal.TryParse(token, out _))
                {
                    _rpn.Add(new RpnItem(token, ItemType.Number));
                    continue;
                }

                if (_operator.TryGetValue(token, out var value))
                {
                    if (value.Type is OpType.Func or OpType.Const)
                    {
                        opt.Push(token);
                        continue;
                    }

                    while (opt.Count != 0)
                    {
                        var o = opt.Peek();
                        if (o == "(")
                        {
                            break;
                        }

                        var opt2 = _operator[o];
                        if (opt2.Type == OpType.Func)
                        {
                            throw new InvalidOperationException("语法错误");
                        }

                        if ((value.Order == OrderType.Left && value.Priority <= opt2.Priority) ||
                            (value.Order == OrderType.Right && value.Priority < opt2.Priority))
                        {
                            var o2 = opt.Pop();
                            _rpn.Add(new RpnItem(o2, ItemType.Operator));
                            continue;
                        }

                        break;
                    }

                    opt.Push(token);

                    continue;
                }

                switch (token)
                {
                    case ",":
                    {
                        var findFlag = false;
                        while (opt.Count != 0)
                        {
                            var t = opt.Peek();
                            if (t == "(")
                            {
                                findFlag = true;
                                break;
                            }

                            opt.Pop();
                            _rpn.Add(new RpnItem(t, ItemType.Operator));
                        }

                        if (opt.Count == 0 && !findFlag)
                        {
                            throw new InvalidOperationException("语法错误");
                        }

                        if (i < _item.Count && _item[i] == "-")
                        {
                            // 逗号的下一个也是 - 那也是一个负号
                            _rpn.Add(new RpnItem("0", ItemType.Number));
                        }

                        continue;
                    }
                    case "(":
                    {
                        opt.Push(token);
                        if (i < _item.Count && _item[i] == "-")
                        {
                            // ( 下一个符号就是 - 那就是负数的符号
                            _rpn.Add(new RpnItem("0", ItemType.Number));
                        }

                        continue;
                    }
                    case ")":
                    {
                        var findFlag = false;
                        while (opt.Count != 0)
                        {
                            var o = opt.Peek();
                            if (o != "(")
                            {
                                _rpn.Add(new RpnItem(o, ItemType.Operator));
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
                            var oi = _operator[o];
                            if (oi.Type == OpType.Func)
                            {
                                _rpn.Add(new RpnItem(o, ItemType.Operator));
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
                    default:
                        throw new InvalidOperationException("发现无法识别的函数或运算符");
                }
            }

            while (opt.Count != 0)
            {
                var t = opt.Pop();
                _rpn.Add(new RpnItem(t, ItemType.Operator));
            }
        }

        public void Exec()
        {
            if (!IsCommand)
            {
                throw new InvalidOperationException("非命令，无法执行");
            }

            var cmdSeq = _item.ToArray()[2..];
            if (!_cmd.TryGetValue(cmdSeq[0], out var value))
            {
                throw new InvalidOperationException("不能识别的命令");
            }

            value(cmdSeq[2..]);
        }

        public BigComplex Calc()
        {
            if (IsCommand)
            {
                throw new InvalidOperationException("命令无法被执行");
            }

            Stack<BigComplex> dc = new();
            foreach (var item in _rpn)
            {
                if (item.IsNumber)
                {
                    dc.Push(item);
                    continue;
                }

                string op = item;
                var opInfo = _operator[op];
                var c = opInfo.CountOfOp;
                var opNum = new BigComplex[c];
                for (var i = opNum.Length - 1; i >= 0; i--)
                {
                    opNum[i] = dc.Pop();
                }

                dc.Push(opInfo.Func(opNum));
            }

            var result = dc.Pop();
            if (dc.Count != 0)
            {
                throw new InvalidOperationException("输入有误，结果不可靠");
            }

            return result;
        }

        public static implicit operator Standard2Rpn(string input)
        {
            return new Standard2Rpn(input);
        }

        public string GetOrigin()
        {
            StringBuilder sb = new();
            foreach (var a in _item)
            {
                sb.Append(a).Append("\t\t");
            }

            return sb.ToString();
        }

        public string GetFormattedOrigin()
        {
            StringBuilder sb = new();
            foreach (var a in _item)
            {
                sb.Append(a).Append(' ');
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            foreach (var rnt in _rpn)
            {
                sb.Append(rnt.Item).Append("    ");
            }

            return sb.ToString();
        }
    }
}