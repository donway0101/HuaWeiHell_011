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
        /// <summary>
        /// Used for tasks that need successful immediately return.
        /// </summary>
        /// <returns></returns>
        public static async Task<WaitBlock> DummyAsyncTask()
        {
            return await Task.Run(() =>
            {
                return new WaitBlock() { Code = ErrorCode.Sucessful };
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="fileName"></param>
        public static void WriteFile(string content, string fileName)
        {
            System.IO.File.WriteAllText(fileName, content);
        }

        /// <summary>
        /// Find even for four numbers.
        /// </summary>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public static double FindEven(double[] dataSource)
        {
            if (dataSource.Length==0)
            {
                return double.NaN;
            }

            double total = 0;
            foreach (var source in dataSource)
            {
                total += source;
            }

            return total / dataSource.Length;
        }

        /// <summary>
        /// Convert a vacuum state to a output state to drive output action.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static OutputState ConvertToOutputState(VacuumState state)
        {
            switch (state)
            {
                case VacuumState.Off:
                    return OutputState.Off;
                case VacuumState.On:
                    return OutputState.On;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Read configuration file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ReadFile(string fileName)
        {
            return System.IO.File.ReadAllText(fileName);
        }

        /// <summary>
        /// Convert an object to json string for storage
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ConvertToJsonString(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        /// <summary>
        /// Convert json string to user settings.
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static List<UserSetting> ConvertToUserSettings(string jsonString)
        {
            return JsonConvert.DeserializeObject<List<UserSetting>>(jsonString);
        }

        /// <summary>
        /// Convert to capture positions.
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static List<CapturePosition> ConvertToCapturePositions(string jsonString)
        {
            return JsonConvert.DeserializeObject<List<CapturePosition>>(jsonString);
        }

        /// <summary>
        /// Find capture position from a list.
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static CapturePosition GetCapturePosition(List<CapturePosition> positions, CaptureId id)
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

        /// <summary>
        /// Match with id and tag.
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="id"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static CapturePosition GetCapturePosition(List<CapturePosition> positions, CaptureId id, string tag)
        {
            foreach (var pos in positions)
            {
                if (pos.CaptureId == id && pos.Tag == tag)
                {
                    return pos;
                }
            }
            throw new Exception("FindCapturePosition fail: " + id);
        }

        /// <summary>
        /// For goes to history point.
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public static CapturePosition GetDevelopmentPoints(List<CapturePosition> positions, string remark)
        {
            foreach (var pos in positions)
            {
                if (pos.Remarks == remark)
                {
                    return pos;
                }
            }
            throw new Exception("FindCapturePosition fail: " + remark);
        }

        /// <summary>
        /// Check task result.
        /// </summary>
        /// <param name="waitBlock"></param>
        public static void CheckTaskResult(Task<WaitBlock> waitBlock)
        {
            if (waitBlock.Result.Code != ErrorCode.Sucessful)
            {
                throw new Exception("Error Code: " + waitBlock.Result.Code + " " +
                    waitBlock.Result.Message);
            }
        }

        /// <summary>
        /// For robot to go.
        /// </summary>
        /// <param name="capPos"></param>
        /// <returns></returns>
        public static Pose ConvertToPose(CapturePosition capPos)
        {
            return new Pose()
            {
                X = capPos.XPosition,
                Y = capPos.YPosition,
                Z = capPos.ZPosition,
                A = capPos.Angle,
            };
        }

        /// <summary>
        /// Zero based index
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bitPosition">0 based index</param>
        public static bool GetBit(int value, int bitPosition)
        {
            return (value & (1 << bitPosition)) != 0;
        }

        /// <summary>
        /// Zero based index
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bitPosition">0 based index</param>
        public static void SetBit(ref int value, int bitPosition)
        {
            value |= 1 << bitPosition;
        }

        /// <summary>
        /// Zero based index
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bitPosition">0 based index</param>
        public static void ResetBit(ref int value, int bitPosition)
        {
            value &= ~(1 << bitPosition);
        }
    }
}
