using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Serpent_Theater_Server.Database;
using Serpent_Theater_Server.FTP;
using Serpent_Theater_Server.Utils;

namespace Serpent_Theater_Server
{
    class Program
    {
        public static TheaterContext Context;
        public static FtpServer Server;

        public static Dictionary<string, Tuple<Action, string>> CommandsDictionary = new Dictionary<string, Tuple<Action, string>>
        {
            {"help", new Tuple<Action, string>(PrintCommands, "Displays a list of all commands available.")},
            {"exit", new Tuple<Action, string>(ExitServer, "Safely shuts down the server and exits the command console.")},
            {"enableAutoStart", new Tuple<Action, string>(SetAutoStartOnLogin, "The server will start automatically when the user logs in.")},
            {"disableAutoStart", new Tuple<Action, string>(DisableAutoStartOnLogin, "The server will no longer start automatically when the user logs in.")},
            {"initServer", new Tuple<Action, string>(InitializaServer, "Manually start the server.")},
            {"shutDownServer", new Tuple<Action, string>(ShutDownServer, "Manually stop the server.")},
            {"restartServer", new Tuple<Action, string>(RestartServer, "Manually restart the server.")},
            {"status", new Tuple<Action, string>(DisplayServerStatus, "Displays the server's status.")},
            {"addToMoviesDirectories", new Tuple<Action, string>(AddToMoviesDirectories, "Adds specified directory to the movies directory list.")},
            {"removeFromMoviesDirectories", new Tuple<Action, string>(RemoveFromMoviesDirectories, "Removes specified directory from the movies directory list.")},
            {"listMoviesDirectories", new Tuple<Action, string>(ListMoviesDirectories, "Lists all known movies directories.")},
            {"addAllOrNewMovies", new Tuple<Action, string>(AddAllOrNewMovies, "Scans all movies directories to find and add all or any missing movies to the database.")},
            {"testImage", new Tuple<Action, string>(testImage, "")}
        };

        private static void testImage()
        {
            Movie movie;
            do
            {
                Console.Write("Movie Name:");
                string movieName = Console.ReadLine();
                movie = Context.Movies.FirstOrDefault(x => x.Title == movieName);
                if (movie != null)
                {
                    var poster = movie.Poster;
                    try
                    {
                        string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
                        if (Environment.OSVersion.Version.Major >= 6)
                        {
                            path = Directory.GetParent(path).ToString();
                            var fileStream =
                           new System.IO.FileStream(path + "\\Desktop\\" + movie.ImdbId +".jpg", System.IO.FileMode.Create,
                                                    System.IO.FileAccess.Write);
                            fileStream.Write(poster, 0, poster.Length);
                            fileStream.Close();
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("Exception caught in process: {0}",
                                          exception.ToString());
                    }
                }
                else
                {
                    Console.WriteLine("Invalid movie name!");
                }
            } while (movie == null);
        }

        private static void ListMoviesDirectories()
        {
            throw new NotImplementedException();
        }

        private static void AddAllOrNewMovies()
        {
            var watch = new Stopwatch();
            watch.Start();
            var databaseBuilder = new DatabaseBuilder(@"F:\Media\Movies\720",DatabaseBuilder.DatabaseBuilderType.Movies, Context);
            databaseBuilder.AddAllMissing();
            watch.Stop();
            Console.WriteLine(watch.Elapsed.ToString());
            Console.ReadLine();
            throw new NotImplementedException();
        }

        private static void ExitServer()
        {
            if(Server.IsActive())
                ShutDownServer();
            Environment.Exit(0);
        }

        private static void RemoveFromMoviesDirectories()
        {
            throw new NotImplementedException();
        }

        private static void AddToMoviesDirectories()
        {
            throw new NotImplementedException();
        }

        private static void DisplayServerStatus()
        {
            Console.WriteLine(Server.IsActive() ? @"Server running at port 21" : @"Server not running.");
        }

        private static void RestartServer()
        {
            ShutDownServer();
            InitializaServer();
        }

        private static void ShutDownServer()
        {
            if (Server.IsActive())
            {
                Server.Stop();
                Console.WriteLine(@"Server shut down.");
            }
            else
            {
                Console.WriteLine(@"Server is not running!");
            }
        }

        private static void InitializaServer()
        {
            if (!Server.IsActive())
            {
                Server.Start();
                Console.WriteLine(@"Server initialized.");
            }
            else
            {
                Console.WriteLine(@"Server is already running!");
            }
        }

        private static void PrintCommands()
        {
            Console.WriteLine(Environment.NewLine + @"List of Available Commands:" + Environment.NewLine);
            var commandsList = CommandsDictionary.Select(keyValuePair => new[] {keyValuePair.Key, "- "+keyValuePair.Value.Item2+"\n"}).ToList();
            Console.WriteLine(Utilities.PadElementsInLines(commandsList));
        }

        private static void DisableAutoStartOnLogin()
        {
            var key = Registry.CurrentUser.OpenSubKey(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
            if (key != null && key.GetValue("SerpentTheaterServer") != null)
            {
                key.DeleteValue("SerpentTheaterServer");
            }
        }

        private static void SetAutoStartOnLogin()
        {
            var key = Registry.CurrentUser.OpenSubKey(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
            if (key != null && (key.GetValue("SerpentTheaterServer") == null || 
                (string)key.GetValue("SerpentTheaterServer") != System.Reflection.Assembly.GetEntryAssembly().Location))
            {
                key.SetValue("SerpentTheaterServer", System.Reflection.Assembly.GetEntryAssembly().Location);
            }
        }

        private static void Main()
        {
            DoInitMessage();
            Context = new TheaterContext();
            Server = new FtpServer();
            InitializaServer();
            DisplayServerStatus();
            while (true)
            {
                PrintName();
                var command = Console.ReadLine();
                if (String.IsNullOrEmpty(command)) continue;
                try
                {
                    CommandsDictionary[command].Item1.Invoke();
                    Console.WriteLine(@"Done!");
                }
                catch (KeyNotFoundException)
                {
                    Console.WriteLine(@"Command not recognized.");
                    PrintCommands();
                }
                catch (NotImplementedException)
                {
                    Console.WriteLine(Environment.NewLine+@"Command not yet implemented."+Environment.NewLine);
                }
            }
        }

        private static void PrintName()
        {
            Console.Write(@"Slyther:$>");
        }

        private static void DoInitMessage()
        {
            const string line = "----------------------------------------";
            const string welcome = "Serpent Theater Server Command Console";
            Console.SetCursorPosition((Console.WindowWidth - line.Length) / 2, Console.CursorTop);
            Console.WriteLine(line);
            Console.SetCursorPosition((Console.WindowWidth - welcome.Length) / 2, Console.CursorTop);
            Console.WriteLine(welcome);
            Console.SetCursorPosition((Console.WindowWidth - line.Length) / 2, Console.CursorTop);
            Console.WriteLine(line);
        }
    }
}
