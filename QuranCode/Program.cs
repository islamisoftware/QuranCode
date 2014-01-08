using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

static class Program
{
    /* Single Instance example
    //static void Main()
    //{
    //    bool is_first_instance = true;
    //    using (Mutex mutex = new Mutex(true, Application.ProductName, out is_first_instance))
    //    {
    //        if (is_first_instance)
    //        {
    //            Application.EnableVisualStyles();
    //            Application.SetCompatibleTextRenderingDefault(false);
    //            Application.Run(new MainForm());
    //        }
    //        else
    //        {
    //            Windows windows = new Windows(true, true);
    //            foreach (Window window in windows)
    //            {
    //                if (window.Title == "Prime Calculator")
    //                {
    //                    window.Visible = true;
    //                    //window.Activate();
    //                    window.BringToTop();
    //                }
    //            }
    //        }
    //    }
    //}
    */

    // disable the X close icon
    const int MF_BYPOSITION = 0x400;
    [DllImport("User32")]
    private static extern int RemoveMenu(IntPtr hMenu, int nPosition, int wFlags);
    [DllImport("User32")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
    [DllImport("User32")]
    private static extern int GetMenuItemCount(IntPtr hWnd);
    static void DisableCloseIcon(IntPtr system_menu_handle)
    {
        int system_menu_item_count = GetMenuItemCount(system_menu_handle);
        RemoveMenu(system_menu_handle, system_menu_item_count - 1, MF_BYPOSITION);
    }

    static void AutoUpdate()
    {
        // NIY
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        if (Control.ModifierKeys == (Keys.Shift | Keys.Control))
        {
            Globals.EDITION = Edition.Research;
        }
        else if (Control.ModifierKeys == Keys.Shift)
        {
            Globals.EDITION = Edition.Lite;
        }
        else if (Control.ModifierKeys == Keys.Control)
        {
            Globals.EDITION = Edition.Grammar;
        }
        else
        {
            Globals.EDITION = Edition.Standard;
        }

        AutoUpdate();

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        MainForm form = new MainForm();

        // disable the X close button of the form
        //IntPtr system_menu_handle = GetSystemMenu(form.Handle, false);
        //DisableCloseIcon(system_menu_handle);
        
        Application.Run(form);
    }
}
