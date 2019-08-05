using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorter
{
    public class UserSetting
    {
        public UserSettingId Id { get; set; }
        public object Value { get; set; }
        public string Remarks { get; set; }
    }

    public enum UserSettingId
    {
        None = 0,

        //1-50 for L
        LSafeXArea = 1,
        LSafeYArea = 2,
        LSafeZHeight = 3,
        LLoadTrayHeight = 4,
        LUnloadTrayHeight = 5,
        LFixtureHeight = 6,
        LUvDelayMs = 7,
        LMaxFailCount = 8,
        LLFixtureHeight = 9,
        LLVisionSimulate = 10,
        LTrayXOffset = 11,
        LTrayYOffset = 12,
        LTrayRowCount = 13,
        LTrayColumneCount = 14,
        LTrayHeight = 15,
        LTrayYIncreaseDirection = 16,
        //Tray info....

        //51-100 for V
        VSafeXArea = 51,
        VSafeYArea = 52,
        VSafeZHeight = 53,
        VLoadTrayHeight = 54,
        VUnloadTrayHeight = 55,
        VFixtureHeight = 56,
        VUvDelayMs = 57,
        VMaxFailCount = 58,
        VLFixtureHeight = 59,
        VLVisionSimulate = 60,
        VTrayXOffset = 61,
        VTrayYOffset = 62,
        VTrayRowCount = 63,
        VTrayColumneCount = 64,
        VTrayHeight = 65,
        VTrayYIncreaseDirection = 66,

        //101-150 for Glue point
        GluePointSafeXArea = 101,
        GluePointSafeYArea = 102,
        GluePointSafeZHeight = 103,
        GluePointFixtureHeight = 106,
        GluePointVisionSimulateMode = 107,
        GluePointNeedleOnZHeight = 108,
        GluePointNeedleOnZHeightCompensation = 109,
        GluePointGlueRadius = 110,
        GluePointDetectNeedleHeightSpeed = 111,
        GluePointNeedleTouchPressure = 112,
        GluePointLaserRefereceZHeight = 113,
        GluePointLaserAboveFixtureHeight = 114,
        GluePointCameraAboveFixtureHeight = 115,
        GluePointCameraAboveChinaHeight = 116,
        GluePointLaserSurfaceToNeckHeightOffset = 117,
        GluePointLaserSurfaceToSpringHeightOffset = 118,
        GluePointNeedleCleaningIntervalSec = 119,

        //151-200 for Glue point
        GlueLineSafeXArea = 151,
        GlueLineSafeYArea = 152,
        GlueLineSafeZHeight = 153,
        GlueLineFixtureHeight = 156,
        GlueLineVisionSimulateMode = 157,
        GlueLineNeedleOnZHeight = 158,
        GlueLineNeedleOnZHeightCompensation = 159,
        GlueLineGlueRadius = 160,
        GlueLineDetectNeedleHeightSpeed = 161,
        GlueLineNeedleTouchPressure = 162,
        GlueLineLaserRefereceZHeight = 163,
        GlueLineLaserAboveFixtureHeight = 164,
        GlueLineCameraAboveFixtureHeight = 165,
        GlueLineCameraAboveChinaHeight = 166,
        GlueLineLaserSurfaceToNeckHeightOffset = 167,
        GlueLineLaserSurfaceToSpringHeightOffset = 168,
        GlueLineNeedleCleaningIntervalSec = 169,

    }

public class SettingManager
    {
        private readonly CentralControl _cc;

        public SettingManager(CentralControl control)
        {
            _cc = control;
        }

        public void ValidateSettings()
        {
            //_cc.LRobot.FixtureHeight = (double)GetSettingValue(UserSettingId.LFixtureHeight);
        }

        /// <summary>
        /// Careful, it may overwrite the origin file.
        /// </summary>
        public void CreateEmptySettingsFile()
        {
            List<UserSetting> settings = new List<UserSetting>();
            foreach (UserSettingId item in Enum.GetValues(typeof(UserSettingId)))
            {
                settings.Add(new UserSetting() { Id = item });
            }
            var jStr = Helper.ConvertToJsonString(settings);
            Helper.WriteFile(jStr, Properties.Settings.Default.UserSettings);
        }

        public object GetSettingValue(UserSettingId id)
        {
            return Helper.GetSettingValue(_cc.UserSettings, id);
        }

    }

}
