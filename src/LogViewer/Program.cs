using System;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

namespace Bluehands.Repository.Diagnostics
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string startUpFile = GetStartUpFilePath(args);
            Application.Run(new LogViewerForm(startUpFile));
        }

        static string GetStartUpFilePath(string[] args)
        {
            string startUpFile = null;
            if (args.Length > 0)
            {
                startUpFile = args[0];
            }
            else if (AppDomain.CurrentDomain.SetupInformation.ActivationArguments != null)
            {
                var activationData = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData;
                if (activationData != null && activationData.Length > 0)
                {
                    startUpFile = activationData[0];
                    if (startUpFile.StartsWith("file:///"))
                    {
                        startUpFile = startUpFile.Replace("file:///", "");
                    }
                    else if (startUpFile.StartsWith("file://"))
                    {
                        startUpFile = startUpFile.Replace("file://", "\\\\");
                    }
                    startUpFile = startUpFile.Replace("%20", " ");
                }
            }
            //MessageBox.Show(startUpFile);

            return startUpFile;
        }
    }
}
