﻿using EasySave.Helpers;
using EasySave.Models;
using EasySave.Properties;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace EasySave.ViewModels
{
    public class Backup : BaseINPC
    {
        public Preferences preferences;
        private LogService LogService = new LogService();
        private ObservableCollection<Travail> _tasks;
        private ObservableCollection<Thread> running_tasks = new ObservableCollection<Thread>();
        public event PropertyChangedEventHandler PropertyChanged;
        //accesseurs de la classe backup
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
        //constructeur
        public Backup()
        {
            tasks = new ObservableCollection<Travail>();
            //tasks.CollectionChanged += taskschanged;
        }
        private void taskschanged()
        {
            LogService.Log(this.tasks.Select(el=>new { name = el.name, source = el.source, destination = el.destination, type = el.type }), new LogService.LogTasks());
        }
        //méthode permettant d'ajouter une nouvelle tache de sauvegarde à la liste des taches
        public void NewTask(string name, string source, string destination, string mode)
        {
            Travail t = new Travail(name, source, destination, mode);
            if (!tasks.Contains(t))
            {
                tasks.Add(t);
                taskschanged();
            }
            else throw new Exception("this task already exists");
        }
        //méthode permettant de lancer le travail de sauvegarde
        public void StartTask(string name)
        {
           Travail t = (Travail)_tasks.Single(el => el.name == name);
            Thread task = t.Start(LogService);
            running_tasks.Add(task);
            task.Start();

        }

        public void ParsePreferences()
        {
            preferences = Preferences.fromFile();
        }
        public void ParseTasks()
        {
            this.tasks = new ObservableCollection<Travail>(Travail.fromFile());
        }
    }
}
