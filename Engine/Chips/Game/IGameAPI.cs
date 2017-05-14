
namespace PixelVisionSDK.Chips
{
    public interface IGameAPI
    {
        int BackgroundColor(int? id = null);

        bool Button(int button, int player);

        bool ButtonReleased(int button, int player);

        void ChangeColor(int index, int id);

        void Clear(int x = 0, int y = 0, int width = 0, int height = 0);
        
        int DisplayHeight(bool visiblePixels = true);

        int DisplayWidth(bool visiblePixels = true);

        int Flag(int column, int row);

        void DrawPixels(int[] pixelData, int x, int y, int width, int height, int mode = 0, bool flipH = false, bool flipV = false, int colorOffset = 0);

        void DrawSprite(int id, int x, int y, bool flipH = false, bool flipV = false, bool aboveBG = true, int colorOffset = 0);

        void DrawText(string text, int x, int y, int mode = 0, string font = "Default", int colorOffset = 0, int spacing = 0);

        void DrawTilemap(int x = 0, int y = 0, int columns = 0, int rows = 0);

        string ReadData(string key);

        void RebuildMap();

        void ScrollTo(int x = 0, int y = 0);

        int ScrollX();

        int ScrollY();

        void Sfx(int id, int channel = 0);

        void Song(int id, bool loop = true);
        
        int SpriteHeight();

        int SpriteWidth();

        int TilemapWidth();

        int TilemapHeight();

        void UpdateSprite(int id, int[] pixelData);

        void UpdateTile(int column, int row, int id = -1, int colorOffset = -1, int flag = -1);

        void WriteData(string key, string value);
        
        // To be removed
        void DrawSprites(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false, bool aboveBG = true, int colorOffset = 0);
        void DrawTile(int id, int column, int row, int colorOffset = 0, int flag = -1);
        void DrawTiles(int[] ids, int column, int row, int columns, int colorOffset = 0, int flag = -1);

        //TODO need mouseX, mouseY, inputString, mouseBtnDown, DrawPixelModes (sprite, tilemap cache, display), Get Key, LoadLib
    }
}
