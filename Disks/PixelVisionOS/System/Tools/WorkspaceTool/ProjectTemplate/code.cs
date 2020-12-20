using PixelVision8.Engine.Chips;
using PixelVision8.Engine;
using System.IO;

//  Pixel Vision 8 - New Template Script [C#]
//  Copyright (C) 2017, Pixel Vision 8 (@pixelvision8)
//  Converted from the Lua file by Drake Williams [drakewill+pv8@gmail.com]

namespace PixelVisionRoslyn
{
	public class RoslynGameChip : PixelVision8.Engine.Chips.GameChip
	{
		public override void Init()
		{
			BackgroundColor(12);
			var message = "EMPTY GAME\n\n\nThis is an empty C# game template.\n\n\nVisit 'www.pixelvision8.com' to learn more about creating games from scratch.";

			var wrap = WordWrap(message, (display.X / 8) - 2);
			var lines = SplitLines(wrap);
			var total = lines.Length;
			var startY = ((display.Y / 8) - 1) - total;

			// We want to render the text from the bottom of the screen so we offset
			// it and loop backwards.
			for (var i = total - 1; i >= 0; i--)
				DrawText(lines[i], 1, startY + (i - 1), DrawMode.Tile, "large", 15);
		}

		public override void Draw()
		{

		}

		public override void Update(int timeDelta)
		{
			RedrawDisplay();
		}
	}
}