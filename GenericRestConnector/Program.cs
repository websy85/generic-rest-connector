using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using QlikView.Qvx.QvxLibrary;

namespace GenericRestConnector
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length >= 2)
            {
                new Server().Run(args[0], args[1]);
            }
        }
    }
}
