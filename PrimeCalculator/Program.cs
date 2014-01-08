using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace PrimeCalculator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for a single instance application.
        /// </summary>
        static void Main()
        {
            bool is_first_instance = true;
            using (Mutex mutex = new Mutex(true, Application.ProductName, out is_first_instance))
            {
                if (is_first_instance)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                }
                else
                {
                    Windows windows = new Windows(true, true);
                    foreach (Window window in windows)
                    {
                        if (window.Title == "Prime Calculator")
                        {
                            window.Visible = true;
                            //window.Activate();
                            window.BringToFront();
                        }
                    }
                }
            }
        }
    }
}
