using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DatabaseDeployer.Database;
using DatabaseDeployer.Database.Models;
using Utilities.Utils;

namespace Serpent_Theater_Server
{
    public class DatabaseBuilder
    {
        public enum DatabaseBuilderType
        {
            Movies,
            Series
        }

        public enum SeriesDatabaseBuilderScope
        {
            Series,
            Season
        }

        private readonly string _path;
        private readonly DatabaseBuilderType _type;
        private readonly TheaterContext _context;
        private readonly OmdbApiHandler _omdbApiHandler;

        public DatabaseBuilder(string path, DatabaseBuilderType type, TheaterContext context)
        {
            _path = path;
            _type = type;
            _context = context;
            _omdbApiHandler = new OmdbApiHandler(_context);
        }

        public void AddAllMissing()
        {
            AddAllMissing(_path);
        }

        public void AddMissingFromScope(SeriesDatabaseBuilderScope scope = SeriesDatabaseBuilderScope.Series)
        {
            AddMissingFromScope(_path, scope);
        }

        public void AddAllMissing(string path)
        {
            var files = Directory.GetFileSystemEntries(path).ToList();
            if (_type == DatabaseBuilderType.Movies)
            {
                BasicLogger.Log("Adding all or any missing movies to the database.");
                BasicLogger.Log("Year of each movie in parentheses is expected to appear on each movie's name. Directory will otherwise be ignored.");
                BasicLogger.Log("Accurate Movie Name is expected for each directory. Movie will not be added otherwise.");
                foreach (var file in files)
                {
                    FileAttributes attr = File.GetAttributes(file);
                    if (!attr.HasFlag(FileAttributes.Directory))
                        continue;
                    string name = Path.GetFileName(file);
                    if (name == null)
                        continue;
                    if (name.Contains("[1080p]"))
                        name = name.Remove(name.IndexOf("[1080p]", StringComparison.Ordinal)).Trim();
                    if (name.Contains("(") && name.Contains(")"))
                    {
                        var year = name.Substring(name.LastIndexOf('(') + 1);
                        name = name.Remove(name.LastIndexOf('(')).Trim();
                        year = year.Remove(year.IndexOf(')')).Trim();
                        name = name.Replace("310 to Yuma", "3:10 to Yuma"); //Hardcoded, can't figure out any other way. Unique case.
                        var movie = Queryable.FirstOrDefault<Movie>(_context.Movies, x => x.Title == name && x.Year == year);
                        if (movie != null)
                        {
                            BasicLogger.Log("Conflicting entry: " + name + " (" + year + ")");
                            continue;
                        }
                        var obtainedMovieTask = _omdbApiHandler.GetRequestedMovie(name, year: year);
                        Movie obtainedMovieToCheck;
                        try
                        {
                            Task.WaitAll(obtainedMovieTask);
                            obtainedMovieToCheck = obtainedMovieTask.Result;
                            if (obtainedMovieToCheck == null)
                                throw new Exception();
                        }
                        catch (Exception)
                        {
                            obtainedMovieTask = _omdbApiHandler.GetRequestedMovie(name); //year might be causing trouble
                            try
                            {
                                Task.WaitAll(obtainedMovieTask);
                                obtainedMovieToCheck = obtainedMovieTask.Result;
                                if (obtainedMovieToCheck == null)
                                    throw new Exception();
                                BasicLogger.Log(name + " was searched for without the year (" + year +
                                                "). Please confirm entry.");
                            }
                            catch (Exception)
                            {
                                if (name.Contains(" and ") || name.Contains(" And "))
                                {
                                    name = name.Replace(" and ", "&");
                                    name = name.Replace(" And ", "&");
                                }
                                obtainedMovieTask = _omdbApiHandler.GetRequestedMovie(name, year: year);
                                try
                                {
                                    Task.WaitAll(obtainedMovieTask);
                                    obtainedMovieToCheck = obtainedMovieTask.Result;
                                    if (obtainedMovieToCheck == null)
                                        throw new Exception();
                                }
                                catch (Exception)
                                {
                                    if (name.Contains(" and ") || name.Contains(" And "))
                                    {
                                        name = name.Replace(" and ", "&");
                                        name = name.Replace(" And ", "&");
                                    }
                                    obtainedMovieTask = _omdbApiHandler.GetRequestedMovie(name);
                                    try
                                    {
                                        Task.WaitAll(obtainedMovieTask);
                                        BasicLogger.Log(name + " was searched for without the year (" + year +
                                                        "). Please confirm entry.");
                                    }
                                    catch (Exception ex)
                                    {
                                        BasicLogger.Log(ex.Message + " " + name + " (" + year + ")");
                                        continue;
                                    }
                                }
                            }
                        }
                        var obtainedMovie = obtainedMovieTask.Result;
                        if (obtainedMovie == null)
                        {
                            BasicLogger.Log("Something went wrong with: " + name + " (" + year + ")");
                            continue;
                        }
                        movie = Queryable.FirstOrDefault<Movie>(_context.Movies, x => x.ImdbId == obtainedMovie.ImdbId);
                        if (movie != null)
                        {
                            BasicLogger.Log("Conflicting entries: " + name + " (" + year + "), " + movie.Title + " (" +
                                            movie.Year + ")");
                            continue;
                        }
                        UpdateDatabaseWithWatchable(obtainedMovie, file);
                    }
                    else
                    {
                        BasicLogger.Log("Skipped: " + name);
                    }
                }
            }
        }

        public void AddMissingFromScope(string path, SeriesDatabaseBuilderScope scope = SeriesDatabaseBuilderScope.Series)
        {
            throw new NotImplementedException();
        }

        private void UpdateDatabaseWithWatchable(CompleteWatchable watchable, string path = "")
        {
            if (_type == DatabaseBuilderType.Movies)
            {
                var files = Directory.GetFileSystemEntries(path).ToList();
                var actors = new List<Actor>(watchable.Actors);
                var writers = new List<Writer>(watchable.Writers);
                var directors = new List<Director>(watchable.Directors);
                var genres = new List<Genre>(watchable.Genres);
                watchable.Actors.Clear();
                watchable.Writers.Clear();
                watchable.Directors.Clear();
                watchable.Genres.Clear();
                _context.Movies.Add((Movie)watchable);
                _context.SaveChanges();
                foreach (var file in files)
                {
                    if (file.EndsWith("srt"))
                    {
                        if (files.Count > 2)
                            BasicLogger.Log(watchable.Title +
                                              "'s subtitles not added due to more than 2 files being present in the directory.");
                        else
                        {
                            var subs = new Subtitles
                            {
                                Language = SubtitleLanguage.English,
                                Path = file,
                                Watchable = watchable
                            };
                            _context.Subtitles.Add(subs);
                            _context.SaveChanges();
                        }
                    }
                    else if (file.EndsWith("mp4") || file.EndsWith("avi") || file.EndsWith("mkv") || file.EndsWith("wmv"))
                    {
                        watchable.Path = file;
                        _context.SaveChanges();
                    }
                }
                foreach (var actor in actors)
                {
                    var databaseActor = Queryable.FirstOrDefault<Actor>(_context.Actors, x => x.Name == actor.Name);
                    if (databaseActor == null)
                    {
                        actor.Watchables = new List<Watchable>{watchable};
                        _context.Actors.Add(actor);
                    }
                    else
                    {
                        if(databaseActor.Watchables == null)
                            databaseActor.Watchables = new List<Watchable>();
                        if (!databaseActor.Watchables.Contains(watchable))
                            databaseActor.Watchables.Add(watchable);
                    }
                    _context.SaveChanges();
                }
                foreach (var writer in writers)
                {
                    var databaseWriter = Queryable.FirstOrDefault<Writer>(_context.Writers, x => x.Name == writer.Name);
                    if (databaseWriter == null)
                    {
                        writer.Watchables = new List<Watchable> { watchable };
                        _context.Writers.Add(writer);
                    }
                    else
                    {
                        if (databaseWriter.Watchables == null)
                            databaseWriter.Watchables = new List<Watchable>();
                        if (!databaseWriter.Watchables.Contains(watchable))
                            databaseWriter.Watchables.Add(watchable);
                    }
                    _context.SaveChanges();
                }
                foreach (var director in directors)
                {
                    var databaseDirector = Queryable.FirstOrDefault<Director>(_context.Directors, x => x.Name == director.Name);
                    if (databaseDirector == null)
                    {
                        director.Watchables = new List<Watchable> { watchable };
                        _context.Directors.Add(director);
                    }
                    else
                    {
                        if (databaseDirector.Watchables == null)
                            databaseDirector.Watchables = new List<Watchable>();
                        if(!databaseDirector.Watchables.Contains(watchable))
                            databaseDirector.Watchables.Add(watchable);
                    }
                    _context.SaveChanges();
                }
                foreach (var genre in genres)
                {
                    var databaseGenre = Queryable.FirstOrDefault<Genre>(_context.Genres, x => x.Name == genre.Name);
                    if (databaseGenre == null)
                    {
                        genre.Watchables = new List<Watchable> { watchable };
                        _context.Genres.Add(genre);
                    }
                    else
                    {
                        if (databaseGenre.Watchables == null)
                            databaseGenre.Watchables = new List<Watchable>();
                        if (!databaseGenre.Watchables.Contains(watchable))
                            databaseGenre.Watchables.Add(watchable);
                    }
                    _context.SaveChanges();
                }
            }
        }
    }
}
