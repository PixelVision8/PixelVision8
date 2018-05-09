//
// Copyright (c) Jesse Freeman. All rights reserved.  
//
// Licensed under the Microsoft Public License (MS-PL) License. 
// See LICENSE file in the project root for full license information. 
//
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//
// Jesse Freeman - @JesseFreeman
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany

using System.Text;
using PixelVisionRunner.Utils;
using PixelVisionSDK;
using PixelVisionSDK.Chips;

namespace PixelVisionRunner.Exporters
{
    public class SystemExporter : AbstractExporter
    {

        private IEngine engine;
        private StringBuilder sb;
        
        public SystemExporter(string fileName, IEngine engine):base(fileName)
        {
           
            this.engine = engine;
            
//            CalculateSteps();
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            
            // Create a new string builder
            steps.Add(CreateStringBuilder);
            
            // Serialize Color Chip
            if (engine.colorChip != null)
            {
                steps.Add(delegate { SerializeColorChip(engine.colorChip); });
            }
            
            // Serialize Display
            if (engine.displayChip != null)
            {
                steps.Add(delegate { SerializeDisplay(engine.displayChip); });
            }
            
            // Serialize Controller
            if (engine.controllerChip != null)
            {
                steps.Add(delegate { SerializeControllerChip(engine.controllerChip); });
            }
            
            // Serialize Font
            if (engine.fontChip != null)
            {
                steps.Add(delegate { SerializeFontChip(engine.fontChip); });
            }
            
            // Serialize Game
            if (engine.gameChip != null)
            {
                steps.Add(delegate { SerializeGameChip(engine.gameChip); });
            }
            
            // Serialize Music
            if (engine.musicChip != null)
            {
                steps.Add(delegate { SerializeMusicChip(engine.musicChip); });
            }
            
            // Serialize Sound
            if (engine.soundChip != null)
            {
                steps.Add(delegate { SerializeSoundChip(engine.soundChip); });
            }
            
            // Serialize Sprite
            if (engine.spriteChip != null)
            {
                steps.Add(delegate { SerializeSpriteChip(engine.spriteChip); });
            }
            
            // Serialize Tilemap
            if (engine.tilemapChip != null)
            {
                steps.Add(delegate { SerializeTilemapChip(engine.tilemapChip); });
            }
            
            // Save the final string builder
            steps.Add(CloseStringBuilder);
            
        }

        private void CreateStringBuilder()
        {
            sb = new StringBuilder();
            
            // Start the json string
            sb.Append("{");
            
            JsonUtil.indentLevel = 1;
            
            currentStep++;
        }

        private void CloseStringBuilder()
        {
            // TODO test to see if there is actually a comma
            // Chances are the last item will have an extra comma so let's remove the last character
            sb.Length--;

            JsonUtil.indentLevel = 0;

            JsonUtil.GetLineBreak(sb);

            // Close the object
            sb.Append("}");

//            Debug.Log("Save bytes");
            bytes = Encoding.UTF8.GetBytes(sb.ToString());
            
            currentStep++;
        }
        
        private void SerializeDisplay(DisplayChip display)
        {

            if (display.export == false)
                return;
            
            JsonUtil.GetLineBreak(sb);
            sb.Append("\"DisplayChip\":");

            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            // Width Value
            sb.Append("\"width\":");
            sb.Append(display.width);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            // Height Value
            sb.Append("\"height\":");
            sb.Append(display.height);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"overscanX\":");
            sb.Append(display.overscanX);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"overscanY\":");
            sb.Append(display.overscanY);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);
            
            sb.Append("\"layers\":");
            sb.Append(display.layers);
            
//            sb.Append(",");
            
            JsonUtil.GetLineBreak(sb, 1);
            
            JsonUtil.GetLineBreak(sb);
            sb.Append("}");

            sb.Append(",");
            
            currentStep++;
        }

        private void SerializeColorChip(ColorChip colorChip)
        {
            if (colorChip.export == false)
                return;
            
            JsonUtil.GetLineBreak(sb);
            sb.Append("\"ColorChip\":");

            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"pages\":");
            sb.Append(colorChip.pages);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"colorsPerPage\":");
            sb.Append(colorChip.colorsPerPage);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);
                
            // Rebuild calculated colors value
//            colorChip.RecalculateSupportedColors();
            
            sb.Append("\"supportedColors\":");
            sb.Append(colorChip.supportedColors);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);
            
            sb.Append("\"maxColors\":");
            sb.Append(colorChip.maxColors);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"backgroundColor\":");
            sb.Append(colorChip.backgroundColor);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"maskColor\":\"");
            sb.Append(colorChip.maskColor);
            sb.Append("\"");
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);
            
            sb.Append("\"unique\":");
            sb.Append(colorChip.unique.ToString().ToLower());
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);
            
            sb.Append("\"debug\":");
            sb.Append(colorChip.debugMode.ToString().ToLower());
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);
            
            JsonUtil.GetLineBreak(sb);
            sb.Append("}");
            sb.Append(",");
            
            currentStep++;
        }

        private void SerializeControllerChip(ControllerChip controllerChip)
        {
            if (controllerChip.export == false)
                return;
            
            JsonUtil.GetLineBreak(sb);
            sb.Append("\"ControllerChip\":");

            JsonUtil.GetLineBreak(sb);
            sb.Append("{");

            JsonUtil.GetLineBreak(sb);
            sb.Append("}");
            sb.Append(",");
            
            currentStep++;
        }

        private void SerializeFontChip(FontChip fontChip)
        {
            if (fontChip.export == false)
                return;
            
            JsonUtil.GetLineBreak(sb);
            sb.Append("\"FontChip\":");
            JsonUtil.GetLineBreak(sb);
            sb.Append("{");

            JsonUtil.GetLineBreak(sb);
            sb.Append("}");
            sb.Append(",");
            
            currentStep++;
        }

        private void SerializeGameChip(GameChip gameChip)
        {
            JsonUtil.GetLineBreak(sb);
            sb.Append("\"GameChip\":");

            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);
            
            // Name
            sb.Append("\"name\":");
            sb.Append("\"");
            sb.Append(gameChip.name);
            sb.Append("\"");
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);
            
            // Description
            sb.Append("\"description\":");
            sb.Append("\"");
            sb.Append(gameChip.description);
            sb.Append("\"");
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);
            
            // Version
            sb.Append("\"version\":");
            sb.Append("\"");
            sb.Append(gameChip.version);
            sb.Append("\"");
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);
            
            
            // ext
            sb.Append("\"ext\":");
            sb.Append("\"");
            sb.Append(gameChip.ext);
            sb.Append("\"");
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);
            
            // Lock Specs
            sb.Append("\"lockSpecs\":");
            sb.Append(gameChip.lockSpecs.ToString().ToLower());
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);
            
            // Max Size
            sb.Append("\"maxSize\":");
            sb.Append(gameChip.maxSize);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);
            
            // Save Slots
            sb.Append("\"saveSlots\":");
            sb.Append(gameChip.saveSlots);
//            sb.Append(",");
//            JsonUtil.GetLineBreak(sb, 1);

            JsonUtil.GetLineBreak(sb);
            sb.Append("}");
            sb.Append(",");
            
            currentStep++;
        }

        private void SerializeMusicChip(MusicChip musicChip)
        {
            
            JsonUtil.GetLineBreak(sb);
            sb.Append("\"MusicChip\":");

            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"totalTracks\":");
            sb.Append(musicChip.totalTracks);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"notesPerTrack\":");
            sb.Append(musicChip.maxNoteNum);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"totalLoop\":");
            sb.Append(musicChip.totalLoops);

            JsonUtil.GetLineBreak(sb);
            sb.Append("}");
            sb.Append(",");
            
            currentStep++;
            
        }

        private void SerializeSoundChip(SoundChip soundChip)
        {
            JsonUtil.GetLineBreak(sb);
            sb.Append("\"SoundChip\":");

            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"totalChannels\":");
            sb.Append(soundChip.totalChannels);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);
            
            sb.Append("\"totalSounds\":");
            sb.Append(soundChip.totalSounds);
//            sb.Append(",");

            JsonUtil.GetLineBreak(sb, 0);
            sb.Append("}");
            sb.Append(",");
            
            currentStep++;
        }

        private void SerializeSpriteChip(SpriteChip spriteChip)
        {
//            var sb = new StringBuilder();
            JsonUtil.GetLineBreak(sb);

            JsonUtil.GetLineBreak(sb);
            sb.Append("\"SpriteChip\":");

            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);
            
            // Max Sprite Count Value
            sb.Append("\"maxSpriteCount\":");
            sb.Append(spriteChip.maxSpriteCount);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);
            
            // Mode
            sb.Append("\"unique\":");
            sb.Append(spriteChip.unique.ToString().ToLower());
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);
            
            // Mode
            sb.Append("\"spriteWidth\":");
            sb.Append(spriteChip.width);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"spriteHeight\":");
            sb.Append(spriteChip.height);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"pages\":");
            sb.Append(spriteChip.pages);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            // Height
            sb.Append("\"cps\":");
            sb.Append(spriteChip.colorsPerSprite);

//            if (serializePixelData)
//            {
//                sb.Append(",");
//                JsonUtil.GetLineBreak(sb, 1);
//
//                //TODO this should just use the TextureData's
//                // serializePixelData
//                sb.Append("\"serializePixelData\":");
//                sb.Append(Convert.ToInt32(serializePixelData));
//
//                sb.Append(",");
//
//                //TODO this should capture all of the texture settings
//                sb.Append("\"pixelData\":");
//                sb.Append(DataUtil.SerializedTextureData(_texture));
//
//                //sb.Append(_texture.SerializeData());
//            }
            
            JsonUtil.GetLineBreak(sb);
            sb.Append("}");
            sb.Append(",");
            
            currentStep++;
        }

        private void SerializeTilemapChip(TilemapChip tilemapChip)
        {
            JsonUtil.GetLineBreak(sb);
            sb.Append("\"TilemapChip\":");

            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            // Columns
            sb.Append("\"columns\":");
            sb.Append(tilemapChip.columns);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            // Rows
            sb.Append("\"rows\":");
            sb.Append(tilemapChip.rows);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            // Total Flags
            sb.Append("\"totalFlags\":");
            sb.Append(tilemapChip.totalFlags);
        
            sb.Append("\"autoImport\":");
            sb.Append(tilemapChip.autoImport.ToString().ToLower());
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);
            
            JsonUtil.GetLineBreak(sb);
            sb.Append("}");
            sb.Append(",");
            
            currentStep++;
        }
    }
    
}