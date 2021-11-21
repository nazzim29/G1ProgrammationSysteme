using EasySave.Helpers;
using EasySave.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace EasySave.ViewModels
{
    public class Backup : BaseINPC
    {
        private ObservableCollection<Travail> _tasks;
        private ObservableCollection<Thread> running_tasks = new ObservableCollection<Thread>();
        public event PropertyChangedEventHandler PropertyChanged;
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
        public Backup()
        {
            tasks = new ObservableCollection<Travail>();
        }

        public void NewTask(string name, string source, string destination, string mode)
        {
            Travail t = new Travail(name, source, destination, mode);
            if (!tasks.Contains(t)) tasks.Add(t);

            else throw new Exception("this task already exists");
        }
        public void StartTask(string name)
        {
            Travail t = (Travail)_tasks.Single(el => el.name == name);
            Thread task = t.Start();
            running_tasks.Add(task);
            task.Start();

        }


    }
}
