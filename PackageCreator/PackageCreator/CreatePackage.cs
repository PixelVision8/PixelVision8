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

using InfinitespaceStudios.MonoGame.PackageCreator;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace InfinitespaceStudios.MonoGame.PackagingTasks
{
	public class CreatePackage : Task
	{
		public ITaskItem[] AdditionalAssemblies
		{
			get;
			set;
		}

		public ITaskItem[] Assemblies
		{
			get;
			set;
		}

		[Required]
		public ITaskItem ContentDirectory
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

		public ITaskItem InfoPlist
		{
			get;
			set;
		}

		[Required]
		public string Name
		{
			get;
			set;
		}

		[Required]
		public ITaskItem OutputDirectory
		{
			get;
			set;
		}

		[Output]
		public ITaskItem[] Outputs
		{
			get;
			set;
		}

		public string PackageId
		{
			get;
			set;
		}

		[Required]
		public ITaskItem SourceDirectory
		{
			get;
			set;
		}

		[Required]
		public string TargetPlatforms
		{
			get;
			set;
		}

		public ITaskItem TemporaryDirectory
		{
			get;
			set;
		}

		public CreatePackage()
		{
		}

		public override bool Execute()
		{
			Generator.Platform platform;
			string itemSpec;
			Generator generator = new Generator();
			bool flag = true;
			string[] strArrays = this.TargetPlatforms.Split(new char[] { ';' });
			int num = 0;
			while (num < (int)strArrays.Length)
			{
				string str = strArrays[num];
				if (Enum.TryParse<Generator.Platform>(str, out platform))
				{
					generator.Name = this.Name;
					generator.Copyright = this.Copyright;
					generator.DisplayName = this.DisplayName;
					generator.Icon = this.Icon;
					generator.PackageId = this.PackageId;
					generator.TargetPlatform = platform;
					generator.ContentDirectory = this.ContentDirectory.ItemSpec;
					generator.OutputDirectory = this.OutputDirectory.ItemSpec;
					generator.SourceDirectory = this.SourceDirectory.ItemSpec;
					if (this.InfoPlist != null)
					{
						generator.InfoPlist = this.InfoPlist.ItemSpec;
					}
					ITaskItem temporaryDirectory = this.TemporaryDirectory;
					if (temporaryDirectory != null)
					{
						itemSpec = temporaryDirectory.ItemSpec;
					}
					else
					{
						itemSpec = null;
					}
					if (!string.IsNullOrEmpty(itemSpec))
					{
						generator.TemporaryDirectory = this.TemporaryDirectory.ItemSpec;
					}
					if (this.Assemblies != null)
					{
						if (((IEnumerable<ITaskItem>)this.Assemblies).Any<ITaskItem>((ITaskItem x) => Path.GetFileNameWithoutExtension(x.ItemSpec) == "System.IO.Compression"))
						{
							generator.RequiresCompression = true;
						}
					}
					if (this.AdditionalAssemblies != null)
					{
						generator.AdditionalAssemblies = string.Join(";", 
							from x in (IEnumerable<ITaskItem>)this.AdditionalAssemblies
							select x.ItemSpec);
					}
					try
					{
						flag &= generator.GeneratePackage();
						if (!flag)
						{
							base.Log.LogWarning(string.Format("{0} is not supported on this Operating System", str), Array.Empty<object>());
						}
						if (flag)
						{
							this.Outputs = (
								from x in generator.ZipFiles
								select new TaskItem(x)).ToArray<TaskItem>();
						}
					}
					catch (Exception exception)
					{
						base.Log.LogErrorFromException(exception);
					}
					num++;
				}
				else
				{
					base.Log.LogError(string.Format("Unknown Target Platform {0}. Valid Platforms are {1}", str, string.Join(",", Enum.GetNames(typeof(Generator.Platform)))), Array.Empty<object>());
					break;
				}
			}
			return !base.Log.HasLoggedErrors;
		}
	}
}