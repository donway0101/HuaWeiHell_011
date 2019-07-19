using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorter
{
    public class RoundTable
    {
        public Holder[] Holders { get; set; } = new Holder[6];
        private readonly MotionController _mc;

        public double HolderAngle { get; set; } = 60.0;

        public bool HomeComplete { get; set; }

        public Motor TableMotor { get; set; }

        public RoundTable(MotionController controller)
        {
            _mc = controller;
            Holders = new Holder[6];
            for (int i = 0; i < 6; i++)
            {
                Holders[i] = new Holder();
            }
        }

        public void Setup()
        {
            TableMotor = _mc.MotorWorkTable;
            ResetAllHolders();
        }

        public void ResetAllHolders()
        {          
            Holders[0].VaccumOutputCenter = Output.VaccumHolerCenterV;
            Holders[0].VaccumOutputCircle = Output.VaccumHolerCircleV;
            Holders[1].VaccumOutputCenter = Output.VaccumHolerCenterGluePoint;
            Holders[1].VaccumOutputCircle = Output.VaccumHolerCircleGluePoint;
            Holders[2].VaccumOutputCenter = Output.VaccumHolerCenterGlueLine;
            Holders[2].VaccumOutputCircle = Output.VaccumHolerCircleGlueLine;
            Holders[3].VaccumOutputCenter = Output.VaccumHolerCenterL;
            Holders[3].VaccumOutputCircle = Output.VaccumHolerCircleL;
            Holders[4].VaccumOutputCenter = Output.VaccumHolerCenterReserve;
            Holders[4].VaccumOutputCircle = Output.VaccumHolerCircleReserve;
            Holders[5].VaccumOutputCenter = Output.VaccumHolerCenterUV;
            Holders[5].VaccumOutputCircle = Output.VaccumHolerCircleUV;

            Holders[0].VaccumInputCenter = Input.VaccumHolerCenterV;
            Holders[0].VaccumInputCircle = Input.VaccumHolerCircleV;
            Holders[1].VaccumInputCenter = Input.VaccumHolerCenterGluePoint;
            Holders[1].VaccumInputCircle = Input.VaccumHolerCircleGluePoint;
            Holders[2].VaccumInputCenter = Input.VaccumHolerCenterGlueLine;
            Holders[2].VaccumInputCircle = Input.VaccumHolerCircleGlueLine;
            Holders[3].VaccumInputCenter = Input.VaccumHolerCenterL;
            Holders[3].VaccumInputCircle = Input.VaccumHolerCircleL;
            Holders[4].VaccumInputCenter = Input.VaccumHolerCenterReserve;
            Holders[4].VaccumInputCircle = Input.VaccumHolerCircleReserve;
            Holders[5].VaccumInputCenter = Input.VaccumHolerCenterUV;
            Holders[5].VaccumInputCircle = Input.VaccumHolerCircleUV;
        }

        /// <summary>
        /// After finish place.
        /// </summary>
        public void ResetFirstHolder()
        {
            Holders[0] = new Holder();
        }

        public void Home()
        {
            HomeComplete = false;
            _mc.Home(_mc.MotorWorkTable);
            ResetAllHolders();
            HomeComplete = true;
        }

        public void SetSpeed(double speed)
        {
            TableMotor.Velocity = Math.Sqrt(speed)*5;
        }

        public void Turns()
        {
            _mc.MoveToTargetRelativeTillEnd(TableMotor, HolderAngle);
            TurnsTableData();
        }

        private void TurnsTableData()
        {
            //Turns array.
            var lastHolderIndex = Holders.Length - 1;

            //Keep the last holder in temp.
            var lastHolder = Holders[lastHolderIndex];

            for (int i = 0; i < Holders.Length - 1; i++)
            {
                //Last holder become the holder before it.
                Holders[lastHolderIndex - i] = Holders[lastHolderIndex - i - 1];
            }

            Holders[0] = lastHolder;
        }

        public void VacuumSucker(FixtureId holder, VacuumState state, VacuumArea area = VacuumArea.Circle)
        {
            switch (state)
            {
                case VacuumState.On:
                    if (area== VacuumArea.Circle)
                    {
                        _mc.VacuumOn(Holders[(int)holder].VaccumOutputCircle, Holders[(int)holder].VaccumInputCircle);
                    }
                    else
                    {
                        _mc.VacuumOn(Holders[(int)holder].VaccumOutputCenter, Holders[(int)holder].VaccumInputCenter);
                    }
                    break;

                case VacuumState.Off:
                    if (area == VacuumArea.Circle)
                    {
                        _mc.VacuumOff(Holders[(int)holder].VaccumOutputCircle, Holders[(int)holder].VaccumInputCircle);
                    }
                    else
                    {
                        _mc.VacuumOff(Holders[(int)holder].VaccumOutputCenter, Holders[(int)holder].VaccumInputCenter);
                    }
                    break;

                default:
                    break;
            }          
        }
    }

    public class Holder
    {
        public bool IsEmpty { get; set; }
        public bool VPartPlaced { get; set; }
        public bool PointGlued { get; set; }
        public bool LineGlued { get; set; }
        public bool LPartPlaced { get; set; }

        public Output VaccumOutputCenter { get; set; }
        public Output VaccumOutputCircle { get; set; }
        public Input VaccumInputCenter { get; set; }
        public Input VaccumInputCircle { get; set; }
    }

    public enum FixtureId
    {
        V = 0,
        GluePoint = 1,
        GlueLine = 2,
        L = 3,
        Reserve = 4,
        UVLight = 5,
    }

    public enum VacuumArea
    {
        Circle,
        Center,
    }

}
