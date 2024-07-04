using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEditor;

namespace LuisLabs.SmartServiceLocator
{
    public class ServiceLocator : MonoBehaviour
    {
        private static ServiceLocator _global;
        private static Dictionary<Scene, ServiceLocator> _sceneContainers;
        private static List<GameObject> _tmpSceneGameObjects;
        private readonly ServiceManager _serviceManger = new();

        private const string GlobalServiceLocatorName = "ServiceLocator [Global]";
        private const string SceneServiceLocatorName = "ServiceLocator [Scene]";

        public IEnumerable<object> RegisteredServices => _serviceManger.RegisteredServices;
        
        internal void ConfigureAsGlobal(bool dontDestroyOnLoad) {
            if (_global == this) {
                Debug.LogWarning("ServiceLocator is already configured as global");
            } else if (_global != null) {
                Debug.LogError("Another Service Locator is already configured as global");
            } else {
                _global = this;
                if (dontDestroyOnLoad) {
                    DontDestroyOnLoad(gameObject);
                }
            }
        }

        internal void ConfigureAsScene() {
            var scene = gameObject.scene;

            _sceneContainers ??= new Dictionary<Scene, ServiceLocator>();

            if (_sceneContainers.ContainsKey(scene)) {
                Debug.LogWarning("ServiceLocator is already configured for this scene");
            } else {
                _sceneContainers.Add(scene, this);
            }
        }

        public static ServiceLocator Global
        {
            get
            {
                if (_global != null)
                {
                    return _global;
                }

                var globalBootstrap = FindFirstObjectByType<ServiceLocatorGlobalBootstrapper>();
                if (globalBootstrap != null) {
                    globalBootstrap.BootstrapOnDemand();
                    return _global;
                }

                var container = new GameObject(GlobalServiceLocatorName, typeof(ServiceLocator));
                container.AddComponent<ServiceLocatorGlobalBootstrapper>().BootstrapOnDemand();

                return _global;
            }
        }

        public static ServiceLocator For(MonoBehaviour monoBehaviour) {
            return monoBehaviour.GetComponentInParent<ServiceLocator>() ?? throw new ArgumentException("ServiceLocator not found in hierarchy");
        }

        public static ServiceLocator ForSceneOf(MonoBehaviour monoBehaviour) {
            var scene = monoBehaviour.gameObject.scene;

            if (_sceneContainers.TryGetValue(scene, out var container) && container != null) {
                return container;
            }

            _tmpSceneGameObjects.Clear();
            scene.GetRootGameObjects(_tmpSceneGameObjects);

            foreach (var item in _tmpSceneGameObjects.Where(go => go.GetComponent<ServiceLocatorSceneBootstrapper>() != null)) {
                if (item.TryGetComponent(out ServiceLocatorSceneBootstrapper bootstrapper) && bootstrapper.Container != monoBehaviour) {
                    bootstrapper.BootstrapOnDemand();
                    return bootstrapper.Container;
                }
            }

            throw new ArgumentException("ServiceLocator not found in hierarchy");
        }

        public ServiceLocator Register<T>(T service) {
            _serviceManger.Register(service);
            return this;
        }

        public ServiceLocator Register<T>(Type type, object service) {
            _serviceManger.Register(type, service);
            return this;
        }

        public ServiceLocator Get<T>(out T service) where T : class {
            if (TryGet(out service)) return this;

            if (TryGetNextInHierarchy(out var container)) {
                container.Get(out service);
            }

            throw new ArgumentException($"Service of type {typeof(T).FullName} is not registered");
        }

        public bool TryGet<T>(out T service) where T : class {
            return _serviceManger.TryGet(out service);
        }

        public bool TryGetNextInHierarchy(out ServiceLocator container) {
            if (this == _global) {
                container = null;
                return false;
            }

            container = transform.parent.GetComponentInParent<ServiceLocator>() ?? ForSceneOf(this);
            return container != null;
        }
        
        public ServiceLocator Unregister<T>() {
            _serviceManger.Unregister<T>();
            return this;
        }

        private void OnDestroy() {
            if (this == _global) {
                _global = null;
            } else if (_sceneContainers.ContainsValue(this)) {
                _sceneContainers.Remove(gameObject.scene);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() {
            _global = null;
            _sceneContainers = new Dictionary<Scene, ServiceLocator>();
            _tmpSceneGameObjects = new List<GameObject>();
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/Service Locator/Add Global")]
        private static void AddGlobal() {
            new GameObject(
                GlobalServiceLocatorName, 
                typeof(ServiceLocator), 
                typeof(ServiceLocatorGlobalBootstrapper));
        }

        [MenuItem("GameObject/Service Locator/Add Scene")]
        private static void AddScene() {
            new GameObject(
                SceneServiceLocatorName, 
                typeof(ServiceLocator), 
                typeof(ServiceLocatorSceneBootstrapper));
        }
#endif
    }
}
