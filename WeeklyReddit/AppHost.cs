using System;
using System.Threading;

namespace WeeklyReddit
{
    public static class AppHost
    {
        public static void RunAndBlock(Action action)
        {
            var done = new ManualResetEventSlim(false);
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                AttachCtrlcSigtermShutdown(done, cancellationTokenSource);

                action();

                Console.WriteLine("Application started. Press Ctrl+C to shut down.");
                done.Wait();
            }
        }

        private static void AttachCtrlcSigtermShutdown(ManualResetEventSlim manualResetEvent,
            CancellationTokenSource cancellationTokenSource)
        {
            void Shutdown()
            {
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    Console.WriteLine("Application is shutting down...");
                    cancellationTokenSource.Cancel();
                }
                
                manualResetEvent.Set();
            }

            AppDomain.CurrentDomain.ProcessExit += (sender, args) => Shutdown();
            Console.CancelKeyPress += (sender, args) =>
            {
                Shutdown();
                args.Cancel = true;
            };
        }
    }
}