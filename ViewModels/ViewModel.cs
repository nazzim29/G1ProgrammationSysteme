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
        private Backup _runningTask;
        private ObservableCollection<Backup> _q;
        private bool _CanLaunch;
        private ObservableCollection<Backup> Q
        {
            get { return _q; }
            set { _q = value; OnPropertyChanged("Q"); }
        }
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
        private Backup RunningTask
        {
            get
            {
                if(_runningTask == null) _runningTask = new Backup();
                return _runningTask;
            }
            set { _runningTask = value; OnPropertyChanged("RunningTask"); }
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
        private ICommand _ChangeModeToSimultaneCommand;
        private ICommand _ChangeModeToSequentielCommand;
        private ICommand _changeLanguageCommand;

        public ICommand ChangeModeToSimultaneCommand
        {
            get
            {
                if (_ChangeModeToSimultaneCommand == null) _ChangeModeToSimultaneCommand = new RelayCommand(() => ChangeCopyMode(CopyMode.simultane),(object p) => Preferences.Mode != CopyMode.simultane && Q.Count() == 0);
                return _ChangeModeToSimultaneCommand;
            }
        }
        public ICommand ChangeModeToSequentielCommand
        {
            get
            {
                if (_ChangeModeToSequentielCommand == null) _ChangeModeToSequentielCommand = new RelayCommand(() => ChangeCopyMode(CopyMode.sequentiel),(object p) => Preferences.Mode != CopyMode.sequentiel && Q.Count() == 0);
                return _ChangeModeToSequentielCommand;
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
                    _addTaskCommand = new RelayCommand(() => AddTask(), (object param) => true) ;
                }
                return _addTaskCommand;
            }
        }
        public ICommand DeleteTaskCommand
        {
            get
            {
                if (_deleteTaskCommand == null) _deleteTaskCommand = new RelayCommand(() => DeleteTask(), (object sender) => true);
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

        public void ChangeCopyMode(CopyMode m)
        {
            if (Preferences.Mode == m) return;
            Preferences.Mode = m;
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
        private void Launch(bool next=false)
        {
            if (!CanLaunch) return;
            if (Preferences.Mode == CopyMode.sequentiel)
            {
                if (next)
                {
                    if(Q.Count() != 0) Q[0].Start(LogService,Preferences.CryptExt);
                    return;
                }
                if(!Q.Contains(Backup))
                {
                    Backup.PropertyChanged += LaunchNext;
                    Q.Add(Backup);
                    Backup.State = BackupState.En_Attente;
                    if (Q.Count() == 1)
                    {
                        Q[0].Start(LogService,Preferences.CryptExt);
                        return;
                    }
                }
            }
            else
            {
                if (next) return;
                if (!Q.Contains(Backup))
                {
                    Backup.PropertyChanged += LaunchNext;
                    Q.Add(Backup);
                    Backup.Start(LogService,Preferences.CryptExt);
                }
            }
        }
        public bool canChangeparam
        {
            get
            {
                return Q.Count() == 0;
            }
        }
        private void LaunchNext(object? sender, PropertyChangedEventArgs e)
        {
            if ((sender as Backup).State == BackupState.Finie)
            {
                Q.Remove((sender as Backup));
                Launch(true);
            }
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
            Q = new ObservableCollection<Backup>();
            Q.CollectionChanged += new NotifyCollectionChangedEventHandler(Changed_q);
            Backups.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Backups_ChangedCollection);
            PropertyChanged += SaveTasks;
            PropertyChanged += checklaunch;

        }

        private void processended(object sender, EventArrivedEventArgs e)
        {
            var proname = e.NewEvent.Properties["ProcessName"].Value.ToString();
            if(proname == Preferences.LogicielMetier)
            {
                CanLaunch = true;
                if (Preferences.Mode == CopyMode.sequentiel)
                {
                    if (Q[0].State == BackupState.En_Attente)
                    {
                        Launch(true);
                    }
                }
                else
                {
                    foreach (var i in Q)
                    {
                        if (i.State == BackupState.En_Attente)
                        {
                            i.State = BackupState.En_Cours;
                            i.Thread.Resume();
                        }
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
                if(Preferences.Mode != CopyMode.sequentiel)
                {
                    foreach(var i  in Q)
                    {
                        i.State = BackupState.En_Attente;
                        i.Thread.Suspend();
                    }
                }
            }
        }

        private void Changed_q(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Q");
            OnPropertyChanged("canChangeparam");
        }

        private void checklaunch(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Backup")
            {
                if(Q.Contains(Backup)) CanLaunch = false;
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
            //foreach (var (task, i) in tasks.Select((el, i) => (el, i)))
            //{
            //    task.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            //    {
            //        int fr = (int)sender.GetType().GetField("nb_file_remaining").GetValue(sender);
            //        string state = (string)sender.GetType().GetProperty("state").GetValue(sender);
            //        if (preferences.ModeCopy == ModeCopy.sequentiel)
            //        {
            //            if (state == "Finished" && fr == 0 && i < tasks.Count())
            //            {
            //                running_tasks.Clear();
            //                var t = tasks[i + 1].Start(LogService);
            //                t.Name = tasks[i + 1].name;
            //                running_tasks.Add(t);
            //                t.Start();
            //            }
            //        }
            //    };
            OnPropertyChanged("Backups");
        }

   
    }
}
