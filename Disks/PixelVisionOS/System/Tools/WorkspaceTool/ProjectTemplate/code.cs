using PixelVision8.Engine.Chips; //Mandatory

namespace PixelVisionRoslyn //Currently mandatory, should probably stay mandatory
{
	public class RoslynGameChip : GameChip // : currently mandatory. Could have the engine search for any class that inherits from GameChip eventually.
	{
		public override void Init()
		{
			BackgroundColor(7);
			DrawText("Holy Carp, a compiled C# Game!", 1, 1, DrawMode.Tile, "large", 15);
		}
		
		public override void Update(int timeDelta)
		{
			
		}
		
		public override void Draw()
		{
			RedrawDisplay();
			DrawText(fps.ToString() + " fps", 1, 2, DrawMode.Tile, "large", 15);
		}
	}
}