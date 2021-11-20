using EasySave.Helpers;
using EasySave.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.ViewModels
{
    public class Backup : BaseINPC
    {
        private ObservableCollection<Travail> _tasks;
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Travail> tasks
        {
            get { return _tasks; }
            set { _tasks = value;}
        }
        public Travail current_task
        {
            get { return current_task; }
            set { current_task = value;}
        }
        public Backup()
        {
            _tasks = new ObservableCollection<Travail>();
        }



    }
}
