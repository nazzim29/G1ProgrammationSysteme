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
using System.Windows;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

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
        private Serveur Serveur;
        private Thread ServerThread;
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
                var fd = Process.GetProcesses().Select(el => el.ProcessName).ToList();
                return !fd.Contains(Preferences.LogicielMetier.Replace(".exe", ""));
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
                if (_StopTask == null) _StopTask = new RelayCommand((object param) => Backup.Stop(), (object sender) => Backup != null && (Backup.State == BackupState.En_Cours || Backup.State == BackupState.En_Attente));
                return _StopTask;
            }
        }
        public ICommand PauseTask
        {
            get
            {
                if (_PauseTask == null) _PauseTask = new RelayCommand((object param) => Backup.Pause(), (object sender) => Backup != null && Backup.State == BackupState.En_Cours);
                return _PauseTask;
            }
        }

        
        public ICommand ChangeTypeCommand
        {
            get
            {
                if (_changetype == null)
                {
                    _changetype = new RelayCommand((object param)=>ChangeType(), (object sender)=>true);
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
                    _addTaskCommand = new RelayCommand((object param) => AddTask(), (object param) => 
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
                if (_deleteTaskCommand == null) _deleteTaskCommand = new RelayCommand((object param) => DeleteTask(), (object sender) => Backup != null && Backup.State != BackupState.En_Cours);
                return _deleteTaskCommand;
            }
        }
        public ICommand LaunchCommand
        {
            get
            {
                if (_LaunchCommand == null) _LaunchCommand = new RelayCommand((object param) => Launch(), (object sender) => Backup != null && CanLaunch);
                return _LaunchCommand;
            }
        }
        public ICommand ChangeLanguageCommand
        {
            get
            {
                if (_changeLanguageCommand == null) _changeLanguageCommand = new RelayCommand((object param) => ChangeLanguage(), (object sender) => true);
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
                Backup.Start(LogService,Preferences.CryptExt,Preferences.LimiteFichier,Preferences.Prioritaire);
                return;
            }
            OnPropertyChanged("CanLaunch");
        }

        public ViewModel()
        {
            Preferences = Preferences.fromFile();
            processStartEvent = new ManagementEventWatcher("SELECT * FROM Win32_ProcessStartTrace");
            processStopEvent = new ManagementEventWatcher("SELECT * FROM Win32_ProcessStopTrace");
            processStartEvent.EventArrived += new EventArrivedEventHandler(processlaunched);
            processStopEvent.EventArrived += new EventArrivedEventHandler(processended);
            processStartEvent.Start();
            processStopEvent.Start();
            Backups = new ObservableCollection<Backup>(Backup.fromFile());
            Backups.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Backups_ChangedCollection);
            PropertyChanged += SaveTasks;
            PropertyChanged += checklaunch;
            Serveur = new Serveur(Preferences.Password);
            Serveur.cmds["GetTasks"] = new GetTasks((object param) => SendTasks(),(object sender)=> true);
            Serveur.cmds["PauseTask"] = new GetTasks((object param) => PauseDist(param),(object sender)=> true);
            Serveur.cmds["StartTask"] = new GetTasks((object param) => LaunchDist(param),(object sender)=> true);
            Serveur.cmds["StopTask"] = new GetTasks((object param) => StopDist(param),(object sender)=> true);
            ServerThread = new Thread(() => Serveur.LaunchServer(Preferences));
            ServerThread.Start();
        }

        private void StopDist(object param)
        {
            List<Backup> bb = ((JArray)param).ToObject<List<Backup>>();
            if (bb == null) return;
            foreach (var b in bb) if (Backups.Contains(b)) Backups[Backups.IndexOf(b)].Stop();
            SendTasks();
        }

        private void LaunchDist(object param)
        {
            List<Backup> bb = ((JArray)param).ToObject<List<Backup>>();
            if (bb == null) return;
            foreach(var b in bb) if(Backups.Contains(b)) Backups[Backups.IndexOf(b)].Start(LogService, Preferences.CryptExt, Preferences.LimiteFichier, Preferences.Prioritaire);
            SendTasks();
        }

        private void PauseDist(object param)
        {
            List<Backup> bb = ((JArray)param).ToObject<List<Backup>>();
            if (bb == null) return;
            foreach (var b in bb) if (Backups.Contains(b)) Backups[Backups.IndexOf(b)].Pause();
            SendTasks();
        }
        private void SendTasks()
        {
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            var currentUiCulture = Thread.CurrentThread.CurrentUICulture;
            Thread th = new Thread(Serveur.envoiData);
            th.CurrentCulture = System.Globalization.CultureInfo.CurrentCulture;
            th.Name = "sendtasks";
            th.CurrentCulture = currentCulture;
            th.CurrentUICulture = currentUiCulture;
            th.Start(new Message { obj = Backups.Select(el=>new {Name = el.Name,Destination=el.Destination,Source=el.Source,Type=el.Type,State=el.State,NbFileRemaining = el.NbFileRemaining,TotalSize=el.TotalSize,NbFile=el.NbFile}).ToList(), Error = false });
        }
        private void processended(object sender, EventArrivedEventArgs e)
        {
            if (e.NewEvent.Properties["ProcessName"].Value.ToString() != Preferences.LogicielMetier) return;
            if (CanLaunch)
            { 
                foreach (var i in Backups)
                {
                    if (i.State == BackupState.En_Attente)
                    {
                        i.Start(LogService, Preferences.CryptExt,Preferences.LimiteFichier,Preferences.Prioritaire);
                    }
                }
            }
        }

        private void processlaunched(object sender, EventArrivedEventArgs e)
        {
            var proname = e.NewEvent.Properties["ProcessName"].Value.ToString();
            if(proname == Preferences.LogicielMetier)
            {
                OnPropertyChanged("CanLaunch");
                foreach (var i  in Backups)
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
            
        }
        private void SaveTasks(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Backups" || e.PropertyName == "Backup") LogService.Log(this.Backups.Select(el => new { name = el.Name, source = el.Source, destination = el.Destination, type = el.Type }), new LogTasks());
            if (e.PropertyName == "IsPrio")
            {
                if (!(sender as Backup).IsPrio)
                {
                    if (Backups.Any(el => el.IsPrio))
                    {
                        (sender as Backup).Pause();
                        return;
                    }
                    foreach (var i in Backups)
                    {
                        if (i.State == BackupState.En_Attente) i.Start(LogService, Preferences.CryptExt,Preferences.LimiteFichier, Preferences.Prioritaire);
                    }
                    return;
                }
                else
                {
                    foreach (var i in Backups)
                    {
                        if (!i.IsPrio && i.State == BackupState.En_Cours) i.Pause();
                    }
                }
            }
        }
        private void Backups_ChangedCollection(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
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
