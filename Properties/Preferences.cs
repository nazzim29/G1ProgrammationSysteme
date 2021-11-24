using EasySave.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Properties
{
    public enum ModeCopy : ushort
    {
        sequentiel = 1,
        simultane = 2
    }
    public class Preferences : BaseINPC
    {
        private static readonly string PrefPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+"\\EasySave\\Preferences";
        private string _language;
        private ModeCopy _modeCopy;
        public ModeCopy ModeCopy
        {
            get { return _modeCopy; }
            set { _modeCopy = value;RaisePropertyChanged("ModeCopy"); }
        }
        public string language
        {
            get { return _language; }
            set { _language = value; RaisePropertyChanged("language"); }
        }
        public static Preferences fromFile()
        {
            if (!Directory.Exists(PrefPath))
            {
                Directory.CreateDirectory(PrefPath);
            }
            if (!File.Exists(PrefPath + "\\config.json"))
            {
                File.Create(PrefPath + "\\config.json").Dispose();
                File.WriteAllText(PrefPath+"\\config.json",JsonConvert.SerializeObject(new { language = "EN",ModeCopy = 1 },Formatting.Indented));
            }
            var pref = JsonConvert.DeserializeObject<Preferences>(File.ReadAllText(PrefPath+"\\config.json"));
            return pref;
        }
        public Preferences(string _language = "EN")
        {
            PropertyChanged += Save;
            this._language = _language;
        }
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
                File.WriteAllText(PrefPath + "\\config.json", JsonConvert.SerializeObject(new { language = this.language,ModeCopy = this.ModeCopy}, Formatting.Indented));
        }
    }
}
