namespace CustomCampaigns.External
{
    public interface IMissionModifier
    {
        public void OnMissionStart();
        public void OnMissionEnd();
        public void OnMissionFail();
    }
}
