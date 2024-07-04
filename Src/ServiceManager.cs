using System;
using System.Collections.Generic;

namespace LuisLabs.SmartServiceLocator
{
    public class ServiceManager
    {
        private readonly Dictionary<Type, object> _services = new();
        public IEnumerable<object> RegisteredServices => _services.Values;

        public bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var serviceObj))
            {
                service = serviceObj as T;
                return true;
            }

            service = null;
            return false;
        }

        public T Get<T>() where T : class {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service)) {
                return service as T;
            }

            throw new InvalidOperationException($"Service of type {type.FullName} is not registered");
        }

        public ServiceManager Register<T>(T service) {
            var type = typeof(T);
            
            TryAddService(type, service);
            
            return this;
        }

        public ServiceManager Register(Type type, object service) {
            if (!type.IsInstanceOfType(service)) {
                throw new InvalidOperationException($"Service of type {type} is not an instance of {type}");
            }

            TryAddService(type, service);

            return this;
        }

        private void TryAddService(Type type, object service) {
            if (!_services.TryAdd(type, service)) {
                throw new InvalidOperationException($"Service of type {type} is already registered");
            }
        }

        internal void Unregister<T>()
        {
            var type = typeof(T);
            if (!_services.Remove(type)) {
                throw new InvalidOperationException($"Service of type {type.FullName} is not registered");
            }
        }
    }
}
