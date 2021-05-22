
namespace PixelVision8.Player
{

    public partial class UIBuilder
    {

        public PaintCanvas CreatePaintCanvas(int width, int height, int x = 0, int y = 0, bool autoManage = true)
        {

            var canvas = new PaintCanvas(this, width, height, x, y);

            if(autoManage == true)
            {
                AddUI(canvas);
            }

            return canvas;

        }


    }

}