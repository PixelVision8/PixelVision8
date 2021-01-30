// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christina-Antoinette Neofotistou @CastPixel
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany
//

using System;
using System.Collections.Generic;

namespace PixelVision8.Player
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
            if (services.ContainsKey(id)) return services[id];

            throw new Exception("The requested service '" + id + "' is not registered");
        }

        public void RemoveService(string id)
        {
            if (services.ContainsKey(id)) services.Remove(id);

            throw new Exception("The requested service '" + id + "' is not registered");
        }
    }
}