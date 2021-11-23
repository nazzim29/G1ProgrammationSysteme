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
        private void onchange(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "source")
            {
                files = new ObservableCollection<string>((new DirectoryInfo(source)).GetFiles("*", SearchOption.AllDirectories).Select(el => el.FullName))?? new ObservableCollection<string>();//récupérer les fichiers du répertoire

                nb_file_remaining = nb_file;
                this.state = "Inactif";

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
                return (100 - (nb_file_remaining * 100) / nb_file) +"%";
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
        
        //redefinition de la méthode Equals qui permet de vérifier l'égalité entre deux instances de la classe travail 
        public override bool Equals(object obj) => obj is Travail && ((Travail)obj).destination.Equals(destination) && ((Travail)obj).source.Equals(source);
        //redefinition de la fonction GetHashCode pour hasher une instance de la classe 
        public override int GetHashCode() => (source.GetHashCode() + destination.GetHashCode()).GetHashCode();
        //fonction qui retourne un thread qui effectue la sauvegarde
        public Thread Start(LogService log)
        {
            var dir = new DirectoryInfo(this.source); 
            files = new ObservableCollection<string>(dir.GetFiles("*", SearchOption.AllDirectories).Select(el => el.FullName));//récupérer les fichiers du répertoire
            return new Thread(delegate ()
            {
                this.state = "Running";
                CreateDirs(this.destination, dir.GetDirectories());//créer les dossiers du répertoire 
                Stopwatch timer = new Stopwatch();
                foreach (var file in files)
                {
                    try
                    {
                        timer.Start();
                        Copyfile(file, file.Replace(this.source, this.destination), this.type == "Diferentiel" ? true : false);//fonction permettant de copier les fichiers
                        timer.Stop();
                        log.Log(new { name = this.name, SourceFile = file, TargetFile = file.Replace(this.source, this.destination), FileSize = (new FileInfo(file)).Length, FileTransfertTime = timer.ElapsedMilliseconds,Time = DateTime.Now.ToString("G") },new LogService.LogJournalier());
                    }catch(Exception ex)
                    {
                        log.Log(new { name = this.name, SourceFile = file, TargetFile = file.Replace(this.source, this.destination), FileSize = (new FileInfo(file)).Length, FileTransfertTime = timer.ElapsedMilliseconds,Time = DateTime.Now.ToString("G") },new LogService.LogJournalier());
                    }
                    this.nb_file_remaining--;
                }
                this.state = "Finished";
            });


        }
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
        //methode permettant de faire la copie des fichiers
        private void Copyfile(string source, string destination, bool dif)
        {
            if (!File.Exists(source)) throw new Exception("Source file not found");//si le fichier n'existe pas on renvoie une exception
            //si la destination existe et que la sauvegarde est différentielle 
            if(File.Exists(destination)&& dif)
            { 
                //on ouvre le fichier source
                using (var sourcef = File.OpenRead(source))
                {
                    //on ouvre le fichier destination
                    using (var destinationf = File.OpenRead(destination))
                    {
                        var hash1 = BitConverter.ToString(MD5.Create().ComputeHash(sourcef));//on hash le contenu du fichier source
                        var hash2 = BitConverter.ToString(MD5.Create().ComputeHash(destinationf));//on hash le contenu du fichier destination
                        //si le hash est le meme on saute a l'itération suivante sans faire de sauvegarde
                        if (hash1 == hash2)
                        {
                            Console.WriteLine($"{source}");
                            return;
                        };
                    }
                }
            }
            File.Copy(source, destination, true);//Copie un fichier existant dans un nouveau fichier. L'écrasement d'un fichier du même nom est autorisé.
        }
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
