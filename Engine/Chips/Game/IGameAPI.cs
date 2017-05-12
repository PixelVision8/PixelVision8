
namespace PixelVisionSDK.Chips
{
    public interface IGameAPI
    {
        void BackgroundColor(int id);

        bool Button(int button, int player);

        bool ButtonReleased(int id, int player);

        void Clear(int x = 0, int y = 0, int width = 0, int height = 0);
        
        int DisplayHeight(bool visiblePixels = true);

        int DisplayWidth(bool visiblePixels = true);

        int Flag(int column, int row);

        void DrawPixels(int[] pixelData, int x, int y, int width, int height, int mode = 0, bool flipH = false, bool flipV = false, int colorOffset = 0);

        void DrawSprite(int id, int x, int y, bool flipH = false, bool flipV = false, bool aboveBG = true, int colorOffset = 0);

        void DrawSprites(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false, bool aboveBG = true, int colorOffset = 0);

        void DrawText(string text, int x, int y, int mode = 0, string font = "Default", int colorOffset = 0, int spacing = 0);

        void DrawTile(int id, int column, int row, int colorOffset = 0, int flag = -1);

        void DrawTiles(int[] ids, int column, int row, int columns, int colorOffset = 0, int flag = -1);

        void DrawTilemap(int x = 0, int y = 0, int columns = 0, int rows = 0);

        string ReadData(string key);

        void RebuildMap();

        void Scroll(int x = 0, int y = 0);

        void Sfx(int id, int channel = 0);

        void Song(int id, bool loop = true);
        
        int SpriteHeight();

        int SpriteWidth();

        int TilemapWidth();

        int TilemapHeight();

        void UpdateSprite(int[] pixelData, int id);

        void UpdateTile(int column, int row, int id = -1, int colorOffset = -1, int flag = -1);

        void ChangeColor(int index, int id);

        void WriteData(string key, string value);
        
    }
}
