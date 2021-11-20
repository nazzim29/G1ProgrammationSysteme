using EasySave.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Models
{
    public class Travail : BaseINPC
    {
        public string name
        {
            get { 
                return name; 
            }
            set
            {
                name= value;
                RaisePropertyChanged("name");
            }
        }
        public string source
        {
            get
            {
                return source;
            }
            set
            {
                source= value;
                RaisePropertyChanged("source");
            }
        }
        public string destination
        {
            get
            {
                return destination;
            }
            set
            {
                destination= value;
                RaisePropertyChanged("destination");
            }
        }
        public string type
        {
            get { return type; }
            set
            {
                if(value == "Complet" || value == "Diferentiel")
                {
                    type = value;
                    RaisePropertyChanged("type");
                }
            }
        }
        public string state
        {
            get
            {
                return state;
            }
            set
            {
                state= value;
                RaisePropertyChanged("state");
            }
        }
        public double nb_file
        {
            get
            {
                return nb_file;
            }
            set
            {
                nb_file= value;
                RaisePropertyChanged("nb_file");
            }
        }
        public double nb_file_remaining
        {
            get
            {
                return nb_file_remaining;
            }
            set
            {
                nb_file_remaining= value;
                RaisePropertyChanged("nb_file_remaining");
            }
        }
        public double total_size
        {
            get { return total_size; }
            set { total_size = value; RaisePropertyChanged("total_size"); }
        }
        ObservableCollection<string> files { get; set; }
        public override bool Equals(object obj) => obj is Travail && ((Travail)obj).destination.Equals(destination) && ((Travail)obj).source.Equals(source);
        public override int GetHashCode()=> (source.GetHashCode() + destination.GetHashCode()).GetHashCode();
    }
}
