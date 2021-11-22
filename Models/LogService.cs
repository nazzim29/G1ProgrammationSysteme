using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Models
{
    public class LogService
    {
        //implementer le design pattern strategy: interface contenant une propriété avec son getter et une méthode non définie
        public interface LogStrategy
        {
            public string Path { get; }
            public void execute(Object obj);
        }

        //class pour la génération du fichier d'état de sauvegarde
        public class LogState : LogStrategy
        {
            public string Path
            {
                get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EasySave\\Log"; }
            }
            public void execute(Object obj)
            {
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
                if (!File.Exists(Path + "\\state.json")) File.Create(Path + "\\state.json").Dispose();
                var jsontxt = File.ReadAllText(Path + "\\state.json");
                List<object> states = new List<object>();
                if (JsonConvert.DeserializeObject<object>(jsontxt) != null)
                    states = new List<object>(JsonConvert.DeserializeObject<object[]>(jsontxt));
                states.Add(obj);
                File.WriteAllText(Path + "\\state.json", JsonConvert.SerializeObject(states));
            }
        }

    }
}
