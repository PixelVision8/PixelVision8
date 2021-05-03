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
using PixelVision8.Player;

namespace PixelVision8.Runner
{

    public partial class Loader
    {
        // This has to be called manually
        public void ParseSpritesFromFolder(string file, PixelVision engine)
        {
            AddParser(new SpriteFolderParser(file, _imageParser, engine.ColorChip, engine.SpriteChip, engine.GameChip));
        }
    }

    public class SpriteFolderParser : SpriteImageParser
    {
        protected GameChip gameChip;
        private List<int> spiteIds = new List<int>();
        
        public SpriteFolderParser(string sourceFile, IImageParser parser, ColorChip colorChip,
            SpriteChip spriteChip = null, GameChip gameChip = null) : base(sourceFile, parser, colorChip, spriteChip)
        {
            this.gameChip = gameChip;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            Steps.Add(CreateMetaSprite);
        }

        private void CreateMetaSprite()
        {
            var name = Parser.FileName.Split('.')[0];

            // TODO this needs to be floored or something
            var columns = ImageData.Width / 8;
            
            // Console.WriteLine("Next ID " + gameChip.NextEmptyMetaSpriteId());
            // TODO need to find the next empty meta sprite

            var index = -1;

            for (int i = 0; i < gameChip.TotalMetaSprites(); i++)
            {
                if(gameChip.MetaSprite(i).Name == "EmptyMetaSprite")
                {
                    index = i;
                    break;
                }
                    
            }

            if(index > -1){
                gameChip.NewMetaSprite(index, name, spiteIds.ToArray(), columns);
            }
            
            StepCompleted();
        }

        protected override void ProcessSpriteData()
        {

            var spriteId = -1;

            // Look to see if the sprite is empty
            if(Utilities.IsEmpty(spriteData) == false)
            {

                // Look for the sprite
                spriteId = spriteChip.FindSprite(spriteData);

                // Look to see if the sprite still doesn't have an Id
                if(spriteId == -1)
                {

                    spriteId = spriteChip.NextEmptyId();

                    if(spriteId > -1)
                    {
                        spriteChip.UpdateSpriteAt(spriteId, spriteData);
                        spritesAdded++;
                    }

                    
                }

            }

            // Add the sprite id to the list
            spiteIds.Add(spriteId);

            // TODO need to output how many sprites were used somewhere

        }

        public override void Dispose()
        {
            base.Dispose();
            gameChip = null;
        }
    }

    
}