namespace CustomCampaigns.External.Interfaces
{
    public interface INotifyMissionCompletionResults
    {
        public void OnMissionCompletionResultsAvailable(MissionCompletionResults missionCompletionResults);
    }
}
