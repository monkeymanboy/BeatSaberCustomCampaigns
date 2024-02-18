using System.Threading;

namespace CustomCampaigns.Managers
{
    public class UserInfoManager
    {
        IPlatformUserModel _platformUserModel;

        public static UserInfo UserInfo;

        public UserInfoManager(IPlatformUserModel platformUserModel)
        {
            Plugin.logger.Debug("user info manager");
            _platformUserModel = platformUserModel;
            GetUserInfo();
        }

        public async void GetUserInfo()
        {
            UserInfo = await _platformUserModel.GetUserInfo(CancellationToken.None);
        }
    }
}
