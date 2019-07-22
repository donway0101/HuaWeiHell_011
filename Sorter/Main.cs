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
        private double _demoSpeed = 20;

        private StationId _selectStation = StationId.L;
        private ActionType _selectSuckerVacuum = ActionType.Load;
        private CaptureId _selectedCaptureId = CaptureId.LTrayPickTop;
        private CapturePosition _selectedCapturePosition;
        private Pose _visionResult;

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
            _manualMotorRLoad = _cc.LRobot.MotorA;

            PositionUpdate();

            comboBoxSelectCapturePosition.Items.Clear();
            foreach (var pos in Enum.GetValues(typeof(CaptureId)))
            {
                comboBoxSelectCapturePosition.Items.Add(pos);
            }
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
                    panel_RLoad2.Visible = true;
                    panel_RUnload2.Visible = true;
                    _manualMotorX = _cc.VRobot.MotorX;
                    _manualMotorY = _cc.VRobot.MotorY;
                    _manualMotorZ = _cc.VRobot.MotorZ;
                    _manualMotorRLoad = _cc.VRobot.MotorA;
                    _manualMotorRUnload = _cc.VRobot.MotorAUnload;
                    break;

                case StationId.L:
                    panel_RLoad2.Visible = true;
                    panel_RUnload2.Visible = false;
                    _manualMotorX = _cc.LRobot.MotorX;
                    _manualMotorY = _cc.LRobot.MotorY;
                    _manualMotorZ = _cc.LRobot.MotorZ;
                    _manualMotorRLoad = _cc.LRobot.MotorA;
                    break;

                case StationId.GLine:
                    panel_RLoad2.Visible = false;
                    panel_RUnload2.Visible = false;
                    _manualMotorX = _cc.GlueLineRobot.MotorX;
                    _manualMotorY = _cc.GlueLineRobot.MotorY;
                    _manualMotorZ = _cc.GlueLineRobot.MotorZ;
                    _manualMotorRLoad = _cc.GlueLineRobot.MotorA;
                    break;

                case StationId.GPoint:
                    panel_RLoad2.Visible = false;
                    panel_RUnload2.Visible = false;
                    _manualMotorX = _cc.GluePointRobot.MotorX;
                    _manualMotorY = _cc.GluePointRobot.MotorY;
                    _manualMotorZ = _cc.GluePointRobot.MotorZ;
                    _manualMotorRLoad = _cc.GluePointRobot.MotorA;
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
            double homeSpeed = 10;
            if (checkBoxFastHome.Checked)
            {
                homeSpeed = 30;
            }
            //buttonStart.Enabled = false;
            var result = await Task.Run(() =>
            {
                try
                {
                    _cc.Start(homeSpeed);
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
                str += textBoxCaptureNote.Text + Environment.NewLine;

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
                _cc.VRobot.LockTray(ActionType.Load);
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
                _cc.VRobot.UnlockTray(ActionType.Load);
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
                _cc.VRobot.LockTray(ActionType.Unload);
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
                _cc.VRobot.UnlockTray(ActionType.Unload);
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
                            case ActionType.Load:
                                _cc.VRobot.Sucker(VacuumState.On, ActionType.Load);
                                break;
                            case ActionType.Unload:
                                _cc.VRobot.Sucker(VacuumState.On, ActionType.Unload);
                                break;
                            default:
                                break;
                        }
                        break;
                    case StationId.L:
                        _cc.LRobot.Sucker(VacuumState.On);
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
            _selectSuckerVacuum = ActionType.Load;
        }

        private void radioButtonSuckerUnload_CheckedChanged(object sender, EventArgs e)
        {
            _selectSuckerVacuum = ActionType.Unload;
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
                            case ActionType.Load:
                                _cc.VRobot.Sucker(VacuumState.Off, ActionType.Load);
                                break;
                            case ActionType.Unload:
                                _cc.VRobot.Sucker(VacuumState.Off, ActionType.Unload);
                                break;
                            default:
                                break;
                        }
                        break;
                    case StationId.L:
                        _cc.LRobot.Sucker(VacuumState.Off);
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
                _visionResult = _cc.VRobot.GetVisionResult(_selectedCapturePosition);

                labelVisionTarget.Text = " X:" + _visionResult.X + Environment.NewLine +
               " Y:" + _visionResult.Y + Environment.NewLine +
               " Z:" + _visionResult.Z + Environment.NewLine +
               " R:" + _visionResult.A + Environment.NewLine +
               " deltaX1:" + _visionResult.XOffset1 + Environment.NewLine +
               " deltaY1:" + _visionResult.YOffset1 + Environment.NewLine +
               " deltaX2:" + _visionResult.XOffset2 + Environment.NewLine +
               " deltaY2:" + _visionResult.YOffset2 + Environment.NewLine ;

                MessageBox.Show("Can go to vision target now...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void buttonDemo_Click(object sender, EventArgs e)
        {
            buttonDemo.Enabled = false;
            await Task.Run(async () =>
            {
                try
                {
                    //var taskTest = _cc.GlueLineRobot.Work();//_cc.RunThisFuckingShit();
                    var taskTest = _cc.RunThisFuckingShit();
                    await taskTest;

                    if (taskTest.Result.Code != 0)
                    {
                        var msg = "";
                        msg += taskTest.Result.Code.ToString() + taskTest.Result.Message.ToString();
                        throw new Exception(msg);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });

            buttonDemo.Enabled = true;
        }

        private void trackBarDemoSpeed_ValueChanged(object sender, EventArgs e)
        {
            _demoSpeed = Convert.ToDouble(trackBarDemoSpeed.Value);
            _cc.SetSpeed(_demoSpeed);
            labelDemoSpeed.Text = _demoSpeed.ToString();
        }

        private void buttonTableCircleVacuumOnV_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.WorkTable.Sucker(FixtureId.V, VacuumState.On);
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
                _cc.WorkTable.Sucker(FixtureId.GluePoint, VacuumState.On);
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
                _cc.WorkTable.Sucker(FixtureId.GlueLine, VacuumState.On);
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
                _cc.WorkTable.Sucker(FixtureId.L, VacuumState.On);
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
                _cc.WorkTable.Sucker(FixtureId.Reserve, VacuumState.On);
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
                _cc.WorkTable.Sucker(FixtureId.UVLight, VacuumState.On);
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
                _cc.WorkTable.Sucker(FixtureId.V, VacuumState.Off);
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
                _cc.WorkTable.Sucker(FixtureId.GluePoint, VacuumState.Off);
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
                _cc.WorkTable.Sucker(FixtureId.GlueLine, VacuumState.Off);
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
                _cc.WorkTable.Sucker(FixtureId.L, VacuumState.Off);
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
                _cc.WorkTable.Sucker(FixtureId.Reserve, VacuumState.Off);
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
                _cc.WorkTable.Sucker(FixtureId.UVLight, VacuumState.Off);
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
            try
            {
                _cc.WorkTable.Sucker(FixtureId.L, VacuumState.On, VacuumArea.Circle);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
                    A = Convert.ToDouble(textBox_MoveToTargetRLoad.Text),
                    RUnloadAngle = Convert.ToDouble(textBox_MoveToTargetRUnload.Text),
                };

                switch (_selectStation)
                {
                    case StationId.V:
                        _cc.VRobot.MoveToTarget(pose, _selectSuckerVacuum);
                        break;
                    case StationId.L:
                        _cc.LRobot.MoveToTarget(pose);
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

        private CapturePosition GetCapturePosition(CaptureId id)
        {
            return  Helper.GetCapturePosition(_cc.CapturePositions,
                _selectedCaptureId, textBoxCaptureTag.Text);
        }

        private async void buttonMoveToCapture_Click(object sender, EventArgs e)
        {
            buttonMoveToCapture.Enabled = false;
            var result = await Task.Run(() =>
            {
                try
                {
                    _selectedCapturePosition = GetCapturePosition(CaptureId.LTrayPickTop);

                    switch (_selectedCaptureId)
                    {
                        case CaptureId.LTrayPickTop:
                            _cc.LRobot.MoveToCapture(_selectedCapturePosition);
                            break;
                        case CaptureId.LLoadCompensationBottom:
                            _cc.LRobot.MoveToCapture(_selectedCapturePosition);
                            break;
                        case CaptureId.LLoadHolderTop:
                            _cc.LRobot.MoveToCapture(_selectedCapturePosition);
                            break;
                        case CaptureId.VTrayPickTop:
                            _cc.VRobot.MoveToCapture(_selectedCapturePosition);
                            break;
                        case CaptureId.VLoadCompensationBottom:
                            _cc.VRobot.MoveToCapture(_selectedCapturePosition);
                            break;
                        case CaptureId.VLoadHolderTop:
                            _cc.VRobot.MoveToCapture(_selectedCapturePosition);
                            break;
                        case CaptureId.VUnloadHolderTop:
                            _cc.VRobot.MoveToCapture(_selectedCapturePosition);
                            break;
                        case CaptureId.VUnloadCompensationBottom:
                            _cc.VRobot.MoveToCapture(_selectedCapturePosition);
                            break;
                        case CaptureId.VTrayPlaceTop:
                            _cc.VRobot.MoveToCapture(_selectedCapturePosition);
                            break;
                        default:
                            throw new NotImplementedException();
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
            buttonMoveToCapture.Enabled = true;
        }

        private async void buttonStartupLoad_Click(object sender, EventArgs e)
        {
            buttonStartupLoad.Enabled = false;
            _cc.LRobot.VisionSimulateMode = checkBoxVisionSimulate.Checked;
            _cc.LRobot.CheckVacuumValue = checkBoxVaccumSimulate.Checked;

            _cc.VRobot.VisionSimulateMode = checkBoxVisionSimulate.Checked;
            _cc.VRobot.CheckVacuumValue = checkBoxVaccumSimulate.Checked;

            await Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1);
                    var vLoadTest = _cc.StartupLoad();
                    await vLoadTest;
                    _cc.CheckTaskResult(vLoadTest);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            });

            buttonStartupLoad.Enabled = true;
        }

        private void comboBoxSelectCapturePosition_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedCaptureId = (CaptureId)comboBoxSelectCapturePosition.SelectedItem;
        }

        private async void buttonMoveToVisionTarget_Click(object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1);
                    switch (_selectedCaptureId)
                    {
                        case CaptureId.LTrayPickTop:
                            _cc.LRobot.MoveToTarget(_visionResult);
                            break;
                        case CaptureId.LLoadCompensationBottom:
                            _cc.LRobot.MoveToTarget(_visionResult);
                            break;
                        case CaptureId.LLoadHolderTop:
                            _cc.LRobot.MoveToTarget(_visionResult);
                            break;
                        case CaptureId.VTrayPickTop:
                            _cc.VRobot.MoveToTarget(_visionResult);
                            break;
                        case CaptureId.VLoadCompensationBottom:
                            _cc.VRobot.MoveToTarget(_visionResult);
                            break;
                        case CaptureId.VLoadHolderTop:
                            _cc.VRobot.MoveToTarget(_visionResult);
                            break;
                        case CaptureId.VUnloadHolderTop:
                            _cc.VRobot.MoveToTarget(_visionResult);
                            break;
                        case CaptureId.VUnloadCompensationBottom:
                            _cc.VRobot.MoveToTarget(_visionResult);
                            break;
                        case CaptureId.VTrayPlaceTop:
                            _cc.VRobot.MoveToTarget(_visionResult);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private async void buttonGoToUnload_Click(object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1);
                    _cc.VRobot.FindBaseUnloadPosition();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private async void buttonGoToUnloadTray_Click(object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1);
                    int i = 12;
                    while (i > 0)
                    {
                        i--;
                        _cc.VRobot.Unload(_cc.VRobot.UnloadTray.CurrentPart);
                    }
                   
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private async void buttonLStationTestRun_Click(object sender, EventArgs e)
        {
            buttonLStationTestRun.Enabled = false;
            await Task.Run(async () =>
            {
                try
                {
                    var taskTest = _cc.LStationTestRun();
                    await taskTest;

                    if (taskTest.Result.Code != 0)
                    {
                        var msg = "";
                        msg += taskTest.Result.Code.ToString() + taskTest.Result.Message.ToString();
                        throw new Exception(msg);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });

            buttonLStationTestRun.Enabled = true;
        }

        private async void buttonVStationTestRun_Click(object sender, EventArgs e)
        {
            buttonVStationTestRun.Enabled = false;
            await Task.Run(async () =>
            {
                try
                {
                    var taskTest = _cc.VStationTestRun();
                    await taskTest;

                    if (taskTest.Result.Code != 0)
                    {
                        var msg = "";
                        msg += taskTest.Result.Code.ToString() + taskTest.Result.Message.ToString();
                        throw new Exception(msg);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });

            buttonVStationTestRun.Enabled = true;
        }

        private async void buttonGluePointTest_Click(object sender, EventArgs e)
        {
            buttonGluePointTest.Enabled = false;
            await Task.Run(async () =>
            {
                try
                {
                    var taskTest = _cc.GluePointStationTestRun();
                    await taskTest;

                    if (taskTest.Result.Code != 0)
                    {
                        var msg = "";
                        msg += taskTest.Result.Code.ToString() + taskTest.Result.Message.ToString();
                        throw new Exception(msg);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });

            buttonGluePointTest.Enabled = true;
        }

        private async void buttonShotGlue_Click(object sender, EventArgs e)
        {
            buttonShotGlue.Enabled = false;
            await Task.Run(() =>
            {
                try
                {
                    switch (_selectStation)
                    {
                        case StationId.V:
                            break;
                        case StationId.L:
                            break;
                        case StationId.GLine:
                            _cc.GlueLineRobot.ShotGlue(Convert.ToUInt16(textBoxShotGlueDelay.Text));
                            break;
                        case StationId.GPoint:
                            _cc.GluePointRobot.ShotGlue(Convert.ToUInt16(textBoxShotGlueDelay.Text));
                            break;
                        default:
                            break;
                    }
                  
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });

            buttonShotGlue.Enabled = true;
        }
    }
}
