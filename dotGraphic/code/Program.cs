using System;
using System.Windows.Forms;

namespace DotGraphic
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] argcs)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 form1 = new Form1();
            //param[0] = "";
            if (argcs.Length > 0)
            {
                form1.inParams[0] = argcs[0].Replace("dotgraphic:", "").Replace("/", "");
            }
            Application.Run(form1);
        }
    }
}
