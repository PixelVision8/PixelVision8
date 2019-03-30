using System.Text;
using PixelVisionRunner.Exporters;
using PixelVisionRunner.Utils;
using PixelVisionSDK;

namespace GameCreator.Exporters
{
    public class SoundExporter :AbstractExporter
    {
        private IEngine targetEngine;
        private StringBuilder sb;
        
        public SoundExporter(string fileName, IEngine targetEngine) : base(fileName)
        {
            this.targetEngine = targetEngine;
            
//            CalculateSteps();
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            
            // Create a new string builder
            steps.Add(CreateStringBuilder);
            
            
            steps.Add(SaveGameData);
            
            // Save the final string builder
            steps.Add(CloseStringBuilder);
        }

        private void SaveGameData()
        {
            var soundChip = targetEngine.soundChip;
            
            JsonUtil.GetLineBreak(sb, 1);
            JsonUtil.indentLevel++;
            sb.Append("\"sounds\": [");

            var total = soundChip.totalSounds;
            for (var i = 0; i < total; i++)
            {
                var sound = soundChip.ReadSound(i);
//                if (sound != null)
//                {
                    JsonUtil.indentLevel++;
                
                
                
//                {
//                    "name":"Melody",
//                    "settings":"0,.5,,.2,,.2,.3,.1266,,,,,,,,,,,,,,,,,,1,,,,,,"
//                },
                
                
                
                sb.Append("{");
                JsonUtil.GetLineBreak(sb, 1);
                
                sb.Append("\"name\":\"");
                sb.Append(sound.name);
                sb.Append("\",");
                JsonUtil.GetLineBreak(sb, 1);
                sb.Append("\"settings\":");
                sb.Append("\"" + sound.ReadSettings() + "\"");
                JsonUtil.GetLineBreak(sb, 1);
                sb.Append("}");
                
//                    sb.Append(sound.ReadSettings());
                    if (i < total - 1)
                        sb.Append(",");
                
                JsonUtil.GetLineBreak(sb, 1);
                    JsonUtil.indentLevel--;
//                }
            }

            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("]");
            
            currentStep++;
        }
        
        private void CreateStringBuilder()
        {
            sb = new StringBuilder();
            
            sb.Append("{");
            JsonUtil.indentLevel++;
            
            JsonUtil.GetLineBreak(sb);
            sb.Append("\"SoundChip\":");

            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);
            
//            JsonUtil.indentLevel++;

            currentStep++;
        }

        private void CloseStringBuilder()
        {
            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb);
            sb.Append("}");
            
            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("}");
            
            bytes = Encoding.UTF8.GetBytes(sb.ToString());
            
            currentStep++;
        }
    }
}