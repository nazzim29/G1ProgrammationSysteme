using EasySave_GUI.Libs;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;


namespace EasySave_GUI.Models
{
    public enum BackupType
    {
        Complete,
        Differentielle
    }
    public enum BackupState
    {
        Inactif,
        En_Cours,
        Finie
    }
    public class Backup : BaseModel
    {
        private string _name, _source, _destination;
        private BackupType _type;
        private BackupState _state;
        private double _nb_file_remaining, _total_size;
        private ObservableCollection<FileInfo> _files;
        
        public event PropertyChangedEventHandler PropertyChanged;   
        public ObservableCollection<FileInfo> Files
        {
            get 
            {
                if (_source == null) return new ObservableCollection<FileInfo>();
                if (_files == null) _files = new ObservableCollection<FileInfo>(new DirectoryInfo(_source).GetFiles("*", SearchOption.AllDirectories));
                return _files; 
            }
            set 
            {
                if (_files == value) return;
                _files = value;
                OnPropertyChanged("Files");
                OnPropertyChanged("NbFile");
                OnPropertyChanged("TotalSize");
            }
        }
        public string Name
        {
            get { return _name; }
            set 
            { 
                if(value == _name) return;
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        public string Source
        {
            get { return _source; }
            set 
            {
                if (_source == value) return;
                _source = value;
                OnPropertyChanged("Source");
                OnPropertyChanged("Files");
            }
        }
        public string Destination
        {
            get { return _destination; }
            set 
            { 
                if(_destination == value) return;
                _destination = value;
                OnPropertyChanged("Destination");
            }
        }
        public BackupType Type
        {
            get { return _type; }
            set 
            {
                if (Type == value) return;
                _type = value;
                OnPropertyChanged("Type");
            }
        }
        public BackupState State
        {
            get { return _state; }
            set 
            { 
                if(State == value) return;
                _state = value;
                OnPropertyChanged("State");
            }
        }
        public double NbFileRemaining
        {
            get { return _nb_file_remaining;}
            set 
            { 
                if(_nb_file_remaining == value) return;
                _nb_file_remaining = value;
                OnPropertyChanged("NbFileRemaining");
            }
        }
        public double TotalSize
        {
            get { return _total_size;}
            set 
            { 
                if(_total_size == value) return;
                _total_size = value;
                OnPropertyChanged("TotalSize");
            }
        }
        public string Progression
        {
            get
            {
                if (NbFileRemaining == 0) return "100%";
                double finit = NbFile - NbFileRemaining;
                if (finit == 0) return "0%";
                double p = finit * 100 / NbFile;
                return p + "%";
            }
        }
        public double NbFile
        {
            get
            {
                return Files.Count();
            }
        }
        private void Files_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            OnPropertyChanged("Files");
        }
        public Backup()
        {
            Files.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Files_CollectionChanged);
        }
        public override bool Equals(object obj) => obj is Backup && ((Backup)obj).Destination.Equals(Destination) && ((Backup)obj).Source.Equals(Source);
        public override int GetHashCode() => (Source.GetHashCode() + Destination.GetHashCode()).GetHashCode();

        private static void CreateDirs(string path, DirectoryInfo[] dirs)
        {
            foreach (var dir in dirs)
            {

                if (!Directory.Exists(Path.Combine(path, dir.Name)))
                {
                    Directory.CreateDirectory(Path.Combine(path, dir.Name));
                }
                CreateDirs(Path.Combine(path, dir.Name), dir.GetDirectories());
            }
        }
        private static long DirSize(DirectoryInfo d)
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
        public static Backup[] fromFile()
        {
            string Path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EasySave\\Log";
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
            if (!File.Exists(Path + $"\\tasks.json"))
            {
                File.Create(Path + $"\\tasks.json").Dispose();
                return new Backup[0];
            }

            return JsonConvert.DeserializeObject<Backup[]>(File.ReadAllText(Path + $"\\tasks.json")) ?? new Backup[0]; ;
        }
    }
}
