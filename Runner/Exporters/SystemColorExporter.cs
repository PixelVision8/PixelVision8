//using System;
//using System.Linq;
//using PixelVisionSDK;
//using PixelVisionSDK.Chips;
//
//namespace PixelVisionRunner.Exporters
//{
//    public class SystemColorExporter : ColorPaletteExporter
//    {
//        public SystemColorExporter(string fileName, ColorChip colorChip, IImageExporter imageExporter) : base(fileName, colorChip, imageExporter)
//        {
//            
//        }
//
//        public override void ConfigureColors()
//        {
//
//            colors = colorChip.supportedColors.Select(c => new ColorData(c) as IColor).ToArray();
//            total = colors.Length;
//
//            width = 8;
//            height = (int) Math.Ceiling(total / (float) width);
//        }
//    }
//}