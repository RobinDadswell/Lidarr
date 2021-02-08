using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lidarr.Http;
using Nancy.Bootstrapper;
using NLog;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;
using NzbDrone.SignalR;

namespace NzbDrone.Host
{
    public class MainAppContainerBuilder : ContainerBuilderBase
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(MainAppContainerBuilder));

        public static IContainer BuildContainer(StartupContext args)
        {
            var assemblies = new List<string>
                             {
                                 "Lidarr.Host",
                                 "Lidarr.Core",
                                 "Lidarr.SignalR",
                                 "Lidarr.Api.V1",
                                 "Lidarr.Http"
                             };

            return new MainAppContainerBuilder(args, assemblies).Container;
        }

        public static void LoadPlugins(IContainer container, string pluginFolder)
        {
            foreach (var folder in Directory.GetDirectories(pluginFolder, "Lidarr.Plugin.*"))
            {
                var assemblyFile = Directory.GetFiles(folder, "Lidarr.Plugin.*.dll");
                LoadPlugin(container, assemblyFile.Single());
            }
        }

        private static void LoadPlugin(IContainer container, string assemblyFile)
        {
            Logger.Info($"Loading plugin {assemblyFile}");

            var pluginTypes = GetAssemblyTypes(assemblyFile);

            var typeCount = 0;

            foreach (var contract in GetAllContracts(pluginTypes))
            {
                Logger.Trace($"Found contract {contract}");
                var implementations = Common.Composition.Container.GetImplementations(pluginTypes, contract).ToList();

                var existing = container.GetImplementations(contract).ToList();
                var toRegister = implementations
                    .Where(x => !x.IsGenericTypeDefinition)
                    .Except(existing)
                    .ToList();

                if (toRegister.Any())
                {
                    Logger.Trace($"Registering {toRegister.Count} implementations for contract {contract}");
                    typeCount += toRegister.Count;
                    container.AutoRegisterPluginImplementations(contract, toRegister);
                }
            }

            Logger.Trace($"Registered {typeCount} new implementations");
        }

        private MainAppContainerBuilder(StartupContext args, List<string> assemblies)
        : base(args, assemblies)
        {
            AutoRegisterImplementations<MessageHub>();

            Container.Register<INancyBootstrapper, LidarrBootstrapper>();

            if (OsInfo.IsWindows)
            {
                Container.Register<INzbDroneServiceFactory, NzbDroneServiceFactory>();
            }
            else
            {
                Container.Register<INzbDroneServiceFactory, DummyNzbDroneServiceFactory>();
            }
        }
    }
}
