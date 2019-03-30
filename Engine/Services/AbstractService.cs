//   
// Copyright (c) Jesse Freeman. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) License. 
// See LICENSE file in the project root for full license information. 
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany

using System;
using System.Collections.Generic;

namespace PixelVision8.Engine.Services
{

    /// <summary>
    ///     The AbstractService allows you to expose classes and API to the chips in
    ///     a save, decoupled way. The Service implements the IService interface
    ///     allowing each instance to take advantage of the RegisterService, getInstance
    ///     and Execute methods.
    /// </summary>
    public abstract class AbstractService : IService
    {

        protected IServiceLocator locator;

        /// <summary>
        ///     This method registers the service with the service locator.
        /// </summary>
        /// <param name="locator"></param>
        public virtual void RegisterService(IServiceLocator locator)
        {
            this.locator = locator;
        }

        /// <summary>
        ///     This method can be used to return a type value of the service's wrapped instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual T GetInstance<T>() where T : IService
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This method exposes a generic way to execute code on a service's internal instance.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual bool Execute(string command, Dictionary<string, object> data)
        {
            throw new NotImplementedException();
        }

    }

}