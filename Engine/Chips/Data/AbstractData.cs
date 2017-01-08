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
// 

using System.Collections.Generic;
using System.Text;
using PixelVisionSDK.Engine.Chips.IO.File;

namespace PixelVisionSDK.Engine.Chips.Data
{

    /// <summary>
    ///     The AbstractData class represents a standard foundation for all
    ///     data objects in the engine. It implements the ISave, ILoad and
    ///     IInvalidate interfaces and provides as standard API for serializing
    ///     the data it contains via the CustomSerializeData() method.
    /// </summary>
    public class AbstractData : ISave, ILoad, IInvalidate
    {

        protected bool _invalid;

        /// <summary>
        ///     The invalid flag allows you to quickly see if data has been changed
        ///     on the data instance. This is used in conjunction with the
        ///     Invalidate() and ResetValidation() methods. Use this flag in classes
        ///     that have the potential to be expensive to update and need to be delayed
        ///     before refreshing their data.
        /// </summary>
        /// <value>Boolean</value>
        public virtual bool invalid
        {
            get { return _invalid; }
        }

        /// <summary>
        ///     This method allows a clean way to set the invalid property to true
        ///     signaling a change in the underlying data. This method could be overridden
        ///     to provide additional logic when the AbstractData is invalidated.
        /// </summary>
        public virtual void Invalidate()
        {
            _invalid = true;
        }

        /// <summary>
        ///     This method allows a clean way to reset the invalid property to false
        ///     signaling underlying data had finished updating. This method could be
        ///     overridden to provide additional logic when the AbstractData is
        ///     done changing.
        /// </summary>
        public virtual void ResetValidation()
        {
            _invalid = false;
        }

        /// <summary>
        ///     The DeserializeData method allows you to pass in a
        ///     Dictionary with a string as the key and a generic object for the
        ///     value. This can be manually parsed to convert each key/value pair
        ///     into data used to configure the class that
        ///     implements this interface.
        /// </summary>
        /// <param name="data">
        ///     A Dictionary with a string as the key and a generic object as the
        ///     value.
        /// </param>
        public virtual void DeserializeData(Dictionary<string, object> data)
        {
            // override with custom parsing logic
        }

        /// <summary>
        ///     Use this method to create a new StringBuilder instance and wrap any
        ///     custom serialized data by leveraging the CustomSerializedData()
        ///     method.
        /// </summary>
        /// <returns name="String">
        ///     A string JSON object.
        /// </returns>
        public virtual string SerializeData()
        {
            var sb = new StringBuilder();
            sb.Append("{");
            CustomSerializedData(sb);
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        ///     Use this method to add additional serialized string data to the
        ///     supplied StringBuilder instance.
        /// </summary>
        /// <param name="sb">
        ///     A reference to a StringBuilder which is supplied by the
        ///     SerializedData() method.
        /// </param>
        public virtual void CustomSerializedData(StringBuilder sb)
        {
            // override with custom serialized data
        }

    }

}