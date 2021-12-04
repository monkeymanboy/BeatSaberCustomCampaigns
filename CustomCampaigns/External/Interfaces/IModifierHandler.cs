using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCampaigns.External.Interfaces
{
    public interface IModifierHandler
    {
        public bool OnLoadMission(string[] inArray);

        public void OnMissionFailedToLoad();
        public void OnMissionEnd();
    }
}
