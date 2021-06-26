namespace CustomCampaigns.CustomMissionObjectives.Formatters
{
    public class MillisecondObjectiveValueFormatterSO : ObjectiveValueFormatterSO
    {
        public override string FormatValue(int value)
        {
            return $"{value} ms";
        }
    }
}
