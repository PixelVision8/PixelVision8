using Microsoft.Xna.Framework;

namespace PixelVision8.Player
{
    public partial class PixelDataUtilitiesTest
    {
        
        class MergeTestData
        {
            public Point DestPos;
            public Rectangle SampleRect;
            public int[] FinalMerge;
            public int[] FinalSet;
            public int[] FinalOffsetMerge;
            public int[] FinalFlipHVMerge;
            public int ColorOffset;
        }

        private readonly Rectangle _srcRect = new Rectangle(0, 0, 3, 3);

        private readonly int[] _srcData = new int[]
        {
            // Normal       Horizontal       Vertical       H+V Flip
            00, 01, 02,//   02, 01, 00,      20, 21, 22,    22, 21, 20,
            10, -1, 12,//   12, -1, 10,      10, -1, 12,    12, -1, 10,
            20, 21, 22,//   22, 21, 20,      00, 01, 02,    02, 01, 00,
        };

        private readonly Rectangle _destRect = new Rectangle(0, 0, 4, 4);

        private readonly int[] _destData = new int[]
        {
            00, 01, 02, 03,
            04, 05, 06, 07,
            08, 09, 10, 11,
            12, 13, 14, 15,
        };


        readonly MergeTestData[] _testData = new MergeTestData[]
        {
            
            // Example 0 - Inside Bottom Right Bounds
            //
            //    -1  0   1   2   3   4
            // -1   
            //  0     --, --, --, --, 
            //  1     --, 00, 01, 02 
            //  2     --, 10, -1, 12 
            //  3     --, 20, 21, 22 
            //  4  
            //
            // Dest   1, 1
            // Sample 0, 0, 3, 3
            //
            new MergeTestData()
            {
                SampleRect = new Rectangle(0, 0, 3, 3),
                DestPos = new Point(1, 1),
                ColorOffset = 10,
                FinalMerge = new int[]
                {
                    00, 01, 02, 03,
                    04, 00, 01, 02,
                    08, 10, 10, 12,
                    12, 20, 21, 22,
                },
                FinalFlipHVMerge = new int[]
                {
                    00, 01, 02, 03,
                    04, 22, 21, 20,
                    08, 12, 10, 10,
                    12, 02, 01, 00,
                },
                FinalOffsetMerge = new int[]
                {
                    00, 01, 02, 03,
                    04, 10, 11, 12,
                    08, 20, 10, 22,
                    12, 30, 31, 32,
                },
                FinalSet = new int[]
                {
                    00, 01, 02, 03,
                    04, 00, 01, 02,
                    08, 10, -1, 12,
                    12, 20, 21, 22,
                }
            },
            
            // Example 1 - Inside Bounds Src Offset
            //
            //    -1  0   1   2   3   4
            // -1   
            //  0     --, --, --, --, 
            //  1     --, -1, 12, -- 
            //  2     --, 21, 22, -- 
            //  3     --, --, --, -- 
            //  4  
            //
            // Dest   1, 1
            // Sample 0, 0, 3, 3
            //
            new MergeTestData()
            {
                DestPos = new Point(1, 1),
                SampleRect = new Rectangle(1, 1, 2, 2),
                FinalMerge = new int[]
                {
                    00, 01, 02, 03,
                    04, 05, 12, 07,
                    08, 21, 22, 11,
                    12, 13, 14, 15,
                },
                FinalFlipHVMerge = new int[]
                {
                    00, 01, 02, 03,
                    04, 22, 21, 07,
                    08, 12, 10, 11,
                    12, 13, 14, 15,
                },
                FinalOffsetMerge = new int[]
                {
                    00, 01, 02, 03,
                    04, 05, 12, 07,
                    08, 21, 22, 11,
                    12, 13, 14, 15,
                },
                FinalSet = new int[]
                {
                    00, 01, 02, 03,
                    04, 00, 01, 07,
                    08, 10, -1, 11,
                    12, 13, 14, 15,
                }
            },
            
            // Example 2 - Out Bounds Upper Left
            //
            //     -1  0   1   2   3   4
            // -1  XX, XX, XX             
            //  0  XX, -1, 12, --, --, 
            //  1  XX, 21, 22, --, --, 
            //  2      --, --, --, --, 
            //  3      --, --, --, --, 
            //  4   
            //
            // Dest   -1,-1
            // Sample 0, 0, 3, 3
            //
            new MergeTestData()
            {
                SampleRect = new Rectangle(0, 0, 3, 3),
                DestPos = new Point(-1, -1),
                ColorOffset = 10,
                FinalMerge = new int[]
                {
                    00, 12, 02, 03,
                    21, 22, 06, 07,
                    08, 09, 10, 11,
                    12, 13, 14, 15,
                },
                FinalFlipHVMerge = new int[]
                {
                    00, 10, 02, 03,
                    01, 00, 06, 07,
                    08, 09, 10, 11,
                    12, 13, 14, 15,
                },
                FinalOffsetMerge = new int[]
                {
                    00, 22, 02, 03,
                    31, 32, 06, 07,
                    08, 09, 10, 11,
                    12, 13, 14, 15,
                },
                FinalSet = new int[]
                {
                    -1, 12, 02, 03,
                    21, 22, 06, 07,
                    08, 09, 10, 11,
                    12, 13, 14, 15,
                }
            },
            
            // Example 3 - Out Bounds Upper Right
            //
            //    -1  0   1   2   3   4
            // -1             XX, XX, XX
            //  0     --, --, 10, -1, XX 
            //  1     --, --, 20, 21, XX 
            //  2     --, --, --, --,   
            //  3     --, --, --, --,    
            //  4                        
            //
            // Dest   2,-1
            // Sample 0, 0, 3, 3
            //
            new MergeTestData()
            {
                SampleRect = new Rectangle(0, 0, 3, 3),
                DestPos = new Point(2, -1),
                ColorOffset = 10,
                FinalMerge = new int[]
                {
                    00, 01, 10, 03,
                    04, 05, 20, 21,
                    08, 09, 10, 11,
                    12, 13, 14, 15,
                },
                FinalFlipHVMerge = new int[]
                {
                    00, 01, 12, 03,
                    04, 05, 02, 01,
                    08, 09, 10, 11,
                    12, 13, 14, 15,
                },
                FinalOffsetMerge = new int[]
                {
                    00, 01, 20, 03,
                    04, 05, 30, 31,
                    08, 09, 10, 11,
                    12, 13, 14, 15,
                },
                FinalSet = new int[]
                {
                    00, 01, 10, -1,
                    04, 05, 20, 21,
                    08, 09, 10, 11,
                    12, 13, 14, 15,
                }
            },
            
            // Example 4 - Out Bounds Bottom Right
            //
            //    -1  0   1   2   3   4
            // -1                      
            //  0     --, --, --, --,   
            //  1     --, --, --, --,   
            //  2     --, --, 00, 01, XX 
            //  3     --, --, 10, -1, XX  
            //  4             XX, XX, XX    
            //
            // Dest   2,2
            // Sample 0, 0, 3, 3
            //
            new MergeTestData()
            {
                SampleRect = new Rectangle(0, 0, 3, 3),
                DestPos = new Point(2, 2),
                ColorOffset = 10,
                FinalMerge = new int[]
                {
                    00, 01, 02, 03,
                    04, 05, 06, 07,
                    08, 09, 00, 01,
                    12, 13, 10, 15,
                },
                FinalFlipHVMerge = new int[]
                {
                    00, 01, 02, 03,
                    04, 05, 06, 07,
                    08, 09, 22, 21,
                    12, 13, 12, 15,
                },
                FinalOffsetMerge = new int[]
                {
                    00, 01, 02, 03,
                    04, 05, 06, 07,
                    08, 09, 10, 11,
                    12, 13, 20, 15,
                },
                FinalSet = new int[]
                {
                    00, 01, 02, 03,
                    04, 05, 06, 07,
                    08, 09, 00, 01,
                    12, 13, 10, -1,
                }
            },
            
            // Example 5 - Out Bounds Bottom Left
            //
            //    -1  0   1   2   3   4
            // -1                      
            //  0     --, --, --, --,   
            //  1     --, --, --, --,   
            //  2  XX, 01, 02, --, --,   
            //  3  XX, -1, 12, --, --,    
            //  4  XX, XX, XX               
            //
            // Dest   -1,2
            // Sample 0, 0, 3, 3
            //
            new MergeTestData()
            {
                SampleRect = new Rectangle(0, 0, 3, 3),
                DestPos = new Point(-1, 2),
                ColorOffset = 10,
                FinalMerge = new int[]
                {
                    00, 01, 02, 03,
                    04, 05, 06, 07,
                    01, 02, 10, 11,
                    12, 12, 14, 15,
                },
                FinalFlipHVMerge = new int[]
                {
                    00, 01, 02, 03,
                    04, 05, 06, 07,
                    21, 20, 10, 11,
                    12, 10, 14, 15,
                },
                FinalOffsetMerge = new int[]
                {
                    00, 01, 02, 03,
                    04, 05, 06, 07,
                    11, 12, 10, 11,
                    12, 22, 14, 15,
                },
                FinalSet = new int[]
                {
                    00, 01, 02, 03,
                    04, 05, 06, 07,
                    01, 02, 10, 11,
                    -1, 12, 14, 15,
                }
            },
            // Example 6 - Out Of Bounds Top Src Offset
            //
            //    -1  0   1   2   3   4
            // -1             XX       
            //  0     --, --, 21, --,   
            //  1     --, --, --, --,   
            //  2     --, --, --, --,   
            //  3     --, --, --, --,    
            //  4                          
            //
            // Dest   -1,2
            // Sample 1, 1, 1, 2
            //
            new MergeTestData()
            {
                SampleRect = new Rectangle(1, 1, 1, 2),
                DestPos = new Point(2, -1),
                ColorOffset = 10,
                FinalMerge = new int[]
                {
                    00, 01, 21, 03,
                    04, 05, 06, 07,
                    08, 09, 10, 11,
                    12, 13, 14, 15,
                },
                FinalFlipHVMerge = new int[]
                {
                    00, 01, 02, 03,
                    04, 05, 06, 07,
                    08, 09, 10, 11,
                    12, 13, 14, 15,
                },
                FinalOffsetMerge = new int[]
                {
                    00, 01, 31, 03,
                    04, 05, 06, 07,
                    08, 09, 10, 11,
                    12, 13, 14, 15,
                },
                FinalSet = new int[]
                {
                    00, 01, 10, 03,
                    04, 05, 06, 07,
                    08, 09, 10, 11,
                    12, 13, 14, 15,
                }
            },
        };
    }
}