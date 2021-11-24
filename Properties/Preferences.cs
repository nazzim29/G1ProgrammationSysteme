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
        private static readonly string PrefPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+"\\EasySave\\Preferences";//the path of the preferences config file
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
                File.WriteAllText(PrefPath+"\\config.json",JsonConvert.SerializeObject(new { language = "EN",ModeCopy = 1 },Formatting.Indented));
                //default app preferences
                
            }
            var pref = JsonConvert.DeserializeObject<Preferences>(File.ReadAllText(PrefPath+"\\config.json"));
            return pref;
        }
        public Preferences(string _language = "EN",ModeCopy mode = ModeCopy.simultane)
        {
            PropertyChanged += Save;
            this._language = _language;
            this._modeCopy = mode;
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
                File.WriteAllText(PrefPath + "\\config.json", JsonConvert.SerializeObject(new { language = this.language,ModeCopy = this.ModeCopy}, Formatting.Indented));
        }
    }
}
