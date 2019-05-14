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

using Mono.Unix;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace InfinitespaceStudios.MonoGame.PackageCreator
{
	public class Generator
	{
		private static string[] baseAssemblies;

		private static string[] CompressionAssemblies;

		private static Dictionary<string, int> icnsFiles;

		private static Dictionary<Generator.Platform, string[]> runtime;

		private static Dictionary<Generator.Platform, string[]> exeFiles;

		private static Dictionary<Generator.Platform, string[]> mcsFiles;

		private static Dictionary<Generator.Platform, string> exeExtension;

		private static Dictionary<Generator.Platform, string> dllSearchPattern;

		private static Dictionary<Generator.Platform, string> zipSuffix;

		public string AdditionalAssemblies
		{
			get;
			set;
		}

		public string ContentDirectory
		{
			get;
			set;
		}

		public string Copyright
		{
			get;
			set;
		}

		public string DisplayName
		{
			get;
			set;
		}

		public string Icon
		{
			get;
			set;
		}

		public bool IncludeMcs
		{
			get;
			set;
		}

		public string InfoPlist
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public string OutputDirectory
		{
			get;
			set;
		}

		public string PackageId
		{
			get;
			set;
		}

		public bool RequiresCompression
		{
			get;
			set;
		}

		public string SourceDirectory
		{
			get;
			set;
		}

		public Generator.Platform SupportedPlatforms
		{
			get;
			set;
		}

		public Generator.Platform TargetPlatform
		{
			get;
			set;
		}

		public string TemporaryDirectory
		{
			get;
			set;
		}

		public IList<string> ZipFiles
		{
			get;
			private set;
		}

		static Generator()
		{
			Generator.baseAssemblies = new string[] { "System.dll", "Mono.Posix.dll", "Mono.Security.dll", "mscorlib.dll", "System.dll", "System.Core.dll", "System.Data.dll", "System.Drawing.dll", "System.Runtime.Serialization.dll", "System.Security.dll", "System.Xml.dll", "System.Xml.Linq.dll", "WindowsBase.dll" };
			Generator.CompressionAssemblies = new string[] { "System.IO.Compression.dll", "System.IO.Compression.dll.config", "System.IO.Compression.FileSystem.dll" };
			Generator.icnsFiles = new Dictionary<string, int>()
			{
				{ "icon_16x16.png", 16 },
				{ "icon_16x16@2x.png", 32 },
				{ "icon_32x32.png", 32 },
				{ "icon_32x32@2x.png", 64 },
				{ "icon_64x64.png", 64 },
				{ "icon_64x64@2x.png", 128 },
				{ "icon_128x128.png", 128 },
				{ "icon_128x128@2x.png", 256 },
				{ "icon_256x256.png", 256 },
				{ "icon_256x256@2x.png", 512 },
				{ "icon_512x512.png", 512 },
				{ "icon_512x512@2x.png", 1024 }
			};
			Dictionary<Generator.Platform, string[]> platforms = new Dictionary<Generator.Platform, string[]>()
			{
				{ Generator.Platform.MacOS, new string[] { "mscorlib.dll" } },
				{ Generator.Platform.Steam, new string[] { "mscorlib.dll" } },
				{ Generator.Platform.Linux, new string[] { "mscorlib.dll.linux" } },
				{ Generator.Platform.Windows, new string[0] }
			};
			Generator.runtime = platforms;
			platforms = new Dictionary<Generator.Platform, string[]>()
			{
				{ Generator.Platform.MacOS, new string[] { "kick.bin.osx" } },
				{ Generator.Platform.Steam, new string[] { "kick.bin.osx", "kick.bin.x86", "kick.bin.x86_64" } },
				{ Generator.Platform.Linux, new string[] { "kick.bin.x86", "kick.bin.x86_64" } },
				{ Generator.Platform.Windows, new string[0] }
			};
			Generator.exeFiles = platforms;
			platforms = new Dictionary<Generator.Platform, string[]>()
			{
				{ Generator.Platform.MacOS, new string[] { "mcs.bin.osx" } },
				{ Generator.Platform.Steam, new string[] { "mcs.bin.osx", "mcs.bin.x86", "mcs.bin.x86_64" } },
				{ Generator.Platform.Linux, new string[] { "mcs.bin.x86", "mcs.bin.x86_64" } },
				{ Generator.Platform.Windows, new string[0] }
			};
			Generator.mcsFiles = platforms;
			Generator.exeExtension = new Dictionary<Generator.Platform, string>()
			{
				{ Generator.Platform.MacOS, "{0}" },
				{ Generator.Platform.Steam, "{0}.bin{1}" },
				{ Generator.Platform.Linux, "{0}.bin{1}" },
				{ Generator.Platform.Windows, null }
			};
			Generator.dllSearchPattern = new Dictionary<Generator.Platform, string>()
			{
				{ Generator.Platform.MacOS, "*.dylib" },
				{ Generator.Platform.Steam, "*.dll;*.dylib;*.so;*.so.*" },
				{ Generator.Platform.Linux, "*.so;*.so.*" },
				{ Generator.Platform.Windows, "*.dll" }
			};
			Generator.zipSuffix = new Dictionary<Generator.Platform, string>()
			{
				{ Generator.Platform.MacOS, "-AppStore" },
				{ Generator.Platform.Steam, "-Steam" },
				{ Generator.Platform.Linux, "" },
				{ Generator.Platform.Itchio, "" },
				{ Generator.Platform.Windows, "" }
			};
		}

		public Generator()
		{
			this.TemporaryDirectory = null;
			this.ZipFiles = new List<string>();
			this.SupportedPlatforms = Generator.Platform.Linux | Generator.Platform.Steam | Generator.Platform.Itchio | Generator.Platform.Windows;
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				this.SupportedPlatforms = Generator.Platform.Linux | Generator.Platform.Steam | Generator.Platform.Itchio | Generator.Platform.Windows;
				if (Directory.Exists("/Applications") && Directory.Exists("/LibraryF/Frameworks/Mono.framework"))
				{
					this.SupportedPlatforms = this.SupportedPlatforms | Generator.Platform.MacOS;
				}
			}
		}

		private void CopyContent(string root, string targetPath = null)
		{
			char directorySeparatorChar;
			int i;
			if (!Directory.Exists(this.ContentDirectory))
			{
				return;
			}
			string contentDirectory = this.ContentDirectory;
			if (!contentDirectory.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				directorySeparatorChar = Path.DirectorySeparatorChar;
				contentDirectory = string.Concat(contentDirectory, directorySeparatorChar.ToString());
			}
			Console.WriteLine(contentDirectory);
			string fileName = Path.GetFileName(Path.GetDirectoryName(contentDirectory));
			string str = Path.Combine(root, targetPath ?? string.Empty, fileName);
			Directory.CreateDirectory(str);
			Console.WriteLine(str);
			string[] directories = Directory.GetDirectories(this.ContentDirectory, "*", SearchOption.AllDirectories);
			for (i = 0; i < (int)directories.Length; i++)
			{
				string str1 = directories[i];
				Console.WriteLine(str1);
				Console.WriteLine(str1.Replace(this.ContentDirectory, str));
				if (!(new DirectoryInfo(str1)).Attributes.HasFlag(FileAttributes.Hidden))
				{
					string contentDirectory1 = this.ContentDirectory;
					directorySeparatorChar = Path.DirectorySeparatorChar;
					Directory.CreateDirectory(str1.Replace(contentDirectory1, string.Concat(str, directorySeparatorChar.ToString())));
				}
			}
			directories = Directory.GetFiles(this.ContentDirectory, "*.*", SearchOption.AllDirectories);
			for (i = 0; i < (int)directories.Length; i++)
			{
				string str2 = directories[i];
				if (!(new FileInfo(str2)).Attributes.HasFlag(FileAttributes.Hidden))
				{
					string contentDirectory2 = this.ContentDirectory;
					directorySeparatorChar = Path.DirectorySeparatorChar;
					File.Copy(str2, str2.Replace(contentDirectory2, string.Concat(str, directorySeparatorChar.ToString())), true);
				}
			}
		}

		private void ExtractAndProcessKickScript(string root, string exeFile)
		{
			if (!this.IncludeMcs)
			{
				this.ExtractResourceTo("KickNoMcs", Path.Combine(root, Path.GetFileNameWithoutExtension(exeFile)));
			}
			else
			{
				this.ExtractResourceTo("Kick", Path.Combine(root, Path.GetFileNameWithoutExtension(exeFile)));
			}
			this.MakeExecutable(Path.Combine(root, Path.GetFileNameWithoutExtension(exeFile)));
			this.ReplaceTagInFile(Path.Combine(root, Path.GetFileNameWithoutExtension(exeFile)), "kick.bin.osx", string.Concat(Path.GetFileNameWithoutExtension(exeFile), ".bin.osx"));
			this.ReplaceTagInFile(Path.Combine(root, Path.GetFileNameWithoutExtension(exeFile)), "kick.bin.x86", string.Concat(Path.GetFileNameWithoutExtension(exeFile), ".bin.x86"));
			this.ReplaceTagInFile(Path.Combine(root, Path.GetFileNameWithoutExtension(exeFile)), "kick.bin.x86_64", string.Concat(Path.GetFileNameWithoutExtension(exeFile), ".bin.x86_64"));
		}

		private void ExtractAssemblies(string root, string targetPath = null)
		{
			int i;
			string[] compressionAssemblies = Generator.baseAssemblies;
			for (i = 0; i < (int)compressionAssemblies.Length; i++)
			{
				string str = compressionAssemblies[i];
				this.ExtractResourceTo(str, Path.Combine(root, targetPath ?? string.Empty, str));
			}
			if (this.RequiresCompression)
			{
				compressionAssemblies = Generator.CompressionAssemblies;
				for (i = 0; i < (int)compressionAssemblies.Length; i++)
				{
					string str1 = compressionAssemblies[i];
					this.ExtractResourceTo(str1, Path.Combine(root, targetPath ?? string.Empty, str1));
				}
			}
			if (!string.IsNullOrEmpty(this.AdditionalAssemblies))
			{
				compressionAssemblies = this.AdditionalAssemblies.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
				for (i = 0; i < (int)compressionAssemblies.Length; i++)
				{
					string str2 = compressionAssemblies[i];
					if (File.Exists(str2))
					{
						File.Copy(str2, Path.Combine(root, targetPath ?? string.Empty, Path.GetFileName(str2)), true);
					}
				}
			}
		}

		private void ExtractExeAndMcs(Generator.Platform platform, string exeFile, string root, string targetPath = null)
		{
			int i;
			string[] item = Generator.exeFiles[platform];
			for (i = 0; i < (int)item.Length; i++)
			{
				string str = item[i];
				string extension = Path.GetExtension(str);
				string str1 = string.Format(Generator.exeExtension[platform], Path.GetFileNameWithoutExtension(exeFile), extension);
				this.ExtractResourceTo(str, Path.Combine(root, targetPath ?? string.Empty, str1));
				this.MakeExecutable(Path.Combine(root, targetPath ?? string.Empty, str1));
			}
			if (this.IncludeMcs)
			{
				item = Generator.mcsFiles[platform];
				for (i = 0; i < (int)item.Length; i++)
				{
					string str2 = item[i];
					Path.GetExtension(str2);
					this.ExtractResourceTo(str2, Path.Combine(root, targetPath ?? string.Empty, str2));
					this.MakeExecutable(Path.Combine(root, targetPath ?? string.Empty, str2));
				}
			}
		}

		private void ExtractMonoConfig(string root, string targetPath = null)
		{
			Directory.CreateDirectory(Path.Combine(root, targetPath ?? string.Empty, "mono", "4.5"));
			this.ExtractResourceTo("config", Path.Combine(root, targetPath ?? string.Empty, "mono", "config"));
			string[] strArrays = new string[] { root, null, null, null, null };
			strArrays[1] = targetPath ?? string.Empty;
			strArrays[2] = "mono";
			strArrays[3] = "4.5";
			strArrays[4] = "machine.config";
			this.ExtractResourceTo("machine.config", Path.Combine(strArrays));
		}

		private void ExtractNativeDependencies(string lib, string root, string targetPath = null)
		{
			this.ExtractResourceTo(lib, Path.Combine(root, targetPath ?? lib));
		}

		private void ExtractResourceTo(string name, string destination)
		{
			using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
			{
				using (FileStream fileStream = new FileStream(destination, FileMode.OpenOrCreate))
				{
					manifestResourceStream.CopyTo(fileStream);
				}
			}
		}

		private void ExtractRuntime(Generator.Platform platform, string root, string targetPath = null)
		{
			string[] item = Generator.runtime[platform];
			for (int i = 0; i < (int)item.Length; i++)
			{
				this.ExtractResourceTo(item[i], Path.Combine(root, targetPath ?? string.Empty, "mscorlib.dll"));
			}
		}

		private bool GenerateGenericPackage(Generator.Platform platform, bool createZip = true, Action<string> additionalFiles = null)
		{
			int i;
			string[] files;
			int j;
			string temporaryDirectory = this.TemporaryDirectory;
			if (string.IsNullOrEmpty(temporaryDirectory))
			{
				temporaryDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			}
			if (!Directory.Exists(temporaryDirectory))
			{
				Directory.CreateDirectory(temporaryDirectory);
			}
			string str = Path.Combine(temporaryDirectory, string.Format("{0}.{1}", this.Name, platform));
			string str1 = Directory.GetFiles(this.SourceDirectory, "*.exe", SearchOption.TopDirectoryOnly).FirstOrDefault<string>();
			if (str1 == null)
			{
				throw new FileNotFoundException(string.Format("Could not find executable in {0}", this.SourceDirectory));
			}
			Directory.CreateDirectory(str);
			string item = Generator.dllSearchPattern[platform];
			string[] strArrays = Directory.GetFiles(this.SourceDirectory, "*.*", SearchOption.TopDirectoryOnly);
			for (i = 0; i < (int)strArrays.Length; i++)
			{
				string str2 = strArrays[i];
				if (!(Path.GetExtension(str2) == ".zip") && (platform != Generator.Platform.Linux || !(Path.GetExtension(str2) == ".dylib")) && (int)((new FileInfo(str2)).Attributes & FileAttributes.Hidden) == 0)
				{
					File.Copy(str2, Path.Combine(str, Path.GetFileName(str2)), true);
				}
			}
			if (Directory.Exists(Path.Combine(this.SourceDirectory, "x86")))
			{
				Directory.CreateDirectory(Path.Combine(str, "x86"));
				strArrays = item.Split(new char[] { ';' });
				for (i = 0; i < (int)strArrays.Length; i++)
				{
					string str3 = strArrays[i];
					files = Directory.GetFiles(Path.Combine(this.SourceDirectory, "x86"), str3, SearchOption.TopDirectoryOnly);
					for (j = 0; j < (int)files.Length; j++)
					{
						string str4 = files[j];
						if ((int)((new FileInfo(str4)).Attributes & FileAttributes.Hidden) == 0)
						{
							File.Copy(str4, Path.Combine(str, "x86", Path.GetFileName(str4)), true);
						}
					}
				}
			}
			if (Directory.Exists(Path.Combine(this.SourceDirectory, "x64")))
			{
				Directory.CreateDirectory(Path.Combine(str, "x64"));
				strArrays = item.Split(new char[] { ';' });
				for (i = 0; i < (int)strArrays.Length; i++)
				{
					string str5 = strArrays[i];
					files = Directory.GetFiles(Path.Combine(this.SourceDirectory, "x64"), str5, SearchOption.TopDirectoryOnly);
					for (j = 0; j < (int)files.Length; j++)
					{
						string str6 = files[j];
						if ((int)((new FileInfo(str6)).Attributes & FileAttributes.Hidden) == 0)
						{
							File.Copy(str6, Path.Combine(str, "x64", Path.GetFileName(str6)), true);
						}
					}
				}
			}
			this.ExtractAndProcessKickScript(str, Path.GetFileNameWithoutExtension(str1));
			this.ExtractExeAndMcs(platform, str1, str, null);
			this.ExtractAssemblies(str, null);
			this.ExtractRuntime(platform, str, null);
			if (this.RequiresCompression)
			{
				this.ExtractNativeDependencies("libMonoPosixHelper.x86", str, Path.Combine("x86", "libMonoPosixHelper.so"));
				this.ExtractNativeDependencies("libMonoPosixHelper.x86_64", str, Path.Combine("x64", "libMonoPosixHelper.so"));
			}
			this.ExtractMonoConfig(str, null);
			this.CopyContent(str, null);
			if (additionalFiles != null)
			{
				additionalFiles(temporaryDirectory);
			}
			if (createZip)
			{
				if (File.Exists(Path.Combine(this.OutputDirectory, string.Format("{0}-{1}{2}.zip", this.Name, platform, Generator.zipSuffix[this.TargetPlatform]))))
				{
					File.Delete(Path.Combine(this.OutputDirectory, string.Format("{0}-{1}{2}.zip", this.Name, platform, Generator.zipSuffix[this.TargetPlatform])));
				}
				Directory.CreateDirectory(this.OutputDirectory);
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(str1);
				ZipFile.CreateFromDirectory(str, Path.Combine(this.OutputDirectory, string.Format("{0}-{1}{2}.zip", this.Name, platform, Generator.zipSuffix[this.TargetPlatform])), new string[] { fileNameWithoutExtension, string.Format("{0}.bin.x86", fileNameWithoutExtension), string.Format("{0}.bin.x86_64", fileNameWithoutExtension) });
				if (File.Exists(Path.Combine(this.OutputDirectory, string.Format("{0}-{1}{2}.zip", this.Name, platform, Generator.zipSuffix[this.TargetPlatform]))))
				{
					this.ZipFiles.Add(Path.Combine(this.OutputDirectory, string.Format("{0}-{1}{2}.zip", this.Name, platform, Generator.zipSuffix[this.TargetPlatform])));
				}
			}
			return true;
		}

		private bool GenerateItchIoPackages()
		{
			string outputDirectory = this.OutputDirectory;
			this.GenerateMacOSPackage(true, (string path) => File.WriteAllText(Path.Combine(path, ".itch.toml"), string.Format("\n[[actions]]\nname = \"play\"\npath = \"{0}.app\"\n", this.Name)));
			this.GenerateWindowsPackage(true, (string path) => File.WriteAllText(Path.Combine(path, string.Concat(this.Name, ".Windows"), ".itch.toml"), string.Format("\n[[actions]]\nname = \"play\"\npath = \"{0}.exe\"\n\n[[prereqs]]\nname = \"net-4.5.2\"\n", this.Name)));
			this.GenerateGenericPackage(Generator.Platform.Linux, true, (string path) => File.WriteAllText(Path.Combine(path, string.Concat(this.Name, ".Linux"), ".itch.toml"), string.Format("\n[[actions]]\nname = \"play\"\npath = \"{0}\"\n", this.Name)));
			return true;
		}

		private bool GenerateMacOSPackage(bool createZip = true, Action<string> additionalFiles = null)
		{
			string temporaryDirectory = this.TemporaryDirectory;
			if (string.IsNullOrEmpty(temporaryDirectory))
			{
				temporaryDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			}
			if (!Directory.Exists(temporaryDirectory))
			{
				Directory.CreateDirectory(temporaryDirectory);
			}
			string str = Path.Combine(temporaryDirectory, string.Format("{0}.app", this.Name), "Contents");
			string str1 = Directory.GetFiles(this.SourceDirectory, "*.exe", SearchOption.TopDirectoryOnly).FirstOrDefault<string>();
			if (str1 == null)
			{
				throw new FileNotFoundException(string.Format("Could not find executable in {0}", this.SourceDirectory));
			}
			Directory.CreateDirectory(Path.Combine(str, "MacOS"));
			Directory.CreateDirectory(Path.Combine(str, "MonoBundle"));
			Directory.CreateDirectory(Path.Combine(str, "Resources"));
			string[] files = Directory.GetFiles(this.SourceDirectory, "*.*", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < (int)files.Length; i++)
			{
				string str2 = files[i];
				if (!(Path.GetExtension(str2) == ".zip") && (int)((new FileInfo(str2)).Attributes & FileAttributes.Hidden) == 0)
				{
					File.Copy(str2, Path.Combine(str, "MonoBundle", Path.GetFileName(str2)), true);
				}
			}
			this.ExtractExeAndMcs(Generator.Platform.MacOS, str1, str, "MacOS");
			this.ExtractAssemblies(str, "MonoBundle");
			this.ExtractRuntime(Generator.Platform.MacOS, str, "MonoBundle");
			if (this.RequiresCompression)
			{
				this.ExtractNativeDependencies("libMonoPosixHelper.dylib", str, Path.Combine("MonoBundle", "libMonoPosixHelper.dylib"));
			}
			this.ExtractMonoConfig(str, "Resources");
			this.CopyContent(str, "Resources");
			if (string.IsNullOrWhiteSpace(this.Icon) || !File.Exists(this.Icon))
			{
				this.ExtractResourceTo("App.icns", Path.Combine(str, "Resources", "App.icns"));
			}
			else if (Path.GetExtension(this.Icon) != ".icns")
			{
				if (Path.GetExtension(this.Icon) != ".png")
				{
					throw new NotSupportedException("Only .icns or .png files are supported. .png files MUST be 1024x1024");
				}
				string str3 = Path.Combine(str, "Resources", "App.iconset");
				Directory.CreateDirectory(str3);
				try
				{
					File.Copy(this.Icon, Path.Combine(str3, "icon_1024x1024.png"), true);
					foreach (KeyValuePair<string, int> icnsFile in Generator.icnsFiles)
					{
						this.RunSips(this.Icon, icnsFile.Value, Path.Combine(str3, icnsFile.Key));
					}
					this.RunIconutil(str3, Path.Combine(str, "Resources", "App.icns"));
				}
				finally
				{
					Directory.Delete(str3, true);
				}
			}
			else
			{
				File.Copy(this.Icon, Path.Combine(str, "Resources", "App.icns"));
			}
			string str4 = Path.Combine(str, "Info.plist");
			if (string.IsNullOrEmpty(this.InfoPlist) || !File.Exists(this.InfoPlist))
			{
				this.ExtractResourceTo("Info.plist", str4);
				this.ReplaceTagInFile(str4, "{Name}", this.Name);
				this.ReplaceTagInFile(str4, "{PackageID}", this.PackageId);
				this.ReplaceTagInFile(str4, "{Copyright}", this.Copyright);
				this.ReplaceTagInFile(str4, "{Exe}", Path.GetFileNameWithoutExtension(str1));
				this.ReplaceTagInFile(str4, "{DisplayName}", this.DisplayName);
			}
			else
			{
				File.Copy(this.InfoPlist, str4);
			}
			if (additionalFiles != null)
			{
				additionalFiles(temporaryDirectory);
			}
			if (createZip)
			{
				if (File.Exists(Path.Combine(this.OutputDirectory, string.Format("{0}-MacOS{1}.zip", this.Name, Generator.zipSuffix[this.TargetPlatform]))))
				{
					File.Delete(Path.Combine(this.OutputDirectory, string.Format("{0}-MacOS{1}.zip", this.Name, Generator.zipSuffix[this.TargetPlatform])));
				}
				Directory.CreateDirectory(this.OutputDirectory);
				ZipFile.CreateFromDirectory(temporaryDirectory, Path.Combine(this.OutputDirectory, string.Format("{0}-MacOS{1}.zip", this.Name, Generator.zipSuffix[this.TargetPlatform])), new string[] { Path.GetFileNameWithoutExtension(str1) });
				if (File.Exists(Path.Combine(this.OutputDirectory, string.Format("{0}-MacOS{1}.zip", this.Name, Generator.zipSuffix[this.TargetPlatform]))))
				{
					this.ZipFiles.Add(Path.Combine(this.OutputDirectory, string.Format("{0}-MacOS{1}.zip", this.Name, Generator.zipSuffix[this.TargetPlatform])));
				}
			}
			return true;
		}

		public bool GeneratePackage()
		{
			if (!this.SupportedPlatforms.HasFlag(this.TargetPlatform))
			{
				return false;
			}
			switch (this.TargetPlatform)
			{
				case Generator.Platform.MacOS:
				{
					return this.GenerateMacOSPackage(true, null);
				}
				case Generator.Platform.Linux:
				{
					return this.GenerateGenericPackage(this.TargetPlatform, true, null);
				}
				case Generator.Platform.Steam:
				{
					return this.GenerateSteamBundles();
				}
				case Generator.Platform.Itchio:
				{
					return this.GenerateItchIoPackages();
				}
				case Generator.Platform.Windows:
				{
					return this.GenerateWindowsPackage(true, null);
				}
			}
			return false;
		}

		private bool GenerateSteamBundles()
		{
			string outputDirectory = this.OutputDirectory;
			this.GenerateMacOSPackage(true, null);
			this.GenerateWindowsPackage(true, null);
			this.GenerateGenericPackage(Generator.Platform.Linux, true, null);
			return true;
		}

		private bool GenerateWindowsPackage(bool createZip = true, Action<string> additionalFiles = null)
		{
			int i;
			string temporaryDirectory = this.TemporaryDirectory;
			if (string.IsNullOrEmpty(temporaryDirectory))
			{
				temporaryDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			}
			if (!Directory.Exists(temporaryDirectory))
			{
				Directory.CreateDirectory(temporaryDirectory);
			}
			string str = Path.Combine(temporaryDirectory, string.Format("{0}.Windows", this.Name));
			if (Directory.GetFiles(this.SourceDirectory, "*.exe", SearchOption.TopDirectoryOnly).FirstOrDefault<string>() == null)
			{
				throw new FileNotFoundException(string.Format("Could not find executable in {0}", this.SourceDirectory));
			}
			Directory.CreateDirectory(str);
			string[] files = Directory.GetFiles(this.SourceDirectory, "*.*", SearchOption.TopDirectoryOnly);
			for (i = 0; i < (int)files.Length; i++)
			{
				string str1 = files[i];
				if (!(Path.GetExtension(str1) == ".zip") && !(Path.GetExtension(str1) == ".dylib") && (int)((new FileInfo(str1)).Attributes & FileAttributes.Hidden) == 0)
				{
					File.Copy(str1, Path.Combine(str, Path.GetFileName(str1)), true);
				}
			}
			if (Directory.Exists(Path.Combine(this.SourceDirectory, "x86")))
			{
				Directory.CreateDirectory(Path.Combine(str, "x86"));
				files = Directory.GetFiles(Path.Combine(this.SourceDirectory, "x86"), "*.dll", SearchOption.TopDirectoryOnly);
				for (i = 0; i < (int)files.Length; i++)
				{
					string str2 = files[i];
					if ((int)((new FileInfo(str2)).Attributes & FileAttributes.Hidden) == 0)
					{
						File.Copy(str2, Path.Combine(str, "x86", Path.GetFileName(str2)), true);
					}
				}
			}
			if (Directory.Exists(Path.Combine(this.SourceDirectory, "x64")))
			{
				Directory.CreateDirectory(Path.Combine(str, "x64"));
				files = Directory.GetFiles(Path.Combine(this.SourceDirectory, "x64"), "*.dll", SearchOption.TopDirectoryOnly);
				for (i = 0; i < (int)files.Length; i++)
				{
					string str3 = files[i];
					if ((int)((new FileInfo(str3)).Attributes & FileAttributes.Hidden) == 0)
					{
						File.Copy(str3, Path.Combine(str, "x64", Path.GetFileName(str3)), true);
					}
				}
			}
			if (!string.IsNullOrEmpty(this.AdditionalAssemblies))
			{
				files = this.AdditionalAssemblies.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
				for (i = 0; i < (int)files.Length; i++)
				{
					string str4 = files[i];
					if (File.Exists(str4))
					{
						File.Copy(str4, Path.Combine(str, string.Empty, Path.GetFileName(str4)), true);
					}
				}
			}
			this.CopyContent(str, null);
			if (additionalFiles != null)
			{
				additionalFiles(temporaryDirectory);
			}
			if (createZip)
			{
				if (File.Exists(Path.Combine(this.OutputDirectory, string.Format("{0}-Windows{1}.zip", this.Name, Generator.zipSuffix[this.TargetPlatform]))))
				{
					File.Delete(Path.Combine(this.OutputDirectory, string.Format("{0}-Windows{1}.zip", this.Name, Generator.zipSuffix[this.TargetPlatform])));
				}
				Directory.CreateDirectory(this.OutputDirectory);
				ZipFile.CreateFromDirectory(str, Path.Combine(this.OutputDirectory, string.Format("{0}-Windows{1}.zip", this.Name, Generator.zipSuffix[this.TargetPlatform])), Array.Empty<string>());
				if (File.Exists(Path.Combine(this.OutputDirectory, string.Format("{0}-Windows{1}.zip", this.Name, Generator.zipSuffix[this.TargetPlatform]))))
				{
					this.ZipFiles.Add(Path.Combine(this.OutputDirectory, string.Format("{0}-Windows{1}.zip", this.Name, Generator.zipSuffix[this.TargetPlatform])));
				}
			}
			return true;
		}

		private void MakeExecutable(string exeFile)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				UnixFileInfo unixFileInfo = new UnixFileInfo(exeFile);
				
				unixFileInfo.FileAccessPermissions = (unixFileInfo.FileAccessPermissions | (FileAccessPermissions) 72);
			}
		}

		private void ReplaceTagInFile(string fileName, string tag, string value)
		{
			string str = File.ReadAllText(fileName);
			File.WriteAllText(fileName, str.Replace(tag, value));
		}

		private void RunIconutil(string sourceDirectory, string outputFile)
		{
			Process.Start(new ProcessStartInfo("iconutil")
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				Arguments = string.Format("--convert icns --out \"{0}\" \"{1}\" ", outputFile, sourceDirectory)
			}).WaitForExit();
		}

		private void RunSips(string sourceImage, int size, string outputImage)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo("sips")
			{
				CreateNoWindow = true,
				Arguments = string.Format("-z {0} {1} \"{2}\" --out \"{3}\"", new object[] { size, size, sourceImage, outputImage }),
				UseShellExecute = false
			};
			Process.Start(processStartInfo).WaitForExit();
		}

		[Flags]
		public enum Platform
		{
			MacOS,
			Linux,
			Steam,
			Itchio,
			Windows
		}
	}
}