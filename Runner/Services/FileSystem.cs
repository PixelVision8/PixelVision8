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
using System.IO;
using System.Linq;

namespace PixelVisionRunner.Services
{
    public class FileSystemService : IFileSystem
    {
        public virtual void CreateDirectory(string path)
        {
            // Only create the directory if it doesn't exist
            if (!DirectoryExists(path))
                Directory.CreateDirectory(path);
        }

        public virtual void DeleteDirectory(string path, bool recursive = true)
        {
            Directory.Delete(path, true);
        }

        public virtual bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public virtual void FileMove(string sourceFileName, string destFileName)
        {
            File.Move(sourceFileName, destFileName);
        }

        public void CopyFile(string src, string dest)
        {
            File.Copy(src, dest);
        }

        public virtual void WriteAllBytes(string path, byte[] byteData)
        {
            File.WriteAllBytes(path, byteData);
        }

        public virtual bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public virtual void FileDelete(string path)
        {
            File.Delete(path);
        }

        public virtual string GetFileExtension(string path)
        {
            return Path.GetExtension(path);
        }

        public virtual string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual string[] GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        public virtual string ReadTextFromFile(string path)
        {
            // Create a new steam reader

            var stream = new StreamReader(File.OpenRead(path));

            // Read file contents
            var fileContents = stream.ReadToEnd();

            // Close stream
            stream.Dispose();

            // Return contents
            return fileContents;
        }

        public virtual string[] FilePathsInDir(string path, string[] files)
        {
            var paths = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                .Where(s => files.Any(e => s.EndsWith(e))).ToArray();

            return paths;
        }

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual DateTime GetLastWriteTime(string path)
        {
            return File.GetLastWriteTime(path);
        }

        public virtual string[] FileNamesInDir(string path, string[] files, bool dropExtension = true)
        {
            var dirs = FilePathsInDir(path, files);

            var names = DirNamesFromPaths(dirs, dropExtension);

            return names;
        }

        public virtual void SaveTextToFile(string fullPath, string data)
        {
            //var data = engine.SerializeData();

            var file = File.Open(fullPath, FileMode.Create);

            using (file)
            {
                using (var writer = new StreamWriter(file))
                {
                    writer.Write(data);
                }
            }

            file.Dispose();
        }

        public virtual FileInfo GetFileInfo(string path)
        {
            return new FileInfo(path);
        }


        public virtual string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        public virtual long GetDirectorySize(string path)
        {
            // 1.
            // Get array of all file names.
            var a = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

            // 2.
            // Calculate total bytes of all files in a loop.
            long b = 0;
            foreach (var name in a)
            {
                // 3.
                // Use FileInfo to get length of each file.
                var info = new FileInfo(name);
                b += info.Length;
            }

            // 4.
            // Return total size
            return b;
        }

        public virtual void MoveFile(string sourceFileName, string destFileName)
        {
            File.Move(sourceFileName, destFileName);
        }

        public void WriteAllText(string path, string text)
        {
            File.WriteAllText(path, text);
        }

        public string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }

        public long GetFileSize(string path)
        {
            long size = -1;

            if (FileExists(path)) size = GetFileInfo(path).Length;

            return size;
        }

        public void CopyAll(string source, string target)
        {
            CopyAll(new DirectoryInfo(source), new DirectoryInfo(target));
        }

        public DirectoryInfo DirectoryInfo(string path)
        {
            return new DirectoryInfo(path);
        }

        public StreamWriter CreateText(string path)
        {
            return File.CreateText(path);
        }

        public void AppendAllText(string path, string text)
        {
            File.AppendAllText(path, text);
        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public StreamReader OpenText(string path)
        {
            return File.OpenText(path);
        }

        public List<string> GetFilePaths(string path)
        {
            var filePaths = new List<string>();
            var source = new DirectoryInfo(path);

            // Copy each file into the new directory.
            foreach (var fi in source.GetFiles()) filePaths.Add(fi.FullName);

            // Copy each subdirectory using recursion.
            foreach (var diSourceSubDir in source.GetDirectories()) GetFilePaths(diSourceSubDir, filePaths);

            return filePaths;
        }

        public void MoveDirectory(string src, string dest)
        {
            Directory.Move(src, dest);
        }

        public bool IsDirectory(string path)
        {
            // get the file attributes for file or directory
            var attr = File.GetAttributes(path);

            //detect whether its a directory or file
            return (attr & FileAttributes.Directory) == FileAttributes.Directory;
        }

        public virtual string[] DirNamesFromPaths(string[] dirs, bool dropExtension = true)
        {
            var names = new string[dirs.Length];

            for (var i = 0; i < dirs.Length; i++)
                names[i] = dropExtension ? GetFileNameWithoutExtension(dirs[i]) : Path.GetFileName(dirs[i]);

            return names;
        }

        protected void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (var fi in source.GetFiles()) fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);

            // Copy each subdirectory using recursion.
            foreach (var diSourceSubDir in source.GetDirectories())
            {
                var nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public void GetFilePaths(DirectoryInfo source, List<string> filePaths)
        {
            // Copy each file into the new directory.
            foreach (var fi in source.GetFiles()) filePaths.Add(fi.FullName);

            // Copy each subdirectory using recursion.
            foreach (var diSourceSubDir in source.GetDirectories()) GetFilePaths(diSourceSubDir, filePaths);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }
        
        public void ImportFilesFromDir(string path, ref Dictionary<string, byte[]> files, string[] extFilter = null)
        {
            var paths = GetFiles(path);

            foreach (var filePath in paths)
            {
                var fileType = GetExtension(filePath);

                if (extFilter == null || (Array.IndexOf(extFilter, fileType) != -1))
                {
                    var fileName = GetFileName(filePath);
                    var data = ReadAllBytes(filePath);

                    if (files.ContainsKey(fileName))
                    {
                        files[fileName] = data;
                    }
                    else
                    {
                        files.Add(fileName, data);
                    }
                }
            }
        }
    }
}