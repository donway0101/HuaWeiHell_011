using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorter
{
    public class ConfigManager
    {
        private CentralControl _cc;

        public bool PowerUpHoming { get; set; } = false;

        public double LLoadFixtureHeight { get; set; } = 0.1;

        public List<UserSetting> Settings { get; set; }

        public ConfigManager(CentralControl centralControl)
        {
            _cc = centralControl;
            Settings = new List<UserSetting>();
        }

        public void SaveSettings()
        {
            var jStr = Helper.ConvertToJsonString(Settings);
            Helper.WriteFile(jStr, Properties.Settings.Default.UserSettings);
        }

        public void ReadSettings()
        {
            string jStr = Helper.ReadFile(Properties.Settings.Default.UserSettings);
            Settings = Helper.ConvertToUserSettings(jStr);
            //_cc.LRobot.FixtureHeight = (double)GetSettingValue(UserSettingId.LFixtureHeight);
            //_cc.LRobot.VisionSimulateMode = (bool)GetSettingValue(UserSettingId.LVisionSimulate);
        }

        public object GetSettingValue(UserSettingId id)
        {
            foreach (var setting in Settings)
            {
                if (setting.Id == id)
                {
                    return setting.Value;
                }
            }

            throw new Exception("Setting not found:" + id);
        }


    }

    public class UserSetting
    {
        public UserSettingId Id { get; set; }
        public object Value { get; set; }
        public string Remarks { get; set; }
    }

    public enum UserSettingId
    {
        None = 0,
        LFixtureHeight = 1,
        LVisionSimulate = 2,
    }

}
