using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;

namespace FootballClubs
{
    [Serializable]
    public sealed class FootballClub : INotifyPropertyChanged
    {
        private int id;
        private string name;
        private string imagePath;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        public string Name { 
            get { return name; } 
            set { 
                name = value;
                OnPropertyChanged("Name");
            } 
        }
        public string ImagePath
        {
            get { return imagePath; }
            set {
                if (File.Exists(value))
                {
                    imagePath = value;
                }
                OnPropertyChanged("ImagePath");
            }
        }

        public FootballClub()
        {

        }

        public FootballClub(int id, string name, string imagePath)
        {
            this.Id = id;
            this.Name = name;
            this.ImagePath = imagePath;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if(PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

    }
}
