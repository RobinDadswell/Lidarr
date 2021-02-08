using Lidarr.Http;
using NzbDrone.Core.Profiles.Delay;

namespace Lidarr.Api.V1.Profiles.Delay
{
    public class DelayProfileSchemaModule : LidarrRestModule<DelayProfileResource>
    {
        private readonly IDelayProfileService _profileService;

        public DelayProfileSchemaModule(IDelayProfileService profileService)
            : base("/delayprofile/schema")
        {
            _profileService = profileService;
            GetResourceSingle = GetSchema;
        }

        private DelayProfileResource GetSchema()
        {
            return _profileService.GetDefaultProfile().ToResource();
        }
    }
}
