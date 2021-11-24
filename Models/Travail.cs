using EasySave.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace EasySave.Models
{
    public class Travail : BaseINPC
    {
        //déclaration des propriétés d'un travail
        //backup properties
        private string _name, _source, _destination, _type, _state;
        private double _nb_file, _nb_file_remaining, _total_size;
        //constructeur
        public Travail()
        {
            PropertyChanged += onchange;
        }
        public Travail(string _n, string _s, string _d, string _m)
        {
            PropertyChanged += onchange;
            this.name = _n;
            this.source = _s;
            this.destination = _d;
            this.type = _m;
        }
        //notifies 
        private void onchange(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "source" || e.PropertyName == "destination"|| e.PropertyName == "type")
            {
                //get all the files of a directory
                files = new ObservableCollection<string>((new DirectoryInfo(source)).GetFiles("*", SearchOption.AllDirectories).Select(el => el.FullName).Where(el=>isEligible(el)))?? new ObservableCollection<string>();//récupérer les fichiers du répertoire
                nb_file_remaining = nb_file;
                this.state = "Inactif";


            }
        }
        //verifies if the file has to be copied or not
        private bool isEligible(string el)
        {
            if (this.type == "Complet")
            {
                return true;
            }
            string pathdest = el.Replace(this.source, this.destination);
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
        //***properties(getter and setter)***//
        public double size_eligible
        {
            get
            {
                double f = 0;
                foreach(var file in files)
                {
                    f+= new FileInfo(file).Length;
                }
                return f;
            }
        }
        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
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
                _source = value;
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
                _destination = value;
                RaisePropertyChanged("destination");
            }
        }
        public string progression
        {
            get
            {
                if (nb_file_remaining == 0) return "100%";
                double finit = nb_file - nb_file_remaining;
                if (finit == 0) return "0%";
                double p = finit * 100 / nb_file;
                return p +"%";
            }
        }
        public string type
        {
            get { return _type; }
            set
            {
                if (value == "Complet" || value == "Differentiel")
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
                _state = value;
                RaisePropertyChanged("state");
            }
        }
        public double nb_file
        {
            get
            {
                return files.Count();
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
                _nb_file_remaining = value;
                
                RaisePropertyChanged("nb_file_remaining");
            }
        }
        public long total_size
        {
            get { return DirSize(new DirectoryInfo(source)); }
        }
        ObservableCollection<string> files { get; set; }
        //***properties(getter and setter)***//

        //redefinition of the Equals method, which makes it possible to verify equality between two bodies of the working class
        //redefinition de la méthode Equals qui permet de vérifier l'égalité entre deux instances de la classe travail 
        public override bool Equals(object obj) => obj is Travail && ((Travail)obj).destination.Equals(destination) && ((Travail)obj).source.Equals(source);
        //redefinition of the GetHashCode method to hash a class instance
        //redefinition de la fonction GetHashCode pour hacher une instance de la classe 
        public override int GetHashCode() => (source.GetHashCode() + destination.GetHashCode()).GetHashCode();
        //function that returns a thread that executes the backup
        //fonction qui retourne un thread qui effectue la sauvegarde
        public Thread Start(LogService log)
        {
            var dir = new DirectoryInfo(this.source);
            files = new ObservableCollection<string>((new DirectoryInfo(source)).GetFiles("*", SearchOption.AllDirectories).Select(el => el.FullName).Where(el => isEligible(el))) ?? new ObservableCollection<string>();//récupérer les fichiers du répertoire
            nb_file_remaining = files.Count();
            return new Thread(delegate ()
            {
                this.state = "Running";
                CreateDirs(this.destination, dir.GetDirectories());//recreates the structure of the directory
                Stopwatch timer = new Stopwatch();
                foreach (var file in files)
                {
                    try
                    {
                        timer.Start();
                        Copyfile(file, file.Replace(this.source, this.destination), this.type == "Diferentiel" ? true : false);//function to copy files
                        timer.Stop();
                        log.Log(new { name = this.name, SourceFile = file, TargetFile = file.Replace(this.source, this.destination), FileSize = (new FileInfo(file)).Length, FileTransfertTime = timer.ElapsedMilliseconds,Time = DateTime.Now.ToString("G") },new LogService.LogJournalier());
                    }catch(Exception ex)
                    {
                        log.Log(new { name = this.name, SourceFile = file, TargetFile = file.Replace(this.source, this.destination), FileSize = (new FileInfo(file)).Length, FileTransfertTime = timer.ElapsedMilliseconds,Time = DateTime.Now.ToString("G") },new LogService.LogJournalier());
                    }
                    
                }
                this.state = "Finished";
            });


        }
        //method to recreate the structure of the source directory
        //méthode permettant de recréer la structure de la source
        private void CreateDirs(string path, DirectoryInfo[] dirs)
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
        //method for copying files
        //methode permettant de faire la copie des fichiers
        private void Copyfile(string source, string destination, bool dif)
        {
            //if the file does not exist an exception is returned
            if (!File.Exists(source)) throw new Exception("Source file not found");//si le fichier n'existe pas on renvoie une exception
            //Copy an existing file to a new file. The overwriting of a file with the same name is allowed.
            File.Copy(source, destination, true);//Copie un fichier existant dans un nouveau fichier. L'écrasement d'un fichier du même nom est autorisé.
            this.nb_file_remaining--;//every time we make a copy the number of files decreases
        }
        //method to return the file size of a directory
        //méthode qui permet de retourner la taille des fichiers d'un répertoire
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
        //method returning a table of backup tasks from the JSON tasks file
        public static Travail[] fromFile()
        {
            string Path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EasySave\\Log";
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
            if (!File.Exists(Path + $"\\tasks.json"))
            {
                File.Create(Path + $"\\tasks.json").Dispose();
                return new Travail[0];
            }
            
            return JsonConvert.DeserializeObject<Travail[]>(File.ReadAllText(Path + $"\\tasks.json")) ?? new Travail[0]; ;
        }
    }
}
