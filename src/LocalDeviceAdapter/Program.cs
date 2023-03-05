using System;
using System.Threading;
using LocalDeviceAdapter.Server;

namespace LocalDeviceAdapter
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // Application.EnableVisualStyles();
            // Application.SetCompatibleTextRenderingDefault(false);
            // Application.Run(new Form1());

            var worker = new ServerWorker();
            worker.RunWorkerAsync();

            Thread.Sleep(10000);

            worker.Dispose();
        }
    }
}