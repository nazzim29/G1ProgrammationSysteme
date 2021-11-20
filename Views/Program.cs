
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




using System;
using System.IO;
using System.Linq;
//using ConsoleTables
using ConsoleTables;
using EasySave.ViewModels;
Backup vm = new Backup();
string select_dir(string path = null)
{
    if(path == null)
    {
        var table = new ConsoleTable("disk", "path", "label", "Total Size", "Free Space", "Drive Type");
        var drives = DriveInfo.GetDrives();
        foreach (var (drive,i) in drives.Select((value, i) => (value, i)))
        {
            table.AddRow(i,drive.Name,drive.VolumeLabel,drive.TotalSize,drive.AvailableFreeSpace,drive.DriveType);
        }
        table.Write();
        Console.Write("\n  choisissez un disk : ");
        try
        {
            var cmd = Int32.Parse(Console.ReadLine());
            if (cmd< drives.Length && cmd > -1) return select_dir(Path.GetFullPath(drives[cmd].ToString()));
        }catch(Exception ex)
        {
            Console.Write(ex);
        }
        return select_dir();
    }
    else
    {
        var dir = new DirectoryInfo(path);
        var dirs = dir.GetDirectories();
        var table = new ConsoleTable("   ", "Name", "Files", "Folders", "Total Size", "Last Write Time");
        foreach (var (_dir, i) in dirs.Select((value, i) => (value, i)))
        {
            table.AddRow(i,Path.GetFullPath(_dir.ToString()), _dir.GetFiles().Length, _dir.GetDirectories().Length, 00, _dir.LastWriteTime.ToString());
        }
        table.Write();
        Console.Write("  Choisissez un dossier: ");
        var cmd = Console.ReadLine();
        if (cmd == "") return Path.GetFullPath(dir.ToString());
        if (Int32.Parse(cmd) > -1 && Int32.Parse(cmd)< dirs.Length) return select_dir(Path.GetFullPath(dirs[Int32.Parse(cmd)].ToString()));
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
    Console.WriteLine("1-Sauvegarde Complete\t2-Sauvgarde Diferentielle");
    Console.Write("  Choisissez un mode: ");
    string mode = Console.ReadLine();
    Console.WriteLine(appelation,source,destination,mode);
}
void MenuPrincipale()
{
    Console.WriteLine(@"  Menu Principale: ");
    Console.WriteLine(@"    1- Afficher les traveaux            2-Ajouter un travail de sauvegarde");
    Console.WriteLine(@"    3- liste des Disque disponible      4-Exit");
    Console.WriteLine("");
    Console.Write(@"  Tappez un chiffre: ");
    switch (Int32.Parse(Console.ReadLine()))
    {
        case 1:
            Console.WriteLine("heeeeeeeeeeeeeeeey");
            break;
        case 2:
            Console.WriteLine(@"creer un travail: ");
            add_task();
            break;
        case 3:
            Console.WriteLine(@"liste des disques");
            break;
        case 4:
            Console.WriteLine(@" Exit");
            break;
        default:
            MenuPrincipale();
            break;
    }
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


