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
        public class LogJournalier : LogStrategy
        {
            public string Path
            {
                get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EasySave\\Log"; }
            }
            private string FileName
            {
                get { return $"log{DateTime.Now.ToString("dd-MM-yyyy")}.json"; }
            }
            public void execute(object obj)
            {
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
                if (!File.Exists(Path + $"\\{FileName}")) File.Create(Path + $"\\{FileName}").Dispose();
                var jsontxt = File.ReadAllText(Path + $"\\{FileName}");
                List<object> logs = new List<object>();
                if (JsonConvert.DeserializeObject<object>(jsontxt) != null)
                    logs = new List<object>(JsonConvert.DeserializeObject<object[]>(jsontxt));
                logs.Add(obj);
                File.WriteAllText(Path + $"\\{FileName}", JsonConvert.SerializeObject(logs,Formatting.Indented));
            }
        }
        public class LogTasks : LogStrategy
        {
            public string Path
            {
                get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EasySave\\Log"; }
            }
            public void execute(object tasks)
            {
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
                if (!File.Exists(Path + $"\\tasks.json")) File.Create(Path + $"\\tasks.json").Dispose();
                File.WriteAllText(Path + $"\\tasks.json", JsonConvert.SerializeObject(tasks,Formatting.Indented));
            }
        }
        public void Log(object obj, LogStrategy strat) => strat.execute(obj);
    }
}
