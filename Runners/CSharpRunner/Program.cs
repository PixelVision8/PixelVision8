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

using System;
using System.Globalization;
using System.IO;

namespace Desktop
{

    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Fix a bug related to parsing numbers in Europe, among other things
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
                    
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content");
                
            // Need to do this for MacOS
            if (root.EndsWith("/MonoBundle/Content"))
            {
                root = root.Replace("/MonoBundle/Content", "/Resources/Content");
            }
            
            using (var game = new CSharpRunner(root))
                game.Run();
        }
    }
}
