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

namespace PixelVisionRunner.Services
{

    public interface IFileSystem
    {

//        string clipboard { get; set; }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="key"></param>
//        /// <param name="defaultValue"></param>
//        /// <returns></returns>
//        string ReadLocalStorage(string key, string defaultValue);
//
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="key"></param>
//        /// <param name="value"></param>
//        void SaveLocalStorage(string key, string value);

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        void CreateDirectory(string path);

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="recursive"></param>
        void DeleteDirectory(string path, bool recursive = true);

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destFileName"></param>
        void FileMove(string sourceFileName, string destFileName);

        void CopyFile(string src, string dest);

        void MoveDirectory(string src, string dest);
        
        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="byteData"></param>
        void WriteAllBytes(string path, byte[] byteData);

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool FileExists(string path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool IsDirectory(string path);
            
        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        void FileDelete(string path);

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetFileExtension(string path);

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetFileNameWithoutExtension(string path);

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string[] GetFiles(string path);

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string ReadTextFromFile(string path);

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        //TODO need to remove dependency on Texture2D
//        Texture2D ReadTextureFromFile(string path);

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        string[] FilePathsInDir(string path, string[] files);

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        DateTime GetLastWriteTime(string path);

        string[] FileNamesInDir(string path, string[] files, bool dropExtension = true);
        //void SaveTextToFile(string path, string name, string data, string ext = "txt");
        void SaveTextToFile(string path, string data);

        // TODO this type should be abstracted?
        FileInfo GetFileInfo(string path);

//        void SaveTextureToFile(string path, string name, Texture2D tex);

        string[] GetDirectories(string path);

        long GetDirectorySize(string path);

        void MoveFile(string sourceFileName, string destFileName);
        void WriteAllText(string path, string text);
        string GetExtension(string path);
        DirectoryInfo DirectoryInfo(string path);
        StreamWriter CreateText(string path);
        void AppendAllText(string path, string text);
        string GetFileName(string path);
        byte[] ReadAllBytes(string path);
        StreamReader OpenText(string path);
//        void ArchiveDirectory(string filename, string sourcePath, string destinationPath, string comment = "A Pixel Vision 8 Archive");
        List<string> GetFilePaths(string path);
        long GetFileSize(string path);

        void CopyAll(string source, string target);
            string ReadAllText(string path);

            void ImportFilesFromDir(string path, ref Dictionary<string, byte[]> files, string[] extFilter = null);

    }

}