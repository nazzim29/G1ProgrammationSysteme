using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave_GUI.Properties
{
    class Anglais 
    {
        public static string backup_name { get { return "Name"; } }
        public static string source { get { return "Source"; } }
        public static string destination { get { return "Destination"; } }
        public static string type { get { return "Type"; } }
        public static string crypt { get { return "Encryption"; } }
        public static string ajouter { get { return "Add"; } }
        public static string lancer { get { return "Launch"; } }
        public static string supp { get { return "Delete"; } }
        public static string quitter { get { return "Exit"; } }
        public static string sequentiel { get { return "One By one  "; } }
        public static string simultane { get { return "Simultaneous"; } }
        public override string ToString()
        {
            return "EN";
        }
    }
}
