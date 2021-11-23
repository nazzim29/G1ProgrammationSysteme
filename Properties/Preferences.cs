using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Properties
{

    public class Preferences
    {
        private static readonly string PrefPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+"\\EasySave\\Preferences";
        public string language;
        public static Preferences fromFile()
        {
            if (!Directory.Exists(PrefPath))
            {
                Directory.CreateDirectory(PrefPath);
            }
            if (!File.Exists(PrefPath + "\\config.json"))
            {
                File.Create(PrefPath + "\\config.json").Dispose();
                File.WriteAllText(PrefPath+"\\config.json",JsonConvert.SerializeObject(new { language = "EN" },Formatting.Indented));
            }
            var pref = JsonConvert.DeserializeObject<Preferences>(File.ReadAllText(PrefPath+"\\config.json"));
            return pref;
        }
        public void Save()
        {

        } 
        public Preferences(string _language = "EN")
        {
            this.language = _language;
        }
    }
}
