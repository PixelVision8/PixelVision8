//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
//
// Based on SharpFileSystem by Softwarebakery Copyright (c) 2013 under
// MIT license at https://github.com/bobvanderlinden/sharpfilesystem.
// Modified for PixelVision8 by Jesse Freeman
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
using System.Linq;

namespace PixelVision8.Runner.Workspace.Collections
{
    public class TypeCombinationDictionary<T>
    {
        private readonly LinkedList<TypeCombinationEntry> _registrations = new LinkedList<TypeCombinationEntry>();

        public IEnumerable<TypeCombinationEntry> GetSupportedRegistrations(Type sourceType, Type destinationType)
        {
            return
                _registrations.Where(
                    r =>
                        r.SourceType.IsAssignableFrom(sourceType) &&
                        r.DestinationType.IsAssignableFrom(destinationType));
        }

        public TypeCombinationEntry GetSupportedRegistration(Type sourceType, Type destinationType)
        {
            return GetSupportedRegistrations(sourceType, destinationType).FirstOrDefault();
        }

        public bool TryGetSupported(Type sourceType, Type destinationType, out T value)
        {
            var r = GetSupportedRegistration(sourceType, destinationType);
            if (r == null)
            {
                value = default(T);
                return false;
            }

            value = r.Value;
            return true;
        }

        public void AddFirst(Type sourceType, Type destinationType, T value)
        {
            _registrations.AddFirst(new TypeCombinationEntry(sourceType, destinationType, value));
        }

        public void AddLast(Type sourceType, Type destinationType, T value)
        {
            _registrations.AddLast(new TypeCombinationEntry(sourceType, destinationType, value));
        }

        public class TypeCombinationEntry
        {
            public TypeCombinationEntry(Type sourceType, Type destinationType, T value)
            {
                SourceType = sourceType;
                DestinationType = destinationType;
                Value = value;
            }

            public Type SourceType { get; }
            public Type DestinationType { get; }
            public T Value { get; }
        }
    }
}