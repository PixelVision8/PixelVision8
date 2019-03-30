
namespace PixelVision8.Runner.Utils
{
    public class TiledUtil
    {
        public static uint CreateGID(int id, bool flipH, bool flipV){
		
		
            var gid = (uint)id;
		
            if(flipH)
                gid |= 1U<<31;
		
            if(flipV)
                gid |= 1U<<30;
		
		
            return gid;
		
        }
	
        public static void ReadGID(uint gid, out int id, out bool flipH, out bool flipV)
        {
        
            // Starts with 0, 31 in an int
        
            // Create mask by subtracting the bits you don't want

            var idMask = (1 << 30) - 1;

            id = (int)(gid & idMask);

            var hMask = 1 << 31;

            flipH = (hMask & gid) != 0;
        
            var vMask = 1 << 30;

            flipV = (vMask & gid) != 0;
        
        }
    }
}