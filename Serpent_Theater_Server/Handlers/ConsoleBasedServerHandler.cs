using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using DatabaseController.Entities;
using DatabaseController.Interfaces;
using Microsoft.Win32;
using Serpent_Theater_Server.FTP;

namespace Serpent_Theater_Server.Handlers
{
    public class ConsoleBasedServerHandler
    {
        public Dictionary<string, Tuple<Action, string>> CommandsDictionary;
        private static FtpServer _ftpServer;
        private readonly DatabaseBuilder _databaseBuilder;
        private readonly IActorsRepository _actorsRepository;
        private readonly IContentPathsRepository _contentPathsRepository;
        private readonly IDirectorsRepository _directorsRepository;
        private readonly IEpisodesRepository _episodesRepository;
        private readonly IGenresRepository _genresRepository;
        private readonly ISeasonsRepository _seasonsRepository;
        private readonly ISeriesRepository _seriesRepository;
        private readonly ISubtitlesRepository _subtitlesRepository;
        private readonly IWritersRepository _writersRepository;
        private readonly IMoviesRepository _moviesRepository;

        public ConsoleBasedServerHandler(FtpServer ftpServer, DatabaseBuilder databaseBuilder, IActorsRepository actorsRepository, 
            IContentPathsRepository contentPathsRepository,
            IDirectorsRepository directorsRepository, IEpisodesRepository episodesRepository,
            IGenresRepository genresRepository, ISeasonsRepository seasonsRepository, ISeriesRepository seriesRepository,
            ISubtitlesRepository subtitlesRepository, IWritersRepository writersRepository, IMoviesRepository moviesRepository)
        {
            _actorsRepository = actorsRepository;
            _contentPathsRepository = contentPathsRepository;
            _directorsRepository = directorsRepository;
            _episodesRepository = episodesRepository;
            _genresRepository = genresRepository;
            _moviesRepository = moviesRepository;
            _seasonsRepository = seasonsRepository;
            _seriesRepository = seriesRepository;
            _subtitlesRepository = subtitlesRepository;
            _writersRepository = writersRepository;

            _ftpServer = ftpServer;
            _databaseBuilder = databaseBuilder;
            CommandsDictionary = new Dictionary<string, Tuple<Action, string>>
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
                {"testImage", new Tuple<Action, string>(TestImage, "Copies the selected movie's poster to the current user's desktop.")}
            };
        }

        public void TestImage()
        {
            Movie movie;
            do
            {
                Console.Write(@"Movie Name:");
                string movieName = Console.ReadLine();
                movie = _moviesRepository.Query(x => x.Title == movieName).FirstOrDefault();
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
                           new FileStream(path + "\\Desktop\\" + movie.ImdbId + ".jpg", FileMode.Create,
                                                    FileAccess.Write);
                            fileStream.Write(poster, 0, poster.Length);
                            fileStream.Close();
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(@"Exception caught in process: {0}",
                                          exception);
                    }
                }
                else
                {
                    Console.WriteLine(@"Invalid movie name!");
                }
            } while (movie == null);
        }

        public void ListMoviesDirectories()
        {
            var moviesList = _contentPathsRepository.Query(x => x.ContentType == ContentType.Movies).ToList();
            if (!moviesList.Any())
            {
                Console.WriteLine(@"No movie directories stored!");
                return;
            }
            Console.WriteLine(Environment.NewLine + @"List of Movie Directories" + Environment.NewLine);
            var commandsList = moviesList.Select(keyValuePair => new[] { keyValuePair.Id.ToString(), "- " + keyValuePair.Path + "\n" }).ToList();
            Console.WriteLine(Utilities.Utilities.PadElementsInLines(commandsList));
        }

        public void AddAllOrNewMovies()
        {
            var watch = new Stopwatch();
            watch.Start();
            _databaseBuilder.AddAllMissing("", DatabaseBuilder.DatabaseBuilderType.Movies);
            watch.Stop();
            Console.WriteLine(watch.Elapsed.ToString());
            Console.ReadLine();
            throw new NotImplementedException();
        }

        public void ExitServer()
        {
            if (_ftpServer.IsActive())
                ShutDownServer();
            Environment.Exit(0);
        }

        public void RemoveFromMoviesDirectories()
        {
            Console.WriteLine(@"Enter the path ID:");
            try
            {
                var id = Int64.Parse(Console.ReadLine());
                _contentPathsRepository.Delete(id);
            }
            catch (Exception)
            {
                Console.WriteLine(@"Couldn't delete path.");
                return;
            }
            Console.WriteLine(@"Path deleted!");
        }

        public void AddToMoviesDirectories()
        {
            Console.WriteLine(@"Enter the path ID:");
            var path = Console.ReadLine();
            var contentPath = new ContentPath
            {
                ContentType = ContentType.Movies,
                Path = path
            };
            if (_contentPathsRepository.Query(x => x.Path == path).FirstOrDefault() != null)
            {
                Console.WriteLine(@"Path already exists!");
                return;
            }
            _contentPathsRepository.Create(contentPath);
            Console.WriteLine(@"Path added!");
        }

        public void DisplayServerStatus()
        {
            Console.WriteLine(_ftpServer.IsActive() ? @"Server running at port 21" : @"Server not running.");
        }

        public void RestartServer()
        {
            ShutDownServer();
            InitializaServer();
        }

        public void ShutDownServer()
        {
            if (_ftpServer.IsActive())
            {
                _ftpServer.Stop();
                Console.WriteLine(@"Server shut down.");
            }
            else
            {
                Console.WriteLine(@"Server is not running!");
            }
        }

        public void InitializaServer()
        {
            if (!_ftpServer.IsActive())
            {
                _ftpServer.Start();
                Console.WriteLine(@"Server initialized.");
            }
            else
            {
                Console.WriteLine(@"Server is already running!");
            }
        }

        public void PrintCommands()
        {
            Console.WriteLine(Environment.NewLine + @"List of Available Commands:" + Environment.NewLine);
            var commandsList = CommandsDictionary.Select(keyValuePair => new[] { keyValuePair.Key, "- " + keyValuePair.Value.Item2 + "\n" }).ToList();
            Console.WriteLine(Utilities.Utilities.PadElementsInLines(commandsList));
        }

        public void DisableAutoStartOnLogin()
        {
            var key = Registry.CurrentUser.OpenSubKey(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (key != null && key.GetValue("SerpentTheaterServer") != null)
            {
                key.DeleteValue("SerpentTheaterServer");
            }
        }

        public void SetAutoStartOnLogin()
        {
            var key = Registry.CurrentUser.OpenSubKey(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (key != null && (key.GetValue("SerpentTheaterServer") == null ||
                (string)key.GetValue("SerpentTheaterServer") != Assembly.GetEntryAssembly().Location))
            {
                key.SetValue("SerpentTheaterServer", Assembly.GetEntryAssembly().Location);
            }
        }
    }
}
