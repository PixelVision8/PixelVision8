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

namespace PixelVision8.Player
{

    public partial class SceneStackEditor
    {        
        
        private int _backgroundCard = 0;
        private int _firstCard = 1;
        private int _lastCard => Stack.Count-1;

        public List<Card> Stack = new List<Card>();

        public int CurrentCardId = -1;

        public Card CurrentCard => CurrentCardId == -1 ? null : Stack[CurrentCardId];

        [MenuAction]
        public void GoToCard(int id)
        {
            if(id < 0 || id > _lastCard)
                return;


            var lastTool = Tools.Pointer;

            if(CurrentCard != null)
            {
                lastTool = CurrentCard.Canvas.Tool;
                CurrentCard.Close();
                Remove(CurrentCard.Name);
            }

            // Redraw background
            DrawPixels(backgroundPixelData.Pixels, 0, 0, backgroundPixelData.Width, backgroundPixelData.Height, false, false, DrawMode.TilemapCache, 0);

            // Force the nav to redraw
            _navBar.Invalidate();

            CurrentCardId = id;

            CurrentCard.Load();

            Add(CurrentCard);

            UpdateGoMenuOptions();

            CurrentCard.Canvas.ChangeTool(lastTool);


            _label = CurrentCardId == 0 ? "BACKGROUND" : "CARD "+CurrentCardId.ToString("D2");

            // _idRect = new Rectangle(Width - 12, Height - 12, 8, 8);

            _idRect.Width = _label.Length * 4;
            _idRect.X = Width - _idRect.Width - 4;
            

        }

        public void UpdateGoMenuOptions()
        {
            _navBar.EnableOption("Go", "GoTo", _lastCard > 1 || CurrentCardId == _backgroundCard);
            _navBar.EnableOption("Go", "FirstCard", CurrentCardId > _firstCard );
            _navBar.EnableOption("Go", "LastCard", CurrentCardId < _lastCard && CurrentCardId != _backgroundCard);
            _navBar.EnableOption("Go", "PreviousCard", CurrentCardId > _firstCard);
            _navBar.EnableOption("Go", "NextCard", CurrentCardId < _lastCard && CurrentCardId != _backgroundCard);
        }

        [MenuAction]
        public void NextCard() => GoToCard(Math.Min(CurrentCardId + 1, _lastCard));

        [MenuAction]
        public void PreviousCard() => GoToCard(Math.Max(CurrentCardId - 1, 0));

        [MenuAction]
        public void FirstCard() => GoToCard(_firstCard);

        [MenuAction]
        public void LastCard() =>  GoToCard(_lastCard);

        [MenuAction]
        public void NewCard(bool jumpTo = true, string name = null)
        {

            if(name == null)
            {
                name = "Card" + Stack.Count.ToString("D2");
            }

            var card = new Card(_uiBuilder, Stack.Count, name);

            Stack.Add(card);

            if(jumpTo)
                GoToCard(card.Id);

        }

        [MenuAction]
        public void ClearStack()
        {
            Stack.Clear();

            // Create background card - 0
            NewCard(false, "Background");

            // Create first card - 1
            NewCard();
        }

        [MenuAction]
        public void DeleteCard(int id = -1)
        {
            if(CurrentCard.CanDelete == false)
                return;

            if(id == -1)
                id = CurrentCardId;

            if(id > 0)
            {
                // TODO does this make sense to go back or should you go forward?
                PreviousCard();
                Stack.RemoveAt(id);
            }
            else
            {
                Console.WriteLine("Error: can't delete the first card.");
            }
            
        }

        [MenuAction]
        public string CopyCard()
        {
            return "card:" + CurrentCard.Serialize();
        }

        [MenuAction]
        public void PasteCard(string value)
        {
            CurrentCard.Deserialize(value);
        }

    }

}