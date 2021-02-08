using System;
using System.Collections.Generic;

namespace NzbDrone.Common.Composition
{
    public interface IContainer
    {
        void Register<T>(T instance)
            where T : class;

        void Register<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class;
        T Resolve<T>()
            where T : class;
        object Resolve(Type type);
        IEnumerable<T> ResolveAll<T>()
            where T : class;
        void AutoRegisterImplementations(Type registrationType);
        void AutoRegisterPluginImplementations(Type registrationType, IEnumerable<Type> implementationList);
        bool IsTypeRegistered(Type type);

        IEnumerable<Type> GetImplementations(Type contractType);
    }
}
