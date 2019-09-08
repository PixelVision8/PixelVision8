//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
//
// Based on https://www.nuget.org/packages/InfinitespaceStudios.MonoGame.Packaging by Dean Ellis | Infinitespace Studios
// and MonoKickstart (https://github.com/MonoGame/MonoKickstart) 
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
using System.IO;
using System.IO.Compression;

namespace InfinitespaceStudios.MonoGame.PackageCreator
{
	public static class ZipFile
	{
		public static void CreateFromDirectory(string directory, string zipfile, params string[] executeItems)
		{

			using (var memoryStream = new MemoryStream())
			{
				using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
				{
					
					foreach (string str in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
					{
						if (str.Contains("/__MACOSX/") || str.EndsWith("/__MACOSX", StringComparison.OrdinalIgnoreCase) || str.EndsWith("/.DS_Store", StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}

						if (!directory.EndsWith(Path.DirectorySeparatorChar.ToString()))
						{
							char directorySeparatorChar = Path.DirectorySeparatorChar;
							directory = string.Concat(directory, directorySeparatorChar.ToString());
						}

						var tmpFile = archive.CreateEntry(str.Replace(directory, "").Replace("\\", "/"));
						
						using (var entryStream = tmpFile.Open())
						{
							File.OpenRead(str).CopyTo(entryStream);
						}
						
					}
					
				}
				
				using (var fileStream = new FileStream(zipfile, FileMode.Create))
				{
					memoryStream.Seek(0, SeekOrigin.Begin);
					memoryStream.CopyTo(fileStream);
				}

				// Make sure we close the stream
				memoryStream.Close();
			}

		}
	}
}