
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




//using ConsoleTables
using ConsoleTables;
using EasySave.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Resources;

Backup vm = new Backup();
ResourceManager resxReader = new ResourceManager("EasySave.Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
string select_dir(string path = null)
{
    if (path == null)
    {
       
        var table = new ConsoleTable("disk", "path", "label", "Total Size", "Free Space", "Drive Type");
        var drives = DriveInfo.GetDrives();
        foreach (var (drive, i) in drives.Select((value, i) => (value, i)))
        {
            table.AddRow(i, drive.Name, drive.VolumeLabel, drive.TotalSize, drive.AvailableFreeSpace, drive.DriveType);
        }
        table.Write();
        Console.Write("\n  choisissez un disk : ");

        var cmd = Int32.Parse(Console.ReadLine());
        if (cmd < drives.Length && cmd > -1) return select_dir(Path.GetFullPath(drives[cmd].ToString()));

        return select_dir(path);
    }
    else
    {
        var dir = new DirectoryInfo(path);
        var dirs = dir.GetDirectories();
        var table = new ConsoleTable("   ", "Name", "Files", "Folders", "Total Size", "Last Write Time");
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
        Console.Write("  Choisissez un dossier: ");
        var cmd = Console.ReadLine();
        if (cmd == "") return Path.GetFullPath(dir.ToString());
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
    Console.Write("  ");
    Console.ForegroundColor = ConsoleColor.White;
    Console.BackgroundColor = ConsoleColor.DarkGreen;
    Console.Write(@"Appelation de la sauvegarde:");
    Console.ResetColor();
    Console.Write(" ");
    string appelation = Console.ReadLine();
    Console.WriteLine(@" Source: ");
    string source = select_dir();
    Console.WriteLine(@" Destination: ");
    string destination = select_dir();
    string mode = "";
    while (mode == "")
    {
        Console.WriteLine("1-Sauvegarde Complete\t2-Sauvgarde Diferentielle");
        Console.Write("  Choisissez un mode: ");
        mode = Console.ReadLine();
        if (mode == "1")
        {
            mode = "Complet"; continue;
        }
        if (mode == "2")
        {
            mode = "Diferentiel"; continue;
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
            Console.WriteLine(" task already exists ");
            Console.ResetColor();
        }
    }
}

void ShowTasks()
{
    var table = new ConsoleTable("Task", "Source", "Destination", "Type", "Etat", "Nb. Files", "Total Size", "Progression", "Last Backup Time");
    foreach (var (task, i) in vm.tasks.Select((value, i) => (value, i)))
    {
        table.AddRow(task.name, task.source, task.destination, task.type, task.state, task.nb_file, task.total_size, task.progression, (new DirectoryInfo(task.destination)).LastWriteTime.ToString());
    }
    table.Write();
}
void LaunchTask()
{
    ShowTasks();
    Console.Write("\n  Choisissez une tache: ");
    string tache = Console.ReadLine();
    try
    {
        vm.StartTask(tache);

    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
}
void MenuPrincipale()
{
    Console.WriteLine(@"  Menu Principale: ");
    Console.WriteLine("    1- Afficher les traveaux\t2-Ajouter un travail de sauvegarde");
    Console.WriteLine("    3- Lancer une tache\t\t4-Exit");
    Console.WriteLine("");
    Console.Write(@"  Tappez un chiffre: ");
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
            Console.WriteLine(@"creer un travail: ");
            add_task();
            break;
        case 3:
            LaunchTask();
            break;
        case 4:
            Console.WriteLine(@" Exit");
            break;
        default:
            MenuPrincipale();
            break;
    }
    MenuPrincipale();
}

#region logo
string s = "        ______                            _____                        ";
Console.BackgroundColor = ConsoleColor.DarkGreen;
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

MenuPrincipale();


