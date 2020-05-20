using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;
using System.Net;
using System.IO;

namespace tarkistin3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        WebClient client = new WebClient();
        WebClient client2 = new WebClient();
        String DBServerURL;
        List<string> browsers = new List<string> { "msedge", "firefox", "chrome", "iexplore" };

        List<string> allowed = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Tick += new EventHandler(timer_Tick);
            client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(client_DownloadDataCompleted);
        }

        void client_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {

            inProgress = false;
        }

        bool inProgress = false;
        void timer_Tick(object sender, EventArgs e)
        {
            if (!inProgress)
            {
                inProgress = true;
                String u = System.Environment.UserName;
                String m = System.Environment.MachineName;

                Process[] ps = Process.GetProcesses();
                int myId = Process.GetCurrentProcess().Id;
                Stream data;
                String allPrograms = "";
                String allDescriptions = "";
                foreach (Process p in ps)
                {
                    if (p.MainWindowTitle.Length > 0 && p.Id != myId && !allowed.Contains(p.ProcessName))
                    {
                        if (!browsers.Contains(p.ProcessName))
                        {
                            allPrograms += p.ProcessName + " ";
                        }

                        if (p.MainWindowTitle.Length > 20)
                            allDescriptions += (p.MainWindowTitle.Substring(0, 20) + ";");
                        else
                            allDescriptions += (p.MainWindowTitle + ";");

                        //

                    }
                }

                if (allPrograms.Length + allDescriptions.Length > 200)
                {
                    allDescriptions = allDescriptions.Substring(0, Math.Min(allDescriptions.Length, 200 - Math.Min(allPrograms.Length, 100)));
                    allPrograms = allPrograms.Substring(0, Math.Min(allPrograms.Length,100));
                }
                client.QueryString.Clear();
                client.QueryString.Add("user", u + "_" + m);
                client.QueryString.Add("programs", allPrograms);
                client.QueryString.Add("descriptions", allDescriptions);
                try
                {
                    data = client.OpenRead(DBServerURL);
                    StreamReader reader = new StreamReader(data);
                    String s = reader.ReadToEnd();
                    Console.WriteLine(s);

                    reader.Close();
                    data.Close();
                }
                catch (WebException ex)
                {
                }
                DateTime dt = DateTime.Now;
                Title = dt.ToShortTimeString() + ":" + dt.Second;
                inProgress = false;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            Stream data = client2.OpenRead("http://staff.hamk.fi/~tlaitinen/new/osoite.txt");
            StreamReader reader = new StreamReader(data);
            DBServerURL = reader.ReadLine();
            int versio = 0;
            int.TryParse(reader.ReadLine(), out versio);
            if (versio < 1)
            {
                MessageBox.Show("Päivitä uudempi versio ohjelmasta.","Vanha versio",MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Close();
            }
            String s = "";
            do
            {
                s = reader.ReadLine();
                if (s != null) allowed.Add(s);
            } while (s != null);
#if DEBUG

            MessageBox.Show(DBServerURL + ":" + allowed[0]);
#endif
            data.Close();
            reader.Close();
            timer.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Haluatko poistua ohjelmasta?", "Confirmation", MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
                timer.Stop();
            }
        }
    }
}
