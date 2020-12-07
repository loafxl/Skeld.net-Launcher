using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Win32;

namespace skeldswitcher //made by LoafX
{
    public partial class MainWindow : Window
    {
        private string savefolder;
        private string RegionInfoLocation;
        private string rootDir;

        public MainWindow()
        {
            InitializeComponent();

            string userfolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string locallow = Path.Combine(userfolder, @"AppData\LocalLow");
            savefolder = Path.Combine(locallow, @"Innersloth\Among Us");
            RegionInfoLocation = Path.Combine(savefolder, @"regionInfo.dat");
            rootDir = Directory.GetCurrentDirectory();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {

            if (File.Exists(RegionInfoLocation))
            {
                LaunchButton.IsEnabled = false;

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
                LaunchButtonText.Text = "Failed";
                MessageBox.Show("Your regionInfo.dat file doesnt exist! Try running Among Us for it to generate.");
                Close();
            }
        }

        private void DLCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                File.Delete(RegionInfoLocation);
                File.Move(rootDir + @"\regionInfo.dat", RegionInfoLocation);
                File.Delete(rootDir + @"\regionInfo.dat");
                SearchForAmongUs();
            }
            catch (Exception ex)
            {
                var crashlog = ex.ToString();
                MessageBox.Show(crashlog);
            }
        }

        public void SearchForAmongUs()
        {
            try
            {
                string steam32path;
                string config32path;
                bool steam = false;
                RegistryKey key32 = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\steam");

                foreach (string k32subKey in key32.GetSubKeyNames())
                {
                    using (RegistryKey subKey = key32.OpenSubKey(k32subKey)) 
                    {
                        object objRegisteredValue = key32.GetValue("SteamPath");
                        steam32path = objRegisteredValue.ToString();
                        config32path = Path.Combine(steam32path, "steamapps/libraryfolders.vdf");
                        string driveRegex = @"[A-Z]:\\";
                        if (File.Exists(config32path))
                        {
                            string[] configLines = File.ReadAllLines(config32path);
                            foreach (var item in configLines)
                            {
                                Match match = Regex.Match(item, driveRegex);
                                if (item != string.Empty && match.Success)
                                {
                                    string matched = match.ToString();
                                    string item2 = item.Substring(item.IndexOf(matched));
                                    item2 = item2.Replace("\\\\", "\\");
                                    item2 = item2.Replace("\"", "\\steamapps\\common\\Among Us\\");
                                    if (Directory.Exists(item2))
                                    {
                                        steam = true;
                                    }
                                }
                            }
                            if (Directory.Exists(steam32path + "\\steamapps\\common\\Among Us\\"))
                            {
                                steam = true;
                            }
                        }
                    }
                }
                if (steam == false)
                {
                    runAU();
                }
                else
                {
                    runAUSteam();
                }
            }
            catch (Exception ex)
            {
                var crashlog = ex.ToString();
                MessageBox.Show(crashlog);
            }
        }

        private void runAU()
        {
            try
            {
                string locationfile = Path.Combine(savefolder, "gameLocation");

                if (File.Exists(locationfile))
                {
                    if (!File.Exists(Path.Combine(File.ReadAllText(locationfile), "Among Us.exe")))
                    {
                        File.Delete(locationfile);
                    }
                }

                if (!File.Exists(locationfile))
                {
                    MessageBox.Show("The Launcher could not locate your Among Us. Select `Ok` to manually select your `Among Us.exe`");

                    OpenFileDialog dlg = new OpenFileDialog();
                    dlg.DefaultExt = ".exe";
                    dlg.Filter = "Among Us.exe|*.exe";
                    dlg.FilterIndex = 1;

                    Nullable<bool> result = dlg.ShowDialog();

                    if (result == true)
                    {
                        string filename = dlg.FileName;
                        var directions = filename.Remove(filename.Length - 12);

                        File.WriteAllText(locationfile, directions);

                        ProcessStartInfo startInfo2 = new ProcessStartInfo(Path.Combine(directions, "Among Us.exe"));
                        startInfo2.WorkingDirectory = directions;
                        Process.Start(startInfo2);
                        ongameclose();
                    }
                }
                else
                {
                    string gamelocation = File.ReadAllText(locationfile);

                    ProcessStartInfo startInfo2 = new ProcessStartInfo(Path.Combine(gamelocation, "Among Us.exe"));
                    startInfo2.WorkingDirectory = gamelocation;
                    Process.Start(startInfo2);
                    ongameclose();
                }
            }
            catch (Exception ex)
            {
                var crashlog = ex.ToString();
                MessageBox.Show(crashlog);
            }
        }

        private void runAUSteam()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam");
                object objRegisteredValue = key.GetValue("SteamExe");
                string regFilePath = objRegisteredValue.ToString();

                Process AUs = new Process();
                AUs.StartInfo.FileName = regFilePath;
                AUs.StartInfo.Arguments = "-applaunch 945360";
                AUs.Start();
                ongameclose();
            }
            catch (Exception ex)
            {
                var crashlog = ex.ToString();
                MessageBox.Show(crashlog);
            }
        }

        private void ongameclose()
        {
            try
            {
                Hide();
                Thread.Sleep(10000);
                Process[] pname = Process.GetProcessesByName("Among Us");
                if (pname.Length > 0)
                {
                    pname[0].WaitForExit();
                    File.WriteAllText(RegionInfoLocation, "");
                    Close();
                }
                else
                {
                    MessageBox.Show("Failed to attach to the game, Skeld.net will not remove when the game closes.");
                    Close();
                }
            }
            catch (Exception ex)
            {
                var crashlog = ex.ToString();
                MessageBox.Show(crashlog);
            }
        }
    }
}
