using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Properties
{
    public abstract class Language
    {
        public static string disk { get; }
        public static string path{ get; }
        public static string label{ get; }
        public static string Total_Size{ get; }
        public static string Free_Space{ get; }
        public static string Drive_Type{ get; }
        public static string Name{ get; }
        public static string Files{ get; }
        public static string Folders{ get; }
        public static string Last_Write_Time{ get; }
        public static string Appelation_de_la_sauvegarde{ get; }
        public static string Source{ get; }
        public static string Destination{ get; }
        public static string Sauvegarde_Complete{ get; }
        public static string Sauvegarde_Différentielle{ get; }
        public static string Task{ get; }
        public static string Type{ get; }
        public static string Etat{ get; }
        public static string Nb_Files{ get; }
        public static string Progression{ get; }
        public static string Last_Backup_Time{ get; }
        public static string Choisissez_un_dossier{ get; }
        public static string Complet{ get; }
        public static string Differentiel{ get; }
        public static string task_exists{ get; }
        public static string Choisissez_une_tache{ get; }
        public static string Afficher_les_travaux{ get; }
        public static string Ajout_sauvegarde{ get; }
        public static string Lancer_une_tache{ get; }
        public static string Modes_sauvegarde { get; }
        public static string exit{ get; }
        public static string creer_un_travail{ get; }
        public static string Menu_Principale { get; }
        public static string Tapp_chiffre { get; }
        public static string Delete_Task { get; }
        public static string Change_Language { get; }
        public static string Limite_taches { get; }
        public static string Revenir_en_arriere { get; }
        public string get(string prop)
        {
            return this.GetType().GetProperty(prop).GetValue(this, null).ToString();
           
        }

    }
}
