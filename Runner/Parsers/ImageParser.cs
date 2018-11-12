
namespace PixelVisionRunner.Parsers
{
    public class ImageParser: AbstractParser
    {
        protected IImageParser imageParser;

        public int imageWidth => imageParser.width;
        public int imageHeight => imageParser.height;
//        public IColor[] colorPixels => imageParser.colorPixels;
//        public List<IColor> colorPalette => imageParser.colorPalette;
        
        public ImageParser(IImageParser imageParser, string maskHex = "#FF00FF")
        {

            this.bytes = bytes;

            this.imageParser = imageParser;

        }
        
        public override void CalculateSteps()
        {
            base.CalculateSteps();
            steps.Add(ParseImageData);
        }
        
        public void ParseImageData()
        {
            if(imageParser.IsImage())
                imageParser.ReadStream();
            
            currentStep++;
        }

    }
}