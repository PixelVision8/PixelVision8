// //   
// // Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
// //  
// // Licensed under the Microsoft Public License (MS-PL) except for a few
// // portions of the code. See LICENSE file in the project root for full 
// // license information. Third-party libraries used by Pixel Vision 8 are 
// // under their own licenses. Please refer to those libraries for details 
// // on the license they use.
// // 
// // Contributors
// // --------------------------------------------------------
// // This is the official list of Pixel Vision 8 contributors:
// //  
// // Jesse Freeman - @JesseFreeman
// // Christina-Antoinette Neofotistou @CastPixel
// // Christer Kaitila - @McFunkypants
// // Pedro Medeiros - @saint11
// // Shawn Rakowski - @shwany
// //
//
// namespace PixelVision8.Engine
// {
//     /// <summary>Defines the keys on a keyboard.</summary>
//     public enum Keys
//     {
//         /// <summary>Reserved.</summary>
//         None = 0,
//
//         /// <summary>BACKSPACE key.</summary>
//         Backspace = 8,
//
//         /// <summary>TAB key.</summary>
//         Tab = 9,
//
//         /// <summary>ENTER key.</summary>
//         Enter = 13, // 0x0000000D
//
//         /// <summary>ESC key.</summary>
//         Escape = 27, // 0x0000001B
//
//         /// <summary>SPACEBAR key.</summary>
//         Space = 32, // 0x00000020
//
//         /// <summary>PAGE UP key.</summary>
//         PageUp = 33, // 0x00000021
//
//         /// <summary>PAGE DOWN key.</summary>
//         PageDown = 34, // 0x00000022
//
//         /// <summary>END key.</summary>
//         End = 35, // 0x00000023
//
//         /// <summary>HOME key.</summary>
//         Home = 36, // 0x00000024
//
//         /// <summary>LEFT ARROW key.</summary>
//         Left = 37, // 0x00000025
//
//         /// <summary>UP ARROW key.</summary>
//         Up = 38, // 0x00000026
//
//         /// <summary>RIGHT ARROW key.</summary>
//         Right = 39, // 0x00000027
//
//         /// <summary>DOWN ARROW key.</summary>
//         Down = 40, // 0x00000028
//
//         /// <summary>INS key.</summary>
//         Insert = 45, // 0x0000002D
//
//         /// <summary>DEL key.</summary>
//         Delete = 46, // 0x0000002E
//
//         /// <summary>
//         ///     Used for miscellaneous characters; it can vary by keyboard.
//         /// </summary>
//         Alpha0 = 48, // 0x00000030
//
//         /// <summary>
//         ///     Used for miscellaneous characters; it can vary by keyboard.
//         /// </summary>
//         Alpha1 = 49, // 0x00000031
//
//         /// <summary>
//         ///     Used for miscellaneous characters; it can vary by keyboard.
//         /// </summary>
//         Alpha2 = 50, // 0x00000032
//
//         /// <summary>
//         ///     Used for miscellaneous characters; it can vary by keyboard.
//         /// </summary>
//         Alpha3 = 51, // 0x00000033
//
//         /// <summary>
//         ///     Used for miscellaneous characters; it can vary by keyboard.
//         /// </summary>
//         Alpha4 = 52, // 0x00000034
//
//         /// <summary>
//         ///     Used for miscellaneous characters; it can vary by keyboard.
//         /// </summary>
//         Alpha5 = 53, // 0x00000035
//
//         /// <summary>
//         ///     Used for miscellaneous characters; it can vary by keyboard.
//         /// </summary>
//         Alpha6 = 54, // 0x00000036
//
//         /// <summary>
//         ///     Used for miscellaneous characters; it can vary by keyboard.
//         /// </summary>
//         Alpha7 = 55, // 0x00000037
//
//         /// <summary>
//         ///     Used for miscellaneous characters; it can vary by keyboard.
//         /// </summary>
//         Alpha8 = 56, // 0x00000038
//
//         /// <summary>
//         ///     Used for miscellaneous characters; it can vary by keyboard.
//         /// </summary>
//         Alpha9 = 57, // 0x00000039
//
//         /// <summary>A key.</summary>
//         A = 65, // 0x00000041
//
//         /// <summary>B key.</summary>
//         B = 66, // 0x00000042
//
//         /// <summary>C key.</summary>
//         C = 67, // 0x00000043
//
//         /// <summary>D key.</summary>
//         D = 68, // 0x00000044
//
//         /// <summary>E key.</summary>
//         E = 69, // 0x00000045
//
//         /// <summary>F key.</summary>
//         F = 70, // 0x00000046
//
//         /// <summary>G key.</summary>
//         G = 71, // 0x00000047
//
//         /// <summary>H key.</summary>
//         H = 72, // 0x00000048
//
//         /// <summary>I key.</summary>
//         I = 73, // 0x00000049
//
//         /// <summary>J key.</summary>
//         J = 74, // 0x0000004A
//
//         /// <summary>K key.</summary>
//         K = 75, // 0x0000004B
//
//         /// <summary>L key.</summary>
//         L = 76, // 0x0000004C
//
//         /// <summary>M key.</summary>
//         M = 77, // 0x0000004D
//
//         /// <summary>N key.</summary>
//         N = 78, // 0x0000004E
//
//         /// <summary>O key.</summary>
//         O = 79, // 0x0000004F
//
//         /// <summary>P key.</summary>
//         P = 80, // 0x00000050
//
//         /// <summary>Q key.</summary>
//         Q = 81, // 0x00000051
//
//         /// <summary>R key.</summary>
//         R = 82, // 0x00000052
//
//         /// <summary>S key.</summary>
//         S = 83, // 0x00000053
//
//         /// <summary>T key.</summary>
//         T = 84, // 0x00000054
//
//         /// <summary>U key.</summary>
//         U = 85, // 0x00000055
//
//         /// <summary>V key.</summary>
//         V = 86, // 0x00000056
//
//         /// <summary>W key.</summary>
//         W = 87, // 0x00000057
//
//         /// <summary>X key.</summary>
//         X = 88, // 0x00000058
//
//         /// <summary>Y key.</summary>
//         Y = 89, // 0x00000059
//
//         /// <summary>Z key.</summary>
//         Z = 90, // 0x0000005A
//
//         /// <summary>Numeric keypad 0 key.</summary>
//         NumPad0 = 96, // 0x00000060
//
//         /// <summary>Numeric keypad 1 key.</summary>
//         NumPad1 = 97, // 0x00000061
//
//         /// <summary>Numeric keypad 2 key.</summary>
//         NumPad2 = 98, // 0x00000062
//
//         /// <summary>Numeric keypad 3 key.</summary>
//         NumPad3 = 99, // 0x00000063
//
//         /// <summary>Numeric keypad 4 key.</summary>
//         NumPad4 = 100, // 0x00000064
//
//         /// <summary>Numeric keypad 5 key.</summary>
//         NumPad5 = 101, // 0x00000065
//
//         /// <summary>Numeric keypad 6 key.</summary>
//         NumPad6 = 102, // 0x00000066
//
//         /// <summary>Numeric keypad 7 key.</summary>
//         NumPad7 = 103, // 0x00000067
//
//         /// <summary>Numeric keypad 8 key.</summary>
//         NumPad8 = 104, // 0x00000068
//
//         /// <summary>Numeric keypad 9 key.</summary>
//         NumPad9 = 105, // 0x00000069
//
//         /// <summary>Multiply key.</summary>
//         Multiply = 106, // 0x0000006A
//
//         /// <summary>Add key.</summary>
//         Add = 107, // 0x0000006B
//
//         /// <summary>Separator key.</summary>
//         Separator = 108, // 0x0000006C
//
//         /// <summary>Subtract key.</summary>
//         Subtract = 109, // 0x0000006D
//
//         /// <summary>Decimal key.</summary>
//         Decimal = 110, // 0x0000006E
//
//         /// <summary>Divide key.</summary>
//         Divide = 111, // 0x0000006F
//
//         /// <summary>Left SHIFT key.</summary>
//         LeftShift = 160, // 0x000000A0
//
//         /// <summary>Right SHIFT key.</summary>
//         RightShift = 161, // 0x000000A1
//
//         /// <summary>Left CONTROL key.</summary>
//         LeftControl = 162, // 0x000000A2
//
//         /// <summary>Right CONTROL key.</summary>
//         RightControl = 163, // 0x000000A3
//
//         /// <summary>Left ALT key.</summary>
//         LeftAlt = 164, // 0x000000A4
//
//         /// <summary>Right ALT key.</summary>
//         RightAlt = 165, // 0x000000A5
//
//         /// <summary>The OEM Semicolon key on a US standard keyboard.</summary>
//         Semicolon = 186, // 0x000000BA
//
//         /// <summary>For any country/region, the '+' key.</summary>
//         Plus = 187, // 0x000000BB
//
//         /// <summary>For any country/region, the ',' key.</summary>
//         Comma = 188, // 0x000000BC
//
//         /// <summary>For any country/region, the '-' key.</summary>
//         Minus = 189, // 0x000000BD
//
//         /// <summary>For any country/region, the '.' key.</summary>
//         Period = 190, // 0x000000BE
//
//         /// <summary>The OEM question mark key on a US standard keyboard.</summary>
//         Question = 191, // 0x000000BF
//
//         /// <summary>The OEM tilde key on a US standard keyboard.</summary>
//         Tilde = 192, // 0x000000C0
//
//         /// <summary>The OEM open bracket key on a US standard keyboard.</summary>
//         OpenBrackets = 219, // 0x000000DB
//
//         /// <summary>The OEM pipe key on a US standard keyboard.</summary>
//         Pipe = 220, // 0x000000DC
//
//         /// <summary>The OEM close bracket key on a US standard keyboard.</summary>
//         CloseBrackets = 221, // 0x000000DD
//
//         /// <summary>
//         ///     The OEM singled/double quote key on a US standard keyboard.
//         /// </summary>
//         Quotes = 222, // 0x000000DE
//
//         /// <summary>
//         ///     The OEM angle bracket or backslash key on the RT 102 key keyboard.
//         /// </summary>
//         Backslash = 226, // 0x000000E2
//
//         /// <summary>CLEAR key.</summary>
//         OemClear = 254 // 0x000000FE
//     }
// }