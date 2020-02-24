using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microsoft.Win32;

namespace FootballClubs
{
    class ViewModel : INotifyPropertyChanged
    {
        private FootballClub selectedClub;
        private bool unsaveDataBlock = false;
        public FootballClub SelectedClub
        {
            get { return selectedClub; }
            set
            {
                if (!this.unsaveDataBlock)
                    selectedClub = value;
                OnPropertyChanged("SelectedClub");
            }
        }

        public ObservableCollection<FootballClub> Clubs { get; set; }

        private Command pickImageCommand;

        public Command PickImageCommand
        {
            get {
                return pickImageCommand ?? (pickImageCommand = new Command(obj =>
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.FileName = "";
                    openFileDialog.DefaultExt = ".png";
                    openFileDialog.Filter = "Изображения (.png)|*.png";

                    Nullable<bool> result = openFileDialog.ShowDialog();

                    if (result == true && SelectedClub != null)
                    {
                        string filename = openFileDialog.FileName;
                        SelectedClub.ImagePath = filename;
                    }
                }));
            }
            set { }
        }

        private Command addCommand;
        public Command AddCommand
        {
            get
            {
                return addCommand ?? (addCommand = new Command(obj =>
                {
                    
                    FootballClub newClub = new FootballClub();
                    newClub.Id = Clubs.Count > 0 ? Clubs[Clubs.Count-1].Id + 1 : 1;
                    newClub.Name = "Укажите название";
                    newClub.ImagePath = "Укажите путь";
                    Clubs.Add(newClub);
                    SelectedClub = newClub;
                    unsaveDataBlock = true;
                },
                    (obj) => !this.unsaveDataBlock
                ));
            }
            set { }
        }

        private Command saveNewCommand;
        public Command SaveNewCommand
        {
            get
            {
                return saveNewCommand ?? (saveNewCommand = new Command(obj =>
                {
                    FootballClub clubToSave = obj as FootballClub;
                    if (clubToSave != null)
                        if (ServerConnection.SaveNewClub(SelectedClub))
                        {
                            MainWindow.ShowMessageBox("Добавление данных", "Данные добавлены");
                            this.unsaveDataBlock = false;
                        }
                },
                    obj => this.unsaveDataBlock
                ));
            }
            set { }
        }

        private Command saveEditedCommand;
        public Command SaveEditedCommand
        {
            get
            {
                return saveEditedCommand ?? (saveEditedCommand = new Command(obj =>
                {
                    FootballClub clubToEdit = obj as FootballClub;
                    if (clubToEdit != null)
                        if (ServerConnection.SaveEditedClub(clubToEdit))
                        {
                            MainWindow.ShowMessageBox("Редактирование данных", "Обновленные данные сохранены");
                            this.unsaveDataBlock = false;
                        }
                },
                    obj => !this.unsaveDataBlock && SelectedClub != null
                ));
            }
            set { }
        }

        private Command removeCommand;
        public Command RemoveCommand
        {
            get
            {
                return removeCommand ?? (removeCommand = new Command(obj =>
                {
                    FootballClub clubToRemove = obj as FootballClub;
                    if (clubToRemove != null)
                    {
                        MainWindow.ShowMessageBox("Удаление данных", ServerConnection.RemoveClub(clubToRemove.Id));
                        Clubs.Remove(clubToRemove);
                    }
                },
                (obj) => Clubs.Count > 0 && !this.unsaveDataBlock && SelectedClub != null));
            }
            set { }
        }

        public ViewModel()
        {
            LoadList();
        }

        private void LoadList()
        {
            List<FootballClub> loadedClubs = ServerConnection.LoadClubs();
            Clubs = new ObservableCollection<FootballClub>(loadedClubs);
            foreach (FootballClub c in Clubs) c.ImagePath = ServerConnection.LoadImage(c);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if(PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
