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

namespace PixelVision8.Player
{

    public partial class UIBuilder
    {

        private ModalPanel _activeModal;
        private EntityManager _modalManager;

        [UIComponentAttribute]
        public void CreateModalManager()
        {
            _modalManager = new EntityManager(this);
            RegisterComponent(_modalManager);
        }

        public ModalPanel NewModalPanel(string name, Rectangle rect, List<Entity> entities, Action onOpen = null, Action onClose = null)
        {
            var modal = new ModalPanel(this, name, rect, entities);

            if(onOpen != null)
                modal.OnOpen = onOpen;

            if(onClose != null)
                modal.OnClose = onClose;
            
            return modal;
        }

        public void OpenModal(ModalPanel modal, bool triggerOpen = true)
        {
            // This check makes sure we don't open a modal twice
            if(modal.IsOpen)
                return;

            // Take a snapshot on the first modal opening
            if(_modalManager.Total == 0)
            {
                // Take a snapshot of the entire screen
                GameChip.SaveTilemapCache();
            }
            
            ClearFocus();

            _modalManager.Add(modal);

            if(triggerOpen)
                modal.Open(false);

            // _uiManager.Pause = true;

        }

        public void CloseModal(string name, bool triggerOnClose = true)
        {

            var modal = _modalManager.Remove(name);

            // Take a snapshot on the first modal opening
            if(_modalManager.Total == 0)
            {
                GameChip.RestoreTilemapCache();
            }

            ClearFocus();

            if(modal != null && modal is ModalPanel && triggerOnClose)
            {
                ((ModalPanel)modal).Close();
            }

            // _uiManager.Pause = false;

        }

        public void CloseAllModals()
        {
            while(_modalManager.Total > 0)
            {
                CloseModal(_modalManager[0].Name, true);
            }
        }

    }

}