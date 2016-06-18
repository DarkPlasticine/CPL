using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
using System.Xml.Serialization;
using VkNet;
using VkNet.Categories;
using VkNet.Enums;
using VkNet.Enums.Filters;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Utils;

namespace Wpf_CPL
{
    /// <summary>
    /// Логика взаимодействия для AuthVk.xaml
    /// </summary>
    public partial class AuthVk : Window
    {
        public VkApi vk = new VkApi();
        Settings scope = Settings.All;
        public static Dictionary<string, string> dicSU = new SerializableDictionary<string, string>();

        public Dictionary<string, string> Get { get; set; }

        public string GetP()
        {
            return dicSU[AccountBox.Text];
        }

        private string Lang(string strLang)
        {
            if (strLang.Contains("ru"))
                return "RU";
            if (strLang.Contains("en"))
                return "EN";
            return "";
        }

        public AuthVk()
        {
            try
            {
                dicSU = Load();
                Get = dicSU;
                InitializeComponent();

                btnLang.Content = Lang(InputLanguageManager.Current.CurrentInputLanguage.Name.ToString());
               

                InputLanguageManager.Current.InputLanguageChanged += new InputLanguageEventHandler((sender, e) =>
                {
                    string strLang = e.NewLanguage.Name;
                    btnLang.Content = Lang(strLang);
                });

                PasswordBox.Password = AccountBox.SelectedValue.ToString();
            }
            catch { }
        }

        private void Auth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AccountBox.Text.Trim() != "" && PasswordBox.Password.Trim() != "")
                {
                    var apia = new ApiAuthParams();
                    apia.Login = AccountBox.Text;
                    apia.Password = PasswordBox.Password;
                    apia.ApplicationId = 4551110;
                    apia.Settings = scope;

                    if (chkRemember.IsChecked == true) //Если сохранять пароль, то заходим
                    {
                        if (dicSU.ContainsKey(apia.Login))//проверяем, есть ли такая пара логин/пароль
                        {
                            if (dicSU[apia.Login] != apia.Password)//если есть, и текущий введеный пароль не совпадает с сохранненым, прашиваем сохранить ли введенный сейчас.
                            {
                                if (MessageBox.Show("Пароль не совпадает с текущим, сохранить новый пароль?", "Ошибка", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    dicSU[apia.Login] = apia.Password;
                                }
                            }
                        }
                        else //иначе просто заносим новую пару
                            dicSU.Add(apia.Login, apia.Password);

                        Save(dicSU);
                        Get = dicSU;
                    }

                    App.AuthPublic.Authorize(apia);
                    DialogResult = true;
                }
                else { MessageBox.Show("Заполнены не все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Save(Dictionary<string, string> _dic)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));
            using (FileStream fs = new FileStream("settings.dat", FileMode.OpenOrCreate))
            {
                xmlSerializer.Serialize(fs, _dic);
            }
        }

        public Dictionary<string, string> Load()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));
            if (File.Exists("settings.dat"))
                using (FileStream fs = new FileStream("settings.dat", FileMode.Open))
                {
                    return (Dictionary<string, string>)xmlSerializer.Deserialize(fs);
                }
            else
            {
                Dictionary<string, string> s = new Dictionary<string,string>();
                return s;
            }
        }

        private void AccountBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AccountBox.SelectedIndex != -1 && dicSU.ContainsKey(AccountBox.Text))
            {
                PasswordBox.Password = AccountBox.SelectedValue.ToString();
                
            }
        }
    }
}
