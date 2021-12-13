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
    public enum CopyMode : ushort
    {
        sequentiel = 1,
        simultane = 2
    }
    public class Preferences : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private static readonly string PrefPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EasySave\\Preferences";//the path of the preferences config file
        private string _language;
        private CopyMode _Mode;
        private string _cryptExt;
        private string _logicielMetier;
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
        public CopyMode Mode
        {
            get { return _Mode; }
            set { _Mode = value; OnPropertyChanged("Mode"); }
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
                File.WriteAllText(PrefPath + "\\config.json", JsonConvert.SerializeObject(new { language = "EN", Mode = 1,CryptExt = "",LogicielMetier = "notepad.exe"}, Formatting.Indented));
                //default app preferences

            }
            var pref = JsonConvert.DeserializeObject<Preferences>(File.ReadAllText(PrefPath + "\\config.json"));
            return pref;
        }
        public Preferences(string _language = "EN", CopyMode mode = CopyMode.simultane,string _ext = "",string _p = "notepad.exe")
        {
            PropertyChanged += Save;
            this._language = _language;
            this._Mode = mode;
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
            File.WriteAllText(PrefPath + "\\config.json", JsonConvert.SerializeObject(new { language = this.language, Mode = this.Mode,CryptExt = this.CryptExt,LogicielMetier=this.LogicielMetier }, Formatting.Indented));
        }
    }
}
