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
using Microsoft.Xna.Framework.Input;

namespace PixelVision8.Player
{

    public struct MenuOption
    {

        public static MenuOption Divider => new MenuOption(){Text="divider", IsDivider = true};

        public string Text;
        public string Tooltip;
        public string ActionName;
        public Shortcut? Shortcut;
        public bool IsDivider;
        public string Sprite;
        public bool Enabled;

        public MenuOption(string text, string tooltip = "", string actionName = "", Shortcut? shortcut = null, bool autoCaps = true, bool enabled = true, string sprite = "")
        {
            if(autoCaps)
                text = text.ToUpper();

            Text = text;
            Tooltip = tooltip;
            ActionName = actionName;
            Shortcut = shortcut;
            IsDivider = false; 
            Sprite = sprite;
            Enabled = enabled;
        }

    }

    public struct Shortcut
    {
        public Keys Key;
        public bool Shift;
        public bool Control;

        public Shortcut(Keys key, bool control = true, bool shift = false)
        {
            Key = key;
            Control = control;
            Shift = shift;
        }

    }

    public class MenuBar : EntityManager
    {
        private ModalPanel modalToOpen;
        private int frameCount = 0;
        public Action<string> OnSelection;
        public Action<string> OnOpen;
        public Action OnClose;

        private Dictionary<string, Shortcut> shortcuts = new Dictionary<string, Shortcut>();

        private Dictionary<string, ModalPanel> _menus = new Dictionary<string, ModalPanel>();
        private string _lastMenu;

        public MenuBar(UIBuilder uiBuilder, string name = "", int x = 0, int y = 0) : base(uiBuilder:uiBuilder, name:name, x:x, y:y, autoResize: false)
        {
            Layout =  EntityLayout.Horizontal;

            FindMenuActions();
        }

        public void FindMenuActions()
        {
            // TODO look for custom attributes to map to the buttons
        }

        public override void Update(int timeDelta)
        {
            if(Pause)
                return;

            // Reset the mouse
            _uiBuilder.CursorID = Cursors.Pointer;

            for (int i = 0; i < _total; i++)
            {
                var tmpEntity = _entities[i] as MenuTextButton;

                if(tmpEntity != null)
                {
                    tmpEntity.CurrentState = (modalToOpen != null && modalToOpen.Name == tmpEntity.Name) ? InteractiveStates.Down : InteractiveStates.Up;

                    // Test to see if the mouse is inside of the menu
                    if(_uiBuilder.CollisionManager.MouseInRect(tmpEntity.Rect))
                    {
                        
                        // When over a menu button, show the hand
                        _uiBuilder.CursorID = Cursors.Hand;

                        var mouseDown = _uiBuilder.CollisionManager.MouseDown;

                        if(modalToOpen != null && modalToOpen.Name != tmpEntity.Name)
                        {
                            CloseMenu();

                            // force the mouse to be down since the other menu is open and you are over this button
                            mouseDown = true;
                        }

                        if( _uiBuilder.CollisionManager.MouseReleased)
                        {
                            if(modalToOpen != null && modalToOpen.Name == tmpEntity.Name)
                            {
                                // CloseMenu();
                                mouseDown= false;
                            }
                        }

                        if(mouseDown)
                        {
                            
                            if(modalToOpen == null && _menus.ContainsKey(tmpEntity.Name))
                            {
                                
                                OpenMenu(tmpEntity.Name, tmpEntity.X, Height);
                                
                            }

                        }
                        

                    }
                    
                    tmpEntity.DisplayState(DrawMode.Sprite);
                }
                

            }

            var shiftDown = _gameChip.Key(Keys.LeftShift) || _gameChip.Key(Keys.RightShift);
            var controlDown = _gameChip.Key(Keys.LeftControl) || _gameChip.Key(Keys.RightControl);

            // TODO need to listen for shortcuts
            foreach (var shortcut in shortcuts)
            {
                if(_gameChip.Key(shortcut.Value.Key, InputState.Released))
                {
                    // Console.WriteLine("Input {0} {1} {2} {3} {4} {5}", shortcut.Value.Key, (shortcut.Value.Control == controlDown) && (shortcut.Value.Shift == shiftDown), shortcut.Value.Control, controlDown, shortcut.Value.Shift, shiftDown);
                    
                    if((shortcut.Value.Control == controlDown) && (shortcut.Value.Shift == shiftDown))
                        OnSelection(shortcut.Key);
                }
            }

        }

        public void OpenMenu(string name, int x = 0, int y = 0)
        {
            modalToOpen = _menus[name];

            modalToOpen.X = x + 1;
            modalToOpen.Y = y + 1;
            modalToOpen.Open();

            // Console.WriteLine("Open Menu {0}", name);

        }

        public void CloseMenu()
        {
            // Console.WriteLine("Close Menu {0}", modalToOpen.Name);

            _uiBuilder.CloseAllModals();
            modalToOpen = null;
            _lastMenu = string.Empty;

            

        }

        protected ModalPanel CreateTextMenuModal(string name)
        {
            return new TinyCardModalPanel(_uiBuilder, name, new Rectangle(-2, 0, 5, 3));
        }

        protected ModalPanel CreateButtonMenuModal(string name)
        {
            return new TinyCardModalPanel(_uiBuilder, name, new Rectangle(-2, 0, 4, 2));
        }



        protected TextButton CreateOptionTextButton(string name, string tooltip, Shortcut? shortcut = null, int width = 0)
        {
            return new OptionTextButton(_uiBuilder, name, tooltip, shortcut, width);
        }

        protected Button CreateOptionSpriteButton(string name, string spriteName, string tooltip)
        {
            return new OptionSpriteButton(editorUI: _uiBuilder, name: name, spriteName: spriteName, tooltip: tooltip);
        }
        

        private void NewMenuOption(string name, string tooltip)
        {

            // Create the button
            var btn  = new MenuTextButton(_uiBuilder, text: name, tooltip: tooltip);

            // Add the buttons to the navbar
            Add(btn);
            
        }

        public void BuildTextMenu(string name, List<MenuOption> options, string tooltip = "")
        {
            
            name = name.ToUpper();

            var maxWidth = 0;

            var endPadding = 9;

            // Loop through and find the max width
            foreach (var option in options)
             {
                var tmpW = option.Text.Length + 2;

                if(option.Shortcut.HasValue)
                    tmpW += endPadding;
                
                maxWidth = Math.Max(maxWidth, tmpW * 4) - 1;
            }
            
            var panel = CreateTextMenuModal(name);
            
            panel.Layout = EntityLayout.Vertical;

            foreach (var option in options)
            {
                
                var btn = CreateOptionTextButton(option.Text, option.Tooltip, option.Shortcut, maxWidth);
                btn.Name = option.ActionName;

                btn.Enable(option.Enabled);

                // Console.WriteLine("Button Action {0}", btn.Name);
                
                btn.OnRelease = new Action<Button>(TriggerMenuOption);
                
                if(option.Shortcut.HasValue)
                {
                    RegisterShortcut(option.ActionName, option.Shortcut.Value);
                }

                panel.Add(btn);
            
            }
            
            NewMenuOption(name, tooltip);
            RegisterMenuPanel(panel);
            
        }

        public void RegisterShortcut(string actionName, Shortcut shortcut)
        {
            // Console.WriteLine("Register shortcut {0}", actionName);

            if(shortcuts.ContainsKey(actionName))
            {
                shortcuts[actionName] = shortcut;
            }
            else
            {
                shortcuts.Add(actionName, shortcut);
            }
        }

        public void TriggerMenuOption(Button button)
        {

            // Console.WriteLine("TriggerMenuOption {0}", button.Name);
            
            var action = button.Name;

            _uiBuilder.CloseAllModals();
            // _navBar.ClearSelections();

            if(OnSelection != null)
                OnSelection(button.Name);
        }

        public ModalPanel BuildButtonMenu(string name, List<MenuOption> options, string tooltip = "")
        {

            name = name.ToUpper();

            var panel = CreateButtonMenuModal(name);
            
            var layout = EntityLayout.IconGrid;
            layout.Columns = 3;

            panel.Layout = layout;

            foreach (var option in options)
            {
                
                var btn = CreateOptionSpriteButton( option.Text, option.Sprite, option.Tooltip);
                btn.Name = option.ActionName;

                btn.OnRelease = new Action<Button>(TriggerMenuOption);

                if(option.Shortcut.HasValue)
                {
                    RegisterShortcut(option.ActionName, option.Shortcut.Value);
                }
                
                panel.Add(btn);
            
            }

            NewMenuOption(name, tooltip);
            RegisterMenuPanel(panel);

            return panel;

        }

        public void RegisterMenuPanel(ModalPanel panel)
        {
            if(_menus.ContainsKey(panel.Name))
            {
                _menus[panel.Name] = panel;
            }
            else
            {
                _menus.Add(panel.Name, panel);
            }
            
            panel.OnOpen = new Action(OnOpenMenu);
            panel.OnClose = new Action(OnCloseMenu);
        }

        private void OnOpenMenu()
        {
            if(OnOpen != null)
                OnOpen(modalToOpen.Name);
        }

        public void OnCloseMenu()
        {
            modalToOpen = null;

            if(OnClose != null)
                OnClose();
            
        }

        public void EnableOption(string menuName, string optionName, bool value)
        {
            
            menuName = menuName.ToUpper();

            if(_menus.ContainsKey(menuName) == false)
                return;

            var menu = _menus[menuName];
            
            foreach (var entity in menu.Entities)
            {
                
                if(entity.Name == optionName)
                {
                    ((Button)entity).Enable(value);
                }
            }

        }

        public void SelectOption(string menuName, string optionName, bool value, bool singleSelection = true)
        {
            
            menuName = menuName.ToUpper();

            if(_menus.ContainsKey(menuName) == false)
                return;

            var menu = _menus[menuName];
            
            foreach (var entity in menu.Entities)
            {
                var btn = entity as Button;

                if(btn != null)
                {

                    if(btn.Name == optionName)
                    {
                        btn.Select(value);
                    }
                    else if(btn.Selected && singleSelection && value)
                    {
                        btn.Select(false);
                    }

                }
                

            }

        }


    }
}