using System;
using System.Collections.Generic;

namespace PixelVision8.Engine.Services
{
    public class ServiceManager : IServiceLocator
    {
        protected Dictionary<string, IService> services = new Dictionary<string, IService>();

        public void AddService(string id, IService service)
        {
            // Add the service to the managed list
            if (services.ContainsKey(id))
                services[id] = service;
            else
                services.Add(id, service);

            // Register service locator
            service.locator = this;

            // Add a reference of the service locator
            service.RegisterService(this);
        }

        public IService GetService(string id)
        {
            if (services.ContainsKey(id))
                return services[id];

            throw new Exception("The requested service '" + id + "' is not registered");
        }

        public void RemoveService(string id)
        {
            if (services.ContainsKey(id))
                services.Remove(id);

            throw new Exception("The requested service '" + id + "' is not registered");
        }
    }
}