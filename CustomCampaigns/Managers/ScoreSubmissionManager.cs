using SiraUtil.Submissions;
using Zenject;

namespace CustomCampaigns.Managers
{
    public class ScoreSubmissionManager : IInitializable
    {
        private const string SOURCE = "Custom Campaigns";
        private Submission _siraSubmission;

        public ScoreSubmissionManager(Submission scoreSubmission)
        {
            _siraSubmission = scoreSubmission;
        }

        public void Initialize()
        {
            if (CustomCampaignManager.currentMissionData.gameplayModifiers.songSpeedMul != 1f)
            {
                _siraSubmission.DisableScoreSubmission(SOURCE, "Non-default song speed");
            }

            if (CustomCampaignManager.currentMissionData.gameplayModifiers.fastNotes)
            {
                _siraSubmission.DisableScoreSubmission(SOURCE, "Fast notes modifier");
            }
        }
    }
}
