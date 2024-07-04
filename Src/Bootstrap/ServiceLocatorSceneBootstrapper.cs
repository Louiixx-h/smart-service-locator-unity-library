using UnityEngine;

namespace LuisLabs.SmartServiceLocator
{
    [AddComponentMenu("ServiceLocator/Service Locator Scene")]
    public class ServiceLocatorSceneBootstrapper : ServiceLocatorBootstrapper
    {
        protected override void Bootstrap()
        {
            Container.ConfigureAsScene();
        }
    }
}