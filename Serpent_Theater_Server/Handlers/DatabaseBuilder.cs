using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DatabaseController.Entities;
using DatabaseController.Interfaces;
using Utilities;

namespace Serpent_Theater_Server.Handlers
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
        private readonly OmdbApiHandler _omdbApiHandler;

        public DatabaseBuilder(IActorsRepository actorsRepository, IContentPathsRepository contentPathsRepository, 
            IDirectorsRepository directorsRepository, IEpisodesRepository episodesRepository, 
            IGenresRepository genresRepository, ISeasonsRepository seasonsRepository, ISeriesRepository seriesRepository, 
            ISubtitlesRepository subtitlesRepository, IWritersRepository writersRepository, 
            IMoviesRepository moviesRepository, OmdbApiHandler omdbApiHandler)
        {
            _actorsRepository = actorsRepository;
            _contentPathsRepository = contentPathsRepository;
            _directorsRepository = directorsRepository;
            _episodesRepository = episodesRepository;
            _genresRepository = genresRepository;
            _seasonsRepository = seasonsRepository;
            _seriesRepository = seriesRepository;
            _subtitlesRepository = subtitlesRepository;
            _writersRepository = writersRepository;
            _moviesRepository = moviesRepository;
            _omdbApiHandler = omdbApiHandler;
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
                        var movie = _moviesRepository.Query(x => x.Title == name && x.Year == year).FirstOrDefault();
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
                        movie = _moviesRepository.Query(x => x.ImdbId == obtainedMovie.ImdbId).FirstOrDefault();
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
                _moviesRepository.Create((Movie)watchable);
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
                            _subtitlesRepository.Create(subs);
                        }
                    }
                    else if (file.EndsWith("mp4") || file.EndsWith("avi") || file.EndsWith("mkv") || file.EndsWith("wmv"))
                    {
                        watchable.Path = file;
                        _moviesRepository.Update((Movie)watchable);
                    }
                }
                foreach (var actor in actors)
                {
                    var databaseActor = _actorsRepository.Query(x => x.Name == actor.Name).FirstOrDefault();
                    if (databaseActor == null)
                    {
                        actor.Watchables = new List<Watchable>{watchable};
                        _actorsRepository.Create(actor);
                    }
                    else
                    {
                        if(databaseActor.Watchables == null)
                            databaseActor.Watchables = new List<Watchable>();
                        if (databaseActor.Watchables.Contains(watchable)) continue;
                        databaseActor.Watchables.Add(watchable);
                        _actorsRepository.Update(databaseActor);
                    }
                }
                foreach (var writer in writers)
                {
                    var databaseWriter = _writersRepository.Query(x => x.Name == writer.Name).FirstOrDefault();
                    if (databaseWriter == null)
                    {
                        writer.Watchables = new List<Watchable> { watchable };
                        _writersRepository.Create(writer);
                    }
                    else
                    {
                        if (databaseWriter.Watchables == null)
                            databaseWriter.Watchables = new List<Watchable>();
                        if (databaseWriter.Watchables.Contains(watchable)) continue;
                        databaseWriter.Watchables.Add(watchable);
                        _writersRepository.Update(databaseWriter);
                    }
                }
                foreach (var director in directors)
                {
                    var databaseDirector = _directorsRepository.Query(x => x.Name == director.Name).FirstOrDefault();
                    if (databaseDirector == null)
                    {
                        director.Watchables = new List<Watchable> { watchable };
                        _directorsRepository.Create(director);
                    }
                    else
                    {
                        if (databaseDirector.Watchables == null)
                            databaseDirector.Watchables = new List<Watchable>();
                        if (databaseDirector.Watchables.Contains(watchable)) continue;
                        databaseDirector.Watchables.Add(watchable);
                        _directorsRepository.Update(databaseDirector);
                    }
                }
                foreach (var genre in genres)
                {
                    var databaseGenre = _genresRepository.Query(x => x.Name == genre.Name).FirstOrDefault();
                    if (databaseGenre == null)
                    {
                        genre.Watchables = new List<Watchable> { watchable };
                        _genresRepository.Create(genre);
                    }
                    else
                    {
                        if (databaseGenre.Watchables == null)
                            databaseGenre.Watchables = new List<Watchable>();
                        if (databaseGenre.Watchables.Contains(watchable)) continue;
                        databaseGenre.Watchables.Add(watchable);
                        _genresRepository.Update(databaseGenre);
                    }
                }
            }
        }
    }
}
