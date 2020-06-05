using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace USTC.Software.hanyizhao.NetSpeedMonitor
{
    /// <summary>
    /// ProcessDetail.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessDetailWindow : Window
    {
        public ProcessDetailWindow(int id) : this(new ProcessView(id))
        {

        }

        public ProcessDetailWindow(ProcessView tempP)
        {
            this.process = tempP;
            InitializeComponent();
            ProcessID.Text = process.ID + "";
            if (process.SuccessGetInfo)
            {
                ProcessName.Text = process.Name ?? Tool.GetStringResource("Unknown");
                ProcessIcon.Source = process.Image;
                if (process.FilePath == null && !Tool.IsAdministrator())
                {
                    OpenButtonImage.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Shield.Handle,
                        Int32Rect.Empty, BitmapSizeOptions.FromRotation(Rotation.Rotate0));
                    OpenButtonText.Text = Tool.GetStringResource("RunAsAdministratorToGetMoreInformation");
                    OpenButton.Click += OpenButton_RunAsAdmin_Click;
					KillButton.IsEnabled = false;
                }
                else
                {
                    if (process.FilePath == null)
                    {
                        OpenButton.IsEnabled = false;
                    }
                    else
                    {
                        ProcessPath.Text = process.FilePath;
                        OpenButton.Click += OpenButton_OpenPath_Click;
					}
					KillButton.IsEnabled = true;
					KillButton.Click += KillButton_KillProcess_Click;
				}
            }
            else
            {
                ContentGrid.IsEnabled = false;
            }
        }

        private void OpenButton_OpenPath_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "/select,\"" + process.FilePath + "\"");
        }

        private void KillButton_KillProcess_Click(object sender, RoutedEventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = "taskkill.exe";
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			proc.StartInfo.Arguments = "/pid " + process.ID + " /f";
            proc.Start();
            proc.WaitForExit(1000);
			if (
                proc.HasExited && (
					// kill success
					proc.ExitCode == 0 ||
					// already killed
					proc.ExitCode == 128
                )
            )
			{
                KillButton.IsEnabled = false;
            }
        }

        private void OpenButton_RunAsAdmin_Click(object sender, RoutedEventArgs e)
        {
            if(Application.Current is App app)
            {
                app.FreeMutex();
                string exe = GetType().Assembly.Location;
                Process p = new Process
                {
                    StartInfo = new ProcessStartInfo(exe, "-processid " + process.ID)
                    {
                        Verb = "runas",
                    },
                };
                try
                {
                    bool b = p.Start();
                    app.TryToExit();
                }
                catch(Exception)
                {

                }
            }
        }

        private ProcessView process;

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            if(!process.SuccessGetInfo)
            {
                Dispatcher.InvokeAsync(new Action(() =>
                {
                    MessageBox.Show(Tool.GetStringResource("CantGetInformationOfThisProcessMaybeItsNotRunningNow_"),
                        Tool.GetStringResource("ERROR"));
                }));
            }
        }
    }
}
