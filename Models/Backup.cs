using EasySave_GUI.Libs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
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
        En_Attente,
        Finie,
        Erreur
    }
    public class Backup : BaseModel
    {
        public EventWaitHandle mre = new AutoResetEvent(false);
        private static Object _locker = new Object();
        public Thread Thread;
        private string _name, _source, _destination;
        private BackupType _type;
        private BackupState _state;
        private double _nb_file_remaining, _total_size;
        private ObservableCollection<FileInfo> _files;
        private Regex Prio;
        private int _currentidx;
        
        public int _currentindex
        {
            get { if (_currentidx == null) _currentidx = 0;return _currentidx; }
            set { if(Files != null && Files.Count >= value) _currentidx = value; }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsPrio
        {
            get
            {
                if (Prio == null) return false;
                return Prio.IsMatch(Files[_currentindex].Name) && State == BackupState.En_Cours;
            }
        }
        private bool isEligible(string el)
        {
            if (this.Type == BackupType.Complete)
            {
                return true;
            }
            string pathdest = el.Replace(this.Source, this.Destination);
            if (File.Exists(pathdest))
            {
                var cryptedFiles = new List<string>();
                if(File.Exists(Destination + "\\cryptedfiles.json")) cryptedFiles = new List<string>(JsonConvert.DeserializeObject<string[]>(File.ReadAllText(Destination + "\\cryptedfiles.json")));
                if (cryptedFiles.Contains(el))
                {
                    return true;
                }
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
        public ObservableCollection<FileInfo> Files
        {
            get 
            {
                if (_source == null) _files = new ObservableCollection<FileInfo>();
                return _files; 
            }
            set 
            {
                if (_files == value) return;
                _files = value;
                NbFileRemaining = _files.Count();
                OnPropertyChanged("Files");
                OnPropertyChanged("NbFile");
                OnPropertyChanged("TotalSize");
                OnPropertyChanged("Progression");
                OnPropertyChanged("IsPrio");
            }
        }
        public string Name
        {
            get { return _name??""; }
            set 
            { 
                if(value == _name) return;
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        public string Source
        {
            get { return _source??""; }
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
            get { return _destination??""; }
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
        private void onchange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "source" || e.PropertyName == "destination" || e.PropertyName == "type")
            {
                //get all the files of a directory
                Files = new ObservableCollection<FileInfo>((new DirectoryInfo(Source)).GetFiles("*", SearchOption.AllDirectories).Where(el => isEligible(el.FullName)))?? new ObservableCollection<FileInfo>();//récupérer les fichiers du répertoire
                Files = OrderFiles(Files);
                NbFileRemaining = NbFile;
                this.State = BackupState.Inactif;
            }
        }
        public BackupState State
        {
            get { return _state; }
            set 
            { 
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
                OnPropertyChanged("Progression");
                OnPropertyChanged("IsPrioritaire");
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
        public int Progression
        {
            get
            {
                if (State == BackupState.Inactif || NbFile == 0) return 0;
                return (int)Math.Round((_currentindex / NbFile) * 100);
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
            OnPropertyChanged("Progression");
            OnPropertyChanged("NbFile");
            OnPropertyChanged("NbFileRemaining");
        }
        public Backup()
        {
            State = BackupState.Finie;
            Files.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Files_CollectionChanged);
            PropertyChanged += nbfilechanged;
        }
        private void nbfilechanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Source") Files = OrderFiles(new ObservableCollection<FileInfo>(new DirectoryInfo(_source).GetFiles("*", SearchOption.AllDirectories).Where(el => isEligible(el.FullName))));
            if (e.PropertyName == "NbFile")
            {
                NbFileRemaining = NbFile;
                _currentindex = 0;
            }
        }
        public void Pause()
        {
            State = BackupState.En_Attente;
            mre.Reset();
        }
        public void Stop()
        {
            State = BackupState.Inactif;
            mre.Reset();
        }
        public override bool Equals(object obj) => obj != null && obj is Backup && ((Backup)obj).Destination.Equals(Destination) && ((Backup)obj).Source.Equals(Source) &&((Backup)obj).Name == Name;
        //public override int GetHashCode() => (Source.GetHashCode() + Destination.GetHashCode()).GetHashCode();
        public void Start(LogService log,string cryptExt,long filemax,string prio)
        {
            Prio = new Regex(prio);
            try
            {
                if(State == BackupState.En_Attente)
                {
                    State = BackupState.En_Cours;
                    mre.Set();
                    return;
                }
                var dir = new DirectoryInfo(Source);
                Files = new ObservableCollection<FileInfo>((new DirectoryInfo(Source)).GetFiles("*", SearchOption.AllDirectories).Where(el => isEligible(el.FullName)));
                if (Files != null && Files.Count != 0) Files = OrderFiles(Files);
                Thread = new Thread(() => ThreadProc(dir, cryptExt,filemax, log));
                NbFileRemaining = Files.Count;
                _currentindex = 0;
                Thread.Start();
            }catch(Exception e)
            {
                Debug.WriteLine(e);
                this.State = BackupState.Erreur;
            }
        }
        private ObservableCollection<FileInfo> OrderFiles(ObservableCollection<FileInfo> l)
        {
            List<FileInfo> lp=  l.Where(el=>Prio.IsMatch(el.Name)).ToList();
            foreach (var t in lp) l.Remove(t);
            lp.AddRange(l);
            return new ObservableCollection<FileInfo>(lp);
        }
        private void ThreadProc(DirectoryInfo dir,string cryptExt,long filemax,LogService log)
        {
                
            this.State = BackupState.En_Cours;
            CreateDirs(this.Destination, dir.GetDirectories());//recreates the structure of the directory
            Stopwatch timer = new Stopwatch();
            foreach (var file in Files)
            {
                if (State == BackupState.Erreur || State == BackupState.Finie) return;
                if (State == BackupState.En_Cours) mre.Set();
                mre.WaitOne();
                if (file.Length < filemax)
                {
                    locked(cryptExt, log, file, timer);
                }
                else
                {
                    lock (_locker)
                    {
                        locked(cryptExt,log, file, timer);
                    }
                }
                if(_currentindex < Files.Count)_currentindex++;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("IsPrio"));
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Progression"));
            }
            this.State = BackupState.Finie;
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs("State"));
        }
        private double CryptFile(FileInfo file)
        {
            var cryptedFiles = new List<string>();
            if (File.Exists(Destination + "\\cryptedfiles.json")) cryptedFiles = new List<string>(JsonConvert.DeserializeObject<string[]>(File.ReadAllText(Destination + "\\cryptedfiles.json")));
            var p = new Process();
            p.StartInfo.FileName = "CryptoSoft.exe";
            p.StartInfo.Arguments = $"{file.FullName} {file.FullName.Replace(Source, Destination)}";
            p.Start();
            p.WaitForExit();
            if(p.ExitCode == 0)
            {
                if(!cryptedFiles.Contains(file.FullName.Replace(this.Source, this.Destination))) cryptedFiles.Add(file.FullName.Replace(this.Source, this.Destination));
                File.WriteAllText($"{Destination}\\cryptedfiles.json", JsonConvert.SerializeObject(cryptedFiles, Formatting.Indented));
                NbFileRemaining--;
                return p.TotalProcessorTime.TotalMilliseconds;

            }
                return (double)p.ExitCode;

        }
        private void locked(string cryptExt,LogService log,FileInfo file,Stopwatch timer)
        {
            
            try
            {
                if (new Regex(cryptExt).IsMatch(file.Extension))
                {
                    timer.Start();
                    var gg = CryptFile(file);
                    timer.Stop();

                    log.Log(new { name = this.Name, SourceFile = file.FullName, TargetFile = file.FullName.Replace(this.Source, this.Destination), FileSize = file.Length, FileTransfertTime = timer.ElapsedMilliseconds, CryptageTime = gg, Time = DateTime.Now.ToString("G") }, new LogJournalier());

                }
                else
                {
                    timer.Start();
                    Copyfile(file, file.FullName.Replace(this.Source, this.Destination));//function to copy files
                    timer.Stop();
                    log.Log(new { name = this.Name, SourceFile = file.FullName, TargetFile = file.FullName.Replace(this.Source, this.Destination), FileSize = file.Length, FileTransfertTime = timer.ElapsedMilliseconds, CryptageTime = 0, Time = DateTime.Now.ToString("G") }, new LogJournalier());
                }

            }
            catch (Win32Exception ex)
            {
                Debug.WriteLine(ex);
                log.Log(new { name = this.Name, SourceFile = file.FullName, TargetFile = file.FullName.Replace(this.Source, this.Destination), FileSize = file.Length, FileTransfertTime = timer.ElapsedMilliseconds, Time = DateTime.Now.ToString("G") }, new LogJournalier());
            }
        }
        private void Copyfile(FileInfo source, string destination)
        {
            //if the file does not exist an exception is returned
            if (!source.Exists) throw new Exception("Source file not found");//si le fichier n'existe pas on renvoie une exception
            //Copy an existing file to a new file. The overwriting of a file with the same name is allowed.
            source.CopyTo(destination, true);//Copie un fichier existant dans un nouveau fichier. L'écrasement d'un fichier du même nom est autorisé.
            this.NbFileRemaining--;//every time we make a copy the number of files decreases
        }
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
        public static List<Backup> fromFile()
        {
            string Path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EasySave\\Log";
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
            if (!File.Exists(Path + $"\\tasks.json"))
            {
                File.Create(Path + $"\\tasks.json").Dispose();
                return new List<Backup>();
            }
            List<Backup> tasks = new List<Backup>(JsonConvert.DeserializeObject<Backup[]>(File.ReadAllText(Path + $"\\tasks.json")));
            return tasks;
        }
    }
}
