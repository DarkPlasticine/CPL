using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Enums;
using VkNet.Enums.Filters;

namespace Wpf_CPL
{
    /// <summary>
    /// Логика взаимодействия для music.xaml
    /// </summary>
    public partial class music : Window
    {
        public VkApi vk;
        public List<CheckedListItem> listVk = new List<CheckedListItem>();
        private List<CheckedListItem> listSearch = new List<CheckedListItem>();
        private List<CheckedListItem> listGet = new List<CheckedListItem>();
        
        string _Query;

        public music()
        {
            GetFriends();
            GetPeople = GetPeopleList;
            
            InitializeComponent();
            cmbFri.DataContext =  GetPeopleList;
            lbxMusic.ItemsSource = listSearch;
        }

        public Dictionary<int, string> GetPeople { get; set; }
        public Dictionary<int, string> GetPeopleList = new Dictionary<int, string>();
        private void GetFriends()
        {
            GetPeopleList.Add((int)App.AuthPublic.UserId, "Моя музыка");
            var _friends = new FriendsGetParams();
            _friends.Fields = ProfileFields.FirstName | ProfileFields.LastName;
            var _r = App.AuthPublic.Friends.Get(_friends);

            foreach (var tmp in _r)
            {
                GetPeopleList.Add((int)tmp.Id, tmp.FirstName + " " + tmp.LastName);
            }
        }

        /// <summary>
        /// Запуск поиска
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            listSearch.Clear();
            _Query = txtSearch.Text;
            GetAudio(_Query, listSearch.Count);
           
        }

        /// <summary>
        /// Добавить к результату поиска
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSAdd_Click(object sender, RoutedEventArgs e)
        {
            GetAudio(_Query, listSearch.Count);
        }

        /// <summary>
        /// Класс хранения песен
        /// </summary>
        public class CheckedListItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsChecked { get; set; }
            public Uri Path { get; set; }
        }

        /// <summary>
        /// запрос аудио
        /// </summary>
        /// <param name="query">Название группы или песни</param>
        /// <param name="_offset">Смещение поиска</param>
        private void GetAudio(string query, int _offset)
        {
            long i;
            var _music = new AudioSearchParams();
            _music.Autocomplete = true;
            if (chkAuthor.IsChecked == true)
                _music.PerformerOnly = true;
            _music.Sort = VkNet.Enums.AudioSort.Popularity;
            _music.Count = 200;
            _music.Query = query;
            _music.Offset = _offset;

            var tmpList = App.AuthPublic.Audio.Search(_music, out i);

            foreach (var s in tmpList)
            {
                CheckedListItem chk = new CheckedListItem();
                chk.Id = (int)s.Id;
                chk.Name = string.Format("{0} - {1}", s.Artist, s.Title);
                chk.Path = s.Url;
                listSearch.Add(chk);
            }
            lbxMusic.Items.Refresh();
        }

        /// <summary>
        /// Добавить в вывод
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (tiSearch.IsSelected)
                listVk.AddRange(listSearch.Where(p => p.IsChecked == true));
            if (tiGet.IsSelected)
                listVk.AddRange(listGet.Where(p => p.IsChecked == true));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
           
        }


    }
        //private void trvMusic_Expanded(object sender, RoutedEventArgs e)
        //{
        //    //      LoadDisk();
           
        //    TreeViewItem item = (TreeViewItem)e.OriginalSource;
        //    item.Items.Clear();
        //    DirectoryInfo dir;
        //    if (item.Tag is DriveInfo)
        //    {
        //        DriveInfo drive = (DriveInfo)item.Tag;
        //        dir = drive.RootDirectory;
        //    }
        //    else dir = (DirectoryInfo)item.Tag;
        //    try
        //    {
        //        foreach (DirectoryInfo subDir in dir.GetDirectories())
        //        {
        //            TreeViewItem newItem = new TreeViewItem();
        //            newItem.Tag = subDir;
        //            newItem.Header = subDir.ToString();
        //            newItem.Items.Add("*");
        //            item.Items.Add(newItem);
        //        }
        //    }
        //    catch
        //    { }
        //}

        //public  class DirectoryRecord
        //{
        //    public DirectoryInfo Info { get; set; }

        //    public IEnumerable<FileInfo> Files 
        //    {
        //        get { return Info.GetFiles(); }
        //    }

        //    public IEnumerable<DirectoryRecord> Directories
        //    {
        //        get
        //        {
        //            return from di in Info.GetDirectories("*", SearchOption.AllDirectories)
        //                   select new DirectoryRecord { Info = di };
        //        }
        //    }
        //}
   
}
