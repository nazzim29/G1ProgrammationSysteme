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
        //implement the design pattern strategy: interface containing a property with its getter and an unimplemented method
        //implementer le design pattern strategy: interface contenant une propriété avec son getter et une méthode non définie
        public interface LogStrategy
        {
            //attribute with its getter
            public string Path { get; }
            //execute method
            public void execute(Object obj);
        }
        //class for generating the backup state file
        //class pour la génération du fichier d'état de sauvegarde
        public class LogState : LogStrategy
        {
            public string Path
            {
                //return the path of the state file
                get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EasySave\\Log"; }
            }
            //method execute creates the state file with its content
            public void execute(Object obj)
            {
                if (!Directory.Exists(Path))//verifies if the directory does not exist, if not we create it
                {
                    Directory.CreateDirectory(Path);
                }
                if (!File.Exists(Path + "\\state.json")) File.Create(Path + "\\state.json").Dispose();//verifies if the state file does not exist, if not we create then we close the file
                //writes in the JSON file using the properties of a given object "obj"
                File.WriteAllText(Path + "\\state.json", JsonConvert.SerializeObject(obj,Formatting.Indented));
            }
        }
        //class for generating the backup log file
        public class LogJournalier : LogStrategy
        {
            public string Path
            {
                get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EasySave\\Log"; }
            }
            //save the log file with the following name format for example: log24-11-2021.json
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
                if (!File.Exists(Path + $"\\{FileName}")) File.Create(Path + $"\\{FileName}").Dispose();//creates the log file if it does not exist
                var jsontxt = File.ReadAllText(Path + $"\\{FileName}");//Opens the JSON file, reads all the text in the file into a string, and then closes the file.
                List<object> logs = new List<object>();//list of logs
                if (JsonConvert.DeserializeObject<object>(jsontxt) != null)//verifies if the file is empty 
                    logs = new List<object>(JsonConvert.DeserializeObject<object[]>(jsontxt));
                logs.Add(obj);//add the task state's stored in obj to the logs list
                File.WriteAllText(Path + $"\\{FileName}", JsonConvert.SerializeObject(logs,Formatting.Indented));//add the task state's to the state JSON file with formatting
            }
        }
        //class for generating the backup list file
        public class LogTasks : LogStrategy
        {
            public string Path
            {
                get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EasySave\\Log"; }
            }
            //method to store the list of backup tasks
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
