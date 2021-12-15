using EasySave.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EasySave.Models
{
    public interface LogStrategy
    {
        //attribute with its getter
        public string Path { get; }
        //execute method
        public void execute(Object obj);
    }
    public interface WriteStrategy
    {
        public string serialize(Object obj);
        public T? deserialize<T>(string s);
        public string ToString();
    }
    public class JSONStrategy : WriteStrategy
    {
        public string serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
        public T? deserialize<T>(string s)
        {
            return JsonConvert.DeserializeObject<T>(s);
        }
        public override string ToString()
        {
            return "json";
        }
    }
    public class XMLStrategy : WriteStrategy
    {
        public string serialize(object obj)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, obj);
                return textWriter.ToString();
            }

        }
        public T? deserialize<T>(string s)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));

            using (StringReader sr = new StringReader(s))
            {
                return (T)ser.Deserialize(sr);
            }
        }
        public override string ToString()
        {
            return "xml";
        }
    }
    //class for generating the backup state file
    //class pour la génération du fichier d'état de sauvegarde
    public class LogState : LogStrategy
    {
        public class state
        {
            [XmlElement(Namespace = "http://www.cpandl.com")]
            public string Name,SourceFilePath,TargetFilePath,Progression,State;
            [XmlElement(Namespace = "http://www.cohowinery.com")]
            public double NbFilesToDo, TotalFileSize, TotalFileToCopy;

        }
        private WriteStrategy WriteStrategy;
        public LogState(logextension v)
        {
            WriteStrategy = v == logextension.json ? new JSONStrategy() : new XMLStrategy();
        }
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
            if (!File.Exists(Path + $"\\state.{WriteStrategy.ToString()}")) File.Create(Path + $"\\state.{WriteStrategy.ToString()}").Dispose();//verifies if the state file does not exist, if not we create then we close the file
            var dd = (obj as object[])[0];
            File.WriteAllText(Path + $"\\state.{WriteStrategy.ToString()}", WriteStrategy.serialize(obj));
        }
    }
    //class for generating the backup log file
    public class LogJournalier : LogStrategy
    {
        private WriteStrategy WriteStrategy;
        public LogJournalier(logextension v)
        {
            WriteStrategy = v == logextension.json?new JSONStrategy():new XMLStrategy();
        }
        public string Path
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EasySave\\Log"; }
        }
        //save the log file with the following name format for example: log24-11-2021.json
        private string FileName
        {
            get { return $"log{DateTime.Now.ToString("dd-MM-yyyy")}.{WriteStrategy.ToString()}"; }
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
            if (WriteStrategy.deserialize<object>(jsontxt) != null)//verifies if the file is empty 
                logs = new List<object>(WriteStrategy.deserialize<object[]>(jsontxt));
            logs.Add(obj);//add the task state's stored in obj to the logs list
            File.WriteAllText(Path + $"\\{FileName}", WriteStrategy.serialize(obj));//add the task state's to the state JSON file with formatting
        }
    }
    //class for generating the backup list file
    public class LogTasks : LogStrategy
    {
        private WriteStrategy WriteStrategy;
        public LogTasks(logextension v)
        {
            WriteStrategy = v == logextension.json ? new JSONStrategy() : new XMLStrategy();
        }
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
            if (!File.Exists(Path + $"\\tasks.{WriteStrategy.ToString()}")) File.Create(Path + $"\\tasks.{WriteStrategy.ToString()}").Dispose();
            File.WriteAllText(Path + $"\\tasks.{WriteStrategy.ToString()}", WriteStrategy.serialize(tasks));
        }
    }
    public class LogService
    {
        //implement the design pattern strategy: interface containing a property with its getter and an unimplemented method
        //implementer le design pattern strategy: interface contenant une propriété avec son getter et une méthode non définie
        public void Log(object obj, LogStrategy strat) => strat.execute(obj);
    }
}
