using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DatabaseController;
using DatabaseController.Context;
using DatabaseController.Entities;
using Utilities;

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
        private readonly TheaterContext _context;
        private readonly OmdbApiHandler _omdbApiHandler;

        public DatabaseBuilder(TheaterContext context)
        {
            _context = context;
            _omdbApiHandler = new OmdbApiHandler(_context);
        }


        public void AddAllMissing(string path, DatabaseBuilderType type)
        {
            var files = Directory.GetFileSystemEntries(path).ToList();
            if (type == DatabaseBuilderType.Movies)
            {
                BasicLogger.Log("Adding all or any missing movies to the database.", Verbosity.Information);
                BasicLogger.Log("Year of each movie in parentheses is expected to appear on each movie's name. Directory will otherwise be ignored.", Verbosity.Information);
                BasicLogger.Log("Accurate Movie Name is expected for each directory. Movie will not be added otherwise.", Verbosity.Information);
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
                        var movie = _context.Movies.FirstOrDefault(x => x.Title == name && x.Year == year);
                        if (movie != null)
                        {
                            BasicLogger.Log("Conflicting entry: " + name + " (" + year + ")", Verbosity.Warning);
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
                                                "). Please confirm entry.", Verbosity.Warning);
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
                                                        "). Please confirm entry.", Verbosity.Warning);
                                    }
                                    catch (Exception ex)
                                    {
                                        BasicLogger.Log(ex.Message + " " + name + " (" + year + ")", Verbosity.Error);
                                        continue;
                                    }
                                }
                            }
                        }
                        var obtainedMovie = obtainedMovieTask.Result;
                        if (obtainedMovie == null)
                        {
                            BasicLogger.Log("Something went wrong with: " + name + " (" + year + ")", Verbosity.Error);
                            continue;
                        }
                        movie = _context.Movies.FirstOrDefault(x => x.ImdbId == obtainedMovie.ImdbId);
                        if (movie != null)
                        {
                            BasicLogger.Log("Conflicting entries: " + name + " (" + year + "), " + movie.Title + " (" +
                                            movie.Year + ")", Verbosity.Warning);
                            continue;
                        }
                        UpdateDatabaseWithWatchable(obtainedMovie, type, file);
                    }
                    else
                    {
                        BasicLogger.Log("Skipped: " + name, Verbosity.Warning);
                    }
                }
            }
        }

        public void AddMissingFromScope(string path, SeriesDatabaseBuilderScope scope = SeriesDatabaseBuilderScope.Series)
        {
            throw new NotImplementedException();
        }

        private void UpdateDatabaseWithWatchable(CompleteWatchable watchable, DatabaseBuilderType type, string path = "")
        {
            if (type == DatabaseBuilderType.Movies)
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
                                              "'s subtitles not added due to more than 2 files being present in the directory.", Verbosity.Error);
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
                    var databaseActor = _context.Actors.FirstOrDefault(x => x.Name == actor.Name);
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
                    var databaseWriter = _context.Writers.FirstOrDefault(x => x.Name == writer.Name);
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
                    var databaseDirector = _context.Directors.FirstOrDefault(x => x.Name == director.Name);
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
                    var databaseGenre = _context.Genres.FirstOrDefault(x => x.Name == genre.Name);
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
