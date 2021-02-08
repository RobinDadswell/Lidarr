using System;
using System.Collections.Generic;
using System.Linq;
using TinyIoC;

namespace NzbDrone.Common.Composition
{
    public class Container : IContainer
    {
        private readonly TinyIoCContainer _container;
        private readonly HashSet<Type> _loadedTypes;

        public Container(TinyIoCContainer container, HashSet<Type> loadedTypes)
        {
            _container = container;
            _loadedTypes = loadedTypes;
            _container.Register<IContainer>(this);
        }

        public void Register<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            _container.Register<TService, TImplementation>();
        }

        public void Register<T>(T instance)
            where T : class
        {
            _container.Register<T>(instance);
        }

        public T Resolve<T>()
            where T : class
        {
            return _container.Resolve<T>();
        }

        public object Resolve(Type type)
        {
            return _container.Resolve(type);
        }

        public void AutoRegisterImplementations(Type registrationType)
        {
            var implementations = GetImplementations(registrationType)
                .Where(c => !c.IsGenericTypeDefinition)
                .ToList();

            if (implementations.Count == 0)
            {
                return;
            }

            if (implementations.Count == 1)
            {
                var impl = implementations.Single();
                RegisterSingleton(registrationType, impl);
            }
            else
            {
                RegisterAllAsSingleton(registrationType, implementations);
            }
        }

        public void AutoRegisterPluginImplementations(Type registrationType, IEnumerable<Type> implementationList)
        {
            _loadedTypes.Add(registrationType);
            _loadedTypes.UnionWith(implementationList);

            AutoRegisterImplementations(registrationType);
        }

        private void RegisterSingleton(Type service, Type implementation)
        {
            var factory = CreateSingletonImplementationFactory(implementation);

            // For Resolve and ResolveAll
            _container.Register(service, factory);

            // For ctor(IEnumerable<T>)
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(service);
            _container.Register(enumerableType, (c, p) =>
            {
                var instance = factory(c, p);
                var result = Array.CreateInstance(service, 1);
                result.SetValue(instance, 0);
                return result;
            });
        }

        public IEnumerable<T> ResolveAll<T>()
            where T : class
        {
            return _container.ResolveAll<T>();
        }

        private void RegisterAllAsSingleton(Type service, IEnumerable<Type> implementationList)
        {
            foreach (var implementation in implementationList)
            {
                var factory = CreateSingletonImplementationFactory(implementation);

                // For ResolveAll and ctor(IEnumerable<T>)
                _container.Register(service, factory, implementation.FullName);
            }
        }

        private Func<TinyIoCContainer, NamedParameterOverloads, object> CreateSingletonImplementationFactory(Type implementation)
        {
            const string singleImplPrefix = "singleImpl_";

            _container.Register(implementation, implementation, singleImplPrefix + implementation.FullName).AsSingleton();

            return (c, p) => _container.Resolve(implementation, singleImplPrefix + implementation.FullName);
        }

        public bool IsTypeRegistered(Type type)
        {
            return _container.CanResolve(type);
        }

        public IEnumerable<Type> GetImplementations(Type contractType)
        {
            return GetImplementations(_loadedTypes, contractType);
        }

        public static IEnumerable<Type> GetImplementations(IEnumerable<Type> types, Type contractType)
        {
            return types.Where(implementation =>
                               contractType.IsAssignableFrom(implementation) &&
                               !implementation.IsInterface &&
                               !implementation.IsAbstract);
        }
    }
}
