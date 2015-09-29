using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using DatabaseController.Entities;
using DatabaseController.Interfaces;
using Utilities;

namespace Serpent_Theater_Server
{
    public class OmdbApiHandler
    {
        public enum PlotVersion
        {
            Short,
            Full,
            Both
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
        private readonly MemoryStream _defaultAvatar;

        public OmdbApiHandler(IActorsRepository actorsRepository, IContentPathsRepository contentPathsRepository,
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

            var image = new Bitmap(Properties.Resources.default_avatar);
            _defaultAvatar = new MemoryStream();
            image.Save(_defaultAvatar, System.Drawing.Imaging.ImageFormat.Png);
        }

        //Returns a Movie based on the information provided. If no movie was found, returns null. No dependencies are modified or added to the TheaterContext.
        public async Task<Movie> GetRequestedMovie(string name = "-1", string imdbId = "-1", string year = "-1", PlotVersion plotVersion = PlotVersion.Both)
        {
            if (name == "-1" && imdbId == "-1")
                throw new InvalidOperationException("While name and ImdbId are both optional, at least one must be provided.");
            var movieToReturn = new Movie();
            var searchBy = new[] {"", ""};
            if (name == "-1")
            {
                searchBy[0] = "i";
                searchBy[1] = imdbId;
                year = "-1";
            }
            else
            {
                searchBy[0] = "t";
                searchBy[1] = name;
            }
            var valuesList = new List<Dictionary<string, string>>();
            if (year == "-1")
            {
                if (plotVersion == PlotVersion.Short || plotVersion == PlotVersion.Both)
                {
                    var values = new Dictionary<string, string>
                    {
                        { searchBy[0], searchBy[1] },
                        { "r", "xml"},
                        { "plot", "short"}
                    };
                    valuesList.Add(values);
                }
                if (plotVersion == PlotVersion.Full || plotVersion == PlotVersion.Both)
                {
                    var values = new Dictionary<string, string>
                    {
                        { searchBy[0], searchBy[1] },
                        { "r", "xml"},
                        { "plot", "full"}
                    };
                    valuesList.Add(values);
                }
            }
            else
            {
                if (plotVersion == PlotVersion.Short || plotVersion == PlotVersion.Both)
                {
                    var values = new Dictionary<string, string>
                    {
                        { searchBy[0], searchBy[1] },
                        { "y", year },
                        { "r", "xml"},
                        { "plot", "short"}
                    };
                    valuesList.Add(values);
                }
                if (plotVersion == PlotVersion.Full || plotVersion == PlotVersion.Both)
                {
                    var values = new Dictionary<string, string>
                    {
                        { searchBy[0], searchBy[1] },
                        { "y", year },
                        { "r", "xml"},
                        { "plot", "full"}
                    };
                    valuesList.Add(values);
                }
            }
            var client = new HttpClient();
            
            var requestUrls = new List<string>();
            foreach (var valuesDictionary in valuesList)
            {
                var omdbUrl = "http://www.omdbapi.com/?";
                for (var i = 0; i < valuesList.ElementAt(0).Count; i++)
                {
                    omdbUrl += (i == 0 ? "" : "&") + valuesDictionary.Keys.ElementAt(i) + "=" + valuesDictionary[valuesDictionary.Keys.ElementAt(i)];
                }
                requestUrls.Add(omdbUrl);
            }
            var responseNodes = new List<XmlNode>();
            for(int i = 0; i < requestUrls.Count; i++)
            {
                var responseString = await client.GetStringAsync(requestUrls.ElementAt(i));
                var doc = new XmlDocument();
                doc.LoadXml(responseString);
                var rootNode = doc.SelectSingleNode("root");
                if (rootNode == null)
                {
                    BasicLogger.Log("Something went wrong while obtaining rootnode from document for " + requestUrls.ElementAt(i).Clone(), Verbosity.Error);
                    requestUrls.Remove(requestUrls.ElementAt(i));
                    i = -1;
                    continue;
                }
                if (rootNode.Attributes != null && rootNode.Attributes.GetNamedItem("response").Value == "False")
                {
                    BasicLogger.Log("Invalid Identifier for " + requestUrls.ElementAt(i), Verbosity.Error);
                    requestUrls.Remove(requestUrls.ElementAt(i));
                    i = -1;
                    continue;
                }
                var docNode = rootNode.SelectSingleNode("movie");
                if (docNode == null)
                {
                    BasicLogger.Log("Empty XML for " + requestUrls.ElementAt(i), Verbosity.Error);
                    requestUrls.Remove(requestUrls.ElementAt(i));
                    i = -1;
                    continue;
                }
                responseNodes.Add(docNode);
            }
            for (var i = 0; i < responseNodes.Count; i++)
            {
                var xmlAttributeCollection = responseNodes.ElementAt(i).Attributes;
                if (xmlAttributeCollection == null) continue;
                if (xmlAttributeCollection.GetNamedItem("type").Value != "movie")
                {
                    BasicLogger.Log("Element "+xmlAttributeCollection.GetNamedItem("imdbID").Value+" is not a movie!", Verbosity.Warning);
                    continue;
                }
                if (i == 0)
                {
                    movieToReturn.Title = xmlAttributeCollection.GetNamedItem("title").Value;
                    movieToReturn.ImdbId = xmlAttributeCollection.GetNamedItem("imdbID").Value;
                    movieToReturn.ImdbRating =
                        decimal.Parse(xmlAttributeCollection.GetNamedItem("imdbRating").Value);
                    var runtime = xmlAttributeCollection.GetNamedItem("runtime").Value;
                    if (runtime.Contains("min"))
                        runtime = runtime.Remove(runtime.IndexOf("min", StringComparison.Ordinal)).Trim();
                    movieToReturn.RunTime = short.Parse(runtime);
                    movieToReturn.Language = xmlAttributeCollection.GetNamedItem("language").Value;
                    movieToReturn.Rating = xmlAttributeCollection.GetNamedItem("rated").Value;
                    movieToReturn.Year = xmlAttributeCollection.GetNamedItem("year").Value;
                    movieToReturn.ReleaseDate =
                        DateTime.Parse(xmlAttributeCollection.GetNamedItem("released").Value);
                    movieToReturn.Actors = new List<Actor>();
                    var actorsString = xmlAttributeCollection.GetNamedItem("actors").Value;
                    while (actorsString.Contains("("))
                    {
                        actorsString = actorsString.Remove(actorsString.IndexOf("(", StringComparison.Ordinal),
                            actorsString.IndexOf(")", StringComparison.Ordinal)+1 - actorsString.IndexOf("(", StringComparison.Ordinal));
                    }
                    var actorsList = actorsString.Split(',').ToList();
                    foreach (var actor in actorsList)
                    {
                        if (actor.Trim() == "")
                            continue;
                        var act = _actorsRepository.Query(x => x.Name == actor.Trim()).FirstOrDefault() ?? new Actor
                        {
                            Name = actor.Trim()
                        };
                        act.Portrait = _defaultAvatar.ToArray();
                        movieToReturn.Actors.Add(act);
                    }
                    movieToReturn.Directors = new List<Director>();
                    var directorsString = xmlAttributeCollection.GetNamedItem("director").Value;
                    while (directorsString.Contains("("))
                    {
                        directorsString = directorsString.Remove(directorsString.IndexOf("(", StringComparison.Ordinal),
                            directorsString.IndexOf(")", StringComparison.Ordinal)+1 - directorsString.IndexOf("(", StringComparison.Ordinal));
                    }
                    var directorsList = directorsString.Split(',').ToList();
                    foreach (var director in directorsList)
                    {
                        if (director.Trim() == "")
                            continue;
                        var dir = _directorsRepository.Query(x => x.Name == director.Trim()).FirstOrDefault() ?? new Director
                        {
                            Name = director.Trim()
                        };
                        dir.Portrait = _defaultAvatar.ToArray();
                        movieToReturn.Directors.Add(dir);
                    }
                    movieToReturn.Writers = new List<Writer>();
                    var writersString = xmlAttributeCollection.GetNamedItem("writer").Value;
                    while (writersString.Contains("("))
                    {
                        writersString = writersString.Remove(writersString.IndexOf("(", StringComparison.Ordinal),
                            writersString.IndexOf(")", StringComparison.Ordinal)+1 - writersString.IndexOf("(", StringComparison.Ordinal));
                    }
                    var writersList = writersString.Split(',').ToList();
                    foreach (var writer in writersList)
                    {
                        if (writer.Trim() == "")
                            continue;
                        var wri = _writersRepository.Query(x => x.Name == writer.Trim()).FirstOrDefault() ?? new Writer
                        {
                            Name = writer.Trim()
                        };
                        wri.Portrait = _defaultAvatar.ToArray();
                        movieToReturn.Writers.Add(wri);
                    }
                    movieToReturn.Genres = new List<Genre>();
                    var genresString = xmlAttributeCollection.GetNamedItem("genre").Value;
                    while (genresString.Contains("("))
                    {
                        genresString = genresString.Remove(genresString.IndexOf("(", StringComparison.Ordinal),
                            genresString.IndexOf(")", StringComparison.Ordinal)+1 - genresString.IndexOf("(", StringComparison.Ordinal));
                    }
                    var genresList = genresString.Split(',').ToList();
                    foreach (var genre in genresList)
                    {
                        var gen = _genresRepository.Query(x => x.Name == genre.Trim()).FirstOrDefault() ?? new Genre
                        {
                            Name = genre.Trim()
                        };
                        movieToReturn.Genres.Add(gen);
                    }
                    var stringImage =
                        client.GetStreamAsync(xmlAttributeCollection.GetNamedItem("poster").Value);
                    await stringImage;
                    var reader = new BinaryReader(stringImage.Result);
                    var imagebytes = reader.ReadAllBytes();
                    movieToReturn.Poster = imagebytes;
                }
                if (requestUrls.ElementAt(i).Contains("plot=short"))
                    movieToReturn.ShortPlot = responseNodes.ElementAt(i).Attributes.GetNamedItem("plot").Value;
                else
                    movieToReturn.LongPlot = responseNodes.ElementAt(i).Attributes.GetNamedItem("plot").Value;
            }
            return (String.IsNullOrEmpty(movieToReturn.ImdbId) ? null : movieToReturn);
        }

        public Task<Series> GetRequestedSeries()
        {
            throw new NotImplementedException();
        }

        public Task<Episode> GetRequestedEpisode()
        {
            throw new NotImplementedException();
        }
    }
}
