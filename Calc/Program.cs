using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Calc
{
    public class Program
    {
        static CancellationTokenSource source;
        static bool Canceling = false;

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += OnKeyCancelPress;
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
                    using (source = new())
                    {
                        Canceling = false;
                        Standard2RPN rpn = line;
                        rpn.CancellationToken = source.Token;
                        //Console.WriteLine(rpn.GetOrigin());
                        //Console.WriteLine($"后缀表达式:{rpn}");
                        Stopwatch sw = new();
                        sw.Start();
                        Console.WriteLine($"{rpn.GetFormattedOrigin()} = {rpn.Calc()}");
                        sw.Stop();
                        Console.WriteLine($"计算总耗时：{sw.Elapsed}");
                    }

                }
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    Console.WriteLine($"您的输入有语法错误, {e.Message}");
                }
            }
        }

        public static void OnKeyCancelPress(object sender, ConsoleCancelEventArgs e)
        {
            if (!Canceling)
            {
                Canceling = true;
                source.Cancel(true);
                Console.WriteLine("正在取消任务");
            }
            e.Cancel = true;
        }
    }
}
