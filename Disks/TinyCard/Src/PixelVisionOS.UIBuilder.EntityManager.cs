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

namespace PixelVision8.Player
{

    public partial class UIBuilder
    {

        protected EntityManager _uiManager;

        [UIComponentAttribute]
        public void CreateUIManager()
        {
            _uiManager = new EntityManager(this, "UIEntityManager");
            RegisterComponent(_uiManager);
        }

        public void AddUI(IEntity entity) => _uiManager.Add(entity);
        public void RemoveUI(string name) => _uiManager.Remove(name);

        public void ClearUI() => _uiManager.Clear();


    }

}