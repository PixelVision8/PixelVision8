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
using MiniJSON;

namespace PixelVisionRunner.Parsers
{

    public class JsonParser : AbstractParser
    {

        protected Dictionary<string, object> data;

        protected string jsonString;

        public JsonParser(string jsonString)
        {
            //Debug.Log("New Json Parser");
            this.jsonString = jsonString;

            CalculateSteps();
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            steps.Add(ParseJson);

            //Debug.Log("Calculating "+totalSteps+" steps.");
        }

        public virtual void ParseJson()
        {
            //Debug.Log("Parsing Json");
            data = Json.Deserialize(jsonString) as Dictionary<string, object>;
            currentStep++;
        }

//                target.DeserializeData(data);
//            if(target != null)
//            //Debug.Log("Applying Settings");
//        {

//        public virtual void ApplySettings()
//            currentStep++;
//        }

    }

}