using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Windows;
using System.Windows.Navigation;

namespace skeldswitcher //made by LoafX
{
    public partial class MainWindow : Window
    {
        private string RegionInfoLocation;
        private string rootDir;
        public const SslProtocols tls11prot = (SslProtocols)0x00000300;
        public const SecurityProtocolType Tls11 = (SecurityProtocolType)tls11prot;

        public MainWindow()
        {
            InitializeComponent();


            Guid localLowId = new Guid("A520A1A4-1780-4FF6-BD18-167343C5AF16");
            string locallow = GetKnownFolderPath(localLowId);
            RegionInfoLocation = locallow + @"\Innersloth\Among Us\regionInfo.dat";

            rootDir = Directory.GetCurrentDirectory();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        private string GetKnownFolderPath(Guid knownFolderId)
        {
            IntPtr pszPath = IntPtr.Zero;
            try
            {
                int hr = SHGetKnownFolderPath(knownFolderId, 0, IntPtr.Zero, out pszPath);
                if (hr >= 0)
                    return Marshal.PtrToStringAuto(pszPath);
                throw Marshal.GetExceptionForHR(hr);
            }
            finally
            {
                if (pszPath != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(pszPath);
            }
        }

        [DllImport("shell32.dll")]
        static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);

        private void DLCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            File.Delete(RegionInfoLocation);
            File.Move(rootDir + @"\regionInfo.dat", RegionInfoLocation);
            File.Delete(rootDir + @"\regionInfo.dat");
            SwitchButtonText.Text = "Switched";
        }
        private void SwitchButton_Click(object sender, RoutedEventArgs e)
        {

            if (File.Exists(RegionInfoLocation))
            {
                SwitchButton.IsEnabled = false;
                SwitchButtonText.Text = "Switching";

                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DLCompletedCallback);
                        webClient.DownloadFileAsync(new Uri("https://skeld.net/setup/regionInfo.dat"), rootDir + @"\regionInfo.dat");
                    }
                }
                catch (Exception ex)
                {
                    var crashlog = ex.ToString();
                    MessageBox.Show(crashlog);
                }

            }
            else
            {
                SwitchButtonText.Text = "Failed";
                MessageBox.Show("Your regionInfo.dat file doesnt exist! Try running Among Us for it to generate.");
                Close();
            }
        }

    }
}
