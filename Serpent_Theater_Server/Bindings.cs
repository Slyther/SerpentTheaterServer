using DatabaseController.Context;
using DatabaseController.Interfaces;
using DatabaseController.Repositories;
using Ninject.Modules;
using Serpent_Theater_Server.FTP;
using Serpent_Theater_Server.Handlers;

namespace Serpent_Theater_Server
{
    class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<ConsoleBasedServerHandler>().ToSelf();
            Bind<TheaterContext>().ToSelf();
            Bind<FtpServer>().ToSelf();
            Bind<DatabaseBuilder>().ToSelf();
            Bind<IActorsRepository>().To<ActorsRepository>();
            Bind<IContentPathsRepository>().To<ContentPathsRepository>();
            Bind<IDirectorsRepository>().To<DirectorsRepository>();
            Bind<IEpisodesRepository>().To<EpisodesRepository>();
            Bind<IGenresRepository>().To<GenresRepository>();
            Bind<IMoviesRepository>().To<MoviesRepository>();
            Bind<ISeasonsRepository>().To<SeasonsRepository>();
            Bind<ISeriesRepository>().To<SeriesRepository>();
            Bind<ISubtitlesRepository>().To<SubtitlesRepository>();
            Bind<IWritersRepository>().To<WritersRepository>();
        }
    }
}
