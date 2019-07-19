using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Bp.Mes;

namespace Sorter
{
    public static class Helper
    {
        public static void SaveConfiguration(string config, string path)
        {
            System.IO.File.WriteAllText(path, config);
        }

        public static void AddCapturePosition(string posInfo, string path)
        {
            System.IO.File.AppendAllText(path, posInfo + Environment.NewLine);
        }

        public static string ReadConfiguration(string path)
        {
            return System.IO.File.ReadAllText(path);
        }

        public static string ConvertObjectToString(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        public static Motor[] ConvertConfigToMotors(string config)
        {
            return JsonConvert.DeserializeObject<Motor[]>(config);
        }

        public static CapturePosition[] ConvertConfigToCapturePositions(string config)
        {
            return JsonConvert.DeserializeObject<CapturePosition[]>(config);
        }

        public static CapturePosition FindCapturePosition(CapturePosition[] positions, CaptureId id)
        {
            foreach (var pos in positions)
            {
                if (pos.CaptureId == id)
                {
                    return pos;
                }
            }
            throw new Exception("FindCapturePosition fail: " + id);
        }

        public static Pose ConvertCapturePositionToPose(CapturePosition capturePosition)
        {
            return new Pose()
            {
                X = capturePosition.XPosition,
                Y = capturePosition.YPosition,
                Z = capturePosition.ZPosition,
            };
        }

        public static Pose ConvertAxisOffsetToPose(AxisOffset offset)
        {
            return new Pose()
            {
                //Todo currently is abs position.
                X = offset.XOffset,
                Y = offset.YOffset,
                // Z is const,
                A = offset.ROffset,
                RUnloadAngle = offset.ROffset,
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bitPosition">0 based index</param>
        public static bool GetBit(int value, int bitPosition)
        {
            return (value & (1 << bitPosition)) != 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bitPosition">0 based index</param>
        public static void SetBit(ref int value, int bitPosition)
        {
            value |= 1 << bitPosition;
        }

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bitPosition">0 based index</param>
        public static void ResetBit(ref int value, int bitPosition)
        {
            value &= ~(1 << bitPosition);
        }
    }
}
