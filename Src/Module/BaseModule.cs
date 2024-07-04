using System;
using UnityEngine;

namespace LuisLabs.SmartServiceLocator
{
    public abstract class BaseModule : MonoBehaviour
    {
        protected abstract void RegisterDependencies();
        
        protected virtual void Awake()
        {
            RegisterDependencies();
        }
        
        protected void Register<T>(T dependency, ServiceScope scope)
        {
            switch (scope)
            {
                case ServiceScope.Global:
                    ServiceLocator.Global.Register(dependency);
                    break;
                case ServiceScope.Scene:
                    ServiceLocator.ForSceneOf(this).Register(dependency);
                    break;
                case ServiceScope.Local:
                    ServiceLocator.For(this).Register(dependency);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
            }
        }

        protected void Register<T>(Type type, T dependency, ServiceScope scope)
        {
            switch (scope)
            {
                case ServiceScope.Global:
                    ServiceLocator.Global.Register<T>(type, dependency);
                    break;
                case ServiceScope.Scene:
                    ServiceLocator.ForSceneOf(this).Register<T>(type, dependency);
                    break;
                case ServiceScope.Local:
                    ServiceLocator.For(this).Register<T>(type, dependency);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
            }
        }
    }
}