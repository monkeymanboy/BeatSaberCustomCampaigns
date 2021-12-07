namespace CustomCampaigns.External.Interfaces
{
    public interface IModifierHandler
    {
        public bool OnLoadMission(string[] inArray);

        public void OnMissionFailedToLoad();
        public void OnMissionEnd();
    }
}
