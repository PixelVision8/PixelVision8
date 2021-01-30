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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace PixelVision8.Runner.Workspace
{
    public struct WorkspacePath : IEquatable<WorkspacePath>, IComparable<WorkspacePath>
    {
        public static readonly char DirectorySeparator = '/';
        public static WorkspacePath Root { get; }

        private readonly string _path;

        public string Path => _path ?? "/";

        public bool IsDirectory => Path[Path.Length - 1] == DirectorySeparator;

        public bool IsFile => !IsDirectory;

        public bool IsRoot => Path.Length == 1;

        public string EntityName
        {
            get
            {
                var name = Path;
                if (IsRoot) return null;

                var endOfName = name.Length;
                if (IsDirectory) endOfName--;

                var startOfName = name.LastIndexOf(DirectorySeparator, endOfName - 1, endOfName) + 1;
                return name.Substring(startOfName, endOfName - startOfName);
            }
        }

        public string EntityNameWithoutExtension
        {
            get
            {
                //                var name = EntityName;
                var ext = GetExtension();
                return IsDirectory ? EntityName : EntityName.Substring(0, EntityName.Length - ext.Length);
            }
        }

        public WorkspacePath ParentPath
        {
            get
            {
                var parentPath = Path;
                if (IsRoot) throw new InvalidOperationException("There is no parent of root.");

                var lookaheadCount = parentPath.Length;
                if (IsDirectory) lookaheadCount--;

                var index = parentPath.LastIndexOf(DirectorySeparator, lookaheadCount - 1, lookaheadCount);
                Debug.Assert(index >= 0);
                parentPath = parentPath.Remove(index + 1);
                return new WorkspacePath(parentPath);
            }
        }

        static WorkspacePath()
        {
            Root = new WorkspacePath(DirectorySeparator.ToString());
        }

        private WorkspacePath(string path)
        {
            _path = path;
        }

        public static bool IsRooted(string s)
        {
            if (s.Length == 0) return false;

            return s[0] == DirectorySeparator;
        }

        public static WorkspacePath Parse(string s)
        {
            if (s == null) throw new ArgumentNullException("s");

            if (!IsRooted(s)) throw new ParseException(s, "Path is not rooted");

            if (s.Contains(string.Concat(DirectorySeparator, DirectorySeparator)))
                throw new ParseException(s, "Path contains double directory-separators.");

            return new WorkspacePath(s);
        }

        public WorkspacePath AppendPath(string relativePath)
        {
            if (IsRooted(relativePath))
                throw new ArgumentException("The specified path should be relative.", "relativePath");

            if (!IsDirectory) throw new InvalidOperationException("This FileSystemPath is not a directory.");

            return new WorkspacePath(Path + relativePath);
        }

        [Pure]
        public WorkspacePath AppendPath(WorkspacePath path)
        {
            if (!IsDirectory) throw new InvalidOperationException("This FileSystemPath is not a directory.");

            return new WorkspacePath(Path + path.Path.Substring(1));
        }

        [Pure]
        public WorkspacePath AppendDirectory(string directoryName)
        {
            if (directoryName.Contains(DirectorySeparator.ToString()))
                throw new ArgumentException("The specified name includes directory-separator(s).", "directoryName");

            if (!IsDirectory) throw new InvalidOperationException("The specified FileSystemPath is not a directory.");

            return new WorkspacePath(Path + directoryName + DirectorySeparator);
        }

        [Pure]
        public WorkspacePath AppendFile(string fileName)
        {
            if (fileName.Contains(DirectorySeparator.ToString()))
                throw new ArgumentException("The specified name includes directory-separator(s).", "fileName");

            if (!IsDirectory) throw new InvalidOperationException("The specified FileSystemPath is not a directory.");

            return new WorkspacePath(Path + fileName);
        }

        [Pure]
        public bool IsParentOf(WorkspacePath path)
        {
            return IsDirectory && Path.Length != path.Path.Length && path.Path.StartsWith(Path);
        }

        [Pure]
        public bool IsChildOf(WorkspacePath path)
        {
            return path.IsParentOf(this);
        }

        [Pure]
        public WorkspacePath RemoveParent(WorkspacePath parent)
        {
            if (!parent.IsDirectory)
                throw new ArgumentException(
                    "The specified path can not be the parent of this path: it is not a directory.");

            if (!Path.StartsWith(parent.Path))
                throw new ArgumentException("The specified path is not a parent of this path.");

            return new WorkspacePath(Path.Remove(0, parent.Path.Length - 1));
        }

        [Pure]
        public WorkspacePath RemoveChild(WorkspacePath child)
        {
            if (!Path.EndsWith(child.Path))
                throw new ArgumentException("The specified path is not a child of this path.");

            return new WorkspacePath(Path.Substring(0, Path.Length - child.Path.Length + 1));
        }

        [Pure]
        public string GetExtension()
        {
            if (!IsFile) throw new ArgumentException("The specified FileSystemPath is not a file.");

            var name = EntityName;
            var extensionIndex = name.LastIndexOf('.');
            if (extensionIndex < 0) return "";

            return name.Substring(extensionIndex);
        }

        [Pure]
        public WorkspacePath ChangeExtension(string extension)
        {
            if (!IsFile) throw new ArgumentException("The specified FileSystemPath is not a file.");

            var name = EntityName;
            var extensionIndex = name.LastIndexOf('.');
            if (extensionIndex >= 0) return ParentPath.AppendFile(name.Substring(0, extensionIndex) + extension);

            return Parse(Path + extension);
        }

        [Pure]
        public string[] GetDirectorySegments()
        {
            var path = this;
            if (IsFile) path = path.ParentPath;

            var segments = new LinkedList<string>();
            while (!path.IsRoot)
            {
                segments.AddFirst(path.EntityName);
                path = path.ParentPath;
            }

            return segments.ToArray();
        }

        [Pure]
        public int CompareTo(WorkspacePath other)
        {
            return Path.CompareTo(other.Path);
        }

        [Pure]
        public override string ToString()
        {
            return Path;
        }

        [Pure]
        public override bool Equals(object obj)
        {
            if (obj is WorkspacePath) return Equals((WorkspacePath) obj);

            return false;
        }

        [Pure]
        public bool Equals(WorkspacePath other)
        {
            return other.Path.Equals(Path);
        }

        [Pure]
        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public static bool operator ==(WorkspacePath pathA, WorkspacePath pathB)
        {
            return pathA.Equals(pathB);
        }

        public static bool operator !=(WorkspacePath pathA, WorkspacePath pathB)
        {
            return !(pathA == pathB);
        }
    }
}