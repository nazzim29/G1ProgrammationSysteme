using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave_GUI.Properties
{
    public abstract class Langue
    {
        public static string backup_name { get; }
        public static string source { get; }
        public static string destination { get; }
        public static string backup_complete { get; }
        public static string backup_diff { get; }
        public static string type { get; }
        public static string crypt { get; }
        public static string ajouter { get; }
        public static string lancer { get; }
        public static string supp { get; }
        public static string quitter { get; }
        public static string sequentiel { get; }
        public static string simultane { get; }

        public virtual string ToString()
        {
            return "hadja";
        }
    }
}
