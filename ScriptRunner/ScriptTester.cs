using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.IO;
using Model;

public class ScriptTester : IScriptRunner
{
    public object Run(object[] args)
    {
        try
        {
            if (args.Length == 2)   // ScriptMethod(Client, string)
            {
                Client client = args[0] as Client;
                string extra = args[1].ToString();
                if (client != null)
                {
                    return PublicScriptMethod(client, extra);
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return null;
        }
    }

    #region Test Methods
    public bool PublicScriptMethod(Client client, string extra)
    {
        try
        {
            MessageBox.Show("PublicScriptMethod() ran successfully.", Application.ProductName);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false;
        }
    }
    public static bool StaticScriptMethod()
    {
        try
        {
            MessageBox.Show("StaticScriptMethod() ran successfully.", Application.ProductName);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false;
        }
    }
    public bool NonStaticScriptMethod()
    {
        try
        {
            MessageBox.Show("NonStaticScriptMethod() ran successfully.", Application.ProductName);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false;
        }
    }
    public long Multiply(int a, int b)
    {
        try
        {
            return a * b;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return 0L;
        }
    }
    #endregion
}
