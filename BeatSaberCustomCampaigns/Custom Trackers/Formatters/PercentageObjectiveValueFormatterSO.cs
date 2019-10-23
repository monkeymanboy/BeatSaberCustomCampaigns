namespace BeatSaberCustomCampaigns.Custom_Trackers
{
    public class PercentageObjectiveValueFormatterSO : ObjectiveValueFormatterSO
    {
        public override string FormatValue(int value)
        {
            return ((value / 100f).ToString("n2")+ "%").Replace("0%", "%").Replace(".0%", "%");
        }
    }
}
