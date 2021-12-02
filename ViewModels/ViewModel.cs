using EasySave_GUI.Libs;
using EasySave_GUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EasySave_GUI.ViewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private Backup _backup;
        private ObservableCollection<Backup> _backups;
        private ICommand _changetype;
        
        public Backup Backup
        {
            get { return _backup; }
            set
            {
                if (_backup == value) return;
                _backup = value;
                OnPropertyChanged("Backup");
            }
        }
        public ObservableCollection<Backup> Backups
        {
            get { return _backups; }
            set
            {
                if(_backups == value) return;
                _backups = value;
                OnPropertyChanged("Backups");
            }
        }
        private ICommand _addTaskCommand;
        public ICommand ChangeTypeCommand
        {
            get
            {
                if (_changetype == null)
                {
                    _changetype = new RelayCommand(ChangeType, null);
                }
                return _changetype;
            }
        }
        public ICommand AddTaskCommand
        {
            get
            {
                if(_addTaskCommand == null)
                {
                    _addTaskCommand = new RelayCommand(AddTask, (object param) => true) ;
                }
                return _addTaskCommand;
            }
        }
        public void AddTask(object task)
        {
            Backup.Name = task.ToString();
            return;
            Backup t = task as Backup;
            if (t == null) return;
            Backups.Add(t);
            Backup = new Backup();
            Backup.Name = "khlas";
            Backup.Source = "c:\\";
        }
        private void ChangeType(object param)
        {
            Backup.Type = BackupType.Complete;
        }

        public ViewModel()
        {
            Backups = new ObservableCollection<Backup>(Backup.fromFile());
            Backups.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Backups_ChangedCollection);

        }

        private void Backups_ChangedCollection(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Backups");
        }
    }
}
