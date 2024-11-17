using System.Threading;

namespace CustomCampaigns.Managers
{
    public class UserInfoManager
    {
        IPlatformUserModel _platformUserModel;

        public static UserInfo UserInfo;

        public UserInfoManager(IPlatformUserModel platformUserModel)
        {
            _platformUserModel = platformUserModel;
            GetUserInfo();
        }

        public async void GetUserInfo()
        {
            UserInfo = await _platformUserModel.GetUserInfo(CancellationToken.None);
        }
    }
}
