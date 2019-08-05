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

        private StationId _selectedStation = StationId.L;
        private StationId _selectedGlueStation = StationId.GlueLine;
        private ActionType _selectedSuckerVacuum = ActionType.Load;
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
            //log.Info("System start");
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
            bool result = await Task.Run(() =>
            {
                try
                {
                    _cc.Setup();

                    return true;
                }
                catch (Exception ex)
                {
                    Show(ex);
                    return false;
                }
            });

            if (result)
            {
                buttonConnect.Text = "Connect OK";
                buttonConnect.BackColor = Color.LightGreen;

                try
                {
                    Setup();
                }
                catch (Exception ex)
                {
                    Show(ex);
                }
            }
            else
            {
                buttonConnect.Enabled = true;
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
            TrayPositionUpdate();

            comboBoxSelectCapturePosition.Items.Clear();
            foreach (var pos in Enum.GetValues(typeof(CaptureId)))
            {
                comboBoxSelectCapturePosition.Items.Add(pos);
            }

            foreach (var pos in _cc.DevelopPoints)
            {
                var bytes = Encoding.Default.GetBytes(pos.Remarks);
                var remarks = Encoding.UTF8.GetString(bytes);

                comboBoxReadDevPoints.Items.Add(remarks);
            }

            GetUserSettings();

            dataGridViewCapturePosition.DataSource = _cc.CapturePositions;
            dataGridViewUserOffset.DataSource = _cc.UserOffsets;
            dataGridViewGlueParameters.DataSource = _cc.GlueParameters;
            dataGridViewUserSettings.DataSource = _cc.UserSettings;
        }

        private void GetUserSettings()
        {

        }

        private void PositionUpdate()
        {
            if (_uiStarted == true)
            {
                return;
            }
            _uiStarted = true;
            Task.Run(() =>
            {
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

                        if (_manualMotorRUnload != null)
                        {
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
                    }
                    catch (Exception)
                    {

                        Thread.Sleep(1000);
                    }
                }
            });
        }

        private void TrayPositionUpdate()
        {
            //if (_uiStarted == true)
            //{
            //    return;
            //}
            //_uiStarted = true;
            Task.Run(() =>
            {
                while (true)
                {

                    try
                    {
                        labelVLoadTrayPos.BeginInvoke((MethodInvoker)(() =>
                        {
                            try
                            {
                                labelVLoadTrayPos.Text = mc.GetPosition(_cc.Mc.MotorVTrayLoad).ToString();
                            }
                            catch (Exception)
                            {
                            }

                        }));
                        Thread.Sleep(200);
                        labelVUnloadTrayPos.BeginInvoke((MethodInvoker)(() =>
                        {
                            try
                            {
                                labelVUnloadTrayPos.Text = mc.GetPosition(_cc.Mc.MotorVTrayUnload).ToString();
                            }
                            catch (Exception)
                            {
                            }

                        }));
                        Thread.Sleep(200);
                        labeLLoadTrayPos.BeginInvoke((MethodInvoker)(() =>
                        {
                            try
                            {
                                labeLLoadTrayPos.Text = mc.GetPosition(_cc.Mc.MotorLTrayLoad).ToString();
                            }
                            catch (Exception)
                            {

                            }

                        }));
                        Thread.Sleep(200);
                        labeLUnloadTrayPos.BeginInvoke((MethodInvoker)(() =>
                        {
                            try
                            {
                                labeLUnloadTrayPos.Text = mc.GetPosition(_cc.Mc.MotorLTrayUnload).ToString();
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

        private void radioButtonLStation_CheckedChanged(object sender, EventArgs e)
        {
            var radio = (RadioButton)sender;
            if (!radio.Checked)
            {
                return;
            }
            _selectedStation = StationId.L;
            SelectRobotStation(_selectedStation);
        }

        private void SelectRobotStation(StationId station)
        {
            switch (station)
            {
                case StationId.V:
                    _manualMotorX = _cc.VRobot.MotorX;
                    _manualMotorY = _cc.VRobot.MotorY;
                    _manualMotorZ = _cc.VRobot.MotorZ;
                    _manualMotorRLoad = _cc.VRobot.MotorA;
                    _manualMotorRUnload = _cc.VRobot.MotorAUnload;
                    break;

                case StationId.L:
                    _manualMotorX = _cc.LRobot.MotorX;
                    _manualMotorY = _cc.LRobot.MotorY;
                    _manualMotorZ = _cc.LRobot.MotorZ;
                    _manualMotorRLoad = _cc.LRobot.MotorA;
                    break;

                case StationId.GlueLine:
                    _manualMotorX = _cc.GlueLineRobot.MotorX;
                    _manualMotorY = _cc.GlueLineRobot.MotorY;
                    _manualMotorZ = _cc.GlueLineRobot.MotorZ;
                    _manualMotorRLoad = _cc.GlueLineRobot.MotorA;
                    break;

                case StationId.GluePoint:
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
            _selectedStation = StationId.V;
            SelectRobotStation(_selectedStation);
        }

        private void radioButtonGluePointStation_CheckedChanged(object sender, EventArgs e)
        {
            var radio = (RadioButton)sender;
            if (!radio.Checked)
            {
                return;
            }
            _selectedStation = StationId.GluePoint;
            SelectRobotStation(_selectedStation);
        }

        private void radioButtonGlueLineStation_CheckedChanged(object sender, EventArgs e)
        {
            var radio = (RadioButton)sender;
            if (!radio.Checked)
            {
                return;
            }
            _selectedStation = StationId.GlueLine;
            SelectRobotStation(_selectedStation);
        }

        private void button_YJogPlus_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_manualMotorY, _manualSpeed, MoveDirection.Positive);
            }
            catch (Exception ex)
            {
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
            buttonStart.Enabled = false;
            buttonStart.Text = "Homing...";
            var result = await Task.Run(() =>
            {
                try
                {
                    _cc.Start(homeSpeed);
                    return true;
                }
                catch (Exception ex)
                {
                    Show(ex);
                    return false;
                }
            });

            if (result)
            {
                buttonStart.Text = "Home complete";
                buttonStart.BackColor = Color.LightGreen;
            }
            else
            {
                buttonStart.Enabled = true;
                buttonStart.Text = "Start";
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
            }
        }

        private void trackBarManualControlSpeed_ValueChanged(object sender, EventArgs e)
        {
            _manualSpeed = Convert.ToDouble(trackBarManualControlSpeed.Value) / 10.0;
            label_manualSpeed.Text = _manualSpeed.ToString("0.0");
            _cc.SetSpeed(_manualSpeed);
        }

        private void buttonHighSpeed_Click(object sender, EventArgs e)
        {
            _manualSpeed = 50;
            label_manualSpeed.Text = _manualSpeed.ToString("0.0");
            _cc.SetSpeed(_manualSpeed);
        }

        private void buttonMiddleSpeed_Click(object sender, EventArgs e)
        {
            _manualSpeed = 20;
            label_manualSpeed.Text = _manualSpeed.ToString("0.0");
            _cc.SetSpeed(_manualSpeed);
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

                Show(ex);
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

                Show(ex);
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

                Show(ex);
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

                Show(ex);
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

                Show(ex);
            }
        }

        private void radioButtonMoveIncPositive_CheckedChanged(object sender, EventArgs e)
        {
            textBox_MoveInc.Text = Math.Abs(Convert.ToDouble(textBox_MoveInc.Text)) + "";
        }

        private void radioButtonMoveIncNegative_CheckedChanged(object sender, EventArgs e)
        {
            textBox_MoveInc.Text = -Math.Abs(Convert.ToDouble(textBox_MoveInc.Text)) + "";
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
                Show(ex);
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

                Show(ex);
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

                Show(ex);
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

                Show(ex);
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

                Show(ex);
            }
        }

        private void buttonSaveCapturePosition_Click(object sender, EventArgs e)
        {
            try
            {
                CaptureId capId = CaptureId.None;
                switch (_selectedStation)
                {
                    case StationId.V:
                        capId = CaptureId.VDevelopment;
                        break;
                    case StationId.L:
                        capId = CaptureId.LDevelopment;
                        break;
                    case StationId.GlueLine:
                        capId = CaptureId.GlueLineDevelopment;
                        break;
                    case StationId.GluePoint:
                        capId = CaptureId.GluePointDevelopment;
                        break;
                    default:
                        break;
                }

                CapturePosition newCap = new CapturePosition()
                {
                    CaptureId = capId,
                    XPosition = _manualMotion.GetPosition(_manualMotorX),
                    YPosition = _manualMotion.GetPosition(_manualMotorY),
                    ZPosition = _manualMotion.GetPosition(_manualMotorZ),
                    Remarks = textBoxCaptureNote.Text,
                };

                _cc.DevelopPoints.Add(newCap);
                comboBoxReadDevPoints.Items.Add(newCap.Remarks);

                var str = Helper.ConvertToJsonString(_cc.DevelopPoints);

                Helper.WriteFile(str, Properties.Settings.Default.DevelopmentPositions);
            }
            catch (Exception ex)
            {
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
            }
        }

        private void buttonSuckerOn_Click(object sender, EventArgs e)
        {
            try
            {
                switch (_selectedStation)
                {
                    case StationId.V:
                        switch (_selectedSuckerVacuum)
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
                    case StationId.GlueLine:
                    case StationId.GluePoint:
                    default:
                        throw new Exception("No sucker");
                }
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void radioButtonSuckerLoad_CheckedChanged(object sender, EventArgs e)
        {
            _selectedSuckerVacuum = ActionType.Load;
        }

        private void radioButtonSuckerUnload_CheckedChanged(object sender, EventArgs e)
        {
            _selectedSuckerVacuum = ActionType.Unload;
        }

        private void buttonSuckerOff_Click(object sender, EventArgs e)
        {
            try
            {
                switch (_selectedStation)
                {
                    case StationId.V:
                        switch (_selectedSuckerVacuum)
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
                        _cc.LRobot.HasPartOnLoadSucker = false;
                        break;
                    case StationId.GlueLine:
                    case StationId.GluePoint:
                    default:
                        throw new Exception("No sucker");
                }
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonClearAllFault_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (var mtr in _cc.Mc.Motors)
                {
                    _cc.Mc.Enable(mtr);
                    _cc.Mc.ClearFault(mtr);
                }
            }
            catch (Exception ex)
            {
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
            }
        }

        private void buttonHeadCylinderUp_Click(object sender, EventArgs e)
        {
            try
            {
                switch (_selectedStation)
                {
                    case StationId.V:
                        _cc.VRobot.CylinderHead(HeadCylinderState.Up, _selectedSuckerVacuum);
                        break;
                    case StationId.L:
                    case StationId.GlueLine:
                    case StationId.GluePoint:
                    default:
                        throw new Exception("No cyliner");
                }
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonHeadCylinderDown_Click(object sender, EventArgs e)
        {
            try
            {
                switch (_selectedStation)
                {
                    case StationId.V:
                        _cc.VRobot.CylinderHead(HeadCylinderState.Down, _selectedSuckerVacuum);
                        break;
                    case StationId.L:
                    case StationId.GlueLine:
                    case StationId.GluePoint:
                    default:
                        throw new Exception("No cyliner");
                }
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonCapturePosition_Click(object sender, EventArgs e)
        {
            try
            {
                switch (_selectedStation)
                {
                    case StationId.V:
                        _visionResult = _cc.VRobot.GetVisionResult(_selectedCapturePosition);
                        break;
                    case StationId.GluePoint:
                        //_visionResult = _cc.GluePointRobot.GetVisionResult(_selectedCapturePosition);
                        break;
                    case StationId.GlueLine:
                        _visionResult = _cc.GlueLineRobot.GetVisionResult(_selectedCapturePosition);
                        break;
                    case StationId.L:
                        _visionResult = _cc.LRobot.GetVisionResult(_selectedCapturePosition);
                        break;
                    case StationId.Reserved:
                        break;
                    case StationId.UV:
                        break;
                    default:
                        break;
                }

                textBox_MoveToTargetX.Text = _visionResult.X.ToString();
                textBox_MoveToTargetY.Text = _visionResult.Y.ToString();
                textBox_MoveToTargetZ.Text = _visionResult.Z.ToString();
                textBox_MoveToTargetRLoad.Text = _visionResult.A.ToString();
                textBox_MoveToTargetRUnload.Text = _visionResult.A.ToString();

                labelVisionTarget.Text = " X:" + _visionResult.X + Environment.NewLine +
               " Y:" + _visionResult.Y + Environment.NewLine +
               " Z:" + _visionResult.Z + Environment.NewLine +
               " R:" + _visionResult.A + Environment.NewLine +
               " deltaX1:" + _visionResult.XOffset1 + Environment.NewLine +
               " deltaY1:" + _visionResult.YOffset1 + Environment.NewLine +
               " deltaX2:" + _visionResult.XOffset2 + Environment.NewLine +
               " deltaY2:" + _visionResult.YOffset2 + Environment.NewLine;
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private async void buttonDemo_Click(object sender, EventArgs e)
        {
            buttonStartProduction.Enabled = false;
            buttonStartProduction.Text = "Running...";

            _cc.KeepThisShitRunning = true;
            _cc.VRobot.StopProduction = false;
            var task = _cc.WorkAsync();
            await task;
            ShowTaskResult(task);

            buttonStartProduction.Text = "Start";
            buttonStartProduction.Enabled = true;
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
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
                Show(ex);
            }
        }

        private CapturePosition GetCapturePosition(CaptureId id)
        {
            return Helper.GetCapturePosition(_cc.CapturePositions,
                _selectedCaptureId);
        }

        private async void buttonMoveToCapture_Click(object sender, EventArgs e)
        {
            buttonMoveToCapture.Enabled = false;
            var result = await Task.Run(() =>
            {
                try
                {
                    switch (_selectedCaptureId)
                    {
                        case CaptureId.LTrayPickTop:
                        case CaptureId.LLoadCompensationBottom:
                        case CaptureId.LLoadHolderTop:
                            _cc.LRobot.MoveToCapture(_selectedCapturePosition);
                            radioButtonLStation.Checked = true;
                            break;

                        case CaptureId.VTrayPickTop:
                        case CaptureId.VLoadCompensationBottom:
                        case CaptureId.VLoadHolderTop:
                        case CaptureId.VUnloadHolderTop:
                        case CaptureId.VUnloadCompensationBottom:
                        case CaptureId.VTrayPlaceTop:
                            _cc.VRobot.MoveToCapture(_selectedCapturePosition);
                            radioButtonVStation.Checked = true;
                            break;

                        case CaptureId.GlueLineChina:
                        case CaptureId.GlueLineBeforeGlue:
                        case CaptureId.GlueLineAfterGlue:
                        case CaptureId.GlueLineLaserOnPressureSensor:
                        case CaptureId.GlueLineNeedleOnPressureSensor:
                        case CaptureId.GlueLineLaserOnCalibrationChina:
                        case CaptureId.GlueLineCleanNeedleShot:
                        case CaptureId.GlueLineNeedleOnCalibrationChina:
                            _cc.GlueLineRobot.MoveToCapture(_selectedCapturePosition);
                            radioButtonGlueLineStation.Checked = true;
                            break;

                        case CaptureId.GluePointChina:
                        case CaptureId.GluePointBeforeGlue:
                        case CaptureId.GluePointAfterGlue:
                        case CaptureId.GluePointLaserOnCalibrationChina:
                        case CaptureId.GluePointCleanNeedleShot:
                        case CaptureId.GluePointNeedleOnCalibrationChina:
                        case CaptureId.GluePointNeedleOnPressureSensor:
                        case CaptureId.GluePointLaserOnPressureSensor:
                            _cc.GluePointRobot.MoveToCapture(_selectedCapturePosition);
                            radioButtonGluePointStation.Checked = true;
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    MessageBox.Show("OK");
                    return true;
                }
                catch (Exception ex)
                {
                    Show(ex);
                    return false;
                }
            });
            buttonMoveToCapture.Enabled = true;
        }

        private void comboBoxSelectCapturePosition_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                _selectedCaptureId = (CaptureId)comboBoxSelectCapturePosition.SelectedItem;

                var pose = GetCapturePosition(_selectedCaptureId);
                _selectedCapturePosition = pose;

                textBox_MoveToTargetX.Text = pose.XPosition + "";
                textBox_MoveToTargetY.Text = pose.YPosition + "";
                textBox_MoveToTargetZ.Text = pose.ZPosition + "";
                textBox_MoveToTargetRLoad.Text = pose.Angle + "";
                textBox_MoveToTargetRUnload.Text = pose.Angle + "";

                labelCapturePosData.Text = " X:" + pose.XPosition + Environment.NewLine +
              " Y:" + pose.YPosition + Environment.NewLine +
              " Z:" + pose.ZPosition + Environment.NewLine +
              " R:" + pose.Angle + Environment.NewLine;
            }
            catch (Exception ex)
            {
                Show(ex);
            }
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
                        case CaptureId.LLoadCompensationBottom:
                        case CaptureId.LLoadHolderTop:
                            _cc.LRobot.MoveToTarget(_visionResult,
                                MoveModeAMotor.Relative, ActionType.Load);
                            break;
                        case CaptureId.VTrayPickTop:
                        case CaptureId.VLoadCompensationBottom:
                        case CaptureId.VLoadHolderTop:
                        case CaptureId.VUnloadHolderTop:
                        case CaptureId.VUnloadCompensationBottom:
                        case CaptureId.VTrayPlaceTop:
                            _cc.VRobot.MoveToTarget(_visionResult,
                                MoveModeAMotor.Relative, _selectedSuckerVacuum);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                catch (Exception ex)
                {
                    Show(ex);
                }
            });
        }

        private async void buttonGoToUnload_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.Enabled = false;
            button.Text = "Finding ....";

            var task = _cc.VRobot.FindBaseUnloadPositionAsync();
            await task;
            ShowTaskResult(task);

            button.Text = task.Result.Message;
            button.Enabled = true;
        }

        private async void buttonLStationTestRun_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.Enabled = false;

            try
            {
                var temp = _cc.LRobot.CurrentCycleId;
                _cc.LRobot.SetForTest();
                var task = _cc.LRobot.WorkAsync(temp + 1);
                await task;
                ShowTaskResult(task);
                _cc.LRobot.CurrentCycleId = temp;

                //_cc.WorkTable.Sucker(FixtureId.L, VacuumState.Off, VacuumArea.Center);
                //_cc.WorkTable.Sucker(FixtureId.L, VacuumState.Off, VacuumArea.Circle);

            }
            catch (Exception ex)
            {
                Show(ex);
            }

            button.Enabled = true;
        }

        private async void buttonShotGlue_Click(object sender, EventArgs e)
        {
            buttonShotGlue.Enabled = false;
            switch (_selectedGlueStation)
            {
                case StationId.V:
                case StationId.L:
                    break;
                case StationId.GluePoint:
                    var taskGL = _cc.GluePointRobot.ShotGlueAsync(Convert.ToUInt16(textBoxShotGlueDelay.Text));
                    await taskGL;
                    ShowTaskResult(taskGL);
                    break;

                case StationId.GlueLine:
                    var taskGC = _cc.GlueLineRobot.ShotGlueAsync(Convert.ToUInt16(textBoxShotGlueDelay.Text));
                    await taskGC;
                    ShowTaskResult(taskGC);
                    break;
                default:
                    break;
            }

            buttonShotGlue.Enabled = true;
        }

        private async void buttonGetLaserHeight_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.Enabled = false;
            double value = double.NaN;
            await Task.Run((Action)(() =>
            {
                try
                {
                    switch (_selectedGlueStation)
                    {
                        case StationId.V:
                            break;
                        case StationId.L:
                            break;
                        case StationId.GlueLine:
                            value = _cc.GlueLineRobot.GetLaserHeightValue();
                            break;
                        case StationId.GluePoint:
                            value = _cc.GluePointRobot.GetLaserHeightValue();
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Show(ex);
                }
            }));
            labelLaserValue.Text = value.ToString();
            button.Enabled = true;
        }

        private async void buttonGetGlueWorkPoints_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.Enabled = false;
            object obj = null;
            try
            {
                switch (_selectedStation)
                {
                    case StationId.V:
                        break;
                    case StationId.L:
                        break;
                    case StationId.GlueLine:
                        var task = _cc.GlueLineRobot.GetVisionResultsForLaserAndWorkAsync();
                        await task;
                        ShowTaskResult(task);
                        break;
                    case StationId.GluePoint:
                        //task = _cc.GluePointRobot.GetVisionResultsForLaserAndWorkAsync();
                        //await task;
                        //ShowTaskResult(task);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Show(ex);
            }

            labelGlueWorkPointsInfo.Text = Helper.ConvertToJsonString(obj);
            button.Enabled = true;
        }

        private async void buttonMoveToHistoryPoint_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.Enabled = false;
            var str = comboBoxReadDevPoints.SelectedItem.ToString();


            await Task.Run(() =>
            {
                try
                {
                    var targt = _cc.GetDevelopPoints(str);

                    switch (targt.CaptureId)
                    {
                        case CaptureId.GluePointDevelopment:
                            _cc.GluePointRobot.MoveToCapture(targt);
                            break;

                        case CaptureId.GlueLineDevelopment:
                            _cc.GlueLineRobot.MoveToCapture(targt);
                            break;

                        case CaptureId.VDevelopment:
                            _cc.VRobot.MoveToCapture(targt);
                            break;

                        case CaptureId.LDevelopment:
                            _cc.LRobot.MoveToCapture(targt);
                            break;

                        default:
                            throw new NotImplementedException("No such dev point.");
                    }
                }
                catch (Exception ex)
                {
                    Show(ex);
                }
            });
            button.Enabled = true;
        }

        private async void buttonDrawACircle_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.Enabled = false;

            ArcInfo arc = new ArcInfo()
            {
                XEnd = Convert.ToDouble(textBoxEndX.Text),
                YEnd = Convert.ToDouble(textBoxEndY.Text),
                XCenter = Convert.ToDouble(textBoxCenterX.Text),
                YCenter = Convert.ToDouble(textBoxCenterY.Text),
            };

            arc.CalculateCenterOffset();

            var task = _cc.GlueLineRobot.DrawArcAsync(arc);
            await task;
            ShowTaskResult(task);

            button.Enabled = true;
        }

        private void ShowTaskResult(Task<WaitBlock> waitBlock)
        {
            try
            {
                Helper.CheckTaskResult(waitBlock);
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonCloseGlue_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.Enabled = false;
            try
            {
                switch (_selectedGlueStation)
                {
                    case StationId.V:
                        break;
                    case StationId.L:
                        break;
                    case StationId.GlueLine:
                        _cc.GlueLineRobot.CloseGlue();
                        break;
                    case StationId.GluePoint:
                        _cc.GluePointRobot.CloseGlue();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Show(ex);
            }
            button.Enabled = true;
        }

        private async void buttonLockAllTray_Click(object sender, EventArgs e)
        {
            try
            {
                Task<WaitBlock>[] blocks = new Task<WaitBlock>[3];
                blocks[0] = _cc.LRobot.LockTrayAsync();
                blocks[1] = _cc.VRobot.LockTrayAsync(ActionType.Load);
                blocks[2] = _cc.VRobot.LockTrayAsync(ActionType.Unload);

                await blocks[0];
                await blocks[1];
                await blocks[2];
                string errorInfo = string.Empty;
                foreach (var block in blocks)
                {
                    if (block.Result.Code != ErrorCode.Sucessful)
                    {
                        errorInfo += "Error Code: " + block.Result.Code +
                            block.Result.Message + Environment.NewLine;
                    }
                }

                if (errorInfo != string.Empty)
                {
                    throw new Exception(errorInfo);
                }
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private async void buttonGetPressureSensor_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.Enabled = false;
            double value = double.NaN;
            await Task.Run((Action)(() =>
            {
                try
                {
                    switch (_selectedGlueStation)
                    {
                        case StationId.V:
                            break;
                        case StationId.L:
                            break;
                        case StationId.GlueLine:
                            value = _cc.GlueLineRobot.GetPressureValue();
                            break;
                        case StationId.GluePoint:
                            value = _cc.GluePointRobot.GetPressureValue();
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Show(ex);
                }
            }));
            labelPressureValue.Text = value.ToString();
            button.Enabled = true;
        }

        private async void buttonCaptureNeedle_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.Enabled = false;
            double needleHeight = 0;

            switch (_selectedGlueStation)
            {
                case StationId.V:
                    break;
                case StationId.L:
                    break;
                case StationId.GlueLine:
                    var task = _cc.GlueLineRobot.FindNeedleHeightAsync();
                    await task;
                    ShowTaskResult(task);
                    needleHeight = _cc.GlueLineRobot.NeedleOnZHeight;
                    break;
                case StationId.GluePoint:
                    task = _cc.GluePointRobot.FindNeedleHeightAsync();
                    await task;
                    ShowTaskResult(task);
                    needleHeight = _cc.GluePointRobot.NeedleOnZHeight;
                    break;
                default:
                    break;
            }

            labelNeedleHeight.Text = needleHeight + "";
            button.Enabled = true;
        }

        private async void buttonFindLaserReference_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.Enabled = false;
            double laserRefereceZHeight = 0;

            switch (_selectedGlueStation)
            {
                case StationId.V:
                    break;
                case StationId.L:
                    break;
                case StationId.GlueLine:
                    var task = _cc.GlueLineRobot.FindLaserRefereceHeightAsync();
                    await task;
                    ShowTaskResult(task);
                    laserRefereceZHeight = _cc.GlueLineRobot.LaserRefereceZHeight;
                    break;

                case StationId.GluePoint:
                    task = _cc.GluePointRobot.FindLaserRefereceHeightAsync();
                    await task;
                    ShowTaskResult(task);
                    laserRefereceZHeight = _cc.GluePointRobot.LaserRefereceZHeight;
                    break;
                default:
                    break;
            }

            labelLaserReference.Text = laserRefereceZHeight + "";
            button.Enabled = true;
        }

        private void buttonMeasureDistance_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.Enabled = false;
            double distance = 0;

            switch (_selectedGlueStation)
            {
                case StationId.V:
                    break;
                case StationId.L:
                    break;
                case StationId.GlueLine:
                    distance = _cc.GlueLineRobot.GetNeedleToWorkSurfaceHeight();
                    break;
                case StationId.GluePoint:
                    distance = _cc.GluePointRobot.GetNeedleToWorkSurfaceHeight();
                    break;
                default:
                    break;
            }

            labelNeedleToSurfaceDistance.Text = distance + "";
            button.Enabled = true;
        }

        private void buttonStopDemo_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.VRobot.StopProduction = true;
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void Show(Exception ex)
        {
            MessageBox.Show(ex.Message);
        }

        private void buttonResetError_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.Reset();
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private async void buttonVStationTestRun_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.Enabled = false;

            try
            {
                var temp = _cc.VRobot.CurrentCycleId;
                _cc.VRobot.SetForTest();
                var task = _cc.VRobot.WorkAsync(temp + 1);
                await task;
                ShowTaskResult(task);
                _cc.VRobot.CurrentCycleId = temp;
            }
            catch (Exception ex)
            {
                Show(ex);
            }

            button.Enabled = true;
        }

        private void buttonUVLightTop_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.WorkTable.UVLightOnAsync(Convert.ToInt32(textBoxUvDelaySec.Text));
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonHeadUvLight_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.LRobot.UVLightOnAsync(Convert.ToInt32(textBoxHeadUvDelay.Text));
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonSaveUserOffset_Click(object sender, EventArgs e)
        {
            try
            {
                var offsets = Helper.ConvertToJsonString(_cc.UserOffsets);
                Helper.WriteFile(offsets, Properties.Settings.Default.CapturePositionOffsets);
                MessageBox.Show("OK");
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonSetFixtureFull_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.WorkTable.SetFixturesFull();
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void textBox_MoveInc_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                double sign = radioButtonMoveIncPositive.Checked ? 1.0 : -1.0;
                textBox_MoveInc.Text =
                    (sign * Math.Abs(Convert.ToDouble(textBox_MoveInc.Text))).ToString();
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private async void buttonCalibrateNeedle_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.Enabled = false;

            switch (_selectedGlueStation)
            {
                case StationId.V:
                    break;
                case StationId.L:
                    break;
                case StationId.GlueLine:
                    _cc.GlueLineRobot.NeedleOnZHeightCompensation =
                        Convert.ToDouble(textBoxNeedleOnPressureSensorCompensation.Text);
                    var task = _cc.GlueLineRobot.CalibrateNeedleAsync();
                    await task;
                    ShowTaskResult(task);
                    break;
                case StationId.GluePoint:
                    task = _cc.GluePointRobot.CalibrateNeedleAsync();
                    await task;
                    ShowTaskResult(task);
                    break;
                default:
                    break;
            }

            button.Enabled = true;
        }

        private async void buttonCleanNeedle_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.Enabled = false;
            try
            {
                int delay = Convert.ToInt32(textBoxShotGlueDelay.Text);
                switch (_selectedGlueStation)
                {
                    case StationId.V:
                        break;
                    case StationId.L:
                        break;
                    case StationId.GlueLine:
                        var task = _cc.GlueLineRobot.CleanNeedleAsync(delay);
                        await task;
                        ShowTaskResult(task);
                        break;

                    case StationId.GluePoint:
                        task = _cc.GluePointRobot.CleanNeedleAsync(delay);
                        await task;
                        ShowTaskResult(task);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Show(ex);
            }

            button.Enabled = true;
        }

        private void buttonSaveCapturePositions_Click(object sender, EventArgs e)
        {
            try
            {
                var capPos = Helper.ConvertToJsonString(_cc.CapturePositions);
                Helper.WriteFile(capPos, Properties.Settings.Default.CapturePositions);
                MessageBox.Show("OK");
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private async void buttonGlueWorkTest_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.Enabled = false;
            try
            {
                switch (_selectedGlueStation)
                {
                    case StationId.V:
                        break;
                    case StationId.L:
                        break;
                    case StationId.GlueLine:
                        _cc.WorkTable.Sucker(FixtureId.GlueLine, VacuumState.On, VacuumArea.Circle);
                        var cycleId = _cc.GlueLineRobot.CurrentCycleId;
                        _cc.GlueLineRobot.CurrentCycleId--;
                        _cc.WorkTable.Fixtures[(int)FixtureId.GlueLine].IsEmpty = false;
                        var task = _cc.GlueLineRobot.WorkAsync(cycleId);
                        await task;
                        ShowTaskResult(task);
                        //_cc.WorkTable.Sucker(FixtureId.GlueLine, VacuumState.Off, VacuumArea.Circle);
                        break;

                    case StationId.GluePoint:
                        _cc.WorkTable.Sucker(FixtureId.GluePoint, VacuumState.On, VacuumArea.Circle);
                        cycleId = _cc.GluePointRobot.CurrentCycleId;
                        _cc.GluePointRobot.CurrentCycleId--;
                        _cc.WorkTable.Fixtures[(int)FixtureId.GluePoint].IsEmpty = false;
                        task = _cc.GluePointRobot.WorkAsync(cycleId);
                        await task;
                        ShowTaskResult(task);
                        //_cc.WorkTable.Sucker(FixtureId.GluePoint, VacuumState.Off, VacuumArea.Circle);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Show(ex);
            }

            button.Enabled = true;
        }

        private void radioButtonGluePoint_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonGluePoint.Checked)
            {
                _selectedGlueStation = StationId.GluePoint;
            }
        }

        private void radioButtonGlueLine_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonGlueLine.Checked)
            {
                _selectedGlueStation = StationId.GlueLine;
            }
        }

        private void buttonStopMotion_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.Mc.StopAllSoft();
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonPauseProduction_Click(object sender, EventArgs e)
        {
            _cc.KeepThisShitRunning = false;
            MessageBox.Show("Production pause after finish current cycle.");
        }

        private void buttonSaveGlueParameters_Click(object sender, EventArgs e)
        {
            try
            {
                //GlueParameter glueParaPoint = new GlueParameter()
                //{
                //    PreShotTime = 100,
                //    GluePeriod = 500,
                //    CloseGlueDelay = 200,
                //    GlueSpeed = 5,
                //    RiseGlueHeight = 2,
                //    RiseGlueSpeed = 0.5,
                //    SecondLineLessPreShot = 50,
                //};

                //GlueParameter glueParaLine = new GlueParameter()
                //{
                //    PreShotTime = 200,
                //    GlueSpeed = 5,
                //    RiseGlueSpeed = 0.5,
                //    RiseGlueHeight = 2,
                //    CloseGlueDelay = 300,
                //    SecondLineLessPreShot = 100,
                //};

                //List<GlueParameter> glues = new List<GlueParameter>();
                //glues.Add(glueParaPoint);
                //glues.Add(glueParaLine);

                var glueParas = Helper.ConvertToJsonString(_cc.GlueParameters);
                Helper.WriteFile(glueParas, Properties.Settings.Default.GlueParameters);

                MessageBox.Show("OK");
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonSaveOtherParas_Click(object sender, EventArgs e)
        {
            try
            {
                var jStr = Helper.ConvertToJsonString(_cc.UserSettings);
                Helper.WriteFile(jStr, Properties.Settings.Default.UserSettings);
                _cc.SettingManager.ValidateSettings();
                MessageBox.Show("OK");
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                _cc.LLoadStation.PushIn();
                _cc.LLoadStation.Home();
            }
            catch (Exception ex)
            {
                Show(ex);
            }

            //_cc.VLoadStation.Home();
        }

        private void buttonVLoadLoadTray_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.VLoadStation.LoadATray();
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonVLoadUnloadTray_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.VLoadStation.UnloadATray();
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonVUnloadLoadTray_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.VUnloadStation.LoadATray();
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonVUnloadUnloadTray_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.VUnloadStation.UnloadATray();
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonLLoadLoadTray_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.LLoadStation.LoadATray();
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonLUnloadUnloadTray_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.LUnloadStation.UnloadATray();
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }
        private void buttonVLoadTrayJogMinus_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_cc.Mc.MotorVTrayLoad, _manualSpeed, MoveDirection.Negative);
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonVUnloadTrayJogMinus_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_cc.Mc.MotorVTrayUnload, _manualSpeed, MoveDirection.Negative);
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonLLoadTrayJogMinus_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_cc.Mc.MotorLTrayLoad, _manualSpeed, MoveDirection.Negative);
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonLUnloadTrayJogMinus_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_cc.Mc.MotorLTrayUnload, _manualSpeed, MoveDirection.Negative);
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonVLoadTrayJogPlus_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_cc.Mc.MotorVTrayLoad, _manualSpeed, MoveDirection.Positive);
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonVUnloadTrayJogPlus_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_cc.Mc.MotorVTrayUnload, _manualSpeed, MoveDirection.Positive);
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonLLoadTrayJogPlus_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_cc.Mc.MotorLTrayLoad, _manualSpeed, MoveDirection.Positive);
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonLUnloadTrayJogPlus_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _manualMotion.Jog(_cc.Mc.MotorLTrayUnload, _manualSpeed, MoveDirection.Positive);
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonVLoadTrayJogMinus_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _cc.Mc.Stop(_cc.Mc.MotorVTrayLoad);
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonVLoadTrayJogPlus_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _cc.Mc.Stop(_cc.Mc.MotorVTrayLoad);
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonVUnloadTrayJogMinus_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _cc.Mc.Stop(_cc.Mc.MotorVTrayUnload);
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonVUnloadTrayJogPlus_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _cc.Mc.Stop(_cc.Mc.MotorVTrayUnload);
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonLLoadTrayJogMinus_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _cc.Mc.Stop(_cc.Mc.MotorLTrayLoad);
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonLLoadTrayJogPlus_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _cc.Mc.Stop(_cc.Mc.MotorLTrayLoad);
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonLUnloadTrayJogMinus_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _cc.Mc.Stop(_cc.Mc.MotorLTrayUnload);
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonLUnloadTrayJogPlus_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _cc.Mc.Stop(_cc.Mc.MotorLTrayUnload);
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonVLoadTrayCylinderIn_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.VLoadStation.PushIn();
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonVLoadTrayCylinderOut_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.VLoadStation.PushOut();
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonVUnloadTrayCylinderOut_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.VUnloadStation.PushOut();
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonVUnloadTrayCylinderIn_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.VUnloadStation.PushIn();
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonLLoadTrayCylinderIn_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.LLoadStation.PushIn();
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonLLoadTrayCylinderOut_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.LLoadStation.PushOut();
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonLUnloadTrayCylinderOut_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.LUnloadStation.PushOut();
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }

        private void buttonLUnloadTrayCylinderIn_Click(object sender, EventArgs e)
        {
            try
            {
                _cc.LUnloadStation.PushIn();
            }
            catch (Exception ex)
            {
                Show(ex);
            }
        }
    }
 }
