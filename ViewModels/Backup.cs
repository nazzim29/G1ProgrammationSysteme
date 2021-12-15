using EasySave.Helpers;
using EasySave.Models;
using EasySave.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace EasySave.ViewModels
{
    public class Backup : BaseINPC
    {
        //class backup attributes
        public Preferences preferences;
        private LogService LogService;
        private ObservableCollection<Travail> _tasks;
        private ObservableCollection<Thread> running_tasks = new ObservableCollection<Thread>();
        public event PropertyChangedEventHandler PropertyChanged;
        
        //accesseurs de la classe backup
        //getter and setter of the class backup
        public ObservableCollection<Travail> tasks
        {
            get { return _tasks; }
            set { _tasks = value; }
        }
        public Travail current_task
        {
            get { return current_task; }
            set { current_task = value; }
        }
        //constructeur/constructor
         public Backup()
        {
            //Dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed.
            //tasks is an instance of Travail
            tasks = new ObservableCollection<Travail>();
            if(preferences.logextension == logextension.json) LogService = new LogService(new JSONStrategy);
            //tasks.CollectionChanged += taskschanged;
        }
        //log state file method, it adds an object that contains task state properties to the state file
        private void stateLogger(object sender, PropertyChangedEventArgs e)
        {
            List<object> l = new List<object>();
            foreach (var task in tasks)
            {
                object obj = new
                {
                    Name = task.name,
                    SourceFilePath = task.source,
                    TargetFilePath = task.destination,
                    State = task.state,
                    TotalFileToCopy = task.state == "Running" ? task.nb_file : 0,
                    TotalFileSize = task.state == "Running" ? task.size_eligible : 0,
                    NbFilesLeftToDo = task.state == "Running" ? task.nb_file_remaining : 0,
                    Progression = task.progression
                };
                l.Add(obj);
            }
            new LogService().Log(l, new LogService.LogState());

        }
        //method to delete a task
        public void DeleteTask(List<Travail> tasks)
        {
            foreach(var task in tasks)
            {
                this.tasks.Remove(task);
            }
            taskschanged();
        }
        //update the content of the log file
        private void taskschanged()
        {
            LogService.Log(this.tasks.Select(el=>new { name = el.name, source = el.source, destination = el.destination, type = el.type }), new LogService.LogTasks());
            foreach(var (task,i) in tasks.Select((el,i)=>(el,i)))
            {
                task.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
                {
                    int fr  = (int)sender.GetType().GetField("nb_file_remaining").GetValue(sender);
                    string state = (string)sender.GetType().GetProperty("state").GetValue(sender);
                    if (preferences.ModeCopy == ModeCopy.sequentiel)
                    {
                        if(state == "Finished" && fr == 0 && i < tasks.Count())
                        {
                            running_tasks.Clear();
                            var t = tasks[i + 1].Start(LogService);
                            t.Name = tasks[i + 1].name;
                            running_tasks.Add(t);
                            t.Start();
                        }
                    }
                };
            }
        }
        //méthode permettant d'ajouter une nouvelle tache de sauvegarde à la liste des taches
        //method for adding a new backup task to the task list
        public void NewTask(string name, string source, string destination, string mode)
        {
            Travail t = new Travail(name, source, destination, mode);
            t.PropertyChanged += stateLogger;//add event handler to the PropertyChanged event handler 
            if (!tasks.Contains(t))
            {
                tasks.Add(t);
                taskschanged();
            }
            else throw new Exception("this task already exists");
        }
        //méthode permettant de lancer le travail de sauvegarde
        public void StartTask(string name = null)
        {
            if(name != null)
            {
                Travail t = (Travail)_tasks.Single(el => el.name == name);
                Thread task = t.Start(LogService);
                task.Name = name;
                running_tasks.Add(task);
                task.Start();
                return;

            }
            if(preferences.ModeCopy == ModeCopy.sequentiel)
            {
                var t  = tasks[0].Start(LogService);
                t.Name = tasks[0].name;
                running_tasks.Clear();
                running_tasks.Add(t);
                t.Start();
                return;
            }


        }
    public void ParsePreferences()
        {
            preferences = Preferences.fromFile();

        }
        //method to change the language of the app depending on the current language in the config file
        public void ChangeLanguage()
        {
            if (this.preferences.language == "EN") { preferences.language = "FR"; return; }
            if (this.preferences.language == "FR") { preferences.language = "EN"; return; }
            
        }
        public void ChangeCopyMode(ModeCopy mode)
        {
            preferences.ModeCopy = mode;
        }
        //xml file
        public void ChangeLogExtension(logextension exstension)
        {
            preferences.logextension = exstension;
        }
        //method to read the list of the task in the task file
        public void ParseTasks()
        {
            this.tasks = new ObservableCollection<Travail>(Travail.fromFile());
            foreach(var (task,i) in tasks.Select((el,i)=>(el,i)))
            {
                task.PropertyChanged += stateLogger;
                task.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
                {
                    double fr = (double)sender.GetType().GetProperty("nb_file_remaining").GetValue(sender);
                    string state = (string)sender.GetType().GetProperty("state").GetValue(sender);
                    if (preferences.ModeCopy == ModeCopy.sequentiel)
                    {
                        if (state == "Finished" && fr == 0 && i+1 < tasks.Count())
                        {
                            running_tasks.Clear();
                            var t = tasks[i + 1].Start(LogService);
                            t.Name = tasks[i + 1].name;
                            running_tasks.Add(t);
                            t.Start();
                        }
                    }
                };
            }
        }
    }
}
