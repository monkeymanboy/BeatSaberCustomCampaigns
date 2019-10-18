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
