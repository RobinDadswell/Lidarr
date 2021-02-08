using System.Collections.Generic;
using NzbDrone.Core.Plugins;

namespace Lidarr.Api.V1.System.Plugins
{
    public class PluginModule : LidarrV1Module
    {
        private readonly IPluginService _pluginService;

        public PluginModule(IPluginService pluginService)
            : base("system/plugins")
        {
            _pluginService = pluginService;

            Get("/installed", x => GetInstalledPlugins());
        }

        private List<PluginResource> GetInstalledPlugins()
        {
            return _pluginService.GetInstalledPlugins().ToResource();
        }
    }
}
