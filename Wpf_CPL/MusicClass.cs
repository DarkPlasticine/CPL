using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf_CPL
{
    public class MusicClass
    {
        public int Id { get; set; } //Id композиции
        public string Name { get; set; } //Название композиции и исполнителя
        public bool IsChecked { get; set; }//Добавление в список
        public Uri Path { get; set; }//Пусть скачки композиции
        public string Duration { get; set; }//Длительность аудиозаписи
    }
}
