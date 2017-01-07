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
// Jesse Freeman
// 

namespace PixelVisionSDK.Engine.Chips.Data
{
    public interface IInvalidate
    {
        /// <summary>
        ///     The invalid flag allows you to quickly see if data has been changed
        ///     on the AbstractData instance. This is used in conjunction with the
        ///     Invalidate() and ResetValidation() methods. Use this flag in classes
        ///     that have the potential to be expensive to update and need to be delayed
        ///     before refreshing their data.<br />When set to true, this value means that the data is dirty and needs be
        ///     updated. Set and Reset this value via the Invalidate() and ResetValidation()
        ///     methods.
        /// </summary>
        /// <value>Boolean</value>
        bool invalid { get; }

        /// <summary>
        ///     This method allows a clean way to set the invalid property to true
        ///     signaling a change in the underlying data. This method could be overridden
        ///     to provide additional logic when the AbstractData is invalidated.
        /// </summary>
        void Invalidate();

        /// <summary>
        ///     This method allows a clean way to reset the invalid property to false
        ///     signaling underlying data had finished updating. This method could be
        ///     overridden to provide additional logic when the AbstractData is
        ///     done changing.
        /// </summary>
        void ResetValidation();
    }
}