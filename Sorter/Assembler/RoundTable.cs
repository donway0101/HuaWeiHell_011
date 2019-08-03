using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sorter
{
    public class RoundTable
    {
        private readonly MotionController _mc;

        public Fixture[] Fixtures { get; set; } = new Fixture[6];      

        public double FixtureAngle { get; set; } = 60.0;

        public bool HomeComplete { get; set; }

        public Motor TableMotor { get; set; }

        public RoundTable(MotionController controller)
        {
            _mc = controller;
            Fixtures = new Fixture[6];
            for (int i = 0; i < 6; i++)
            {
                Fixtures[i] = new Fixture();
            }
        }

        public void Setup()
        {
            TableMotor = _mc.MotorWorkTable;
            ResetAllFixtures();
        }

        public void ResetAllFixtures()
        {          
            Fixtures[0].VaccumOutputCenter = Output.VaccumHolerCenterV;
            Fixtures[0].VaccumOutputCircle = Output.VaccumHolerCircleV;
            Fixtures[1].VaccumOutputCenter = Output.VaccumHolerCenterGluePoint;
            Fixtures[1].VaccumOutputCircle = Output.VaccumHolerCircleGluePoint;
            Fixtures[2].VaccumOutputCenter = Output.VaccumHolerCenterGlueLine;
            Fixtures[2].VaccumOutputCircle = Output.VaccumHolerCircleGlueLine;
            Fixtures[3].VaccumOutputCenter = Output.VaccumHolerCenterL;
            Fixtures[3].VaccumOutputCircle = Output.VaccumHolerCircleL;
            Fixtures[4].VaccumOutputCenter = Output.VaccumHolerCenterReserve;
            Fixtures[4].VaccumOutputCircle = Output.VaccumHolerCircleReserve;
            Fixtures[5].VaccumOutputCenter = Output.VaccumHolerCenterUV;
            Fixtures[5].VaccumOutputCircle = Output.VaccumHolerCircleUV;

            Fixtures[0].VaccumInputCenter = Input.VaccumHolerCenterV;
            Fixtures[0].VaccumInputCircle = Input.VaccumHolerCircleV;
            Fixtures[1].VaccumInputCenter = Input.VaccumHolerCenterGluePoint;
            Fixtures[1].VaccumInputCircle = Input.VaccumHolerCircleGluePoint;
            Fixtures[2].VaccumInputCenter = Input.VaccumHolerCenterGlueLine;
            Fixtures[2].VaccumInputCircle = Input.VaccumHolerCircleGlueLine;
            Fixtures[3].VaccumInputCenter = Input.VaccumHolerCenterL;
            Fixtures[3].VaccumInputCircle = Input.VaccumHolerCircleL;
            Fixtures[4].VaccumInputCenter = Input.VaccumHolerCenterReserve;
            Fixtures[4].VaccumInputCircle = Input.VaccumHolerCircleReserve;
            Fixtures[5].VaccumInputCenter = Input.VaccumHolerCenterUV;
            Fixtures[5].VaccumInputCircle = Input.VaccumHolerCircleUV;
        }

        public Task UVLightOnAsync(int delaySec = 5)
        {
            return Task.Run(() => {
                _mc.SetOutput(Output.UVLightTable, OutputState.On);
                Delay(delaySec * 1000);
                _mc.SetOutput(Output.UVLightTable, OutputState.Off);
            });
        }

        public void Delay(int delayMs)
        {
            Thread.Sleep(delayMs);
        }

        /// <summary>
        /// After finish place.
        /// </summary>
        public void ResetFirstFixture()
        {
            Fixtures[0] = new Fixture();
        }

        public void SetFixturesFull()
        {
            foreach (var fix in Fixtures)
            {
                fix.IsEmpty = false;
            }
        }

        public void Home()
        {
            HomeComplete = false;
            _mc.Home(_mc.MotorWorkTable);
            ResetAllFixtures();
            HomeComplete = true;
        }

        public void SetSpeed(double speed)
        {
            TableMotor.Velocity = Math.Sqrt(speed)*5;
        }

        public void Turns()
        {
            _mc.MoveToTargetRelativeTillEnd(TableMotor, FixtureAngle);
            TurnsTableData();
        }

        public async Task<WaitBlock> TurnsAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    Turns();
                    return new WaitBlock() { Message = "Table turns Finished Successful." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.TobeCompleted,
                        Message = "Table turns fail: " + ex.Message
                    };
                }
            });
        }

        private void TurnsTableData()
        {
            //Turns array.
            var lastFixtureIndex = Fixtures.Length - 1;

            //Keep the last Fixture in temp.
            var lastFixture = Fixtures[lastFixtureIndex];

            for (int i = 0; i < Fixtures.Length - 1; i++)
            {
                //Last Fixture become the Fixture before it.
                Fixtures[lastFixtureIndex - i] = Fixtures[lastFixtureIndex - i - 1];
            }

            Fixtures[0] = lastFixture;

            //Reset missions.
            foreach (var fixture in Fixtures)
            {
                fixture.MissionAccomplished = false;
            }
        }

        public void Sucker(FixtureId Fixture, VacuumState state, VacuumArea area = VacuumArea.Circle, bool checkVacuum = true)
        {
            switch (state)
            {
                case VacuumState.On:
                    if (area== VacuumArea.Circle)
                    {
                        _mc.VacuumOn(Fixtures[(int)Fixture].VaccumOutputCircle, Fixtures[(int)Fixture].VaccumInputCircle, checkVacuum);
                    }
                    else
                    {
                        _mc.VacuumOn(Fixtures[(int)Fixture].VaccumOutputCenter, Fixtures[(int)Fixture].VaccumInputCenter, checkVacuum);
                    }
                    break;

                case VacuumState.Off:
                    if (area == VacuumArea.Circle)
                    {
                        _mc.VacuumOff(Fixtures[(int)Fixture].VaccumOutputCircle, Fixtures[(int)Fixture].VaccumInputCircle, checkVacuum);
                    }
                    else
                    {
                        _mc.VacuumOff(Fixtures[(int)Fixture].VaccumOutputCenter, Fixtures[(int)Fixture].VaccumInputCenter, checkVacuum);
                    }
                    break;

                default:
                    break;
            }          
        }
    }

    public class Fixture
    {
        public bool IsEmpty { get; set; } = true;
        public bool NG { get; set; } = false;
        public bool MissionAccomplished { get; set; } = false;

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
