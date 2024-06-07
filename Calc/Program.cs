using System;
using System.Diagnostics;
using System.Threading;

namespace Calc
{
    public static class Program
    {
        private static CancellationTokenSource _source;
        private static bool _canceling = true;

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += OnKeyCancelPress;
            Console.WriteLine("支持 + - * / ^ % ! 以及 位运算符 and, or, xor, not 和 常见数学函数 以及 常数 PI, e, 引力常量G，阿伏伽德罗常数 Na");
            while (true)
            {
                try
                {
                    Console.Write("请输入一个数学表达式：");
                    var line = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if (line.Contains("bye"))
                    {
                        break;
                    }

                    using (_source = new CancellationTokenSource())
                    {
                        _canceling = false;
                        Standard2Rpn rpn = line;
                        Standard2Rpn.CancellationToken = _source.Token;
                        //Console.WriteLine(rpn.GetOrigin());
                        //Console.WriteLine($"后缀表达式:{rpn}");
                        Stopwatch sw = new();
                        sw.Start();
                        if (rpn.IsCommand)
                        {
                            rpn.Exec();
                        }
                        else
                        {
                            Console.WriteLine($"{rpn.GetFormattedOrigin()} = {rpn.Calc()}");
                        }

                        sw.Stop();
                        Console.WriteLine($"计算总耗时：{sw.Elapsed}");
                        _canceling = true;
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception e)
                {
                    Console.WriteLine($"您的输入有语法错误, {e.Message}");
                }
            }
        }

        private static void OnKeyCancelPress(object sender, ConsoleCancelEventArgs e)
        {
            if (!_canceling)
            {
                _canceling = true;
                _source.Cancel(true);
                Console.WriteLine("正在取消任务");
            }

            e.Cancel = true;
        }
    }
}