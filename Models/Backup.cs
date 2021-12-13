using EasySave_GUI.Libs;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

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
        public double size_eligible
        {
            get
            {
                if (Files == null) return 0;
                double f = 0;
                foreach (var file in Files)
                {
                    f += file.Length;
                }
                return f;
            }
        }
        private void Copyfile(FileInfo source, string destination, bool dif)
        {
            //if the file does not exist an exception is returned
            if (!source.Exists) throw new Exception("Source file not found");//si le fichier n'existe pas on renvoie une exception
            //Copy an existing file to a new file. The overwriting of a file with the same name is allowed.
            File.Copy(source.FullName, destination, true);//Copie un fichier existant dans un nouveau fichier. L'écrasement d'un fichier du même nom est autorisé.
            this.NbFileRemaining--;//every time we make a copy the number of files decreases
        }
        public Thread Start(object log)
        {
            var dir = new DirectoryInfo(this.Source);
            Files = new ObservableCollection<FileInfo>((new DirectoryInfo(Source)).GetFiles("*", SearchOption.AllDirectories).Where(el => isEligible(el.FullName))) ?? new ObservableCollection<FileInfo>();//récupérer les fichiers du répertoire
            NbFileRemaining = Files.Count();
            return new Thread(delegate ()
            {
                this.State = BackupState.En_Cours;
                CreateDirs(this.Destination, dir.GetDirectories());//recreates the structure of the directory
                Stopwatch timer = new Stopwatch();
                foreach (var file in Files)
                {
                    try
                    {
                        timer.Start();
                        Copyfile(file, file.FullName.Replace(this.Source, this.Destination), this.Type == BackupType.Differentielle ? true : false);//function to copy files
                        timer.Stop();
                        //log.Log(new { name = this.Name, SourceFile = file.FullName, TargetFile = file.FullName.Replace(this.Source, this.Destination), FileSize = file.Length, FileTransfertTime = timer.ElapsedMilliseconds, Time = DateTime.Now.ToString("G") }, new LogService.LogJournalier());
                    }
                    catch (Exception ex)
                    {
                        //log.Log(new { name = this.Name, SourceFile = file.FullName, TargetFile = file.FullName.Replace(this.Source, this.Destination), FileSize = file.Length, FileTransfertTime = -1*timer.ElapsedMilliseconds, Time = DateTime.Now.ToString("G") }, new LogService.LogJournalier());
                    }

                }
                this.State = BackupState.Finie;
            });


        }
        private string GetCryptedExtension()
        {
            if (!File.Exists(this.Destination + "\\crypt.json"))
            {
                File.Create(this.Destination + "\\crypt.json");
            };
            
            return "";
        }
        private bool isEligible(string el)
        {
            if (this.Type == BackupType.Complete)
            {
                return true;
            }
            string cryptedExtensions = GetCryptedExtension();
            string pathdest = el.Replace(this.Source, this.Destination);
            if (File.Exists(pathdest))
            {
                using (var sourcef = File.OpenRead(el))
                {
                    //on ouvre le fichier destination
                    //opens the destination file
                    using (var destinationf = File.OpenRead(pathdest))
                    {
                        var hash1 = BitConverter.ToString(MD5.Create().ComputeHash(sourcef));//we hash the content of the source file//on hash le contenu du fichier source
                        var hash2 = BitConverter.ToString(MD5.Create().ComputeHash(destinationf));//we hash the content of the destination file;//on hash le contenu du fichier destination

                        //if the hash is the same we move to the next iteration without making a backup
                        //si le hash est le meme on saute a l'itération suivante sans faire de sauvegarde
                        if (hash1 == hash2)
                        {
                            return false; ;
                        };
                    }
                }
            }
            return true;
        }
        private void onchange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "source" || e.PropertyName == "destination" || e.PropertyName == "type")
            {
                //get all the files of a directory
                Files = new ObservableCollection<FileInfo>((new DirectoryInfo(Source)).GetFiles("*", SearchOption.AllDirectories).Where(el => isEligible(el.FullName))) ?? new ObservableCollection<FileInfo>();//récupérer les fichiers du répertoire
                NbFileRemaining = NbFile;
                this.State = BackupState.Inactif;
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
