using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using VkNet;
using VkNet.Categories;
using VkNet.Enums;
using VkNet.Enums.Filters;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Utils;
using Ookii.Dialogs.Wpf;
using System.Net;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;

namespace Wpf_CPL
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public VkApi vkapi = new VkApi();
        Settings scope = Settings.All;
        Random rnd = new Random();
        public List<FileItem> fullList = new List<FileItem>(); //Полный список песен
        public List<FileItem> vkList = new List<FileItem>(); //Лист с песнями вк;
        List<FileItem> tmpList = new List<FileItem>(); //Временный список
        int CountM = 0; //Количество всех песен
        private BackgroundWorker backgroundWorker;

        // Dispatcher mWorker =  Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));

        public MainWindow()
        {
            InitializeComponent();
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(BackgroundWorker_DoWork);
            backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(BackgroundWorker_ProgressChanged);
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);

        }


        private void btnAuthVk_Click(object sender, RoutedEventArgs e)
        {
            AuthVk avk = new AuthVk();
            avk.Owner = this;
            if (avk.ShowDialog() == true)
            {
                //vkapi = avk.vk;

                ProfileFields pf = new ProfileFields();
                pf = ProfileFields.Photo50;

                imgLogo.Source = new BitmapImage(new Uri(App.AuthPublic.Users.Get(App.AuthPublic.UserId.Value, pf).Photo50.ToString()));
                btnAdd.IsEnabled = true;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            music frmMusic = new music();
            frmMusic.Owner = this;

            if (frmMusic.ShowDialog() == true)
            {
                foreach (var r in frmMusic.listVk)
                {
                    FileItem fi = new FileItem(r.Name);
                    fi.Url = r.Path;
                    vkList.Add(fi);
                    lbMusic.Items.Add(fi.FullName);
                }
                frmMusic.Close();
            }
        }

        private void lbMusic_Drop(object sender, DragEventArgs e)
        {
            string[] droppedFiles = null;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                droppedFiles = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            }

            if ((null == droppedFiles) || (!droppedFiles.Any())) { return; }

            foreach (string s in droppedFiles)
            {
                lbMusic.Items.Add(s);
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lbMusic.Items.Remove(lbMusic.SelectedItem);
            }
            catch { }
        }

        /// <summary>
        /// Поиск файла в папке
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="pattern"></param>
        /// <param name="recursive"></param>
        public void FindInDir(DirectoryInfo dir, string pattern, bool recursive)
        {
            try
            {
                foreach (FileInfo file in dir.GetFiles(pattern))
                {
                    FileItem s = new FileItem(file.FullName);
                    fullList.Add(s);
                }
                if (recursive)
                {
                    foreach (DirectoryInfo subdir in dir.GetDirectories())
                    {
                        FindInDir(subdir, pattern, recursive);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void FindFiles(string dir, string pattern)
        {
            FindInDir(new DirectoryInfo(dir), pattern, true);

        }


        private void btnStart_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    if (lbMusic.Items.Count < 1)
                    {
                        throw new Exception("Добавьте файлы в список!");
                    }
                    else
                    {
                        if (chkCopy.IsChecked == true || chkPls.IsChecked == true)
                        {
                            fullList.Clear();
                            tmpList.Clear();

                            for (int i = 0; i < lbMusic.Items.Count; i++)
                            {
                                if (lbMusic.Items[i].ToString().Contains(@"\"))
                                    FindFiles(lbMusic.Items[i].ToString(), "*.mp3");
                            }
                            fullList.AddRange(vkList);
                            CountM = fullList.Count;
                            backgroundWorker.RunWorkerAsync();
                            // CreatePL();
                        }
                        else
                            throw new Exception("Выберите: Копировать на устройство или создать *.PLS");
                    }
                });
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btnChoose_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dlg.ShowDialog() == true)
            {
                txtPath.Text = dlg.SelectedPath;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        public void CreatePL()
        {
            int num = 1;
            for (int i = 0; i < CountM.ToString().Length; i++)
                num *= 10;
            int qq = 1;

            FileItem fi;

            for (int f = 0; f <= CountM; f++)
            {
                int _tmpRnd = rnd.Next(fullList.Count);
                fi = fullList[_tmpRnd];

                double q = (double)qq / (double)num;

                string numName = q.ToString().Substring(q.ToString().IndexOf(',') + 1);

                if (numName.Length <= CountM.ToString().Length)
                {
                    int tt = CountM.ToString().Length - numName.Length;

                    for (int i = 0; i < tt; i++)
                        numName += "0";
                }

                string name = numName + ".mp3";

                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    if (fi.Url != null)
                    {
                        WebClient _web = new WebClient();
                        _web.DownloadFile(fi.Url, Path.Combine(txtPath.Text, name));
                    }
                    else
                        File.Copy(fi.FullName, Path.Combine(txtPath.Text, name), true);
                });
                qq++;
                fullList.RemoveAt(_tmpRnd);
                backgroundWorker.ReportProgress((qq / CountM) * 100, Convert.ToString((qq / (double)CountM) * 100) + " %");
            }
        }
    
        public void DownloadMP3(FileItem _fi, string _path)
        {
            
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // pbProgress.Value = 0;
            //pbProgress.Maximum = fullList.Count;
            //  txbProgress.Text = "0 %";
           
                SetPL();
            
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                pbProgress.Value = e.ProgressPercentage;
                txbProgress.Text = e.UserState.ToString();
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
                MessageBox.Show("Cancel");
            else if (!(e.Error == null))
                MessageBox.Show("Error: " + e.Error.Message);
            else
            {
                txbProgress.Text = "100,0 %";
               // MessageBox.Show(String.Format("Копирование завершено! Скопировано {0} файлов!", CountM));
            }
        }

        public void SetPL()
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                if (chkCopy.IsChecked == true)
                    CreatePL();
                if (chkPls.IsChecked == true)
                { int i = 0; }
                else
                    backgroundWorker.ReportProgress(100, "100,0 %");

            });
        }
    }

    public class FileItem
    {
        public string FullName { get; set; }

        public Uri Url { get; set; }

        public FileItem(string file)
        {
            this.FullName = file;
        }

        public override string ToString()
        {
            return Path.GetFileNameWithoutExtension(FullName);
        }
    }
}
