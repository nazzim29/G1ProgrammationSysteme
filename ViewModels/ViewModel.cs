using EasySave_GUI.Libs;
using EasySave_GUI.Models;
using EasySave_GUI.Properties;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Management;
using System.Diagnostics;
using System;

namespace EasySave_GUI.ViewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ManagementEventWatcher processStartEvent;
        private ManagementEventWatcher processStopEvent;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private Preferences _preferences;
        private Backup _backup;
        private Backup _newbackup;
        private bool _CanLaunch;

        public Preferences Preferences
        {
            get
            {
                return _preferences;
            }
            set
            {
                _preferences = value; OnPropertyChanged("Preferences");
            }
        }

        public bool CanLaunch
        {
            get
            {
                return _CanLaunch;
            }
            set
            {
                _CanLaunch = value;
                OnPropertyChanged("CanLaunch");
            }
        }
        public Backup NewBackup
        {
            get { 
                if( _newbackup == null ) _newbackup = new Backup();
                return _newbackup; }
            set 
            { 
                _newbackup = value;
                OnPropertyChanged("NewBackup");
            }
        }
        private ObservableCollection<Backup> _backups;
        private LogService _logService;
        private LogService LogService
        {
            get
            {
                if(_logService == null) _logService = new LogService();
                return _logService;
            }
        }
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

        private ICommand _changetype;
        private ICommand _addTaskCommand;
        private ICommand _deleteTaskCommand;
        private ICommand _LaunchCommand;
        private ICommand _changeLanguageCommand;
        private ICommand _PauseTask;
        private ICommand _StopTask;


        public ICommand StopTask
        {
            get
            {
                if (_StopTask == null) _StopTask = new RelayCommand(() => Backup.Stop(), (object sender) => Backup != null && Backup.State == BackupState.En_Cours);
                return _StopTask;
            }
        }
        public ICommand PauseTask
        {
            get
            {
                if (_PauseTask == null) _PauseTask = new RelayCommand(() => Backup.Pause(), (object sender) => Backup != null && Backup.State == BackupState.En_Cours);
                return _PauseTask;
            }
        }

        
        public ICommand ChangeTypeCommand
        {
            get
            {
                if (_changetype == null)
                {
                    _changetype = new RelayCommand(()=>ChangeType(), (object sender)=>true);
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
                    _addTaskCommand = new RelayCommand(() => AddTask(), (object param) => 
                    NewBackup != null && NewBackup.Name != null && NewBackup.Name!="" && NewBackup.Destination!= null && 
                    NewBackup.Destination != "" && NewBackup.Source != null && NewBackup.Source != "") ;
                }
                return _addTaskCommand;
            }
        }
        public ICommand DeleteTaskCommand
        {
            get
            {
                if (_deleteTaskCommand == null) _deleteTaskCommand = new RelayCommand(() => DeleteTask(), (object sender) => Backup != null && Backup.State != BackupState.En_Cours);
                return _deleteTaskCommand;
            }
        }
        public ICommand LaunchCommand
        {
            get
            {
                if (_LaunchCommand == null) _LaunchCommand = new RelayCommand(() => Launch(), (object sender) => Backup != null && CanLaunch);
                return _LaunchCommand;
            }
        }
        public ICommand ChangeLanguageCommand
        {
            get
            {
                if (_changeLanguageCommand == null) _changeLanguageCommand = new RelayCommand(() => ChangeLanguage(), (object sender) => true);
                return _changeLanguageCommand;
            }
        }

        private void ChangeLanguage()
        {
            Preferences.language = Preferences.language.ToString() == "EN" ? "FR" : "EN";
        }
        public void AddTask()
        {
            this.Backups.Add(new Backup
            {
                Name = NewBackup.Name,
                Destination = NewBackup.Destination,
                Source = NewBackup.Source,
                Type = NewBackup.Type
            });
        }
        private void ChangeType()
        {
            Backup.Type = BackupType.Complete;
        }
        private void DeleteTask()
        {
            Backups.Remove(Backup);
        }
        private void Launch()
        {
            if (!CanLaunch) return;
            
            if (Backup.State != BackupState.En_Cours)
            {
                Backup.Start(LogService,Preferences.CryptExt);
                return;
            }
            CanLaunch = false;
        }

        public ViewModel()
        {
            Preferences = Preferences.fromFile();
            processStartEvent = new ManagementEventWatcher("SELECT * FROM Win32_ProcessStartTrace");
            processStopEvent = new ManagementEventWatcher("SELECT * FROM Win32_ProcessStopTrace");
            var fd = System.Diagnostics.Process.GetProcesses().Select(el=>el.ProcessName).ToList();
            CanLaunch = !fd.Contains(Preferences.LogicielMetier.Replace(".exe",""));
            processStartEvent.EventArrived += new EventArrivedEventHandler(processlaunched);
            processStopEvent.EventArrived += new EventArrivedEventHandler(processended);
            processStartEvent.Start();
            processStopEvent.Start();
            Backups = new ObservableCollection<Backup>(Backup.fromFile());
            Backups.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Backups_ChangedCollection);
            PropertyChanged += SaveTasks;
            PropertyChanged += checklaunch;

        }

        private void processended(object sender, EventArrivedEventArgs e)
        {
            var fd = System.Diagnostics.Process.GetProcesses().Select(el => el.ProcessName).ToList();
            CanLaunch = Backup != null && Backup.State != BackupState.En_Cours && !fd.Contains(Preferences.LogicielMetier.Replace(".exe", ""));
            if (CanLaunch)
            { 
                foreach (var i in Backups)
                {
                    if (i.State == BackupState.En_Attente)
                    {
                        i.Start(LogService, Preferences.CryptExt);
                    }
                }
            }
        }

        private void processlaunched(object sender, EventArrivedEventArgs e)
        {
            var proname = e.NewEvent.Properties["ProcessName"].Value.ToString();
            if(proname == Preferences.LogicielMetier)
            {
                CanLaunch = false;
                foreach(var i  in Backups)
                {
                    if(i.State == BackupState.En_Cours)
                    {
                        i.Pause();
                    }
                }
            }
        }


        private void checklaunch(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Backup")
            {
                var fd = System.Diagnostics.Process.GetProcesses().Select(el => el.ProcessName).ToList();
                CanLaunch = Backup.State != BackupState.En_Cours && !fd.Contains(Preferences.LogicielMetier.Replace(".exe", ""));
            }
        }
        private void SaveTasks(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Backups" || e.PropertyName == "Backup") LogService.Log(this.Backups.Select(el => new { name = el.Name, source = el.Source, destination = el.Destination, type = el.Type }), new LogTasks());
        }
        private void Backups_ChangedCollection(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach(var item in e.NewItems)
                {
                    (item as Backup).PropertyChanged += SaveTasks;
                }
            }
            OnPropertyChanged("Backups");
        }

   
    }
}
