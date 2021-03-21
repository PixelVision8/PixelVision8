//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
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

using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using PixelVision8.Workspace;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PixelVision8.Runner
{
    public class ScriptLoaderUtil : IScriptLoader
    {
        private readonly WorkspaceService _workspace;

        public ScriptLoaderUtil(WorkspaceService workspace)
        {
            _workspace = workspace;
        }

        public bool ScriptFileExists(string name)
        {
            throw new System.NotImplementedException();
        }

        public object LoadFile(string file, Table globalContext)
        {
            
            List<WorkspacePath> sharedLibPaths = _workspace.SharedLibDirectories();

            sharedLibPaths.Insert(0, WorkspacePath.Root.AppendDirectory("Game"));

            // Loop through all the different locations
            string script = "";

            for (int i = 0; i < sharedLibPaths.Count; i++)
            {
                //
                // var scripts = _workspace.GetEntities(sharedLibPaths[i]);

                script = _workspace.ReadTextFromFile(sharedLibPaths[i].AppendFile(file));

                if (!string.IsNullOrEmpty(script))
                {
                    // Replace math operators
                    var pattern = @"(\S+)\s*([+\-*/%])\s*=";
                    var replacement = "$1 = $1 $2 ";
                    script = Regex.Replace(script, pattern, replacement, RegexOptions.Multiline);

                    // Replace != conditions
                    pattern = @"!\s*=";
                    replacement = "~=";
                    script = Regex.Replace(script, pattern, replacement, RegexOptions.Multiline);

                    return script;
                }
            }

            _workspace.UpdateLog($"Could not load '{file}' file because it is either missing or empty.",
                LogType.Warning);

            return script;

        }

        public string ResolveFileName(string filename, Table globalContext)
        {
            // Add extension
            if (!filename.EndsWith(".lua"))
            {
                filename = filename + ".lua";
            }

            if (filename.StartsWith("/"))
            {
                // TODO need to make sure this is an relative path
            }

            return filename;
        }

        public string ResolveModuleName(string modname, Table globalContext)
        {
            if (!modname.EndsWith(".lua"))
            {
                modname = modname + ".lua";
            }

            if (modname.StartsWith("/"))
            {
                // TODO need to make sure this is an relative path
            }

            List<WorkspacePath> sharedLibPaths = _workspace.SharedLibDirectories();

            sharedLibPaths.Insert(0, WorkspacePath.Root.AppendDirectory("Game"));
            WorkspacePath filePath;

            foreach (var path in sharedLibPaths)
            {
                filePath = path.AppendFile(modname);
                if (_workspace.Exists(filePath))
                {
                    modname = _workspace.GetPhysicalPath(filePath);
                    break;
                }
            }

            // _workspace.GetPhysicalPath()

            return modname;
        }
    }
}