
/*
 *                                  ______                            _____                     
 *                                 / ____/  ____ _   _____   __  __  / ___/  ____ _ _   __  ___ 
 *                                / __/    / __ `/  / ___/  / / / /  \__ \  / __ `/| | / / / _ \
 *                               / /___   / /_/ /  (__  )  / /_/ /  ___/ / / /_/ / | |/ / /  __/
 *                              /_____/   \__,_/  /____/   \__, /  /____/  \__,_/  |___/  \___/ 
 *                                                        /____/                                
 */
/**
 *                                                   ❤️❤️❤️ Made with Love ❤️❤️❤️
 */



using ConsoleTables;
using EasySave.Models;
using EasySave.ViewModels;
using System;
using System.IO;
using System.Linq;
using EasySave.Properties;
using EasySave.ViewModels;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;

namespace EasySave.Views
{
    public class ViewMain
    {
        Backup vm;
        Language lang;

        void __init__()
        {
            vm = new Backup();
            vm.ParsePreferences();
            vm.preferences.PropertyChanged += OnPrefChanged;
            vm.ParseTasks();
            OnPrefChanged(this, new PropertyChangedEventArgs("language"));

        }
        private void OnPrefChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "language")
            {
                switch (vm.preferences.language)
                {
                    case "FR":
                        this.lang = new Francais();
                        break;
                    case "EN":
                    default:
                        this.lang = new Anglais();
                        break;

                }
            }

        }
        string select_dir(string path = null)
        {
            if (path == null)
            {
                var table = new ConsoleTable(lang.get("disk"), lang.get("path"), lang.get("label"), lang.get("Total_Size"), lang.get("Free_Space"), lang.get("Drive_Type"));
                var drives = DriveInfo.GetDrives();
                foreach (var (drive, i) in drives.Select((value, i) => (value, i)))
                {
                    table.AddRow(i, drive.Name, drive.VolumeLabel, drive.TotalSize, drive.AvailableFreeSpace, drive.DriveType);
                }
                table.Write();
                Console.Write("\n" + lang.get("Choisissez_un_disk"));

                var cmd = Int32.Parse(Console.ReadLine());
                if (cmd < drives.Length && cmd > -1) return select_dir(Path.GetFullPath(drives[cmd].ToString()));

                return select_dir(path);
            }
            else
            {
                var dir = new DirectoryInfo(path);
                var dirs = dir.GetDirectories();
                var table = new ConsoleTable("   ", lang.get("Name"), lang.get("Files"), lang.get("Folders"), lang.get("Total_Size"), lang.get("Last_Write_Time"));
                foreach (var (_dir, i) in dirs.Select((value, i) => (value, i)))
                {
                    try
                    {
                        table.AddRow(i, _dir.Name, _dir.GetFiles().Length, _dir.GetDirectories().Length, 00, _dir.LastWriteTime.ToString());
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType().Name == "UnauthorizedAccessException") continue;
                    }
                }
                table.Write();
                Console.Write(lang.get("Choisissez_un_dossier") + (dir.Parent != null? "(vide pour selectionner)" : "(vide pour selectionner et  .. pour revenir en Arriere)"));
                var cmd = Console.ReadLine();
                Console.WriteLine(dir.Parent);
                if (cmd == ".." && dir.Parent != null) return select_dir(dir.Parent.FullName);
                if (cmd == "" || cmd == "..") return Path.GetFullPath(dir.ToString());
                try
                {
                    if (Int32.Parse(cmd) > -1 && Int32.Parse(cmd) < dirs.Length) return select_dir(Path.GetFullPath(dirs[Int32.Parse(cmd)].ToString()));

                }
                catch (Exception ex)
                {
                    return select_dir(path);
                }
                return select_dir(path);
            }
        }
        void add_task()
        {
            if (vm.tasks.Count() >= 5)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Vous avez atteint la limite de tache enregistré"); //todo:add to language
                Console.ResetColor();
                return;
            }
            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.Write(lang.get("Appelation_de_la_sauvegarde"));
            Console.ResetColor();
            Console.Write(" ");
            string appelation = Console.ReadLine();
            Console.WriteLine("  " + lang.get("Source") + " : ");
            string source = select_dir();
            Console.WriteLine("  " + lang.get("Destination") + " : ");
            string destination = select_dir();
            string mode = "";
            while (mode == "")
            {
                Console.WriteLine("\n" + lang.get("Modes_sauvegarde") + "\n  1- " + lang.get("Sauvegarde_Complete") + "\t2- " + lang.get("Sauvegarde_Différentielle"));
                Console.Write(lang.get("Choisissez_un_mode"));
                mode = Console.ReadLine();
                if (mode == "1")
                {
                    mode = "Complet"; continue;
                }
                if (mode == "2")
                {
                    mode = "Differentiel"; continue;
                }
                else mode = "";
            }
            try
            {
                vm.NewTask(appelation, source, destination, mode);
            }
            catch (Exception ex)
            {
                if (ex.Message == "this task already exists")
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(lang.get("task_exists"));
                    Console.ResetColor();
                }
            }
        }

        void ShowTasks()
        {
            var table = new ConsoleTable(lang.get("Task"), lang.get("Source"), lang.get("Destination"), lang.get("Type"), lang.get("Etat"), lang.get("Nb_Files"), lang.get("Total_Size"), lang.get("Progression"), lang.get("Last_Backup_Time"));
            foreach (var (task, i) in vm.tasks.Select((value, i) => (value, i)))
            {
                table.AddRow(task.name, task.source, task.destination, task.type, task.state, task.nb_file, task.total_size, task.progression, (new DirectoryInfo(task.destination)).LastWriteTime.ToString());
            }
            table.Write();
        }
        void LaunchTask()
        {
            if (vm.preferences.ModeCopy == ModeCopy.sequentiel) { vm.StartTask(); return; }
            ShowTasks();
            Console.Write("\n" + lang.get("Choisissez_une_tache"));
            string tache = Console.ReadLine();
            try
            {
                vm.StartTask(tache);

            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(InvalidOperationException)) LaunchTask();
            }
        }
        //supprimer une tache 
        void DeleteTask()
        {
            ShowTasks();
            Console.Write("\n" + lang.get("Choisissez_une_tache"));
            string task = Console.ReadLine();
            if (task == "") return;
            if (vm.tasks.Select(el => el.name).Contains(task))
            {
                vm.DeleteTask(new List<Travail>(vm.tasks.Where(el => el.name == task)));
            }
            else
            {
                DeleteTask();
            }
        }
        //changer de langue
        void ChangeLang()
        {
            vm.ChangeLanguage();
        }
        void MenuPrincipale()
        {
            Console.WriteLine(lang.get("Menu_Principale"));
            Console.WriteLine(lang.get("Afficher_les_travaux") + lang.get("Ajout_sauvegarde"));
            Console.WriteLine(lang.get("Lancer_une_tache") + lang.get("Delete_Task"));
            Console.WriteLine(lang.get("Change_Language") + "\t6- " + lang.get("exit"));
            Console.WriteLine("");
            Console.Write(lang.get("Tapp_chiffre"));
            int cmd = 0;
            try
            {
                cmd = int.Parse(s: Console.ReadLine());

            }
            catch (Exception ex)
            {
                MenuPrincipale();
            }
            switch (cmd)
            {
                case 1:
                    ShowTasks();
                    break;
                case 2:
                    Console.WriteLine("\n" + lang.get("creer_un_travail"));
                    add_task();
                    break;
                case 3:
                    LaunchTask();
                    break;
                case 4:
                    DeleteTask();
                    break;
                case 5:
                    ChangeLang();
                    break;
                case 6:
                    Console.WriteLine(lang.get("exit"));
                    break;
                default:
                    MenuPrincipale();
                    break;
            }
            MenuPrincipale();
        }
        [DllImport("kernel32.dll", ExactSpelling = true)]

        private static extern IntPtr GetConsoleWindow();
        private static IntPtr ThisConsole = GetConsoleWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        static void Main(string[] args)
        {
            ShowWindow(ThisConsole, 3);
            #region logo
            string s = "        ______                            _____                        ";
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.WriteLine(@"        ______                            _____                        ");
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.WriteLine(@"       / ____/  ____ _   _____   __  __  / ___/  ____ _ _   __  ___    ");
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.WriteLine(@"      / __/    / __ `/  / ___/  / / / /  \__ \  / __ `/| | / / / _ \   ");
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.WriteLine(@"     / /___   / /_/ /  (__  )  / /_/ /  ___/ / / /_/ / | |/ / /  __/   ");
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.WriteLine(@"    /_____/   \__,_/  /____/   \__, /  /____/  \__,_/  |___/  \___/    ");
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.WriteLine(@"                              /____/                                   ");
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.WriteLine(@"                                                                       ");

            Console.ResetColor();
            Console.WriteLine(@"");
            #endregion //logo
            ViewMain obj = new ViewMain();
            obj.__init__();
            obj.MenuPrincipale();

        }
    }
}
