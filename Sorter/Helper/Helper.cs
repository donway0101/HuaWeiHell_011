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

        public static void SaveDevelopmentPoints(string config, string path = "Development.config")
        {
            System.IO.File.WriteAllText(path, config);
        }

        public static void AddCapturePosition(string posInfo, string path)
        {
            System.IO.File.AppendAllText(path, posInfo + Environment.NewLine);
        }

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

        public static string ReadConfiguration(string path)
        {
            return System.IO.File.ReadAllText(path);
        }

        public static string ConvertToJsonString(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        public static Motor[] ConvertConfigToMotors(string config)
        {
            return JsonConvert.DeserializeObject<Motor[]>(config);
        }

        public static List<CapturePosition> ConvertToCapturePositions(string config)
        {
            return JsonConvert.DeserializeObject<List<CapturePosition>>(config);
        }

        public static GluePosition[] ConvertToGluePositions(Pose[] poses)
        {
            GluePosition[] gluePositions = new GluePosition[4];
            for (int i = 0; i < poses.Length; i++)
            {
                gluePositions[i].X = poses[i].X;
                gluePositions[i].Y = poses[i].Y;
                gluePositions[i].Z = poses[i].Z;
            }
            return gluePositions;
        }

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

        public static CapturePosition GetDevelopmentPoints(List<CapturePosition> positions, string tag)
        {
            foreach (var pos in positions)
            {
                if (pos.Tag == tag)
                {
                    return pos;
                }
            }
            throw new Exception("FindCapturePosition fail: " + tag);
        }

        public static void CheckTaskResult(Task<WaitBlock> waitBlock)
        {
            if (waitBlock.Result.Code != 0)
            {
                throw new Exception("Task not finished, Error Code: " + waitBlock.Result.Code + 
                    waitBlock.Result.Message);
            }
        }

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

        public static Pose ConvertToPose(AxisOffset offset)
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
