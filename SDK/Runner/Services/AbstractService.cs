//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
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

namespace PixelVision8.Runner
{
    /// <summary>
    ///     The AbstractService allows you to expose classes and API to the chips in
    ///     a save, decoupled way. The Service implements the IService interface
    ///     allowing each instance to take advantage of the RegisterService, getInstance
    ///     and Execute methods.
    /// </summary>
    public abstract class AbstractService : IService
    {
        public IServiceLocator locator { get; set; }

        /// <summary>
        ///     This method registers the service with the service locator.
        /// </summary>
        /// <param name="locator"></param>
        public virtual void RegisterService(IServiceLocator locator)
        {
            this.locator = locator;
        }
    }
}