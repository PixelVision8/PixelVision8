using System;
using System.Collections.Generic;
using PixelVision8.Player;

namespace PixelVision8.Runner
{
    public partial class SystemParser
    {
        [ChipParser("GameChip")]
        public void ConfigureGameChip(Dictionary<string, object> data)
        {
            var gameChip = Target.GameChip;


            // Flag chip to export
            //gameChip.export = true;

            // loop through all data and save it to the game's memory

            //            if (data.ContainsKey("name"))
            //                gameChip.name = (string) data["name"];
            //
            //            if (data.ContainsKey("description"))
            //                gameChip.description = (string) data["description"];
            //
            //            if (data.ContainsKey("version"))
            //                gameChip.version = (string) data["version"];
            //
            //            if (data.ContainsKey("ext"))
            //                gameChip.ext = (string) data["ext"];

            if (data.ContainsKey("maxSize")) gameChip.maxSize = (int) (long) data["maxSize"];

            if (data.ContainsKey("saveSlots")) gameChip.SaveSlots = (int) (long) data["saveSlots"];

            if (data.ContainsKey("lockSpecs")) gameChip.lockSpecs = Convert.ToBoolean(data["lockSpecs"]);

            if (data.ContainsKey("savedData"))
                foreach (var entry in data["savedData"] as Dictionary<string, object>)
                {
                    var name = entry.Key;
                    var value = entry.Value as string;
                    gameChip.WriteSaveData(name, value);
                }


            // TODO need to look for MetaSprite properties
            if (data.ContainsKey("totalMetaSprites")) gameChip.TotalMetaSprites((int) (long) data["totalMetaSprites"]);

            if (data.ContainsKey("metaSprites"))
            {
                // Console.WriteLine("ContainsKey  metaSprites");

                var metaSprites = data["metaSprites"] as List<object>;

                var total = Utilities.Clamp(metaSprites.Count, 0, gameChip.TotalMetaSprites());

                for (var i = 0; i < total; i++)
                {
                    var metaSprite = gameChip.MetaSprite(i);
                    var spriteData = metaSprites[i] as Dictionary<string, object>;

                    if (spriteData.ContainsKey("name")) metaSprite.Name = spriteData["name"] as string;

                    if (spriteData.ContainsKey("sprites"))
                        if (spriteData["sprites"] is List<object> childSprites)
                        {
                            var subTotal = childSprites.Count;
                            for (var j = 0; j < subTotal; j++)
                            {
                                var childData = childSprites[j] as Dictionary<string, object>;

                                metaSprite.AddSprite(
                                    childData.ContainsKey("id") ? (int) (long) childData["id"] : 0,
                                    childData.ContainsKey("x") ? (int) (long) childData["x"] : 0,
                                    childData.ContainsKey("y") ? (int) (long) childData["y"] : 0,
                                    childData.ContainsKey("flipH") && Convert.ToBoolean(childData["flipH"]),
                                    childData.ContainsKey("flipV") && Convert.ToBoolean(childData["flipV"]),
                                    childData.ContainsKey("colorOffset") ? (int) (long) childData["colorOffset"] : 0
                                );
                            }
                        }
                }

                // var total = metaSprites.Length;
                // for (int i = 0; i < UPPER; i++)
                // {
                //     
                //         var metaSprite = gameChip.MetaSprite()
                //
                //         // TODO need a way to parse this out
                //         Console.WriteLine("Found Sprites");
                //         // var name = entry.Key;
                //         // var value = entry.Value as string;
                //         // gameChip.WriteSaveData(name, value);
                //     }
            }
        }
    }
}