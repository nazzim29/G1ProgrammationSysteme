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
    public enum logextension : ushort
    {
        json = 1,
        xml = 2
    }
    public class Preferences : BaseINPC
    {
        private static readonly string PrefPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+"\\EasySave\\Preferences";//the path of the preferences config file
        private string _language;
        private ModeCopy _modeCopy;
        private logextension _logextension;
        public ModeCopy ModeCopy
        {
            get { return _modeCopy; }
            set { _modeCopy = value;RaisePropertyChanged("ModeCopy"); }
        }
        public logextension logextension
        {
            get { return _logextension; }
            set { _logextension = value; RaisePropertyChanged("logextension"); }
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
                File.WriteAllText(PrefPath + "\\config.json", JsonConvert.SerializeObject(new { language = "EN", ModeCopy = 1, logextension = 1 }, Formatting.Indented));
                //default app preferences
                
            }
            var pref = JsonConvert.DeserializeObject<Preferences>(File.ReadAllText(PrefPath+"\\config.json"));
            return pref;
        }
        public Preferences(string _language = "EN",ModeCopy mode = ModeCopy.simultane, logextension extension = logextension.json)
        {
            PropertyChanged += Save;
            this._language = _language;
            this._modeCopy = mode;
            this._logextension = extension;
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
                File.WriteAllText(PrefPath + "\\config.json", JsonConvert.SerializeObject(new { language = this.language,ModeCopy = this.ModeCopy, logextension = this.logextension}, Formatting.Indented));
        }
    }
}
