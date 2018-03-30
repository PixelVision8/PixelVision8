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

using System;
using System.Collections.Generic;
using PixelVisionSDK;
using PixelVisionSDK.Chips;

namespace PixelVisionRunner.Parsers
{
    public class TileColorOffsetJson : JsonParser
	{
		protected IEngine target;
		
		public TileColorOffsetJson(string jsonString, IEngine target) : base(jsonString)
		{
			this.target = target;
		}
		
		public override void CalculateSteps()
		{
			base.CalculateSteps();
			steps.Add(ConfigureTilemap);
		}

		public virtual void ConfigureTilemap()
		{

			var tilemapChip = target.tilemapChip;
			
			try
			{
				var layerType = (TilemapChip.Layer) Enum.Parse(typeof(TilemapChip.Layer), ((string)data["name"]));

				var columns = (int) (long) data["width"];
				var rows = (int) (long) data["height"];

				var rawLayerData = data["data"] as List<object>;
						
				int[] dataValues = rawLayerData.ConvertAll(x => ((int) (long)x) < -1 ? -1 : ((int) (long)x)).ToArray();
						
				if (tilemapChip.columns != columns || tilemapChip.rows != rows)
				{
					var tmpPixelData = new TextureData(columns, rows);
					tmpPixelData.SetPixels(0, 0, columns, rows, dataValues);
							
					Array.Resize(ref dataValues, tilemapChip.total);
							
					tmpPixelData.CopyPixels(ref dataValues, 0, 0, tilemapChip.columns, tilemapChip.rows);
				}
						
				Array.Copy(dataValues, tilemapChip.layers[(int)layerType], dataValues.Length); 
				
			}
			catch (Exception e)
			{
				// Just igonre any layers that don't exist
				throw new Exception("Unable to parse 'tilemap.json' file. It may be corrupt. Try deleting it and creating a new one.");
			}
			
			currentStep++;

		}
	}
}