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

using System.Collections.Generic;
using System.Text;

namespace PixelVision8.Runner
{
    public class JsonParser : AbstractParser
    {
        protected Dictionary<string, object> Data;

        protected string JsonString;

        public JsonParser(string filePath, IFileLoader fileLoadHelper)
        {
            FileLoadHelper = fileLoadHelper;
            SourcePath = filePath;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            Steps.Add(ParseJson);
        }

        public virtual void ParseJson()
        {
            Data = Json.Deserialize(JsonString) as Dictionary<string, object>;
            StepCompleted();
        }

        public override void LoadSourceData()
        {
            if (FileLoadHelper != null)
            {
                JsonString = Encoding.UTF8.GetString(FileLoadHelper.ReadAllBytes(SourcePath));
            }

            StepCompleted();
        }
    }
}