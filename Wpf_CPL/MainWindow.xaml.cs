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
using System.Runtime.CompilerServices;

namespace Wpf_CPL
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public VkApi vkapi = new VkApi();
        Settings scope = Settings.All;
        Random rnd = new Random();
        public List<FileItem> fullList = new List<FileItem>(); //Полный список песен
        public List<FileItem> vkList = new List<FileItem>(); //Лист с песнями вк;
        List<FileItem> tmpList = new List<FileItem>(); //Временный список
        int CountM = 0; //Количество всех песен
        string SavePath;
        BackgroundWorker backgroundWorker;

        public event PropertyChangedEventHandler PropertyChanged;

        public int MyCount
        {
            get { return CountM; }
            set { CountM = value; OnPropertyChanged(); }
        }

        private void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

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

            CountM += vkList.Count;
            tbxCount.Text = CountM.ToString();
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
                CountM += Directory.GetFiles(s, "*.mp3", SearchOption.AllDirectories).Count();
                lbMusic.Items.Add(s);
                tbxCount.Text = CountM.ToString();
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(lbMusic.SelectedItem.ToString()))
                    CountM -= Directory.GetFiles(lbMusic.SelectedItem.ToString(), "*.mp3", SearchOption.AllDirectories).Count();
                else
                    CountM -= 1;
                lbMusic.Items.Remove(lbMusic.SelectedItem);
                tbxCount.Text = CountM.ToString();

            }
            catch { }
        }

        #region Поиск файлов

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

        #endregion

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
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
                           if (Directory.Exists(lbMusic.Items[i].ToString()))
                                FindFiles(lbMusic.Items[i].ToString(), "*.mp3");
                        }

                        fullList.AddRange(vkList);
                        CountM = fullList.Count;
                        pbProgress.Value = 0;
                        pbProgress.Maximum = 100;
                        SavePath = txtPath.Text;
                        pbCircle.Visibility = Visibility.Visible;
                        tbxCount.Text = CountM.ToString();
                        backgroundWorker.RunWorkerAsync();
                    }
                    else
                        throw new Exception("Выберите: Копировать на устройство или создать *.PLS");
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btnChoose_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new VistaFolderBrowserDialog();
            if (dlg.ShowDialog() == true)
            {
                txtPath.Text = dlg.SelectedPath;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        public void CreatePL(List<FileItem> _fullList)
        {
            int num = 1;
            int _countM = _fullList.Count;
            int percent = 100 / _countM; 
            for (int i = 0; i < _countM.ToString().Length; i++)
                num *= 10;
            int qq = 1;

            FileItem fi;

            do
            {
                int _tmpRnd = rnd.Next(_fullList.Count);
                fi = _fullList[_tmpRnd];

                double q = (double)qq / (double)num;

                string numName = q.ToString().Substring(q.ToString().IndexOf(',') + 1);

                if (numName.Length <= _countM.ToString().Length)
                {
                    int tt = _countM.ToString().Length - numName.Length;

                    for (int i = 0; i < tt; i++)
                        numName += "0";
                }

                string name = numName + ".mp3";

                if (fi.Url != null)
                {
                    WebClient _web = new WebClient();
                    _web.DownloadFile(fi.Url, Path.Combine(SavePath, name));
                }
                else
                    File.Copy(fi.FullName, Path.Combine(SavePath, name), true);
                qq++;
                _fullList.RemoveAt(_tmpRnd);
                backgroundWorker.ReportProgress(percent);
                percent += 100 / _countM;
            } while (_fullList.Count != 0);
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            CreatePL(fullList);
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbProgress.Value = e.ProgressPercentage;
            txbProgress.Text = e.ProgressPercentage.ToString() + " %";
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
                MessageBox.Show("Cancel");
            else if (!(e.Error == null))
                MessageBox.Show("Error: " + e.Error.Message);
            else
            {
                txbProgress.Text = "100 %";
                pbProgress.Value = 100;
                pbCircle.Visibility = Visibility.Collapsed;
            }
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
