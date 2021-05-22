//   
// Copyright (c) Jesse Freeman, Tiny Card. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by TinyCard are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Tiny Card contributors:
//  
// Jesse Freeman - @JesseFreeman
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Input;

namespace PixelVision8.Player
{
    public class MenuAction : Attribute {
        
        public MethodInfo MethodInfo;

    }

    public partial class SceneStackEditor : Scene
    {
        public const string STACK_EDITOR = "SceneStackEditor";

        private MenuBar _navBar;
        private string _currentStackName;

        private Dictionary<string, MethodInfo> _menuActions = new Dictionary<string, MethodInfo>();

        private Rectangle _idRect = new Rectangle();

        private PixelData backgroundPixelData;

        private string _label = "none";

        public SceneStackEditor(UIBuilder uiBuilder) : base(uiBuilder, STACK_EDITOR)
        {
            AutoReset = false;

            // This are fixed and don't change from card to card
            _idRect.Y = Height - 12;
            _idRect.Height = 8;

            

        }
        
        public override void Draw()
        {
            base.Draw();
            
            // Draw background for card ID offset by 1 pixel all around for a boarder
            DrawRect(_idRect.X - 2, _idRect.Y - 1, _idRect.Width + 3, _idRect.Height + 2, 0);

            // Draw card Id on top of background rect
            DrawText(_label, _idRect.X, _idRect.Y, DrawMode.Sprite, "medium", 1, -4);

        }

        public void CreateBackground()
        {
            // There is no tilemap so use this canvas to store the background
            var bgCanvas = NewCanvas(Width, Height);
            bgCanvas.Clear(1);
            
            // Set the canvas stroke
            bgCanvas.SetStroke(0, 1);
            
            // Create top left and right corners
            bgCanvas.SetPixels(0, 0, 2, 2, new[]{0,0,0,1});
            bgCanvas.SetPixels(Width - 2, 0, 2, 2, new[]{0,0,1,0});

            // Draw nav bar
            bgCanvas.DrawLine(0, 9, Width, 9);

            backgroundPixelData = new PixelData(Width, Height);
            backgroundPixelData.SetPixels(bgCanvas.GetPixels());
            
        }
        

        public void CreateNavBar()
        {

            // Create the nav bar
            _navBar = _uiBuilder.CreateMenuBar("NavBar", x: 4, autoManage: false);
            _navBar.OnSelection = new Action<string>(OnSelection);
            _navBar.OnOpen = new Action<string>(OnMenuOpen);
            _navBar.OnClose = new Action(OnMenuClose);

            CreateAppMenu();
            CreateFileMenu();
            CreateEditMenu();
            CreateGoMenu();
            CreateToolsMenu();
            CreateOptionsMenu();

            // We want the nav to percists so add it to the UI Builder
            _uiBuilder.AddUI(_navBar);

            RegisterNavActions();
        }

        private void OnMenuOpen(string name)
        {
            CurrentCard.Pause = true;
        }

        private void OnMenuClose()
        {
            CurrentCard.Pause = false;
        }

        public void RegisterNavActions()
        {
            // TODO find all callback actions

            var methods = GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(MenuAction), false).Length > 0)
                .ToArray();

            for (int i = 0; i < methods.Length; i++)
            {
                
                Type thisType = this.GetType();
                
                var methodName = methods[i].Name; 
                var methodInfo = thisType.GetMethod(methods[i].Name);
                
                if(_menuActions.ContainsKey(methodName))
                {
                    _menuActions[methodName] = methodInfo;
                }
                else
                {
                    _menuActions.Add(methodName, methodInfo);
                }

            }
        }

        public void OnSelection(string methodName)
        {
            if(methodName == string.Empty)
                return;

            if(_menuActions.ContainsKey(methodName))
            {
                Console.WriteLine("Call new action {0}", methodName);

                _menuActions[methodName].Invoke(this, new object[] {});
            }
            else
            {
                Console.WriteLine("Error: Could not find menu action '{0}'.", methodName);
            }
            
        }

        public void CreateAppMenu()
        {
            
            var options = new List<MenuOption>()
            {
                new MenuOption("about", "about tooltip."),
                MenuOption.Divider,
                new MenuOption("help", "open tooltip.", "Help"),
                new MenuOption("settings", "new tooltip.", "Settings"),
                MenuOption.Divider,
                new MenuOption("quit", "quit tooltip.", "Quit", new Shortcut(Keys.Q))
            };

            _navBar.BuildTextMenu("*", options, "App tooltip.");

        }

        public void CreateFileMenu()
        {
            
            var options = new List<MenuOption>()
            {
                // new MenuOption("about", "about tooltip."),
                // MenuOption.Divider,
                new MenuOption("new", "new tooltip.", "New", new Shortcut(Keys.N)),
                new MenuOption("open", "open tooltip.", "Open", new Shortcut(Keys.O), enabled: false),
                new MenuOption("save", "save tooltip.", "Save", new Shortcut(Keys.S)),
                new MenuOption("delete", "delete tooltip.", "Delete"),
                MenuOption.Divider,
                new MenuOption("import", "import tooltip.", "Import", enabled: false),
                new MenuOption("export", "export tooltip.", "Export"),
                // MenuOption.Divider,
                // new MenuOption("quit", "quit tooltip.", "Quit", new Shortcut(Keys.Q))
            };

            _navBar.BuildTextMenu("File", options, "File tooltip.");

        }

        public void CreateEditMenu()
        {

            var options = new List<MenuOption>()
            {
                new MenuOption("Cut", "empty tooltip.", string.Empty, new Shortcut(Keys.X, true), enabled: false),
                new MenuOption("Copy", "empty tooltip.", string.Empty, new Shortcut(Keys.C, true), enabled: false),
                new MenuOption("Paste", "empty tooltip.", string.Empty, new Shortcut(Keys.V, true), enabled: false),
                new MenuOption("Delete", "empty tooltip.", string.Empty, new Shortcut(Keys.D, true), enabled: false),
                MenuOption.Divider,
                new MenuOption("New Card", "empty tooltip.", "OnNewCard", new Shortcut(Keys.N, true, true)),
                new MenuOption("Copy Card", "empty tooltip.", string.Empty, new Shortcut(Keys.C, true, true)),
                new MenuOption("Paste Card", "empty tooltip.", string.Empty, new Shortcut(Keys.V, true, true), enabled: false),
                new MenuOption("Delete Card", "empty tooltip.", "OnDeleteCard", new Shortcut(Keys.D, true, true)),
                MenuOption.Divider,
                new MenuOption("Bring Closer", "empty tooltip.", "OnBringCloser", enabled: false),
                new MenuOption("Send Farther", "empty tooltip.", "OnSendFarther"),
                MenuOption.Divider,
                new MenuOption("Select All", "empty tooltip.", string.Empty, new Shortcut(Keys.A, true)),
                // new MenuOption("Edit Pattern", "empty tooltip.", string.Empty, new Shortcut(Keys.E, true, true)),
                new MenuOption("Background", "empty tooltip.", "OnEditBackground"),
            };

            _navBar.BuildTextMenu("Edit", options);
        }

        public void CreateGoMenu()
        {
            var options = new List<MenuOption>()
            {
                new MenuOption("Go To", "empty tooltip.", "GoTo", new Shortcut(Keys.G)), // TODO this needs to show a pop up
                MenuOption.Divider,
                new MenuOption("First", "empty tooltip.", "FirstCard", new Shortcut(Keys.Up, false), enabled: false),
                new MenuOption("Last", "empty tooltip.", "LastCard", new Shortcut(Keys.Down, false)),
                new MenuOption("Back", "empty tooltip.", "PreviousCard", new Shortcut(Keys.Left, false), enabled: false),
                new MenuOption("Next", "empty tooltip.", "NextCard", new Shortcut(Keys.Right, false)),
            };

            _navBar.BuildTextMenu("Go", options);
        }

        public void CreateToolsMenu()
        {
            
            var options = new List<MenuOption>()
            {
                new MenuOption("pointer", "empty tooltip.", "SelectPointerTool", new Shortcut(Keys.V, false)){Sprite = "tool-pointer"},
                new MenuOption("button", "tooltip.", "SelectButtonTool"){Sprite = "tool-button"},
                new MenuOption("textfield", "empty tooltip.", "SelectTextFieldTool"){Sprite = "tool-textfield"},

                MenuOption.Divider,
                MenuOption.Divider,
                MenuOption.Divider,

                new MenuOption("selection", "empty tooltip.", "SelectSelectionTool", new Shortcut(Keys.M, false)){Sprite = "tool-selection"},
                new MenuOption("pen", "empty tooltip.", "SelectPenTool", new Shortcut(Keys.B, false)){Sprite = "tool-pen"},
                new MenuOption("eraser", "empty tooltip.", "SelectEraserTool", new Shortcut(Keys.E, false)){Sprite = "tool-eraser"},
               
                new MenuOption("text", "empty tooltip.", "SelectTextTool", new Shortcut(Keys.T, false)){Sprite = "tool-text"},
                new MenuOption("brush", "empty tooltip.", "SelectBrushTool", new Shortcut(Keys.V, false, true)){Sprite = "tool-brush"},
                new MenuOption("line", "empty tooltip.", "SelectLineTool", new Shortcut(Keys.L, false)){Sprite = "tool-line"},

                new MenuOption("rectangle", "empty tooltip.", "SelectRectangleTool", new Shortcut(Keys.U, false)){Sprite = "tool-box"},
                new MenuOption("ellipse", "empty tooltip.", "SelectEllipseTool", new Shortcut(Keys.U, false, true)){Sprite = "tool-circle"},
                new MenuOption("fill", "empty tooltip.", "SelectFillTool", new Shortcut(Keys.F, false)){Sprite = "tool-fill"},
            };

            _navBar.BuildButtonMenu("Tools", options);

        }
        
        public void CreateOptionsMenu()
        {
            
            var options = new List<MenuOption>()
            {
                new MenuOption("Button Info", "empty tooltip.", string.Empty, enabled: false),
                new MenuOption("Field Info", "tooltip.", string.Empty, enabled: false),
                new MenuOption("Card Info", "empty tooltip.", string.Empty),
                new MenuOption("Deck Info", "empty tooltip.", string.Empty),
                MenuOption.Divider,
                new MenuOption("Bring Closer", "empty tooltip.", string.Empty, new Shortcut(Keys.Up, false), enabled: false),
                new MenuOption("Send Farther", "empty tooltip.", string.Empty, new Shortcut(Keys.Down, false), enabled: false),
                MenuOption.Divider,
                new MenuOption("New Button", "empty tooltip.", "OnNewButton", new Shortcut(Keys.B, true)),
                new MenuOption("New Field", "empty tooltip.", string.Empty, new Shortcut(Keys.F, true)),
                MenuOption.Divider,
                new MenuOption("Grid", "empty tooltip.", "ToggleGrid", new Shortcut(Keys.G, true, true)),
                new MenuOption("Toggle Filled", "empty tooltip.", "ToggleFilled", new Shortcut(Keys.F, true, true)),
                new MenuOption("Brush Size", "empty tooltip.", "OpenBrushOptions", new Shortcut(Keys.B, true, true)),
                new MenuOption("Patterns", "empty tooltip.", "OpenPatternPicker", new Shortcut(Keys.P, true, true)),
            };

            _navBar.BuildTextMenu("Options", options);
        }

        public override void Activate()
        {

            if(_firstRun)
            {

                // Create background
                CreateBackground();

                CreateNavBar();

                NewStack();

            }

            base.Activate();

        }

        public void NewStack(string name = "Untitled Stack")
        {

            ClearStack();

            _currentStackName = name;

            ChangeMode(Modes.Pointer);

        }

        [MenuAction]
        public void OnNewCard()
        {
            // TODO need to open up an editor
            NewCard();
        }

        [MenuAction]
        public void OnDeleteCard()
        {
            // TODO need to open up an editor
            DeleteCard();
        }

        [MenuAction]
        public void OnEditBackground()
        {
            GoToCard(0);
        }

        [MenuAction]
        public void OnNewButton() 
        {
            ChangeMode(Modes.Button);

            CurrentCard.NewButton();
        }

    }

}