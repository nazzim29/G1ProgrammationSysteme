using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EasySave_GUI.Properties
{
    public class Preferences : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private static readonly string PrefPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EasySave\\Preferences";//the path of the preferences config file
        private string _language;
        private string _cryptExt;
        private string _logicielMetier;
        private string _Prioritaire;
        private string _Password;
        private long _LimiteBandePassante;
        private long _LimiteFichier;
        

        
        public long LimiteBandePassante
        {
            get { return _LimiteBandePassante; }
            set { _LimiteBandePassante = value;OnPropertyChanged("LimiteBandePassante"); }
        }
        public long LimiteFichier
        {
            get { return _LimiteFichier; }
            set { _LimiteFichier = value;OnPropertyChanged("LimiteFichier"); }
        }
        public string Password
        {
            get { return _Password; }
            set { _Password = value; OnPropertyChanged("Password");}
        }
        public string Prioritaire
        {
            get
            {
                return _Prioritaire;
            }
            set
            {
                _Prioritaire = value;
                OnPropertyChanged("Prioritaire");
            }
        }
        public string LogicielMetier
        {
            get { return _logicielMetier; }
            set
            {
                if (_logicielMetier == value) return; 
                _logicielMetier = value;
                OnPropertyChanged("LogicielMetier");
            }
        }
        public string CryptExt
        {
            get { return _cryptExt; }
            set { _cryptExt = value; OnPropertyChanged("CryptExt"); }
        }
       
        public string language
        {
            get { return _language; }
            set { _language = value; OnPropertyChanged("language"); }
        }
        //Parse the preferences from the JSON config file
        public static Preferences fromFile()
        {
            if (!Directory.Exists(PrefPath))
            {
                Directory.CreateDirectory(PrefPath);
            }
            if (!File.Exists(PrefPath + "\\config.json"))
            {
                File.Create(PrefPath + "\\config.json").Dispose();
                File.WriteAllText(PrefPath + "\\config.json", JsonConvert.SerializeObject(new { language = "EN", Mode = 1,CryptExt = "",LogicielMetier = "notepad.exe",Prioritaire ="",Password="",LimiteBandePassante = 20,LimiteFichier=20}, Formatting.Indented));
                //default app preferences

            }
            var pref = JsonConvert.DeserializeObject<Preferences>(File.ReadAllText(PrefPath + "\\config.json"));
            return pref;
        }
        public Preferences(string _language = "EN",string _ext = "",string _p = "notepad.exe")
        {
            PropertyChanged += Save;
            this._language = _language;
            this._cryptExt = _ext;
            this._logicielMetier = _p;
        }
        //save the current preferences in the config file
        private void Save(object sender, PropertyChangedEventArgs e)
        {
            if (!Directory.Exists(PrefPath))
            {
                Directory.CreateDirectory(PrefPath);
            }
            if (!File.Exists(PrefPath + "\\config.json"))
            {
                File.Create(PrefPath + "\\config.json").Dispose();
            }
            File.WriteAllText(PrefPath + "\\config.json", JsonConvert.SerializeObject(new { language = this.language,CryptExt = this.CryptExt,LogicielMetier=this.LogicielMetier, Prioritaire = this.Prioritaire,Password = this.Password,LimiteBandePassante = this.LimiteBandePassante, LimiteFichier = this.LimiteFichier}, Formatting.Indented));
        }
    }
}
