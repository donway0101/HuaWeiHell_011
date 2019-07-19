using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sorter
{
    public class FunctionTest
    {
        //private 
        MotionController mc = new MotionController();
        public RoundTable table;
        AssemblyRobot VRobot;
        AssemblyRobot LRobot;
        AssemblyRobot GlueLineRobot;
        AssemblyRobot GluePointRobot;

        public void Start()
        {
            
        }

        public CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public Task<WaitBlock> VRobotTest()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(100, cancellationTokenSource.Token);
                    //cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    //code = 1;
                    //await Task.Delay(10000, cancellationTokenSource.Token);
                    //code = 2;

                    return new WaitBlock() { Code = 0 };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = 1, Message = ex.Message };
                }
            }, cancellationTokenSource.Token);
        }

        public Task<WaitBlock> LRobotTest()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(100, cancellationTokenSource.Token);
                    //cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    //code = 1;
                    //await Task.Delay(10000, cancellationTokenSource.Token);
                    //code = 2;

                    return new WaitBlock() { Code = 0 };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = 1, Message = ex.Message };
                }
            }, cancellationTokenSource.Token);
        }


        public Task<WaitBlock> GlueLineRobotTest()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(100, cancellationTokenSource.Token);
                    //cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    //code = 1;
                    //await Task.Delay(10000, cancellationTokenSource.Token);
                    //code = 2;

                    return new WaitBlock() { Code = 0 };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = 1, Message = ex.Message };
                }
            }, cancellationTokenSource.Token);
        }

        public Task<WaitBlock> GluePointRobotTest()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(100, cancellationTokenSource.Token);
                    //cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    //code = 1;
                    //await Task.Delay(10000, cancellationTokenSource.Token);
                    //code = 2;

                    return new WaitBlock() { Code = 0 };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = 1, Message = ex.Message };
                }
            }, cancellationTokenSource.Token);
        }

    }

    /*
     using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        MotionController mc = new MotionController();
        FunctionTest test = new FunctionTest();

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                test.Start();
                //mc.Connect();
                //mc.Setup();
                //motor = mc.MotorLX;
                //motor = mc.MotorGlueLineZ;
                //motor = mc.MotorLRotateLoad;
                //motor = mc.MotorGluePointZ;
                //motor = mc.MotorVZ;
                //motor = mc.MotorLZ;
                //motor = mc.MotorGlueLineX;
                //motor = mc.MotorGluePointY;
                //motor = mc.MotorGlueLineY;
                //motor = mc.MotorGluePointX;
                //motor = mc.MotorLY;
                //motor = mc.MotorVY;
                //motor = mc.MotorVX;
                //motor = mc.MotorWorkTable;
                //motor = mc.MotorLTrayLoad;

                //timer1.Enabled = true;
                button1.Text = "Home OK";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

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
        }

        private void button2_Click(object sender, EventArgs e)
        {         
            //BasicFunction.cancellationTokenSource.Cancel();
            //mc.Jog();
            //mc.P2P();
            //mc.Home();
            //mc.TestLineMove();
            //bool b = mc.GetState(motor, MotorState.Enabled);
            //if (!b)
            //{
            //    mc.Enable(motor);
            //}
            //else
            //{
            //    mc.Disable(motor);
            //}

            //mc.SetIO();

            //button2.Text = "" + mc.GetIO();
        }


        private Motor motor;
        private async void button3_Click(object sender, EventArgs e)
        {
            //mc.Stop();
            //mc.MoveToTarget(motor, 10);
            //mc.SetIO2();

            try
            {
                //mc.MoveToTargetTillEnd(motor, -20);
                //mc.Delay(5000);
                //mc.MoveToTargetTillEnd(motor, 0);
                //mc.Home(motor);
                //mc.MoveToTargetRelativeTillEnd(motor, 60);
                //Task.Run(()=> { mc.MoveToTargetRelativeTillEnd(motor, 50); });
                //mc.GetHomeSensor(motor);
                //mc.HomeAllMotors();

                string msg = "";

                await Task.Run(async() => {

                    while (true)
                    {
                        Task<WaitBlock> pointRobotTest = test.GluePointRobotTest();
                        

                        Task<WaitBlock> vRobotTest = test.VRobotTest();
                        

                        Task<WaitBlock> lRobotTest = test.LRobotTest();
                        

                        Task<WaitBlock> lineRobotTest = test.GlueLineRobotTest();

                        await lineRobotTest;
                        await pointRobotTest;
                        await vRobotTest;
                        await lRobotTest;

                        if (pointRobotTest.Result.Code!=0 ||
                        vRobotTest.Result.Code != 0 ||
                        lRobotTest.Result.Code != 0 ||
                        lineRobotTest.Result.Code != 0
                        )
                        {
                            msg = "";
                            msg += vRobotTest.Result.Code.ToString() + vRobotTest.Result.Message.ToString();
                            msg += lRobotTest.Result.Code.ToString() + lRobotTest.Result.Message.ToString();
                            msg += lineRobotTest.Result.Code.ToString() + lineRobotTest.Result.Message.ToString();
                            msg += pointRobotTest.Result.Code.ToString() + pointRobotTest.Result.Message.ToString();
                            break;
                        }

                        test.table.Turns();
                    }
                    


                });

                label2.Text = msg;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button4_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                mc.Jog(motor, 5, MoveDirection.Negative);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button4_MouseUp(object sender, MouseEventArgs e)
        {
            mc.Stop(motor);
        }

        private void button5_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                mc.Jog(motor, 5, MoveDirection.Positive);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button5_MouseUp(object sender, MouseEventArgs e)
        {
            mc.Stop(motor);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = mc.GetPosition(motor).ToString();
        }
    }
}

     */
}
