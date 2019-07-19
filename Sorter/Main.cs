using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bp.Mes;

/// <summary>
/// Tips:
/// For password https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/protecting-connection-information
///     or secret of https://blog.submain.com/app-config-basics-best-practices/
/// For some user settings, use properties-Settings.settings, save or reset method
///  
/// </summary>
namespace Sorter
{
    public partial class Main : Form
    {
        //Todo log as Json format
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _basePath = Application.StartupPath + @"\";

        private bool _uiStarted;
        private double _manualSpeed = 10;
        private double _demoSpeed = 10;

        private StationId _selectStation = StationId.L;
        private ProcedureId _selectSuckerVacuum = ProcedureId.Load;

        private Motor _manualMotorX;
        private Motor _manualMotorY;
        private Motor _manualMotorZ;
        private Motor _manualMotorRLoad;
        private Motor _manualMotorRUnload;
        private MotionController _manualMotion;

        private CentralControl _cc = new CentralControl();

        MotionController mc = new MotionController();

        private ManualResetEvent UpdatePositionManualResetEvent = new ManualResetEvent(false);

        public Main()
        {
            InitializeComponent();
            //Set tab draw mode to ownerdraw
            tabControlMenu.DrawItem += new DrawItemEventHandler(tabControlMenu_DrawItem);
            log.Info("System start");                       
        }

        private void tabControlMenu_DrawItem(Object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush _textBrush;

            // Get the item from the collection.
            TabPage _tabPage = tabControlMenu.TabPages[e.Index];

            // Get the real bounds for the tab rectangle.
            Rectangle _tabBounds = tabControlMenu.GetTabRect(e.Index);

            if (e.State == DrawItemState.Selected)
            {

                // Draw a different background color, and don't paint a focus rectangle.
                _textBrush = new SolidBrush(Color.DarkRed);
                g.FillRectangle(Brushes.LightGray, e.Bounds);
            }
            else
            {
                _textBrush = new System.Drawing.SolidBrush(e.ForeColor);
                e.DrawBackground();
            }

            // Use our own font.
            Font _tabFont = new Font("Arial", 14.0f, FontStyle.Bold, GraphicsUnit.Pixel);

            // Draw string. Center the text.
            StringFormat _stringFlags = new StringFormat();
            _stringFlags.Alignment = StringAlignment.Center;
            _stringFlags.LineAlignment = StringAlignment.Center;
            g.DrawString(_tabPage.Text, _tabFont, _textBrush, _tabBounds, new StringFormat(_stringFlags));
        }

        private void SaveMotorConfiguration()
        {
            //Helper.ConvertObjectToString();
            Helper.SaveConfiguration("Test", Properties.Settings.Default.MotorConfig);
        }

        private void ReadMotorConfiguration()
        {
            Helper.ReadConfiguration(Properties.Settings.Default.MotorConfig);
        }

        //SocketClient SocketClient = new SocketClient("192.168.1.192", 1000);

        //BasicFunction BasicFunction = new BasicFunction();

            /// <summary>
            /// Connect to controller.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
        private async void button1_Click(object sender, EventArgs e)
        {
            buttonConnect.Enabled = false;
            buttonConnect.Text = "Initializing...";
            bool result = await Task.Run(() => {
                try
                {
                    _cc.Setup();
                    Setup();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }
            });
            buttonConnect.Enabled = !result;

            if (result)
            {
                buttonConnect.Text = "Connect OK";
                buttonConnect.BackColor = Color.LightGreen;
            }

            #region MyRegion
            //Properties.Settings.Default.Reset();
            //SaveMotorConfiguration();
            //Properties.Settings.Default.Save();
            //SocketClient.Start();
            //BasicFunction.cancellationTokenSource = new System.Threading.CancellationTokenSource();
            //Task<WaitBlock> waitBlock =  BasicFunction.TaskTest();
            //await waitBlock;
            //button1.Text = waitBlock.Result.Code.ToString();

            //mc.Connect();

            //mc.SetCoordinateSystem(); 
            #endregion
        }

        /// <summary>
        /// For manual control.
        /// </summary>
        private void Setup()
        {
            _manualMotion = _cc.Mc;
            _manualMotorX = _cc.LRobot.MotorX;
            _manualMotorY = _cc.LRobot.MotorY;
            _manualMotorZ = _cc.LRobot.MotorZ;
            _manualMotorRLoad = _cc.LRobot.MotorRotateLoad;

            PositionUpdate();
        }

        #region updata position

        private void PositionUpdate()
        {
            if (_uiStarted == true)
            {
                return;
            }
            _uiStarted = true;
            Task.Run(() => {
                while (true)
                {

                    try
                    {
                        labelFeedbackPosX.BeginInvoke((MethodInvoker)(() =>
                        {
                            try
                            {
                                labelFeedbackPosX.Text = mc.GetPosition(_manualMotorX).ToString();
                            }
                            catch (Exception)
                            {
                            }

                        }));
                        Thread.Sleep(200);
                        labelFeedbackPosY.BeginInvoke((MethodInvoker)(() =>
                        {
                            try
                            {
                                labelFeedbackPosY.Text = mc.GetPosition(_manualMotorY).ToString();
                            }
                            catch (Exception)
                            {
                            }

                        }));
                        Thread.Sleep(200);
                        labelFeedbackPosZ.BeginInvoke((MethodInvoker)(() =>
                        {
                            try
                            {
                                labelFeedbackPosZ.Text = mc.GetPosition(_manualMotorZ).ToString();
                            }
                            catch (Exception)
                            {

                            }

                        }));
                        Thread.Sleep(200);
                        labelFeedbackPosRLoad.BeginInvoke((MethodInvoker)(() =>
                        {
                            try
                            {
                                labelFeedbackPosRLoad.Text = mc.GetPosition(_manualMotorRLoad).ToString();
                            }
                            catch (Exception)
                            {
                            }

                        }));
                        Thread.Sleep(200);
                        labelFeedbackPosRUnload.BeginInvoke((MethodInvoker)(() =>
                        {
                            try
                            {
                                labelFeedbackPosRUnload.Text = mc.GetPosition(_manualMotorRUnload).ToString();
                            }
                            catch (Exception)
                            {
                            }
                        }));
                        Thread.Sleep(200);

                    }
                    catch (Exception)
                    {

                        Thread.Sleep(1000);
                    }
                }
            });
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                //timer1.Enabled = false;
                //UpdatePositionManualResetEvent.WaitOne();
                //label100.BeginInvoke((MethodInvoker)(() =>
                //{
                //    label100.Text = mc.GetPosition(motorX).ToString();
                //}));
                //labelFollowingError.BeginInvoke((MethodInvoker)(() =>
                //{
                //    labelFollowingError.Text = mc.GetPosition(motorY).ToString();
                //}));

                //if (panel6.Visible)
                //{
                //    label4.Text = mc.GetPosition(motorRLoad).ToString();
                //}
                //if (panel7.Visible)
                //{
                //    label5.Text = mc.GetPosition(motorRUnload).ToString();
                //}
                Thread.Sleep(100);
            });

        } 
        #endregion

        private void radioButtonLStation_CheckedChanged(object sender, EventArgs e)
        {
            var radio = (RadioButton)sender;
            if (!radio.Checked)
            {
                return;
            }
            _selectStation = StationId.L;
            SelectRobotStation(_selectStation);
        }

        private void SelectRobotStation(StationId station)
        {
            switch (station)
            {
                case StationId.V:
                    panel_RUnload2.Visible = true;
                    _manualMotorX = _cc.VRobot.MotorX;
                    _manualMotorY = _cc.VRobot.MotorY;
                    _manualMotorZ = _cc.VRobot.MotorZ;
                    _manualMotorRLoad = _cc.VRobot.MotorRotateLoad;
                    _manualMotorRUnload = _cc.VRobot.MotorRotateUnload;
                    break;

                case StationId.L:
                    panel_RUnload2.Visible = false;
                    _manualMotorX = _cc.LRobot.MotorX;
                    _manualMotorY = _cc.LRobot.MotorY;
                    _manualMotorZ = _cc.LRobot.MotorZ;
                    _manualMotorRLoad = _cc.LRobot.MotorRotateLoad;
                    break;

                case StationId.GLine:
                    break;

                case StationId.GPoint:
                    break;

                default:
                    break;
            }
        }

        private void radioButtonVStation_CheckedChanged(object sender, EventArgs e)
        {
            var radio = (RadioButton)sender;
            if (!radio.Checked)
            {
                return;
            }
            _selectStation = StationId.V;
            SelectRobotStation(_selectStation);
        }

        private void radioButtonGluePointStation_CheckedChanged(object sender, EventArgs e)
        {
            var radio = (RadioButton)sender;
            if (!radio.Checked)
            {
                return;
            }
            _selectStation = StationId.GPoint;
            SelectRobotStation(_selectStation);
        }

        private void radioButtonGlueLineStation_CheckedChanged(object sender, EventArgs e)
        {
            var radio = (RadioButton)sender;
            if (!radio.Checked)
            {
                return;
            }
            _selectStation = StationId.GLine;
            SelectRobotStation(_selectStation);
        }

        private void button_YJogPlus_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_manualMotorY, _manualSpeed, MoveDirection.Positive);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_YJogPlus_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _cc.Mc.Stop(_manualMotorY);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_ZJogPlus_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_manualMotorZ, _manualSpeed, MoveDirection.Positive);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonXJogPlus_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_manualMotorX, _manualSpeed, MoveDirection.Positive);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_YJogMinus_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_manualMotorY, _manualSpeed, MoveDirection.Negative);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_YJogMinus_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Stop(_manualMotorY);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button2_Click_1(object sender, EventArgs e)
        {
            //buttonStart.Enabled = false;
            var result = await Task.Run(() =>
            {
                try
                {
                    _cc.Start();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                } });

            if (result)
            {
                buttonStart.BackColor = Color.LightGreen;
            }
            else
            {
                buttonStart.BackColor = Color.Red;
            }
        }

        private void buttonXJogMinus_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_manualMotorX, _manualSpeed, MoveDirection.Negative);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonXJogMinus_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Stop(_manualMotorX);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonXJogPlus_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Stop(_manualMotorX);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonZJogMinus_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_manualMotorZ, _manualSpeed, MoveDirection.Negative);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonZJogMinus_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Stop(_manualMotorZ);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_ZJogPlus_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Stop(_manualMotorZ);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_RLoadCW_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_manualMotorRLoad, _manualSpeed, MoveDirection.Positive);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_RLoadCW_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Stop(_manualMotorRLoad);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_RLoadCCW_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_manualMotorRLoad, _manualSpeed, MoveDirection.Negative);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_RLoadCCW_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Stop(_manualMotorRLoad);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void trackBarManualControlSpeed_ValueChanged(object sender, EventArgs e)
        {
            _manualSpeed = Convert.ToDouble(trackBarManualControlSpeed.Value) / 10.0;
            label_manualSpeed.Text = _manualSpeed.ToString("0.0");
        }

        private void buttonHighSpeed_Click(object sender, EventArgs e)
        {
            _manualSpeed = 50;
            label_manualSpeed.Text = _manualSpeed.ToString("0.0");
        }

        private void buttonMiddleSpeed_Click(object sender, EventArgs e)
        {
            _manualSpeed = 20;
            label_manualSpeed.Text = _manualSpeed.ToString("0.0");
        }

        private void button_MoveToTargetX_Click(object sender, EventArgs e)
        {
            try
            {
                double target = Convert.ToDouble(textBox_MoveToTargetX.Text);
                _manualMotion.MoveToTargetTillEnd(_manualMotorX, target);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button_MoveToTargetY_Click(object sender, EventArgs e)
        {
            try
            {
                double target = Convert.ToDouble(textBox_MoveToTargetY.Text);
                _manualMotion.MoveToTargetTillEnd(_manualMotorY, target);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button_MoveToTargetZ_Click(object sender, EventArgs e)
        {
            try
            {
                double target = Convert.ToDouble(textBox_MoveToTargetZ.Text);
                _manualMotion.MoveToTargetTillEnd(_manualMotorZ, target);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button_MoveToTargetRLoad_Click(object sender, EventArgs e)
        {
            try
            {
                double target = Convert.ToDouble(textBox_MoveToTargetRLoad.Text);
                _manualMotion.MoveToTargetTillEnd(_manualMotorRLoad, target);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button_MoveToTargetRUnload_Click(object sender, EventArgs e)
        {
            try
            {
                double target = Convert.ToDouble(textBox_MoveToTargetRUnload.Text);
                _manualMotion.MoveToTargetTillEnd(_manualMotorRUnload, target);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private MoveDirection direction = MoveDirection.Positive;
        private void radioButtonMoveIncPositive_CheckedChanged(object sender, EventArgs e)
        {
            textBox_MoveInc.Text = -Convert.ToDouble(textBox_MoveInc.Text) + "";
        }

        private void radioButtonMoveIncNegative_CheckedChanged(object sender, EventArgs e)
        {
            direction = MoveDirection.Negative;
        }

        private void button_MoveIncX_Click(object sender, EventArgs e)
        {
            try
            {
                double target = Convert.ToDouble(textBox_MoveInc.Text);
                _manualMotion.MoveToTargetRelativeTillEnd(_manualMotorX, target);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button_MoveIncY_Click(object sender, EventArgs e)
        {
            try
            {
                double target = Convert.ToDouble(textBox_MoveInc.Text);
                _manualMotion.MoveToTargetRelativeTillEnd(_manualMotorY, target);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button_MoveIncZ_Click(object sender, EventArgs e)
        {
            try
            {
                double target = Convert.ToDouble(textBox_MoveInc.Text);
                _manualMotion.MoveToTargetRelativeTillEnd(_manualMotorZ, target);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button_MoveIncRLoad_Click(object sender, EventArgs e)
        {
            try
            {
                double target = Convert.ToDouble(textBox_MoveInc.Text);
                _manualMotion.MoveToTargetRelativeTillEnd(_manualMotorRLoad, target);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button_MoveIncRUnload_Click(object sender, EventArgs e)
        {
            try
            {
                double target = Convert.ToDouble(textBox_MoveInc.Text);
                _manualMotion.MoveToTargetRelativeTillEnd(_manualMotorRUnload, target);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonSaveCapturePosition_Click(object sender, EventArgs e)
        {
            try
            {
                CapturePosition capture = new CapturePosition()
                {
                    CaptureId = CaptureId.LLoadHolderTop,
                    XPosition = _manualMotion.GetPosition(_manualMotorX),
                    YPosition = _manualMotion.GetPosition(_manualMotorY),
                    ZPosition = _manualMotion.GetPosition(_manualMotorZ),
                };

                CapturePosition[] pos = new CapturePosition[2] { capture, capture };
                //var pos = DateTime.Now.ToLongTimeString();
                //pos += " " + textBoxCaptureNote.Text;
                //pos += " X: " + _manualMotion.GetPosition(_manualMotorX);
                //pos += " Y: " + _manualMotion.GetPosition(_manualMotorY);
                //pos += " Z: " + _manualMotion.GetPosition(_manualMotorZ);

                var str = Helper.ConvertObjectToString(capture);
                str += textBoxCaptureNote.Text;

                Helper.AddCapturePosition(str, @"D://SomePosition.txt");

                //var config = Helper.ReadConfiguration(@"D://SomePosition.txt");
                //SavedCapturePosition[] positions = Helper.ConvertConfigToCapturePositions(config);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonReadActPos_Click(object sender, EventArgs e)
        {
            textBox_MoveToTargetX.Text = labelFeedbackPosX.Text;
            textBox_MoveToTargetY.Text = labelFeedbackPosY.Text;
            textBox_MoveToTargetZ.Text = labelFeedbackPosZ.Text;
            textBox_MoveToTargetRLoad.Text = labelFeedbackPosRLoad.Text;
            textBox_MoveToTargetRUnload.Text = labelFeedbackPosRUnload.Text;
        }

        private void button_RUnloadCW_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_manualMotorRUnload, _manualSpeed, MoveDirection.Positive);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_RUnloadCW_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Stop(_manualMotorRUnload);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_RUnloadCCW_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_manualMotorRUnload, _manualSpeed, MoveDirection.Negative);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_RUnloadCCW_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Stop(_manualMotorRUnload);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonVLoadLockTray_Click(object sender, EventArgs e)
        {
           

            try
            {
                _cc.VRobot.LockTray(ProcedureId.Load);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void buttonVLoadUnlockTray_Click(object sender, EventArgs e)
        {
          
            try
            {
                _cc.VRobot.UnlockTray(ProcedureId.Load);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonVUnloadLockTray_Click(object sender, EventArgs e)
        {
          
            try
            {
                _cc.VRobot.LockTray(ProcedureId.Unload);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonVUnloadUnlockTray_Click(object sender, EventArgs e)
        {
          
            try
            {
                _cc.VRobot.UnlockTray(ProcedureId.Unload);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonSuckerOn_Click(object sender, EventArgs e)
        {
            try
            {
                switch (_selectStation)
                {
                    case StationId.V:
                        switch (_selectSuckerVacuum)
                        {
                            case ProcedureId.Load:
                                _cc.VRobot.VacuumSucker(VacuumState.On, ProcedureId.Load);
                                break;
                            case ProcedureId.Unload:
                                _cc.VRobot.VacuumSucker(VacuumState.On, ProcedureId.Unload);
                                break;
                            default:
                                break;
                        }
                        break;
                    case StationId.L:
                        _cc.LRobot.VacuumSucker(VacuumState.On, ProcedureId.Load);
                        break;
                    case StationId.GLine:
                    case StationId.GPoint:
                    default:
                        throw new Exception("No sucker");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void radioButtonSuckerLoad_CheckedChanged(object sender, EventArgs e)
        {
            _selectSuckerVacuum = ProcedureId.Load;
        }

        private void radioButtonSuckerUnload_CheckedChanged(object sender, EventArgs e)
        {
            _selectSuckerVacuum = ProcedureId.Unload;
        }

        private void buttonSuckerOff_Click(object sender, EventArgs e)
        {
            try
            {
                switch (_selectStation)
                {
                    case StationId.V:
                        switch (_selectSuckerVacuum)
                        {
                            case ProcedureId.Load:
                                _cc.VRobot.VacuumSucker(VacuumState.Off, ProcedureId.Load);
                                break;
                            case ProcedureId.Unload:
                                _cc.VRobot.VacuumSucker(VacuumState.Off, ProcedureId.Unload);
                                break;
                            default:
                                break;
                        }
                        break;
                    case StationId.L:
                        _cc.LRobot.VacuumSucker(VacuumState.Off, ProcedureId.Load);
                        break;
                    case StationId.GLine:
                    case StationId.GPoint:
                    default:
                        throw new Exception("No sucker");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonClearAllFault_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (var mtr in _cc.Mc.Motors)
                {
                    _cc.Mc.ClearFault(mtr);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonLLoadLockTray_Click(object sender, EventArgs e)
        {

            try
            {
                _cc.LRobot.LockTray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonLLoadUnlockTray_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.LRobot.UnlockTray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonTableCWStep_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.WorkTable.Turns();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonHeadCylinderUp_Click(object sender, EventArgs e)
        {
            try
            {
                switch (_selectStation)
                {
                    case StationId.V:
                        _cc.VRobot.CylinderHead(HeadCylinderState.Up, _selectSuckerVacuum);
                        break;
                    case StationId.L:
                    case StationId.GLine:
                    case StationId.GPoint:
                    default:
                        throw new Exception("No cyliner");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonHeadCylinderDown_Click(object sender, EventArgs e)
        {
            try
            {
                switch (_selectStation)
                {
                    case StationId.V:
                        _cc.VRobot.CylinderHead(HeadCylinderState.Down, _selectSuckerVacuum);
                        break;
                    case StationId.L:
                    case StationId.GLine:
                    case StationId.GPoint:
                    default:
                        throw new Exception("No cyliner");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonCapturePosition_Click(object sender, EventArgs e)
        {
            try
            {
                CapturePosition capturePosition = new CapturePosition();
                if (radioButton1.Checked)
                {
                    capturePosition = CapturePositions.LTrayPickTop;
                }
                if (radioButton2.Checked)
                {
                    capturePosition = CapturePositions.LLoadCompensationBottom;
                }
                if (radioButton3.Checked)
                {
                    capturePosition = CapturePositions.LLoadHolderTop;
                }

                if (radioButton4.Checked)
                {
                    capturePosition = CapturePositions.VTrayPickTop;
                }
                if (radioButton5.Checked)
                {
                    capturePosition = CapturePositions.VLoadCompensationBottom;
                }
                if (radioButton6.Checked)
                {
                    capturePosition = CapturePositions.VLoadHolderTop;
                }

                if (radioButton7.Checked)
                {
                    capturePosition = CapturePositions.VUnloadHolderTop;
                }
                if (radioButton8.Checked)
                {
                    capturePosition = CapturePositions.VUnloadCompensationBottom;
                }
                if (radioButton9.Checked)
                {
                    capturePosition = CapturePositions.VTrayPlaceTop;
                }

                var offset = _cc.Vision.RequestVisionCalibration(capturePosition);
                var target = Helper.ConvertAxisOffsetToPose(offset);

                textBox_MoveToTargetX.Text = target.X.ToString();
                textBox_MoveToTargetY.Text = target.Y.ToString();
                //textBox_MoveToTargetZ.Text = capturePosition.ZPosition.ToString();

                textBox_MoveToTargetRLoad.Text = target.RLoadAngle.ToString();
                textBox_MoveToTargetRUnload.Text = target.RUnloadAngle.ToString();

                MessageBox.Show("位置已获取，设定Z的抓料位置之后，点击Move Robot可移动到相机获取的位置.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void buttonDemo_Click(object sender, EventArgs e)
        {
            buttonDemo.Enabled = false;
            _cc.LRobot.VisionSimulateMode = checkBoxVisionSimulate.Checked;
            _cc.LRobot.VacuumSimulateMode = checkBoxVaccumSimulate.Checked;
            int testTimes = 15;
            if (checkBox1.Checked==false)
            {
                testTimes = 1;
            }
            await Task.Run(async () =>
            {
                while (testTimes>0)
                {
                    testTimes--;
                    try
                    {
                        //_cc.LRobot.LockTray();
                        //_cc.VRobot.LockTray(ProcedureId.Load);
                        //_cc.VRobot.LockTray(ProcedureId.Unload);

                        var lRobotTest = _cc.LRobot.LWork();
                        var vRobotTest = _cc.VRobot.VWork();

                        await vRobotTest;
                        await lRobotTest;

                        if (vRobotTest.Result.Code != 0 || lRobotTest.Result.Code != 0)
                        {
                            var msg = "";
                            msg += vRobotTest.Result.Code.ToString() + vRobotTest.Result.Message.ToString();
                            msg += lRobotTest.Result.Code.ToString() + lRobotTest.Result.Message.ToString();
                            throw new Exception(msg);
                        }

                        _cc.WorkTable.Turns();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        break;
                    }
                }
            });
            buttonDemo.Enabled = true;
        }

        private void trackBarDemoSpeed_ValueChanged(object sender, EventArgs e)
        {
            _demoSpeed = Convert.ToDouble(trackBarDemoSpeed.Value);
            _cc.SetSpeed(_demoSpeed);
        }

        private void buttonTableCircleVacuumOnV_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.WorkTable.VacuumSucker(HolderId.V, VacuumState.On);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonTableCircleVacuumOnGP_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.WorkTable.VacuumSucker(HolderId.GluePoint, VacuumState.On);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonTableCircleVacuumOnGL_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.WorkTable.VacuumSucker(HolderId.GlueLine, VacuumState.On);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonTableCircleVacuumOnL_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.WorkTable.VacuumSucker(HolderId.L, VacuumState.On);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonTableCircleVacuumOnReserve_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.WorkTable.VacuumSucker(HolderId.Reserve, VacuumState.On);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttoTableCircleVacuumOnUV_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.WorkTable.VacuumSucker(HolderId.UVLight, VacuumState.On);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonTableCircleVacuumOffV_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.WorkTable.VacuumSucker(HolderId.V, VacuumState.Off);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonTableCircleVacuumOffGP_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.WorkTable.VacuumSucker(HolderId.GluePoint, VacuumState.Off);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonTableCircleVacuumOffGL_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.WorkTable.VacuumSucker(HolderId.GlueLine, VacuumState.Off);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonTableCircleVacuumOffL_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.WorkTable.VacuumSucker(HolderId.L, VacuumState.Off);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonTableCircleVacuumOffReserve_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.WorkTable.VacuumSucker(HolderId.Reserve, VacuumState.Off);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonTableCircleVacuumOffUV_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.WorkTable.VacuumSucker(HolderId.UVLight, VacuumState.Off);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonTableCenterVacuumOnV_Click(object sender, EventArgs e)
        {

        }

        private void buttonTableCenterVacuumOnL_Click(object sender, EventArgs e)
        {

        }

        private void buttonMoveRobot_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.SetSpeed(_manualSpeed);
                Pose pose = new Pose()
                {
                    X = Convert.ToDouble(textBox_MoveToTargetX.Text),
                    Y = Convert.ToDouble(textBox_MoveToTargetY.Text),
                    Z = Convert.ToDouble(textBox_MoveToTargetZ.Text),
                    RLoadAngle = Convert.ToDouble(textBox_MoveToTargetRLoad.Text),
                    RUnloadAngle = Convert.ToDouble(textBox_MoveToTargetRUnload.Text),
                };

                switch (_selectStation)
                {
                    case StationId.V:
                        _cc.VRobot.MoveToTarget(pose, _selectSuckerVacuum);
                        break;
                    case StationId.L:
                        _cc.LRobot.MoveToTarget(pose, ProcedureId.Load);
                        break;
                    case StationId.GLine:
                        break;
                    case StationId.GPoint:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void buttonMoveToCapture_Click(object sender, EventArgs e)
        {
            var result = await Task.Run(() =>
            {
                try
                {


                    //CapturePosition[] capturePositions = new CapturePosition[2];
                    //capturePositions[0] = CapturePositions.LTrayPickTop;
                    //capturePositions[1] = CapturePositions.LLoadCompensationBottom;
                    //var str = Helper.ConvertObjectToString(capturePositions);
                    //Helper.SaveConfiguration(str, "CapturePositions.config");

                    //var cps = Helper.ConvertConfigToCapturePositions(str);

                    CapturePosition capturePosition = new CapturePosition();


                    if (radioButton1.Checked)
                    {
                        capturePosition = CapturePositions.LTrayPickTop;
                    }
                    if (radioButton2.Checked)
                    {
                        capturePosition = CapturePositions.LLoadCompensationBottom;
                    }
                    if (radioButton3.Checked)
                    {
                        capturePosition = CapturePositions.LLoadHolderTop;
                    }

                    if (radioButton4.Checked)
                    {
                        capturePosition = CapturePositions.VTrayPickTop;
                    }
                    if (radioButton5.Checked)
                    {
                        capturePosition = CapturePositions.VLoadCompensationBottom;
                    }
                    if (radioButton6.Checked)
                    {
                        capturePosition = CapturePositions.VLoadHolderTop;
                    }

                    if (radioButton7.Checked)
                    {
                        capturePosition = CapturePositions.VUnloadHolderTop;
                    }
                    if (radioButton8.Checked)
                    {
                        capturePosition = CapturePositions.VUnloadCompensationBottom;
                    }
                    if (radioButton9.Checked)
                    {
                        capturePosition = CapturePositions.VTrayPlaceTop;
                    }

                    switch (_selectStation)
                    {
                        case StationId.V:
                            _cc.VRobot.MoveToTarget(capturePosition, ProcedureId.Load);
                            break;
                        case StationId.L:
                            _cc.LRobot.MoveToTarget(capturePosition, ProcedureId.Load);
                            break;

                        case StationId.GLine:
                            break;
                        case StationId.GPoint:
                            break;
                        default:
                            break;
                    }
                    MessageBox.Show("OK");
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }
            });
        }
    }
}
