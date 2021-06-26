using System.Globalization;

namespace CustomCampaigns.CustomMissionObjectives.Formatters
{
    public class PercentageObjectiveValueFormatterSO : ObjectiveValueFormatterSO
    {
        public override string FormatValue(int value)
        {
            return $"{(value / 100f).ToString("G4", CultureInfo.InvariantCulture)}%";
        }
    }
}
