using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class FixDeemixDelayProfile : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public FixDeemixDelayProfile(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"UPDATE DelayProfiles
                                 SET Items = REPLACE(Items, 'usenet', 'UsenetDownloadProtocol')");
                mapper.Execute(@"UPDATE DelayProfiles
                                 SET Items = REPLACE(Items, 'torrent', 'TorrentDownloadProtocol')");
                mapper.Execute(@"UPDATE DelayProfiles
                                 SET Items = REPLACE(Items, 'deemix', 'DeemixDownloadProtocol')");
            }
        }
    }
}
