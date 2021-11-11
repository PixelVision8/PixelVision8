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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using PixelVision8.Player;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace PixelVision8.Runner
{

    public class RoslynRunner : GameRunner
    {
        
        private KeyboardInputChip _controllerChip;

        // Store the path to the game's files
        // private readonly string gamePath;
        // private readonly string gameClass;
        
        /// <summary>
        ///     This constructor saves the path to the game's files and kicks off the base constructor
        /// </summary>
        /// <param name="gamePath"></param>
        public RoslynRunner(string gamePath) : base(gamePath)
        {
            // this.gamePath = gamePath;
            // this.gameClass = gameClass;
        }
        
        /// <summary>
        ///     This is called when the runner first starts up.
        /// </summary>
        protected override void Initialize()
        {

            // Configure the runner
            ConfigureDisplayTarget();
            
            // Manually override scale on boot up
            Scale(1);

            // Load the game
            LoadDefaultGame();
        }

        public override void ActivateEngine(PixelVision engine)
        {
            base.ActivateEngine(engine);
            
            // Reset the game's resolution before it loads up
            ResetResolution();

            // Make sure that the first frame is cleared with the default color
            ActiveEngine.GameChip.Clear();
        }

        /// <summary>
        ///     This mthod manually loads the game file's binary data then configures the engine and processes the files.
        /// </summary>
        private void LoadDefaultGame()
        {

            // Create a list of valid files we want to load from the game directory
            var fileExtensions = new[]
            {
                "cs", 
                "png",
                "json"
            };

            // Create a new dictionary to store the file binary data
            var gameFiles = new List<string>();

            // Get only the files we need from the directory base on their extension above.
            var files = from p in Directory.EnumerateFiles(RootPath)
                        where fileExtensions.Any(val => p.EndsWith(val))
                        select p;

            // Loop through each file in the list
            foreach (string file in files)
            {
                // Read the binary data and save it along with the file name to the dictionary.
                gameFiles.Add(file);
            }
            
            var chips = new List<string>
            {
                typeof(ColorChip).FullName,
                typeof(SpriteChip).FullName,
                typeof(TilemapChip).FullName,
                typeof(MusicChip).FullName,
                typeof(FontChip).FullName,
                typeof(ControllerChip).FullName,
                typeof(KeyboardInputChip).FullName,
                typeof(DisplayChip).FullName,
                typeof(SoundChip).FullName,
            };

            // Configure a new PV8 engine to play the game
            TmpEngine = new PixelVision(chips.ToArray());

            // Load the default class based on it's full package path
            BuildRoslynGameChip(gameFiles.FindAll(f => f.EndsWith(".cs")).ToArray());

            var fileHelper = new FileLoadHelper();
            var imageParser = new PNGParser(Graphics.GraphicsDevice);

            var loader = new Loader(fileHelper, imageParser);

            // Process the files
            loader.ParseFiles(gameFiles.ToArray(), TmpEngine);
            loader.LoadAll();

            RunGame();
            
            _controllerChip = ActiveEngine.KeyboardInputChip;

        }
        
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            if (_controllerChip.GetKeyDown(Keys.LeftControl) ||
                _controllerChip.GetKeyDown(Keys.LeftControl))
                if (_controllerChip.GetKeyUp(Keys.R) || _controllerChip.GetKeyUp(Keys.D4))
                    ResetGame();
        }

        private void ResetGame()
        {
            LoadDefaultGame();
        }
        
        public void BuildRoslynGameChip(string[] files, bool buildDebugData = true)
        {
            var total = files.Length;
            var syntaxTrees = new SyntaxTree[total];
            var embeddedTexts = new EmbeddedText[total];

            for (var i = 0; i < total; i++)
            {
                var path = Path.Combine(RootPath, files[i]);
                
                if (File.Exists(path))
                {
                    var data = File.ReadAllText(path);
                    syntaxTrees[i] = CSharpSyntaxTree.ParseText(data);
                    var st = SourceText.From(text: data, encoding: Encoding.UTF8);
                    embeddedTexts[i] = EmbeddedText.FromSource(files[i], st);
                }
                
            }

            //Compilation options, should line up 1:1 with Visual Studio since it's the same underlying compiler.
            var options = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: buildDebugData ? OptimizationLevel.Debug : OptimizationLevel.Release,
                moduleName: "RoslynGame");

            //This list of references is what limits the user from breaking out of the PV8 limitations through Roslyn.
            //The first few lines are mandatory references, with the later ones being common helpful core libraries.
            //We specifically exclude some from the list (System.IO.File, System.Net specifically for security, and System.Threading.Tasks to avoid bugs around parallel DrawX() calls).
            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Private.CoreLib").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Console").Location),
                MetadataReference.CreateFromFile(Assembly.Load("Microsoft.CSharp").Location),
                MetadataReference.CreateFromFile(Assembly.Load("Roslyn Runner").Location),
                MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location), //Required due to a .NET Standard 2.0 dependency somewhere.
                MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location), //required for Linq
                MetadataReference.CreateFromFile(Assembly.Load("System.Linq").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Linq.Expressions").Location),
            };

            var compiler = CSharpCompilation.Create("LoadedGame", syntaxTrees, references, options);

            //Compile the existing file into memory, or error out.
            var dllStream = new MemoryStream();
            var pdbStream = new MemoryStream();

            //This lets us get data if we hit a runtime error.
            var emitOptions = new EmitOptions(
                debugInformationFormat: DebugInformationFormat.PortablePdb
            );

            var compileResults = compiler.Emit(peStream: dllStream, pdbStream: pdbStream, embeddedTexts: embeddedTexts,
                options: emitOptions);
            if (compileResults.Success)
            {
                dllStream.Seek(0, SeekOrigin.Begin);
                pdbStream.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                var errors = compileResults.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
                var errorData = errors[0].Location.GetLineSpan();
                var lineNumber = errorData.StartLinePosition.Line + 1;
                var charNumber = errorData.StartLinePosition.Character + 1;
                
                Console.WriteLine("Error compiling");
                // DisplayError(ErrorCode.Exception,
                //     new Dictionary<string, string>
                //     {
                //         {
                //             "@{error}",
                //             errors.Count > 0
                //                 ? "Line " + lineNumber + " Pos " + charNumber + ": " + errors[0].GetMessage()
                //                 : "There was an unknown error trying to compile a C# file."
                //         }
                //     });
                return;
            }

            //Get the DLL into active memory so we can use it. Runtime errors will give the wrong line number if we're in Release mode, so don't include the pdbStream for that.
            var loadedAsm = Assembly.Load(dllStream.ToArray(), buildDebugData ? pdbStream.ToArray() : null);

            // TODO change this for net5.0
            // var roslynGameChipType = loadedAsm.GetTypes().Where(t => t.IsAssignableTo(typeof(GameChip))).FirstOrDefault();  //code.cs must use this namespace and class name.

            // TODO this is only for net3.1
            var roslynGameChipType =
                loadedAsm.GetTypes().Where(t => typeof(GameChip).IsAssignableFrom(t)).FirstOrDefault();

            //Could theoretically iterate over types until one that inherits from GameChip is found, but this strictness may be a better idea.

            dllStream.Close();
            dllStream.Dispose();
            pdbStream.Close();
            pdbStream.Dispose();

            Console.WriteLine("roslynGameChipType " + roslynGameChipType);

            if (roslynGameChipType != null)
            {
                TmpEngine.ActivateChip("GameChip",
                    (AbstractChip) Activator.CreateInstance(
                        roslynGameChipType)); //Inserts the DLL's GameChip descendent into the engine.
            }
        }

    }

   
}