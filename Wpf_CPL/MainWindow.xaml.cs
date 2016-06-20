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

        public MainWindow()
        {
            InitializeComponent();
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
                        CreatePL();
                    }
                    else
                        throw new Exception("Выберите: Копировать на устройство или создать *.PLS");
                }
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
            for (int i = 0; i < fullList.Count.ToString().Length; i++)
                num *= 10;

            int qq = 1;

            foreach (var f in fullList)
            {
                double q = (double)qq / (double)num;

                string numName = q.ToString().Substring(q.ToString().IndexOf(',') + 1);

                if (numName.Length < fullList.Count.ToString().Length)
                {
                    int tt = fullList.Count.ToString().Length - numName.Length;

                    for (int i = 0; i < tt; i++)
                        numName += "0";
                }

                string name = numName + ".mp3";

                if (f.Url != null)
                {
                    DownloadMP3(f, Path.Combine(txtPath.Text, name));
                }
                else
                    File.Copy(f.FullName, Path.Combine(txtPath.Text, name), true);
            }
        }

        public void DownloadMP3(FileItem _fi, string _path)
        {
            WebClient _web = new WebClient();
            _web.DownloadFileAsync(_fi.Url, _path);
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
