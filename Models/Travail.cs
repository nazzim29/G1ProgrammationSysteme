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
        private string _name,_source, _destination,_type,_state;
        private double _nb_file,_nb_file_remaining,_total_size;
        public Travail(string _n,string _s, string _d, string _m)
        {
            this.name = _n;
            this.source = _s;
            this.destination = _d;
            this.type = _m;
            this.files = new ObservableCollection<string>();
            this.state = "Inactif";
        }
        public string name
        {
            get { 
                return _name; 
            }
            set
            {
                _name= value;
                RaisePropertyChanged("name");
            }
        }
        public string source
        {
            get
            {
                return _source;
            }
            set
            {
                _source= value;
                RaisePropertyChanged("source");
            }
        }
        public string destination
        {
            get
            {
                return _destination;
            }
            set
            {
                _destination= value;
                RaisePropertyChanged("destination");
            }
        }
        public string progression
        {
            get
            {
                return (nb_file - nb_file_remaining) / nb_file * 100 >-1 ? String.Format("{0:0.##%}", ((nb_file-nb_file_remaining)/nb_file)) : "0%";
            }
        }
        public string type
        {
            get { return _type; }
            set
            {
                if(value == "Complet" || value == "Diferentiel")
                {
                    _type = value;
                    RaisePropertyChanged("type");
                }
            }
        }
        public string state
        {
            get
            {
                return _state;
            }
            set
            {
                _state= value;
                RaisePropertyChanged("state");
            }
        }
        public double nb_file
        {
            get
            {
                return files.Count() ;
            }

        }
        public double nb_file_remaining
        {
            get
            {
                return _nb_file_remaining;
            }
            set
            {
                _nb_file_remaining= value;
                RaisePropertyChanged("nb_file_remaining");
            }
        }
        public long total_size
        {
            get { return DirSize(new DirectoryInfo(source)); }
        }
        ObservableCollection<string> files { get; set; }
        public override bool Equals(object obj) => obj is Travail && ((Travail)obj).destination.Equals(destination) && ((Travail)obj).source.Equals(source);
        public override int GetHashCode()=> (source.GetHashCode() + destination.GetHashCode()).GetHashCode();
        public Thread Start()
        {
            var dir = new DirectoryInfo(this.source);
            files = new ObservableCollection<string>(dir.GetFiles("*", SearchOption.AllDirectories).Select(el=>el.FullName));
            nb_file_remaining = nb_file;
            return new Thread(delegate ()
            {
                this.state = "Running";
                foreach(var file in files)
                {
                    File.Copy(file, file.Replace(this.source, this.destination) , true);
                    nb_file_remaining--;
                }
                this.state = "Finished";
            });


        }
        private long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }
    }
}
