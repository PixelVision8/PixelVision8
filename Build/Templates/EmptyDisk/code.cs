//
// Pixel Vision 8 - New Template Script
// Copyright (C) 2017, Pixel Vision 8 (@pixelvision8)
// Created by Jesse Freeman (@jessefreeman)
// Converted from the Lua file by Drake Williams [drakewill+pv8@gmail.com]
//
// This project was designed to display some basic instructions when you create
// a new game. Simply delete the following code and implement your own Init(),
// Update() and Draw() logic.
// 
// Learn more about making Pixel Vision 8 games at
// https://www.pixelvision8.com/getting-started
// 

/*
 For C# games, we need to put our main class inside of the 
 `PixelVision8.Player` namespace.
*/
namespace PixelVision8.Player
{
	
	/*
		Our main class also need to extend the `GameChip` class. This is how
		the C# engine knows which class to use as the main game class.
	*/
	public class CustomGameChip : GameChip
	{

		/*
			The Init() method is part of the game's lifecycle and called a game
			starts. We are going to use this method to configure background
			color, ScreenBufferChip and draw a text box.
		*/
		public override void Init()
		{
			
			/*
				We combined two sets of fonts into the default.font.png. Use
				uppercase for larger characters and lowercase for a smaller one.
			*/
			var message = "EMPTY C# GAME\n\n\nThis is an empty game template.\n\n\nVisit 'www.pixelvision8.com' to learn more about creating games from scratch.";

			var display = Display();
			
			/*
				We are going to render the message in a box as tiles. To do
				this, we need to wrap the text, then split it into lines and
				draw each line.
			*/
			var wrap = WordWrap(message, (display.X / 8) - 2);
			var lines = SplitLines(wrap);
			var total = lines.Length;
			var startY = ((display.Y / 8) - 1) - total;

			/*
				We want to render the text from the bottom of the screen so we
				offset it and loop backwards.
			*/
			for (var i = total - 1; i >= 0; i--)
				DrawText(lines[i], 1, startY + (i - 1), DrawMode.Tile, "large", 15);

		}
		
		/*
			The Update() method is part of the game's life cycle. The engine 
			calls Update() on every frame before the Draw() method. It accepts 
			one argument, timeDelta, which is the difference in milliseconds 
			since the last frame.
		*/
		public override void Update(int timeDelta)
		{

			// TODO add your own update logic here

		}

		/* 
			The Draw() method is part of the game's life cycle. It is called 
			after Update() and is where all of our draw calls should go. We'll 
			be using this to render sprites to the display.
		*/
		public override void Draw()
		{

			/* 
				We can use the RedrawDisplay() method to clear the screen and 
				redraw the tilemap in a single call.
			*/
			RedrawDisplay();
			
			// TODO add your own draw logic here.

		}
	}
}