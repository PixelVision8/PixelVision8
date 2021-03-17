using System.Text;
using PixelVision8.Runner;

namespace PixelVision8.Player
{
    public partial class GameChip
    {
        private StringBuilder _printSB = new StringBuilder();
        
        public void Print(params object[] values)
        {
            
            _printSB.Clear();

            for (int i = 0; i < values.Length; i++)
            {
                _printSB.Append(values[i].ToString()).Append(' ');
            }

            Log.Print(_printSB.ToString(), LogType.Log);

        }
    }
}