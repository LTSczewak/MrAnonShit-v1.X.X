using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
// Replace with the actual namespace of the KeyAuth library if it's different
using KeyAuth;

namespace MrAnonCrypter
{
    internal static class Program
    {
        private static readonly string _binPath = AppDomain.CurrentDomain.BaseDirectory + "bin";

        public static api KeyAuthApp = new api(
            name: "",
            ownerid: "",
            secret: "",
            version: "1.0"
        );

        [STAThread]
        private static void Main()
        {
            if (!Directory.Exists(_binPath))
                Directory.CreateDirectory(_binPath);
            Settings.SaveDirectory = _binPath;
            SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            KeyAuthApp.init();
            if (!KeyAuthApp.response.success)
            {
                MessageBox.Show("KeyAuth initialization failed: " + KeyAuthApp.response.message);
                return;
            }

            var loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                Application.Run(new Form1());
            }
            else
            {
                MessageBox.Show("Login failed or was cancelled.");
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}