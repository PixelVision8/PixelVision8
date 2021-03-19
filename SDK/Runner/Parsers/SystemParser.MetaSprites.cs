using System;
using System.Collections.Generic;
using PixelVision8.Player;

namespace PixelVision8.Runner
{
    public partial class SystemParser
    {
        // Meta Sprite Json Example
        // {
        //     "MetaSprites": {
        //         "version": "v1",
        //         "total": 96,
        //         "collections": [{
        //             "name": "tileset_1",
        //             "spriteIDs": [0, 1, 24, 25],
        //             "sprites": [{
        //                 "id": 0,
        //                 "x": 0,
        //                 "y": 0,
        //                 "flipH": false,
        //                 "flipV": false,
        //                 "colorOffset": 0
        //
        //             }],
        //             "width": 2,
        //             "colorOffset": 0
        //         }]
        //     }
        // }
        [ChipParser("MetaSprites")]
        public void ConfigureMetaSprites(Dictionary<string, object> data)
        {
            var gameChip = Target.GameChip;

            // Prepare to parse v1 of the MetaSprite json template/
            if (data.ContainsKey("version") && (string) data["version"] == "v1")
            {
                if (data.ContainsKey("total"))
                    gameChip.TotalMetaSprites(Convert.ToInt32((long) data["total"]));

                var spriteWidth = data.ContainsKey("spriteWidth")
                    ? Convert.ToInt32((long) data["spriteWidth"])
                    : gameChip.SpriteSize().X;
                var spriteHeight = data.ContainsKey("spriteHeight")
                    ? Convert.ToInt32((long) data["spriteHeight"])
                    : gameChip.SpriteSize().Y;

                // Look for songs
                if (data.ContainsKey("collections"))
                {
                    // Get the list of song data
                    var collections = data["collections"] as List<object>;
                    var total = Math.Min(collections.Count, gameChip.TotalMetaSprites());

                    // Loop through each of teh 
                    for (var i = 0; i < total; i++)
                    {
                        var collectionData = collections[i] as Dictionary<string, object>;
                        var metaSprite = gameChip.MetaSprite(i);

                        // TODO this is redundant
                        metaSprite.SpriteWidth = spriteWidth;
                        metaSprite.SpriteHeight = spriteHeight;

                        if (collectionData.ContainsKey("name")) metaSprite.Name = collectionData["name"] as string;

                        // Test to see if the basic sprite data exists, IDs and Width.
                        if (collectionData.ContainsKey("spriteIDs") && collectionData.ContainsKey("width"))
                        {
                            var width = Convert.ToInt32((long) collectionData["width"]);

                            var spriteData = (List<object>) collectionData["spriteIDs"];
                            var totalSprites = spriteData.Count;

                            for (var j = 0; j < totalSprites; j++)
                            {
                                if (Convert.ToInt32((long) spriteData[j]) > -1)
                                {
                                    var pos = Utilities.CalculatePosition(j, width);

                                    metaSprite.AddSprite(Convert.ToInt32((long) spriteData[j]), pos.X * spriteWidth,
                                        pos.Y * spriteHeight);
                                }
                            }

                            collections[i] = collectionData;
                        }
                        // Test to see if the more advanced sprite data exists
                        else if (collectionData.ContainsKey("sprites"))
                        {
                            // TODO this is where we need to manually create each sprite in the collection
                        }
                    }
                }
            }
        }
    }
}