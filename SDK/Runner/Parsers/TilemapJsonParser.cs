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

using PixelVision8.Player;
using PixelVision8.Runner;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelVision8.Runner
{
    public class TilemapJsonParser : JsonParser
    {
        protected PixelVision target;

        public TilemapJsonParser(string filePath, IFileLoader fileLoadHelper, PixelVision target) : base(filePath,
            fileLoadHelper)
        {
            this.target = target;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            Steps.Add(ParseMap);
        }

        public void ParseMap()
        {
            var version = (int) (long) Data["version"];

            if (version == 2)
            {
                Steps.Add(ConfigureTilemapV2);
            }
            else
            {
                Steps.Add(ConfigureTilemapV1);
            }

            StepCompleted();
        }

        public virtual void ConfigureTilemapV2()
        {
            var tilemapChip = target.TilemapChip;


            if (Data.ContainsKey("layers"))
            {
                var layers = Data["layers"] as List<object>;
                // var tileSets = Data["tilesets"] as List<object>;

                var total = layers.Count;

                for (var i = 0; i < total; i++)
                    try
                    {
                        var layer = layers[i] as Dictionary<string, object>;

                        var layerType = (string) layer["type"];

                        if (layerType == "objectgroup")
                        {
                            var objects = layer["objects"] as List<object>;

                            var totalTiles = objects.Count;

                            for (var j = 0; j < totalTiles; j++)
                            {
                                var tileObject = objects[j] as Dictionary<string, object>;

                                var column = (int) Math.Floor((float) (long) tileObject["x"] / 8);
                                var row = (int) Math.Floor((float) (long) tileObject["y"] / 8) - 1;

                                var tile = tilemapChip.GetTile(column, row);

                                var gid = (uint) (long) tileObject["gid"];

                                var idMask = (1 << 30) - 1;

                                tile.SpriteId = (int) (gid & idMask) - 1;

                                var hMask = 1 << 31;

                                tile.FlipH = (hMask & gid) != 0;

                                var vMask = 1 << 30;

                                tile.FlipV = (vMask & gid) != 0;

                                var properties = tileObject["properties"] as List<object>;

                                //								int flagID = -1;
                                //								int colorOffset = 0;

                                for (var k = 0; k < properties.Count; k++)
                                {
                                    var prop = properties[k] as Dictionary<string, object>;

                                    var propName = (string) prop["name"];

                                    if (propName == "flagID")
                                        tile.Flag = (int) (long) prop["value"];
                                    else if (propName == "colorOffset")
                                        tile.ColorOffset = (int) (long) prop["value"];
                                }

                                // tile.Invalidate();
                            }
                        }

                        tilemapChip.Invalidate();
                        // TODO need to make sure that the layer is the same size as the display chip

                        // TODO copy the tilemap data over to layer correctly
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);

                        // Just ignore any layers that don't exist
                        throw new Exception(
                            "Unable to parse 'tilemap.json' file. It may be corrupt. Try deleting it and creating a new one.");
                    }
            }

            StepCompleted();
        }

        public virtual void ConfigureTilemapV1()
        {
            var tilemapChip = target.TilemapChip;


            if (Data.ContainsKey("layers"))
            {
                var layers = Data["layers"] as List<object>;
                var tileSets = Data["tilesets"] as List<object>;

                var total = layers.Count;

                for (var i = 0; i < total; i++)
                    try
                    {
                        var layer = layers[i] as Dictionary<string, object>;

                        var layerType = (string) layer["type"];

                        if (layerType == "tilelayer")
                        {
                            var tileSet = tileSets[i] as Dictionary<string, object>;


                            var offset = (int) (long) tileSet["firstgid"];

                            var columns = (int) (long) layer["width"];
                            var rows = (int) (long) layer["height"];

                            var rawLayerData = layer["data"] as List<object>;

                            var dataValues = rawLayerData
                                .Select(x => (int) (long) x - offset < -1 ? -1 : (int) (long) x - offset).ToArray();

                            if (columns != tilemapChip.Columns || rows > tilemapChip.Rows)
                            {
                                // Create texture data that matches the memory of the tilemap chip
                                var tmpPixelData = new PixelData(tilemapChip.Columns, tilemapChip.Rows);
                                Utilities.Clear(tmpPixelData);
                                // tmpPixelData.Clear();

                                var jsonData = new PixelData(columns, rows);
                                Utilities.Clear(jsonData);
                                // jsonData.Clear();
                                Utilities.SetPixels(dataValues, 0, 0, columns, rows, jsonData);
                                // jsonData.SetPixels(0, 0, columns, rows, dataValues);

                                var tmpCol = columns > tilemapChip.Columns ? tilemapChip.Columns : columns;
                                var tmpRow = rows > tilemapChip.Rows ? tilemapChip.Rows : rows;

                                if (tmpCol > columns) tmpCol = columns;

                                if (tmpRow > rows) tmpRow = rows;

                                // var tmpData = new int[tmpCol * tmpRow];

                                var tmpData = Utilities.GetPixels(jsonData, 0, 0, tmpCol, tmpRow);
                                // jsonData.CopyPixels(ref tmpData, 0, 0, tmpCol, tmpRow);

                                Utilities.SetPixels(tmpData, 0, 0, tmpCol, tmpRow, tmpPixelData);
                                // tmpPixelData.SetPixels(0, 0, tmpCol, tmpRow, tmpData);

                                // TODO why is this happening twice?
                                // PixelDataUtil.CopyPixels(ref dataValues, tmpPixelData, 0, 0, tmpPixelData.Width, tmpPixelData.Height);
                                // tmpPixelData.CopyPixels(ref dataValues, 0, 0, tmpPixelData.width, tmpPixelData.height);
                                dataValues = Utilities.GetPixels(tmpPixelData, 0, 0, tilemapChip.Columns, tilemapChip.Rows);

                                // tmpPixelData.CopyPixels(ref dataValues, 0, 0, tilemapChip.columns, tilemapChip.rows);
                            }


                            for (var j = 0; j < tilemapChip.Total; j++)
                            {
                                var tile = tilemapChip.tiles[j];

                                if ((string) layer["name"] == "Sprites")
                                    tile.SpriteId = dataValues[j];
                                else if ((string) layer["name"] == "Flags") tile.Flag = dataValues[j];

                                // tile.Invalidate();
                            }
                        }
                        else if (layerType == "objectgroup")
                        {
                            var objects = layer["objects"] as List<object>;

                            var totalTiles = objects.Count;

                            for (var j = 0; j < totalTiles; j++)
                            {
                                var tileObject = objects[j] as Dictionary<string, object>;

                                var column = (int) Math.Floor((float) (long) tileObject["x"] / 8);
                                var row = (int) Math.Floor((float) (long) tileObject["y"] / 8) - 1;

                                var tile = tilemapChip.GetTile(column, row);

                                var gid = (uint) (long) tileObject["gid"];

                                var idMask = (1 << 30) - 1;

                                tile.SpriteId = (int) (gid & idMask) - 1;

                                var hMask = 1 << 31;

                                tile.FlipH = (hMask & gid) != 0;

                                var vMask = 1 << 30;

                                tile.FlipV = (vMask & gid) != 0;

                                var properties = tileObject["properties"] as List<object>;

                                //								int flagID = -1;
                                //								int colorOffset = 0;

                                for (var k = 0; k < properties.Count; k++)
                                {
                                    var prop = properties[k] as Dictionary<string, object>;

                                    var propName = (string) prop["name"];

                                    if (propName == "flagID")
                                        tile.Flag = (int) (long) prop["value"];
                                    else if (propName == "colorOffset") tile.ColorOffset = (int) (long) prop["value"];
                                }

                                // tile.Invalidate();
                            }
                        }

                        tilemapChip.Invalidate();
                        // TODO need to make sure that the layer is the same size as the display chip

                        // TODO copy the tilemap data over to layer correctly
                    }
                    catch
                    {
                        // Just ignore any layers that don't exist
                        throw new Exception(
                            "Unable to parse 'tilemap.json' file. It may be corrupt. Try deleting it and creating a new one.");
                    }
            }

            StepCompleted();
        }
    }

    public partial class Loader
    {
        [FileParser("tilemap.json", FileFlags.Tilemap)]
        public void ParseTilemapJson(string file, PixelVision engine)
        {
            AddParser(new TilemapJsonParser(file, _fileLoadHelper, engine));
        }
    }
}