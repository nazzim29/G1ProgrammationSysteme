using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave_GUI.Properties
{
    class Francais : Langue
    {
        public static string backup_name { get { return "Appelation"; } }
        public static string source { get { return "Source"; } }
        public static string destination { get { return "Destination"; } }
        public static string type { get { return "Type"; } }
        public static string crypt { get { return "Cryptage"; } }
        public static string ajouter { get { return "Ajouter"; } }
        public static string lancer { get { return "Lancer"; } }
        public static string supp { get { return "Supprimer"; } }
        public static string quitter { get { return "Quitter"; } }
        public static string sequentiel { get { return "Séquentiel"; } }
        public static string simultane { get { return "Simultané"; } }
        public override string ToString()
        {
            return "hadja";
        }
    }
}
