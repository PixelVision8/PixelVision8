//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
//
// Based on code example by Stefano Tempesta Copyright (c) 2016 under MS-LPL License
// at https://code.msdn.microsoft.com/windowsdesktop/Ordinal-String-Sorting-1cbac582
// Modified for PixelVision8 by Jesse Freeman
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
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PixelVision8.Runner.Utils
{
	/// <summary>
	/// Defines a method that a string implements to compare two values according to their ordinal value (text and number).
	/// This class implements the <c>IComparer&lt;T&gt;</c> interface with type string.
	/// </summary>
	public class OrdinalStringComparer : IComparer<string>
	{
		private bool _ignoreCase = true;

		/// <summary>
		/// Creates an instance of <c>OrdinalStringComparer</c> for case-insensitive string comparison.
		/// </summary>
		public OrdinalStringComparer()
			: this(true)
		{
		}

		/// <summary>
		/// Creates an instance of <c>OrdinalStringComparer</c> for case comparison according to the value specified in input.
		/// </summary>
		/// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false.</param>
		public OrdinalStringComparer(bool ignoreCase)
		{
			_ignoreCase = ignoreCase;
		}

		/// <summary>
		/// Compares two strings and returns a value indicating whether one is less than, equal to, or greater than the other.
		/// </summary>
		/// <param name="x">The first string to compare.</param>
		/// <param name="y">The second string to compare.</param>
		/// <returns>A signed integer that indicates the relative values of x and y, as in the Compare method in the <c>IComparer&lt;T&gt;</c> interface.</returns>
		public int Compare(string x, string y)
		{
			// check for null values first: a null reference is considered to be less than any reference that is not null
			if (x == null && y == null)
			{
				return 0;
			}
			if (x == null)
			{
				return -1;
			}
			if (y == null)
			{
				return 1;
			}

			StringComparison comparisonMode = _ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;

			string[] splitX = Regex.Split(x.Replace(" ", ""), "([0-9]+)");
			string[] splitY = Regex.Split(y.Replace(" ", ""), "([0-9]+)");

			int comparer = 0;

			for (int i = 0; comparer == 0 && i < splitX.Length; i++)
			{
				if (splitY.Length <= i)
				{
					comparer = 1; // x > y
				}

				int numericX = -1;
				int numericY = -1;
				if (int.TryParse(splitX[i], out numericX))
				{
					if (int.TryParse(splitY[i], out numericY))
					{
						comparer = numericX - numericY;
					}
					else
					{
						comparer = 1; // x > y
					}
				}
				else
				{
					comparer = String.Compare(splitX[i], splitY[i], comparisonMode);
				}
			}

			return comparer;
		}
	}
}
