using System;

namespace Calc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("支持 + - * / ^ % ! 以及 位运算符 and, or, xor, not 和 常见数学函数 以及 常数 PI, e, 引力常量G，阿伏伽德罗常数 Na");
            while (true)
            {
                try
                {
                    Console.Write("请输入一个数学表达式：");
                    string line = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    if (line.Contains("bye"))
                    {
                        break;
                    }
                    Standard2RPN rpn = line;
                    //Console.WriteLine(rpn.GetOrigin());
                    //Console.WriteLine($"后缀表达式:{rpn}");
                    Console.WriteLine($"{rpn.GetFormattedOrigin()}= {rpn.Calc()}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"您的输入有语法错误, {e.Message}");
                }
            }
            Console.ReadKey();
        }
    }
}
