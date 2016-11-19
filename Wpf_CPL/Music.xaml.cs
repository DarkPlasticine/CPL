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
using TagLib;

namespace Wpf_CPL
{
    /// <summary>
    /// Логика взаимодействия для music.xaml
    /// </summary>
    public partial class music : Window
    {
        public VkApi vk;
        public List<MusicClass> listVk = new List<MusicClass>();
        private List<MusicClass> listSearch = new List<MusicClass>();
        private List<MusicClass> listGet = new List<MusicClass>();
        
        string _Query;

        public music()
        {
            GetFriends();//Получаем друзей
            GetPeople = GetPeopleList; //Выводим в список
            
            InitializeComponent();
            cmbFri.ItemsSource =  GetPeopleList;
            lbxMusic.ItemsSource = listSearch;
            lbxGetFriends.ItemsSource = listGet;
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
            public int Id { get; set; } //Id композиции
            public string Name { get; set; } //Название композиции и исполнителя
            public bool IsChecked { get; set; }//Добавление в список
            public Uri Path { get; set; }//Пусть скачки композиции
            public string Duration { get; set; }//Длительность аудиозаписи
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
                MusicClass chk = new MusicClass();
                chk.Id = (int)s.Id;
                chk.Name = string.Format("{0} - {1}", s.Artist.Trim(), s.Title.Trim());
                chk.Path = s.Url;
                chk.Duration = String.Format("{0}:{1:00}", s.Duration / 60,  s.Duration - ((s.Duration / 60)*60));

                string _sUrl = s.Url.ToString().Substring(0, s.Url.ToString().IndexOf(".mp3")+4);

                //TagLib.File _file = TagLib.File.Create(new VfsFileAbstraction());
                //ICodec _codec = (ICodec)_file.Properties.Codecs;
                //IAudioCodec _acodec = _codec as IAudioCodec;

                //chk.Duration = _acodec.AudioBitrate.ToString();

                if (listSearch.Where(p => p.Name.ToLower() == chk.Name.ToLower()).Count() < 1)
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
            listGet.Clear();
            chkAll.IsChecked = false;
            var _audioParams = new AudioGetParams();
            int key = GetPeopleList.FirstOrDefault(x => x.Value == (string)cmbFri.Text).Key;
            _audioParams.OwnerId = key;
            var _user = new User();
            var _audioList = App.AuthPublic.Audio.Get(out _user, _audioParams);

            foreach (var s in _audioList)
            {
                MusicClass chk = new MusicClass();
                chk.Id = (int)s.Id;
                chk.Name = string.Format("{0} - {1}", s.Artist, s.Title);
                chk.Path = s.Url;
                chk.Duration = String.Format("{0}:{1:00}", s.Duration / 60, s.Duration - ((s.Duration / 60) * 60));

                listGet.Add(chk);
            }
            lbxGetFriends.Items.Refresh();
        }

        private void btnGetWall_Click(object sender, RoutedEventArgs e)
        {
            listGet.Clear();
            chkAll.IsChecked = false;
            int key = GetPeopleList.FirstOrDefault(x => x.Value == (string)cmbFri.Text).Key;
            var _wallParams = new WallGetParams();
            _wallParams.OwnerId = key;
            int offset = 0;
            
            _wallParams.Count = 100;
            var _wall = App.AuthPublic.Wall.Get(_wallParams);

            foreach (var w in _wall.WallPosts)
            {

            }

            var _user = new User();
            
                
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (chkAll.IsChecked == true)
                listGet.ForEach(p => p.IsChecked = true);
            else
                listGet.ForEach(p => p.IsChecked = false);

            lbxGetFriends.Items.Refresh();
        }

        private void txtSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                btnSearch_Click(sender,e);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            listSearch.ForEach(p => p.IsChecked = true);
            lbxMusic.Items.Refresh();
        }
               
        private void cmiDisableAll_Click(object sender, RoutedEventArgs e)
        {
            listSearch.ForEach(p => p.IsChecked = false);
            lbxMusic.Items.Refresh();
        }

        private void cmiInvert_Click(object sender, RoutedEventArgs e)
        {
            listSearch.ForEach(p => { if (p.IsChecked == false) p.IsChecked = true; else p.IsChecked = false; });
            lbxMusic.Items.Refresh();
        }

        
    }
}
