using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseController.Interfaces;

namespace DatabaseController.Handlers
{
    public class RepositoriesHandler
    {
        public IActorsRepository ActorsRepository;
        public IContentPathsRepository ContentPathsRepository;
        public IDirectorsRepository DirectorsRepository;
        public IEpisodesRepository EpisodesRepository;
        public IGenresRepository GenresRepository;
        public ISeasonsRepository SeasonsRepository;
        public ISeriesRepository SeriesRepository;
        public ISubtitlesRepository SubtitlesRepository;
        public IWritersRepository WritersRepository;
        public IMoviesRepository MoviesRepository;

        public RepositoriesHandler(IActorsRepository actorsRepository, IContentPathsRepository contentPathsRepository,
            IDirectorsRepository directorsRepository, IEpisodesRepository episodesRepository,
            IGenresRepository genresRepository, ISeasonsRepository seasonsRepository, ISeriesRepository seriesRepository,
            ISubtitlesRepository subtitlesRepository, IWritersRepository writersRepository, IMoviesRepository moviesRepository)
        {
            ActorsRepository = actorsRepository;
            ContentPathsRepository = contentPathsRepository;
            DirectorsRepository = directorsRepository;
            EpisodesRepository = episodesRepository;
            GenresRepository = genresRepository;
            MoviesRepository = moviesRepository;
            SeasonsRepository = seasonsRepository;
            SeriesRepository = seriesRepository;
            SubtitlesRepository = subtitlesRepository;
            WritersRepository = writersRepository;
        }
    }
}
