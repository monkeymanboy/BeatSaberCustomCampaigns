using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberCustomCampaigns.Custom_Trackers
{
    public class MillisecondObjectiveValueFormatterSO : ObjectiveValueFormatterSO
    {
        public override string FormatValue(int value)
        {
            return $"{value} ms";
        }
    }
}
