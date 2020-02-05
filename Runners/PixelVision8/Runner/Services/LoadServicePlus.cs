// //   
// // Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
// //  
// // Licensed under the Microsoft Public License (MS-PL) except for a few
// // portions of the code. See LICENSE file in the project root for full 
// // license information. Third-party libraries used by Pixel Vision 8 are 
// // under their own licenses. Please refer to those libraries for details 
// // on the license they use.
// // 
// // Contributors
// // --------------------------------------------------------
// // This is the official list of Pixel Vision 8 contributors:
// //  
// // Jesse Freeman - @JesseFreeman
// // Christina-Antoinette Neofotistou @CastPixel
// // Christer Kaitila - @McFunkypants
// // Pedro Medeiros - @saint11
// // Shawn Rakowski - @shwany
// //
//
// using System.IO;
// using PixelVision8.Runner.Workspace;
//
// namespace PixelVision8.Runner.Services
// {
//     public class LoadServicePlus : LoadService
//     {
//         protected WorkspaceService WorkspaceService;
//
//         public LoadServicePlus(WorkspaceService workspaceService)
//         {
//             WorkspaceService = workspaceService;
//         }
//
//         public override string GetFileName(string path)
//         {
//             return WorkspacePath.Parse(path).EntityName;
//         }
//         public override byte[] ReadAllBytes(string file)
//         {
//             
//             var path = WorkspacePath.Parse(file);
//
//             using (var memoryStream = new MemoryStream())
//             {
//                 using (var fileStream = WorkspaceService.OpenFile(path, FileAccess.Read))
//                 {
//                     fileStream.CopyTo(memoryStream);
//                     fileStream.Close();
//                 }
//
//                 //                    Console.WriteLine("Add File " + file.Path.Substring(1));
//
//                 return memoryStream.ToArray();
//                 // files.Add(file.Path.Substring(1), file.Path);//memoryStream.ToArray());
//             }
//
//             // return base.ReadAllBytes(file);
//         }
//     }
// }