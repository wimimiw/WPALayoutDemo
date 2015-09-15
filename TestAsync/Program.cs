using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TestAsync
{
    class Program
    {
        delegate void FunctionDefine(int idx);

        static void Task(int idx)
        {
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(500);
                Console.WriteLine("Thread ID = " + Thread.CurrentThread.ManagedThreadId+"\tTask"+idx+" = "+i);
            }            
        }

        static void Main(string[] args)
        {
            FunctionDefine func1 = new FunctionDefine(Task);
            FunctionDefine func2 = new FunctionDefine(Task); 

            IAsyncResult iar1 = func1.BeginInvoke(1, null, null);
            IAsyncResult iar2 = func2.BeginInvoke(2, null, null);

            Console.WriteLine("Thread ID = " + Thread.CurrentThread.ManagedThreadId + " Start...");

            func1.EndInvoke(iar1);
            func2.EndInvoke(iar2);

            Console.WriteLine("Finish!");

            for (; ; ) ;
        }
    }
}
