using ForexAITradingPC1Main.Forms;
using System;
using System.Windows.Forms;

namespace ForexAITradingPC1Main
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