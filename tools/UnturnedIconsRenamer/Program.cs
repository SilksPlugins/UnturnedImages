using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnturnedIconsRenamer
{
    public class Program
    {
        private static void DisplayHelp()
        {
            var sectionSplitterLength = Console.BufferWidth;

            if (sectionSplitterLength > 20)
            {
                sectionSplitterLength = 20;
            }

            Console.WriteLine(new string('=', sectionSplitterLength));
            Console.WriteLine();
            Console.WriteLine("Purpose:");
            Console.WriteLine();

            Console.WriteLine("This application renames item icon files to then be committed to an online CDN.");

            Console.WriteLine();
            Console.WriteLine(new string('=', sectionSplitterLength));
            Console.WriteLine();
            Console.WriteLine("Exporting Icons:");
            Console.WriteLine();

            Console.WriteLine("In the Unturned main menu, enter the Workshop menu.");
            Console.WriteLine("From there, press F1 and press the 'All Item Icons' button in the top left.");
            Console.WriteLine();
            Console.WriteLine("You can then use this application. You will need to locate your Unturned installation directory/path.");

            Console.WriteLine();
            Console.WriteLine(new string('=', sectionSplitterLength));
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine();

            Console.WriteLine("UnturnedIconsRenamer.exe [-y] [-v] <unturned install path>");
            Console.WriteLine();
            Console.WriteLine("  -y | --yes        Skip confirming renaming of files.");
            Console.WriteLine("  -v | --verbose    Display verbose output.");

            Console.WriteLine();
            Console.WriteLine(new string('=', sectionSplitterLength));
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine();

            Console.WriteLine(@"UnturnedIconsRenamer.exe C:\SteamLibrary\steamapps\common\Unturned");

            Console.WriteLine();
            Console.WriteLine(new string('=', sectionSplitterLength));
        }

        private static string GetInstallDirectory(string[] args)
        {
            if (args.Length == 0)
            {
                throw new NiceException("No path specified.");
            }

            var path = args.Last();

            if (!Directory.Exists(path))
            {
                throw new NiceException("The given directory does not exist.");
            }

            return path;
        }

        private static bool ShouldSkipConfirm(string[] args)
        {
            return args.Contains("-y") || args.Contains("--yes");
        }

        private static bool ShouldShowVerboseOutput(string[] args)
        {
            return args.Contains("-v") || args.Contains("--verbose");
        }

        private static bool ShouldContinue(string message)
        {
            Console.WriteLine(message);

            while (true)
            {
                Console.Write("Are you sure you'd like to continue? (Y/N): ");
                var key = Console.ReadKey();

                Console.WriteLine();

                switch (key.KeyChar)
                {
                    case 'y':
                    case 'Y':
                        return true;
                    case 'n':
                    case 'N':
                        Console.WriteLine("Cancelling the renaming of the files.");
                        return false;
                    default:
                        Console.WriteLine("Unknown input.");
                        break;
                }
            }
        }

        private static void ProcessInput(string[] args)
        {
            if (args.Length == 0)
            {
                DisplayHelp();

                return;
            }

            var unturnedInstallPath = GetInstallDirectory(args);
            var shouldSkipConfirm = ShouldSkipConfirm(args);
            var verboseOutput = ShouldShowVerboseOutput(args);

            var iconsPath = Path.Combine(unturnedInstallPath, "Extras", "Icons");

            if (!Directory.Exists(iconsPath))
            {
                throw new NiceException("Could not find icons directory from install directory.");
            }

            var iconsDirectory = new DirectoryInfo(iconsPath);

            var files = iconsDirectory.GetFiles("*.png");

            if (files.Length == 0)
            {
                throw new NiceException("No files found in the icons directory.");
            }

            if (!shouldSkipConfirm && !ShouldContinue($"This program will rename {files.Length} files in the {iconsPath} directory."))
            {
                return;
            }

            Console.WriteLine($"Preparing to rename {files.Length} icon files...");

            var plannedNewNames = new Dictionary<string, FileInfo>();

            foreach (var file in files)
            {
                var newName = Path.GetFileNameWithoutExtension(file.Name).Split('_').LastOrDefault();

                if (newName == null || !ushort.TryParse(newName, out _))
                {
                    throw new NiceException($"Icon name for file {file.FullName} cannot be determined.");
                }

                newName += file.Extension;
                newName = newName.ToLower();

                if (verboseOutput)
                {
                    Console.WriteLine($"{file.Name} -> {newName}");
                }

                if (plannedNewNames.ContainsKey(newName))
                {
                    throw new NiceException(
                        $"Two icon files would have the same name '{newName}'. Use the verbose output to find the two files.");
                }

                plannedNewNames.Add(newName, file);
            }

            Console.WriteLine($"Renaming {files.Length} icon files...");

            foreach (var plannedNewName in plannedNewNames)
            {
                plannedNewName.Value.MoveTo(Path.Combine(plannedNewName.Value.DirectoryName!, plannedNewName.Key));
            }
            

            Console.WriteLine($"Renamed {files.Length} icon files.");
        }

        private static void Main(string[] args)
        {
            try
            {
                ProcessInput(args);
            }
            catch (NiceException ex)
            {
                var color = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = color;
            }
            // ReSharper disable once RedundantCatchClause
            catch
            {
#if DEBUG
                throw;
#else
                Console.WriteLine("An exception occurred during the execution of this program. Please report this to the developers.");

                Console.WriteLine(ex);
#endif
            }
        }
    }
}
