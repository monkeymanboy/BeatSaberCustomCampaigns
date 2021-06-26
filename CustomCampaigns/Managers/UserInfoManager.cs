using BeatSaberCustomCampaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCampaigns.Managers
{
    class UserInfoManager
    {
        IPlatformUserModel _platformUserModel;
        public UserInfoManager(IPlatformUserModel platformUserModel)
        {
            Plugin.logger.Debug("user info manager");
            _platformUserModel = platformUserModel;
            GetUserInfo();
        }

        public async void GetUserInfo()
        {
            UserInfo userInfo = await _platformUserModel.GetUserInfo();
            APITools.SetUserInfo(userInfo);
        }
    }
}
