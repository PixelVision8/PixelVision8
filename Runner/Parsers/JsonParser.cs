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

using System.Collections.Generic;
using System.Text;
using PixelVision8.Runner.Utils;

namespace PixelVision8.Runner.Parsers
{

    public class JsonParser : AbstractParser
    {

        protected Dictionary<string, object> data;

        public override byte[] bytes
        {
            get { return Encoding.ASCII.GetBytes(jsonString); }
            set
            {
                jsonString = Encoding.UTF8.GetString(value);
            }
        }
        
        protected string jsonString;

        public JsonParser(string jsonString = "")
        {
            this.jsonString = jsonString;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            steps.Add(ParseJson);
        }

        public virtual void ParseJson()
        {
            data = Json.Deserialize(jsonString) as Dictionary<string, object>;
            currentStep++;
        }

    }

}