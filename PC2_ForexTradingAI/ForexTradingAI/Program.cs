using System;
using System.Windows.Forms;
using ForexAITradingPC2.Forms;

namespace ForexAITradingPC2
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}