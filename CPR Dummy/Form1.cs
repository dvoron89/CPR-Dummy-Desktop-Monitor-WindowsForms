using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Policy;
using System.Data.SQLite;
using System.Media;
using System.Drawing.Printing;

namespace ECG_PreProduction
{
    public partial class Form1 : Form
    {
        // TCP CONNECTION
        public static IPAddress IP = IPAddress.Parse("192.168.88.88");
        public static int Port = 8888;
        public static TcpClient TCPClient = new TcpClient();
        public static NetworkStream NWStream;

        // SERIAL
        SerialPort CurrentPort;

        //SQLITE
        SQLiteConnection DBConnection;

        // DATETIMES
        public DateTime TimeDrawingStarted;

        // INTs
        public int FormWidth;
        public int FormHeight;
        public int PictureBoxWidth;
        public int PictureBoxHeight;
        public int TrainingPictureBoxHeight;
        public int TrainingPictureBoxWidth;

        public int StartIndex;
        public int EndIndex;

        public int YellowZoneCount;
        public int GreenZoneCount;
        public int RedZoneCount;
        public int TotalECGCount;

        public int LastXPos = 0;
        public int PointIndex = 1;

        public int PulseTriesTotal = 0;
        public int PulseTriesCorrect = 0;
        public int PulseTriesOver = 0;
        public int PulseTriesUnder = 0;

        public int VentsTriesTotal = 0;
        public int VentsTriesCorrect = 0;
        public int VentsTriesOver = 0;
        public int VentsTriesUnder = 0;


        // STRINGs
        public string ECGString;

        public string DataBaseName = "\\Data.db";
        public string DummyOnlineSoundName = "\\dummyonline.snd";
        public string AppLocation;
        public string DBPath;


        // BOOLs
        public bool ECGStarts = false;
        public bool ECGEnds = false;
        public bool SerialPortActive = false;
        public bool WiFiActive = false;
        public bool DummyConnected = false;
        public bool DrawingInProgress = false;
        public bool ExamStarted = false;

        // LISTs
        public List<int> IncomingData = new List<int>();
        public List<int> PulseList = new List<int>();
        public List<int> BreathList = new List<int>();
        public List<string> ReadStreamList = new List<string>();
        public List<int> HeartPointsList = new List<int>();
        public List<int> LungsPointsList = new List<int>();
        public List<float> DistancePointsList = new List<float>();
        public List<float> PressurePointsList = new List<float>();
        public List<int> HealthyECGPoints = new List<int>();
        public List<int> HealthyPulsePoints = new List<int>();
        public List<int> HealthyBreathPoints = new List<int>();

        // THREADs
        public Thread ThreadIncomingData;
        public Thread ThreadDrawECG;
        public Thread ThreadReadStream;
        public Thread ThreadSortData;
        public Thread ThreadWiFiStreamReader;
        public Thread ThreadVisualizeStreamData;
        public Thread ThreadSerialStreamReader;
        public Thread ThreadCustomVisualizer;
        public Thread ThreadDrawHealthyECG;
        public Thread ThreadDrawHealthyECGLongTerm;
        public Thread ThreadDrawStreamData;

        public Thread ThreadSoundBeeps;

        // GRAPHICs
        public Graphics ECGGraphics;
        public Graphics PulseGraphics;
        public Graphics BreathGraphics;

        // PENs
        public Pen PenDarkNight = new Pen(Color.FromArgb(0, 0, 20), 2);

        // BRUSHes
        public Brush BrushDarkNight = new SolidBrush(Color.FromArgb(0, 0, 20));
        public Brush BrushDarkBlue = new SolidBrush(Color.FromArgb(20, 20, 50));

        //SOUNDs
        public SoundPlayer DummyOnlineSound = new SoundPlayer(Application.StartupPath + "\\DummyOnline.wav");
        public SoundPlayer DummyOfflineSound = new SoundPlayer(Application.StartupPath + "\\DummyOffline.wav");
        public SoundPlayer BeepSound01 = new SoundPlayer(Application.StartupPath + "\\beep01.wav");
        public SoundPlayer BeepSound02 = new SoundPlayer(Application.StartupPath + "\\beep02.wav");
        public SoundPlayer StartVentsSound = new SoundPlayer(Application.StartupPath + "\\StartVents.wav");
        public SoundPlayer StartCPRSound = new SoundPlayer(Application.StartupPath + "\\StartCPR.wav");
        public SoundPlayer CPRSuccessSound = new SoundPlayer(Application.StartupPath + "\\CPRSuccess.wav");
        public SoundPlayer CPRFailedSound = new SoundPlayer(Application.StartupPath + "\\CPRFailed.wav");

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FormSetUp();
            GetDBPath();
        }


        // METHODS FOR FORM

        void ResetStartScreen() // Set start screen to original state
        {
            ButtonWiredConnection.Invoke(new Action(() => ButtonWiredConnection.Enabled = true));
            ButtonWirelessConnection.Invoke(new Action(() => ButtonWirelessConnection.Enabled = true));
            labelConnectionStatus.Invoke(new Action(() => labelConnectionStatus.Text = "Отсутствует"));
            labelConnectionStatus.Invoke(new Action(() => labelConnectionStatus.ForeColor = Color.Red));
        }

        void ResetTrainingScreen() // Set training screen to original state
        {
            labelHeartBeats.Invoke(new Action(() => labelHeartBeats.Text = "XX"));
            labelPulse.Invoke(new Action(() => labelPulse.Text = "XX"));
            labelVents.Invoke(new Action(() => labelVents.Text = "XX"));
            labelBloodPressure.Invoke(new Action(() => labelBloodPressure.Text = "XX"));

            // BEATS
            labelCHPS.Invoke(new Action(() => labelCHPS.Text = "XX"));
            labelCHNS.Invoke(new Action(() => labelCHNS.Text = "XX"));
            labelCHIS.Invoke(new Action(() => labelCHIS.Text = "XX"));
            labelCHSS.Invoke(new Action(() => labelCHSS.Text = "XX"));
            labelPS.Invoke(new Action(() => labelPS.Text = "XX"));
            labelNS.Invoke(new Action(() => labelNS.Text = "XX"));
            labelIS.Invoke(new Action(() => labelIS.Text = "XX"));

            //VENTS
            labelCHPV.Invoke(new Action(() => labelCHPV.Text = "XX"));
            labelCHNV.Invoke(new Action(() => labelCHNV.Text = "XX"));
            labelCHIV.Invoke(new Action(() => labelCHIV.Text = "XX"));
            labelCHVL.Invoke(new Action(() => labelCHVL.Text = "XX"));
            labelPV.Invoke(new Action(() => labelPV.Text = "XX"));
            labelNV.Invoke(new Action(() => labelNV.Text = "XX"));
            labelIV.Invoke(new Action(() => labelIV.Text = "XX"));

            // OTHERS
            labelVSLR.Invoke(new Action(() => labelVSLR.Text = "XX"));
            labelCHPSCHPV.Invoke(new Action(() => labelCHPSCHPV.Text = "XX"));

            textBoxStudentsName.Invoke(new Action(() => textBoxStudentsName.Text = string.Empty));
            textBoxStudentsFaculty.Invoke(new Action(() => textBoxStudentsFaculty.Text = string.Empty));
            textBoxStudentsGroup.Invoke(new Action(() => textBoxStudentsGroup.Text = string.Empty));

            numericUpDownCPRTime.Invoke(new Action(() => numericUpDownCPRTime.Value = 300));
            numericUpDownCHSS.Invoke(new Action(() => numericUpDownCHSS.Value = 60));

            textBoxStudentsName.Invoke(new Action(() => textBoxStudentsName.Enabled = true));
            textBoxStudentsFaculty.Invoke(new Action(() => textBoxStudentsFaculty.Enabled = true));
            textBoxStudentsGroup.Invoke(new Action(() => textBoxStudentsGroup.Enabled = true));

            numericUpDownCPRTime.Invoke(new Action(() => numericUpDownCPRTime.Enabled = true));
            numericUpDownCHSS.Invoke(new Action(() => numericUpDownCHSS.Enabled = true));
            numericUpDownMinPS.Invoke(new Action(() => numericUpDownMinPS.Enabled = true));
            numericUpDownMinPV.Invoke(new Action(() => numericUpDownMinPV.Enabled = true));

            buttonStartTraining.Invoke(new Action(() => buttonStartTraining.Enabled = true));
            buttonStartExam.Invoke(new Action(() => buttonStartExam.Enabled = true));
            buttonStopTraining.Invoke(new Action(() => buttonStopTraining.Enabled = false));

            PulseGraphics.FillRectangle(BrushDarkNight, 0, 0, TrainingPictureBoxWidth, TrainingPictureBoxHeight);
            ECGGraphics.FillRectangle(BrushDarkNight, 0, 0, TrainingPictureBoxWidth, TrainingPictureBoxHeight);
            BreathGraphics.FillRectangle(BrushDarkNight, 0, 0, TrainingPictureBoxWidth, TrainingPictureBoxHeight);
        }

        void ResetStatsVariables() // Set statistics at training sceen to original state
        {
            PulseTriesTotal = 0;
            PulseTriesCorrect = 0;
            PulseTriesOver = 0;
            PulseTriesUnder = 0;

            VentsTriesTotal = 0;
            VentsTriesCorrect = 0;
            VentsTriesOver = 0;
            VentsTriesUnder = 0;

        }

        void DisableTrainingControls() // Disables controls at training screen
        {
            textBoxStudentsName.Enabled = false;
            textBoxStudentsGroup.Enabled = false;
            textBoxStudentsFaculty.Enabled = false;
            numericUpDownCPRTime.Enabled = false;
            numericUpDownCHSS.Enabled = false;
            numericUpDownMinPS.Enabled = false;
            numericUpDownMinPV.Enabled = false;
        }

        private void FormSetUp() // Runs at start. Setting VARs and preparing to work
        {
            ThreadIncomingData = new Thread(new ThreadStart(ImcomingDataSimulation));
            ThreadReadStream = new Thread(new ThreadStart(ReadStreamSimulation));
            ThreadSortData = new Thread(new ThreadStart(SortStreamData));
            ThreadWiFiStreamReader = new Thread(new ThreadStart(WiFiStreamReader));
            ThreadSerialStreamReader = new Thread(new ThreadStart(SerialStreamReader));
            ThreadDrawHealthyECG = new Thread(new ThreadStart(DrawHealthyECG));
            ThreadDrawHealthyECGLongTerm = new Thread(new ThreadStart(DrawHealthyECGLongTerm));
            ThreadDrawStreamData = new Thread(new ThreadStart(DrawStreamData));

            ThreadSoundBeeps = new Thread(new ThreadStart(SoundBeeps));

            AppLocation = Application.StartupPath;

            //ECGGraphics = ECG_Box.CreateGraphics();
            //PulseGraphics = Pulse_Box.CreateGraphics();
            //BreathGraphics = Breathe_Box.CreateGraphics();

            //TrainingHeartBeatsGraphics = pictureBoxECG.CreateGraphics();

            ECGGraphics = pictureBoxECG.CreateGraphics();
            PulseGraphics = pictureBoxPulse.CreateGraphics();
            BreathGraphics = pictureBoxVents.CreateGraphics();

            GetControlSizes();

            ResetStartScreen();
            ResetTrainingScreen();

            StartVentsSound.Load();
            BeepSound01.Load();
            BeepSound02.Load();
            StartCPRSound.Load();
            CPRSuccessSound.Load();
            CPRFailedSound.Load();
            DummyOnlineSound.Load();
            DummyOfflineSound.Load();
        }

        private void GetControlSizes() // Get sizes of control forms (picture boxes)
        {
            FormWidth = this.Width;
            FormHeight = this.Height;
            //PictureBoxWidth = ECG_Box.Width;
            //PictureBoxHeight = ECG_Box.Height;
            TrainingPictureBoxHeight = pictureBoxECG.Height;
            TrainingPictureBoxWidth = pictureBoxECG.Width;
        }


        // METHODS TO CONNECT AND READ STREAM

        void EstablishWiFiConnection() // Connect to dummy via WiFi
        {
            labelConnectionStatus.Invoke(new Action(() => labelConnectionStatus.Text = "Попытка установить беспроводное подключение"));
            try
            {
                TCPClient.Connect(IP, Port);
                NWStream = TCPClient.GetStream();
                DummyConnected = true;
            }
            catch (Exception)
            {
            }
            if (DummyConnected)
            {
                ButtonWiredConnection.Enabled = false;
                ButtonWirelessConnection.Enabled = false;
                labelConnectionStatus.Text = "Беспроводное соединение установлено";
                labelConnectionStatus.ForeColor = Color.GreenYellow;
                if (ThreadWiFiStreamReader.ThreadState == ThreadState.Suspended)
                {
                    ThreadWiFiStreamReader.Resume();
                }
                else
                {
                    ThreadWiFiStreamReader.Start();
                }
                DummyOnlineSound.Play();
            }
            else
            {
                ResetStartScreen();
                MessageBox.Show("Не удалось установить беспроводное соединение\n" +
                    "Убедитесь, что вы следовали инструкциям по беспроводному подключению");
            }
        }

        void EstablishSerialPortConnection() // Connect to dummy via USB cable and start reading thread
        {
            string[] SerialPortsNames = SerialPort.GetPortNames();
            //string[] SerialPortsNames = new string[] { "COM1", "COM2", "COM3", "COM4", "COM5" };
            string TestLine;

            foreach (string Port in SerialPortsNames)
            {
                CurrentPort = new SerialPort(Port, 9600)
                {
                    ReadTimeout = 2500
                };
                labelConnectionStatus.Invoke(new Action(() => labelConnectionStatus.Text = "Попытка установить проводное подключение"));

                try
                {
                    CurrentPort.Open();
                    //CurrentPort.DiscardInBuffer();
                    Thread.Sleep(1000);
                    TestLine = CurrentPort.ReadLine();
                    CurrentPort.Close();

                    if (TestLine.Trim().StartsWith("_") && TestLine.Trim().EndsWith(";"))
                    {
                        DummyConnected = true;
                        //SerialPortActive = true;
                        CurrentPort.Open();
                    }
                }
                catch (Exception)
                {
                }
            }

            if (DummyConnected)
            {
                ButtonWiredConnection.Enabled = false;
                ButtonWirelessConnection.Enabled = false;
                labelConnectionStatus.Text = "Проводное соединение установлено";
                labelConnectionStatus.ForeColor = Color.GreenYellow;

                if (ThreadSerialStreamReader.ThreadState == ThreadState.Suspended)
                {
                    ThreadSerialStreamReader.Resume();
                }
                else
                {
                    ThreadSerialStreamReader.Start();
                }
                DummyOnlineSound.Play();
            }
            else
            {
                ResetStartScreen();
                MessageBox.Show("Не удалось установить проводное соединение\n" +
                    "Убедитесь, что кабель подключен и повторите попытку");
            }
        }

        void WiFiStreamReader() // Start reading data from dummy via WiFi
        {
            string Line = string.Empty;
            bool LineStarted = false;

            while (DummyConnected)
            {
                byte[] buffer = new byte[1];
                try
                {
                    NWStream.Read(buffer, 0, buffer.Length);
                }
                catch (Exception)
                {
                    ConnectionLost();
                    ThreadWiFiStreamReader.Suspend();
                }
                char IncomingChar = Encoding.UTF8.GetChars(buffer)[0];

                if (IncomingChar == '_')
                {
                    LineStarted = true;
                }

                while (LineStarted)
                {
                    try
                    {
                        NWStream.Read(buffer, 0, buffer.Length);
                    }
                    catch (Exception)
                    {
                        ConnectionLost();
                        ThreadWiFiStreamReader.Suspend();
                    }

                    IncomingChar = Encoding.UTF8.GetChars(buffer)[0];

                    if (IncomingChar != ';')
                    {
                        Line += IncomingChar;
                    }
                    else if (IncomingChar == ';')
                    {
                        LineStarted = false;

                        Line = Line.Trim(' ', ';');

                        if (Line.StartsWith("D") && DrawingInProgress)
                        {
                            Line = Line.Trim('D');
                            DistancePointsList.Add(float.Parse(Line, System.Globalization.CultureInfo.InvariantCulture));
                        }
                        else if (Line.StartsWith("P") && DrawingInProgress)
                        {
                            Line = Line.Trim('P');
                            PressurePointsList.Add(float.Parse(Line, System.Globalization.CultureInfo.InvariantCulture));
                        }

                        Line = string.Empty;
                    }
                }
            }
        }

        void SerialStreamReader() // Start reading data from dummy via USB cable
        {
            string Line = string.Empty;

            while (DummyConnected) // Read data from USB as long as connection is available
            {
                // Stop this thread if connection is lost
                try
                {
                    Line = CurrentPort.ReadLine();
                }
                catch (Exception)
                {
                    ConnectionLost();
                    ThreadSerialStreamReader.Suspend();
                }

                Line = Line.Trim();

                if (Line.StartsWith("_") && Line.EndsWith(";") && DrawingInProgress) // Add data to lists only during training and exam
                {
                    Line = Line.Trim('_', ';');

                    if (Line.StartsWith("D"))
                    {
                        Line = Line.Trim('D');

                        try
                        {
                            DistancePointsList.Add(float.Parse(Line, System.Globalization.CultureInfo.InvariantCulture));
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("D cant be added");
                        }
                    }
                    else if (Line.StartsWith("P"))
                    {
                        Line = Line.Trim('P');

                        try
                        {
                            PressurePointsList.Add(float.Parse(Line, System.Globalization.CultureInfo.InvariantCulture));
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("P cant be added");
                        }
                    }
                }
            }
        }

        void ConnectionLost()
        {
            DrawingInProgress = false;
            ExamStarted = false;
            LastXPos = 0;
            PointIndex = 1;

            DummyConnected = false; // instead of IF check for ThreadDrawStreamReader state

            if (ThreadDrawHealthyECG.ThreadState != ThreadState.Suspended && ThreadDrawHealthyECG.ThreadState != ThreadState.Unstarted)
            {
                ThreadDrawHealthyECG.Suspend();
            }
            if (ThreadSoundBeeps.ThreadState != ThreadState.Suspended && ThreadSoundBeeps.ThreadState != ThreadState.Unstarted)
            {
                ThreadSoundBeeps.Suspend();
            }
            if (ThreadDrawHealthyECGLongTerm.ThreadState != ThreadState.Suspended && ThreadDrawHealthyECGLongTerm.ThreadState != ThreadState.Unstarted)
            {
                try
                {
                    ThreadDrawHealthyECGLongTerm.Suspend();
                }
                catch (Exception)
                {
                }
            }
            if (DistancePointsList.Count() > 2)
            {
                DistancePointsList.RemoveRange(0, DistancePointsList.Count() - 2);
            }
            if (PressurePointsList.Count() > 2)
            {
                PressurePointsList.RemoveRange(0, PressurePointsList.Count() - 2);
            }

            ResetStatsVariables();
            ResetTrainingScreen();
            DummyOfflineSound.Play();
            DummyConnected = false;
            ResetStartScreen();
            MessageBox.Show("Соединение с тренажером разорвано\n" +
                "Установите соединение повторно");
        }


        // METHODS TO PREPARE AND VISUALIZE HEALTHY GRAPHS

        void SetUpHealthyStats() // Setup data for drawing training graphs
        {
            HealthyECGPoints.Clear();
            HealthyBreathPoints.Clear();
            HealthyPulsePoints.Clear();
            //int HeartBeatRate = 60;
            int HeartBeatRate = (int)numericUpDownCHSS.Value;
            int BreathRate = (int)Math.Round(HeartBeatRate / 4.3F);

            int HeartBeatPointsCounter = 60 * 20 / HeartBeatRate;
            int BreathPointsCounter = 60 * 20 / BreathRate;


            labelHeartBeats.Invoke(new Action(() => labelHeartBeats.Text = HeartBeatRate.ToString()));
            labelPulse.Invoke(new Action(() => labelPulse.Text = HeartBeatRate.ToString()));
            labelVents.Invoke(new Action(() => labelVents.Text = BreathRate.ToString()));
            labelBloodPressure.Invoke(new Action(() => labelBloodPressure.Text = "120/80"));

            if (HeartBeatPointsCounter <= 7)
            {
                HealthyECGPoints.Add(70);
                HealthyECGPoints.Add(65);
                HealthyECGPoints.Add(0);
                HealthyECGPoints.Add(100);
                HealthyECGPoints.Add(60);
                HealthyECGPoints.Add(70);

                HealthyPulsePoints.Add(90);
                HealthyPulsePoints.Add(85);
                HealthyPulsePoints.Add(10);
                HealthyPulsePoints.Add(50);
                HealthyPulsePoints.Add(60);
                HealthyPulsePoints.Add(90);
            }
            else
            {
                HealthyECGPoints.Add(70);
                HealthyECGPoints.Add(65);
                HealthyECGPoints.Add(70);
                HealthyECGPoints.Add(0);
                HealthyECGPoints.Add(100);
                HealthyECGPoints.Add(70);
                HealthyECGPoints.Add(60);
                HealthyECGPoints.Add(70);

                HealthyPulsePoints.Add(90);
                HealthyPulsePoints.Add(70);
                HealthyPulsePoints.Add(20);
                HealthyPulsePoints.Add(10);
                HealthyPulsePoints.Add(20);
                HealthyPulsePoints.Add(50);
                HealthyPulsePoints.Add(60);
                HealthyPulsePoints.Add(90);

                for (int i = 8; i < HeartBeatPointsCounter; i++)
                {
                    HealthyECGPoints.Add(70);
                    HealthyPulsePoints.Add(90);
                }
            }

            for (int i = -13; i <= 13; i++)
            {
                HealthyBreathPoints.Add((int)(i * i / 1.69));
            }
            for (int i = HealthyBreathPoints.Count; i < BreathPointsCounter; i++)
            {
                HealthyBreathPoints.Add(100);
            }
        }

        void DrawHealthyECG() // Draw all HEALTHY graphs
        {
            int lastBreathPoint = 0;
            int PointsCounter;
            int counter = 0;
            int startY;
            int endY;

            Point startPoint;
            Point endPoint;

            while (true)
            {
                PointsCounter = HealthyECGPoints.Count();

                if (LastXPos >= TrainingPictureBoxWidth)
                {
                    ECGGraphics.FillRectangle(BrushDarkNight, 0, 0, TrainingPictureBoxWidth, TrainingPictureBoxHeight);
                    PulseGraphics.FillRectangle(BrushDarkNight, 0, 0, TrainingPictureBoxWidth, TrainingPictureBoxHeight);
                    BreathGraphics.FillRectangle(BrushDarkNight, 0, 0, TrainingPictureBoxWidth, TrainingPictureBoxHeight);
                    LastXPos = 0;
                }

                if (LastXPos >= TrainingPictureBoxWidth / 2)
                {
                    labelPulse.Invoke(new Action(() => labelPulse.Text = "0"));
                    labelVents.Invoke(new Action(() => labelVents.Text = "0"));

                    if (ThreadDrawStreamData.ThreadState == ThreadState.Suspended)
                    {
                        ThreadDrawStreamData.Resume();
                    }
                    else
                    {
                        ThreadDrawStreamData.Start();
                    }

                    if (!ExamStarted)
                    {
                        if (ThreadSoundBeeps.ThreadState == ThreadState.Unstarted)
                        {
                            ThreadSoundBeeps.Start();
                        }
                        else if (ThreadSoundBeeps.ThreadState == ThreadState.Suspended)
                        {
                            ThreadSoundBeeps.Resume();
                        }
                    }

                    counter = 0;
                    lastBreathPoint = 0;
                    PointsCounter = 0;
                    DrawingInProgress = true;
                    TimeDrawingStarted = DateTime.Now;
                    ThreadDrawHealthyECG.Suspend();

                }

                // ECG graphics
                startY = HealthyECGPoints[counter] * TrainingPictureBoxHeight / 100;
                endY = HealthyECGPoints[counter + 1] * TrainingPictureBoxHeight / 100;
                startPoint = new Point(LastXPos, startY);
                endPoint = new Point(LastXPos + 5, endY);
                ECGGraphics.DrawLine(new Pen(Color.GreenYellow, 3), startPoint, endPoint);

                // Pulse graphics
                startY = HealthyPulsePoints[counter] * TrainingPictureBoxHeight / 100;
                endY = HealthyPulsePoints[counter + 1] * TrainingPictureBoxHeight / 100;
                startPoint = new Point(LastXPos, startY);
                endPoint = new Point(LastXPos + 5, endY);
                PulseGraphics.DrawLine(new Pen(Color.Cyan, 3), startPoint, endPoint);

                // Breath graphics
                if (lastBreathPoint >= HealthyBreathPoints.Count() - 1)
                {
                    lastBreathPoint = 0;
                }
                startY = HealthyBreathPoints[lastBreathPoint] * TrainingPictureBoxHeight / 100;
                endY = HealthyBreathPoints[lastBreathPoint + 1] * TrainingPictureBoxHeight / 100;
                startPoint = new Point(LastXPos, startY);
                endPoint = new Point(LastXPos + 5, endY);
                BreathGraphics.DrawLine(new Pen(Color.Yellow, 3), startPoint, endPoint);
                lastBreathPoint++;

                counter++;

                if (counter == PointsCounter - 1)
                {
                    counter = 0;
                }

                LastXPos += 5;

                Thread.Sleep(50);

                if (!WiFiActive && !SerialPortActive && !DummyConnected)
                {
                    ResetTrainingScreen();
                    ThreadDrawHealthyECG.Suspend();
                }
            }
        }

        void DrawHealthyECGLongTerm() // Draw all HEALTHY graphs without pausing
        {
            int lastBreathPoint = 0;
            int PointsCounter;
            int counter = 0;
            int startY;
            int endY;

            Point startPoint;
            Point endPoint;

            while (true)
            {
                PointsCounter = HealthyECGPoints.Count();

                if (LastXPos >= TrainingPictureBoxWidth)
                {
                    ECGGraphics.FillRectangle(BrushDarkNight, 0, 0, TrainingPictureBoxWidth, TrainingPictureBoxHeight);
                    PulseGraphics.FillRectangle(BrushDarkNight, 0, 0, TrainingPictureBoxWidth, TrainingPictureBoxHeight);
                    BreathGraphics.FillRectangle(BrushDarkNight, 0, 0, TrainingPictureBoxWidth, TrainingPictureBoxHeight);
                    LastXPos = 0;
                }

                // ECG graphics
                startY = HealthyECGPoints[counter] * TrainingPictureBoxHeight / 100;
                endY = HealthyECGPoints[counter + 1] * TrainingPictureBoxHeight / 100;
                startPoint = new Point(LastXPos, startY);
                endPoint = new Point(LastXPos + 5, endY);
                ECGGraphics.DrawLine(new Pen(Color.GreenYellow, 3), startPoint, endPoint);

                // Pulse graphics
                startY = HealthyPulsePoints[counter] * TrainingPictureBoxHeight / 100;
                endY = HealthyPulsePoints[counter + 1] * TrainingPictureBoxHeight / 100;
                startPoint = new Point(LastXPos, startY);
                endPoint = new Point(LastXPos + 5, endY);
                PulseGraphics.DrawLine(new Pen(Color.Cyan, 3), startPoint, endPoint);

                // Breath graphics
                if (lastBreathPoint >= HealthyBreathPoints.Count() - 1)
                {
                    lastBreathPoint = 0;
                }
                startY = HealthyBreathPoints[lastBreathPoint] * TrainingPictureBoxHeight / 100;
                endY = HealthyBreathPoints[lastBreathPoint + 1] * TrainingPictureBoxHeight / 100;
                startPoint = new Point(LastXPos, startY);
                endPoint = new Point(LastXPos + 5, endY);
                BreathGraphics.DrawLine(new Pen(Color.Yellow, 3), startPoint, endPoint);
                lastBreathPoint++;

                counter++;

                if (counter == PointsCounter - 1)
                {
                    counter = 0;
                }

                LastXPos += 5;

                Thread.Sleep(50);

                if (!WiFiActive && !SerialPortActive && !DummyConnected)
                {
                    ResetTrainingScreen();
                    ThreadDrawHealthyECG.Suspend();
                }
            }

        }


        // METHODS TO VISUALIZE DATA 

        void DrawStreamData() // Start drawing all graphs based on incoming data
        {
            int StartY;
            int EndY;

            float DistanceDeadPoint = 150.0F;
            float DistanceMax = 75.0F;
            float DistancePassed;
            float DistancePercent;
            float ECGPeakPoint = 0;

            float PressureMax = 50.0F;
            float PressurePercent;
            float Pressure;
            float VentPeakPoint = 0;

            Point StartPoint;
            Point EndPoint;

            DateTime PulseStartTime = DateTime.Now;
            DateTime PulseEndTime;
            DateTime VentStartTime = DateTime.Now;
            DateTime VentEndTime;

            bool PulseStartPointReached = false;
            bool PulsePeakPointReached = false;
            bool VentStartPointReached = false;
            bool VentPeakPointReached = false;

            while (true)
            {
                if (!DummyConnected) // In case of lost connection
                {
                    // stop this thread, reset screen, reset points lists
                    ThreadDrawStreamData.Suspend();
                }

                if (DistancePointsList.Count() > PointIndex && PressurePointsList.Count() > PointIndex)
                {
                    // if true, go on drawing graphs
                    if (LastXPos >= TrainingPictureBoxWidth)
                    {
                        ECGGraphics.FillRectangle(BrushDarkNight, 0, 0, TrainingPictureBoxWidth, TrainingPictureBoxHeight);
                        PulseGraphics.FillRectangle(BrushDarkNight, 0, 0, TrainingPictureBoxWidth, TrainingPictureBoxHeight);
                        BreathGraphics.FillRectangle(BrushDarkNight, 0, 0, TrainingPictureBoxWidth, TrainingPictureBoxHeight);

                        // Drawing safe zones
                        PulseGraphics.FillRectangle(
                            BrushDarkBlue,
                            0,
                            TrainingPictureBoxHeight * 20 / 100,
                            TrainingPictureBoxWidth, TrainingPictureBoxHeight * 27 / 100);
                        BreathGraphics.FillRectangle
                            (BrushDarkBlue,
                            0,
                            TrainingPictureBoxHeight * 10 / 100,
                            TrainingPictureBoxWidth, TrainingPictureBoxHeight * 20 / 100);

                        LastXPos = 0;
                    }

                    #region Distance points to draw

                    DistancePassed = DistanceDeadPoint - DistancePointsList[PointIndex - 1];
                    if (DistancePassed < 0)
                    {
                        DistancePassed = 0;
                    }
                    DistancePercent = DistancePassed / DistanceMax * 100;

                    StartY = Convert.ToInt32(DistancePercent * TrainingPictureBoxHeight / 100);

                    DistancePassed = DistanceDeadPoint - DistancePointsList[PointIndex];
                    if (DistancePassed < 0)
                    {
                        DistancePassed = 0;
                    }
                    DistancePercent = DistancePassed / DistanceMax * 100;

                    EndY = Convert.ToInt32(DistancePercent * TrainingPictureBoxHeight / 100);

                    StartPoint = new Point(LastXPos, Math.Abs(StartY - TrainingPictureBoxHeight));
                    EndPoint = new Point(LastXPos + 5, Math.Abs(EndY - TrainingPictureBoxHeight));

                    PulseGraphics.DrawLine(new Pen(Color.Cyan, 3), StartPoint, EndPoint);

                    #endregion

                    #region Distance peak point

                    if (DistancePassed <= 15.0F && !PulseStartPointReached)
                    {
                        // dead zone reached
                        PulseStartPointReached = true;
                        PulseStartTime = DateTime.Now;
                    }
                    if (DistancePassed > 15.0F && PulseStartPointReached)
                    {
                        // out of dead zone, increasing or decreasing
                        PulsePeakPointReached = true;
                        if (DistancePassed > ECGPeakPoint)
                        {
                            ECGPeakPoint = DistancePassed;
                        }
                    }
                    if (DistancePassed <= 15.0F && PulseStartPointReached && PulsePeakPointReached) // and was in peak before
                    {
                        // dead zone reached second time, iteration complete
                        PulseEndTime = DateTime.Now;
                        float Seconds = (float)(PulseEndTime - PulseStartTime).TotalSeconds;
                        int BeatsPerMinute = Convert.ToInt32(60.0F / Seconds);
                        labelPulse.Invoke(new Action(() => labelPulse.Text = BeatsPerMinute.ToString()));

                        PulseStartPointReached = false;
                        PulsePeakPointReached = false;

                        PulseTriesTotal++;

                        if (ECGPeakPoint <= 40.0F)
                        {
                            PulseTriesUnder++;
                        }
                        else if (ECGPeakPoint <= 60.0F)
                        {
                            PulseTriesCorrect++;
                        }
                        else if (ECGPeakPoint <= 100.0F)
                        {
                            PulseTriesOver++;
                        }

                        ECGPeakPoint = 0;

                        #region Set beats stats

                        labelCHPS.Invoke(new Action(() => labelCHPS.Text = PulseTriesCorrect.ToString()));
                        labelCHNS.Invoke(new Action(() => labelCHNS.Text = PulseTriesUnder.ToString()));
                        labelCHIS.Invoke(new Action(() => labelCHIS.Text = PulseTriesOver.ToString()));
                        //labelCHSS.Invoke(new Action(() => labelCHSS.Text = "вроде то же самое что пульс хз"));

                        if (PulseTriesTotal > 0)
                        {
                            labelPS.Invoke(new Action(() => labelPS.Text = (PulseTriesCorrect * 100 / PulseTriesTotal).ToString() + "%"));
                            labelNS.Invoke(new Action(() => labelNS.Text = (PulseTriesUnder * 100 / PulseTriesTotal).ToString() + "%"));
                            labelIS.Invoke(new Action(() => labelIS.Text = (PulseTriesOver * 100 / PulseTriesTotal).ToString() + "%"));

                        }

                        #endregion
                    }



                    #endregion

                    #region Pressure points to draw

                    Pressure = PressurePointsList[PointIndex - 1];
                    if (Pressure > PressureMax)
                    {
                        Pressure = PressureMax;
                    }
                    PressurePercent = Pressure / PressureMax * 100;

                    StartY = Convert.ToInt32(PressurePercent * TrainingPictureBoxHeight / 100);

                    Pressure = PressurePointsList[PointIndex];
                    if (Pressure > PressureMax)
                    {
                        Pressure = PressureMax;
                    }
                    PressurePercent = Pressure / PressureMax * 100;

                    EndY = Convert.ToInt32(PressurePercent * TrainingPictureBoxHeight / 100);

                    StartPoint = new Point(LastXPos, Math.Abs(StartY - TrainingPictureBoxHeight));
                    EndPoint = new Point(LastXPos + 5, Math.Abs(EndY - TrainingPictureBoxHeight));

                    BreathGraphics.DrawLine(new Pen(Color.Yellow, 3), StartPoint, EndPoint);

                    #endregion

                    #region Pressure peak point

                    if (PressurePointsList[PointIndex] <= 4.0F && !VentStartPointReached)
                    {
                        // dead zone reached
                        VentStartPointReached = true;
                        VentStartTime = DateTime.Now;
                    }
                    if (PressurePointsList[PointIndex] > 4.0F && VentStartPointReached)
                    {
                        // out of dead zone, increasing or decreasing
                        VentPeakPointReached = true;
                        if (PressurePointsList[PointIndex] > VentPeakPoint)
                        {
                            VentPeakPoint = PressurePointsList[PointIndex];
                        }
                    }
                    if (PressurePointsList[PointIndex] <= 4.0F && VentStartPointReached && VentPeakPointReached) // and was in peak before
                    {
                        // dead zone reached second time, iteration complete
                        VentEndTime = DateTime.Now;
                        float Seconds = (float)(VentEndTime - VentStartTime).TotalSeconds;
                        int VentsPerMinute = Convert.ToInt32(60.0F / Seconds);
                        labelVents.Invoke(new Action(() => labelVents.Text = VentsPerMinute.ToString()));

                        VentStartPointReached = false;
                        VentPeakPointReached = false;

                        VentsTriesTotal++;

                        if (VentPeakPoint <= 35.0F)
                        {
                            VentsTriesUnder++;
                        }
                        else if (VentPeakPoint <= 45.0F)
                        {
                            VentsTriesCorrect++;
                        }
                        else if (VentPeakPoint <= 100.0F)
                        {
                            VentsTriesOver++;
                        }

                        VentPeakPoint = 0;

                        #region Set vents stats

                        labelCHPV.Invoke(new Action(() => labelCHPV.Text = VentsTriesCorrect.ToString()));
                        labelCHNV.Invoke(new Action(() => labelCHNV.Text = VentsTriesUnder.ToString()));
                        labelCHIV.Invoke(new Action(() => labelCHIV.Text = VentsTriesOver.ToString()));
                        //labelCHVL.Invoke(new Action(() => labelCHVL.Text = "а это число вентиляций?"));

                        if (VentsTriesTotal > 0)
                        {
                            labelPV.Invoke(new Action(() => labelPV.Text = (VentsTriesCorrect * 100 / VentsTriesTotal).ToString() + "%"));
                            labelNV.Invoke(new Action(() => labelNV.Text = (VentsTriesUnder * 100 / VentsTriesTotal).ToString() + "%"));
                            labelIV.Invoke(new Action(() => labelIV.Text = (VentsTriesOver * 100 / VentsTriesTotal).ToString() + "%"));
                        }

                        #endregion
                    }

                    #endregion

                    #region ECGDeadLine to draw

                    StartY = Convert.ToInt32(30 * TrainingPictureBoxHeight / 100);
                    EndY = Convert.ToInt32(30 * TrainingPictureBoxHeight / 100);

                    StartPoint = new Point(LastXPos, Math.Abs(StartY - TrainingPictureBoxHeight));
                    EndPoint = new Point(LastXPos + 5, Math.Abs(EndY - TrainingPictureBoxHeight));

                    ECGGraphics.DrawLine(new Pen(Color.GreenYellow, 3), StartPoint, EndPoint);

                    #endregion

                    labelHeartBeats.Invoke(new Action(() => labelHeartBeats.Text = "0"));
                    labelBloodPressure.Invoke(new Action(() => labelBloodPressure.Text = "0"));

                    int MinutesFromStart = (DateTime.Now - TimeDrawingStarted).Minutes;
                    int SecondsFromStart = (DateTime.Now - TimeDrawingStarted).Seconds;
                    float TotalSecondsFromStart = (float)(DateTime.Now - TimeDrawingStarted).TotalSeconds;
                    int CHSS = Convert.ToInt32(PulseTriesTotal / TotalSecondsFromStart * 60);
                    int CHVl = Convert.ToInt32(VentsTriesTotal / TotalSecondsFromStart * 60);
                    string TimerFromStart;
                    if (SecondsFromStart < 10)
                    {
                        TimerFromStart = MinutesFromStart + ":0" + SecondsFromStart;
                    }
                    else
                    {
                        TimerFromStart = MinutesFromStart + ":" + SecondsFromStart;
                    }

                    labelVSLR.Invoke(new Action(() => labelVSLR.Text = TimerFromStart));
                    labelCHSS.Invoke(new Action(() => labelCHSS.Text = CHSS.ToString()));
                    labelCHVL.Invoke(new Action(() => labelCHVL.Text = CHVl.ToString()));
                    if (PulseTriesTotal > 0 && VentsTriesTotal > 0)
                    {
                        labelCHPSCHPV.Invoke(new Action(() => labelCHPSCHPV.Text = (Math.Round((float)PulseTriesCorrect / (float)VentsTriesCorrect)).ToString()));
                    }

                    #region TIMER OUT

                    if (TotalSecondsFromStart >= (float)numericUpDownCPRTime.Value)
                    {
                        if (ExamStarted)
                        {
                            if (PulseTriesCorrect > 0 && (PulseTriesCorrect * 100 / PulseTriesTotal) >= numericUpDownMinPS.Value && (VentsTriesCorrect * 100 / VentsTriesTotal) >= numericUpDownMinPV.Value)
                            {
                                CPRSuccessSound.Play();
                                if (ThreadDrawHealthyECGLongTerm.ThreadState == ThreadState.Unstarted)
                                {
                                    ThreadDrawHealthyECGLongTerm.Start();
                                }
                                else
                                {
                                    ThreadDrawHealthyECGLongTerm.Resume();
                                }
                                // save data to db
                                DateTime Now = DateTime.Now;
                                string ExamDate = Now.ToString("yyyy-MM-dd HH:mm:ss");

                                DBConnection.Open();

                                using (IDbCommand DBCmd = DBConnection.CreateCommand())
                                {
                                    string SQLQuery = String.Format("INSERT INTO Exams (name, stfaculty, stgroup, datetime, CHPS, CHNS, CHIS, CHSS, PSpercent, NSpercent, ISpercent, CHPV, CHNV, CHIV, CHVL, PVpercent, NVpercent, IVpercent, VSLR, CHPSCHPV) " +
                                        "VALUES (\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\", \"{5}\", \"{6}\", \"{7}\", \"{8}\", \"{9}\", \"{10}\", \"{11}\", \"{12}\", \"{13}\", \"{14}\", \"{15}\", \"{16}\", \"{17}\", \"{18}\", \"{19}\")",
                                        textBoxStudentsName.Text,
                                        textBoxStudentsFaculty.Text,
                                        textBoxStudentsGroup.Text,
                                        ExamDate,
                                        labelCHPS.Text,
                                        labelCHNS.Text,
                                        labelCHIS.Text,
                                        labelCHSS.Text,
                                        labelPS.Text,
                                        labelNS.Text,
                                        labelIS.Text,
                                        labelCHPV.Text,
                                        labelCHNV.Text,
                                        labelCHIV.Text,
                                        labelCHVL.Text,
                                        labelPV.Text,
                                        labelNV.Text,
                                        labelIV.Text,
                                        numericUpDownCPRTime.Value,
                                        labelCHPSCHPV.Text);


                                    DBCmd.CommandText = SQLQuery;

                                    DBCmd.ExecuteNonQuery();
                                }

                                DBConnection.Close();

                            }
                            else
                            {
                                CPRFailedSound.Play();

                                // save data to db
                                DateTime Now = DateTime.Now;
                                string ExamDate = Now.ToString("yyyy-MM-dd HH:mm:ss");

                                DBConnection.Open();

                                using (IDbCommand DBCmd = DBConnection.CreateCommand())
                                {
                                    string SQLQuery = String.Format("INSERT INTO Exams (name, stfaculty, stgroup, datetime, CHPS, CHNS, CHIS, CHSS, PSpercent, NSpercent, ISpercent, CHPV, CHNV, CHIV, CHVL, PVpercent, NVpercent, IVpercent, VSLR, CHPSCHPV) " +
                                        "VALUES (\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\", \"{5}\", \"{6}\", \"{7}\", \"{8}\", \"{9}\", \"{10}\", \"{11}\", \"{12}\", \"{13}\", \"{14}\", \"{15}\", \"{16}\", \"{17}\", \"{18}\", \"{19}\")",
                                        textBoxStudentsName.Text,
                                        textBoxStudentsFaculty.Text,
                                        textBoxStudentsGroup.Text,
                                        ExamDate,
                                        labelCHPS.Text,
                                        labelCHNS.Text,
                                        labelCHIS.Text,
                                        labelCHSS.Text,
                                        labelPS.Text,
                                        labelNS.Text,
                                        labelIS.Text,
                                        labelCHPV.Text,
                                        labelCHNV.Text,
                                        labelCHIV.Text,
                                        labelCHVL.Text,
                                        labelPV.Text,
                                        labelNV.Text,
                                        labelIV.Text,
                                        numericUpDownCPRTime.Value,
                                        labelCHPSCHPV.Text);


                                    DBCmd.CommandText = SQLQuery;

                                    DBCmd.ExecuteNonQuery();
                                }

                                DBConnection.Close();

                                ResetTrainingScreen();
                            }
                            ExamStarted = false;
                        }
                        else
                        {
                            if (PulseTriesCorrect > 0 && (PulseTriesCorrect * 100 / PulseTriesTotal) >= numericUpDownMinPS.Value && (VentsTriesCorrect * 100 / VentsTriesTotal) >= numericUpDownMinPV.Value)
                            {
                                CPRSuccessSound.Play();
                                if (ThreadDrawHealthyECGLongTerm.ThreadState == ThreadState.Unstarted)
                                {
                                    ThreadDrawHealthyECGLongTerm.Start();
                                }
                                else
                                {
                                    ThreadDrawHealthyECGLongTerm.Resume();
                                }
                            }
                            else
                            {
                                CPRFailedSound.Play();
                                ResetTrainingScreen();
                            }
                        }

                        ResetStatsVariables();

                        PulseGraphics.FillRectangle(BrushDarkNight, 0, 0, TrainingPictureBoxWidth, TrainingPictureBoxHeight);
                        ECGGraphics.FillRectangle(BrushDarkNight, 0, 0, TrainingPictureBoxWidth, TrainingPictureBoxHeight);
                        BreathGraphics.FillRectangle(BrushDarkNight, 0, 0, TrainingPictureBoxWidth, TrainingPictureBoxHeight);


                        DrawingInProgress = false;
                        LastXPos = 0;
                        PointIndex = 1;

                        if (DistancePointsList.Count() > 2)
                        {
                            DistancePointsList.RemoveRange(0, DistancePointsList.Count() - 2);
                        }
                        if (PressurePointsList.Count() > 2)
                        {
                            PressurePointsList.RemoveRange(0, PressurePointsList.Count() - 2);
                        }
                        if (ThreadSoundBeeps.ThreadState != ThreadState.Suspended && ThreadSoundBeeps.ThreadState != ThreadState.Unstarted)
                        {
                            ThreadSoundBeeps.Suspend();
                        }
                        if (ThreadDrawStreamData.ThreadState != ThreadState.Suspended && ThreadDrawStreamData.ThreadState != ThreadState.Unstarted)
                        {
                            ThreadDrawStreamData.Suspend();
                        }
                        if (ThreadDrawHealthyECG.ThreadState != ThreadState.Suspended && ThreadDrawHealthyECG.ThreadState != ThreadState.Unstarted)
                        {
                            ThreadDrawHealthyECG.Suspend();
                        }
                    }

                    #endregion

                    LastXPos += 5;
                    PointIndex++;
                }
            }
        }

        void StopDrawing()
        {
            DrawingInProgress = false;
            ExamStarted = false;
            LastXPos = 0;
            PointIndex = 1;
            if (ThreadDrawStreamData.ThreadState != ThreadState.Suspended && ThreadDrawStreamData.ThreadState != ThreadState.Unstarted)
            {
                ThreadDrawStreamData.Suspend();
            }
            if (ThreadDrawHealthyECG.ThreadState != ThreadState.Suspended && ThreadDrawHealthyECG.ThreadState != ThreadState.Unstarted)
            {
                ThreadDrawHealthyECG.Suspend();
            }
            if (ThreadSoundBeeps.ThreadState != ThreadState.Suspended && ThreadSoundBeeps.ThreadState != ThreadState.Unstarted)
            {
                //BeepSound01.Stop();
                //BeepSound01.Dispose();
                //StartVentsSound.Stop();
                //StartVentsSound.Dispose();
                ThreadSoundBeeps.Suspend();
            }
            if (ThreadDrawHealthyECGLongTerm.ThreadState != ThreadState.Suspended && ThreadDrawHealthyECGLongTerm.ThreadState != ThreadState.Unstarted)
            {
                ThreadDrawHealthyECGLongTerm.Suspend();
            }


            if (DistancePointsList.Count() > 2)
            {
                DistancePointsList.RemoveRange(0, DistancePointsList.Count() - 2);
            }
            if (PressurePointsList.Count() > 2)
            {
                PressurePointsList.RemoveRange(0, PressurePointsList.Count() - 2);
            }

            ResetStatsVariables();
            ResetTrainingScreen();
        }


        // METHODS TO HANDLE DATABASE

        void GetDBPath()
        {
            DBPath = Application.StartupPath + DataBaseName;
            DBConnection = new SQLiteConnection("URI = file:" + DBPath);
        }

        void ReadDB()
        {
            dataGridViewArchive.Rows.Clear();
            DBConnection.Open();

            using (IDbCommand DBCmd = DBConnection.CreateCommand())
            {
                DBCmd.CommandText = "SELECT * FROM Exams ORDER BY id";
                using (IDataReader DBReader = DBCmd.ExecuteReader())
                {
                    while (DBReader.Read())
                    {
                        dataGridViewArchive.Rows.Add(new object[]
                        {
                            DBReader.GetValue(0).ToString(),
                            DBReader.GetValue(1).ToString(),
                            DBReader.GetValue(2).ToString(),
                            DBReader.GetValue(3).ToString(),
                            DBReader.GetValue(4).ToString(),
                            DBReader.GetValue(5).ToString(),
                            DBReader.GetValue(6).ToString(),
                            DBReader.GetValue(7).ToString(),
                            DBReader.GetValue(8).ToString(),
                            DBReader.GetValue(9).ToString(),
                            DBReader.GetValue(10).ToString(),
                            DBReader.GetValue(11).ToString(),
                            DBReader.GetValue(12).ToString(),
                            DBReader.GetValue(13).ToString(),
                            DBReader.GetValue(14).ToString(),
                            DBReader.GetValue(15).ToString(),
                            DBReader.GetValue(16).ToString(),
                            DBReader.GetValue(17).ToString(),
                            DBReader.GetValue(18).ToString(),
                            DBReader.GetValue(19).ToString(),
                            DBReader.GetValue(20).ToString()
                        });
                    }
                    DBReader.Close();
                }
            }
            DBConnection.Close();
        }


        //METHODS TO PLAY SOUNDS

        void SoundBeeps()
        {
            //int counter = 1;
            //while (true)
            //{
            //    if (PointIndex == 1)
            //    {
            //        counter = 1;
            //    }
            //    if (counter > 30)
            //    {
            //        counter = 1;
            //    }
            //    if (counter == 1)
            //    {
            //        StartVentsSound.Play();
            //        Thread.Sleep(8000);
            //    }
            //    BeepSound01.Play();
            //    Thread.Sleep(600);
            //    counter++;
            //}

            while (true)
            {
                for (int i = 1; i <= 30; i++)
                {
                    BeepSound01.Play();
                    Thread.Sleep(600);
                }
                StartVentsSound.PlaySync();
            }
        }


        //METHODS TO SIMULATE ACTIVITY

        void ImcomingDataSimulation()
        {
            Random random = new Random();
            while (true)
            {
                IncomingData.Add(random.Next(0, 100));
                PulseList.Add(random.Next(10, 90));
                BreathList.Add(random.Next(0, 80));
                Thread.Sleep(50);
            }
        }

        void ReadStreamSimulation()
        {
            Random random = new Random();
            string data;
            while (true)
            {
                data = "beat" + random.Next(0, 100).ToString();
                ReadStreamList.Add(data);
                data = "breath" + random.Next(0, 100).ToString();
                ReadStreamList.Add(data);
                Thread.Sleep(50);
            }
        }

        void SortStreamData()
        {
            int counter = ReadStreamList.Count();
            string data;

            while (true)
            {
                if (ReadStreamList.Count() > counter)
                {
                    counter++;
                    data = ReadStreamList.Last<string>();
                    if (data.StartsWith("beat"))
                    {
                        data = data.TrimStart('b', 'e', 'a', 't');
                    }
                    if (data.StartsWith("breath"))
                    {
                        data = data.TrimStart('b', 'r', 'e', 'a', 't', 'h');
                    }
                }
            }
        }


        // BUTTONS ON START SCREEN
        private void ButtonWirelessConnection_Click(object sender, EventArgs e)
        {
            EstablishWiFiConnection();
        }

        private void ButtonWiredConnection_Click(object sender, EventArgs e) // Connect dummy USB cable
        {
            EstablishSerialPortConnection();
        }

        private void button7_Click(object sender, EventArgs e) // Show all available serial ports
        {
            //string[] ports = SerialPort.GetPortNames();
            //foreach (var port in ports)
            //{
            //    MessageBox.Show(port.ToString());
            //}
        }


        // BUTTONS ON TRAINING SCREEN

        private void buttonStartTraining_Click(object sender, EventArgs e)
        {
            if (DummyConnected)
            {
                SetUpHealthyStats();

                if (ThreadDrawHealthyECG.ThreadState == ThreadState.Suspended)
                {
                    ThreadDrawHealthyECG.Resume();
                }
                else
                {
                    ThreadDrawHealthyECG.Start();
                }

                DisableTrainingControls();
                buttonStartExam.Enabled = false;
                buttonStartTraining.Enabled = false;
                buttonStopTraining.Enabled = true;
            }
            else
            {
                MessageBox.Show("Перед началом работы необходимо подключить тренажер");
            }
        }

        private void buttonStartExam_Click(object sender, EventArgs e)
        {
            if (DummyConnected)
            {
                if (textBoxStudentsName.Text != string.Empty)
                {
                    SetUpHealthyStats();

                    if (ThreadDrawHealthyECG.ThreadState == ThreadState.Suspended)
                    {
                        ThreadDrawHealthyECG.Resume();
                    }
                    else
                    {
                        ThreadDrawHealthyECG.Start();
                    }

                    DisableTrainingControls();
                    buttonStartExam.Enabled = false;
                    buttonStartTraining.Enabled = false;
                    buttonStopTraining.Enabled = true;
                    ExamStarted = true;
                }
                else
                {
                    MessageBox.Show("Перед началом экзамена введите имя студента");
                }
            }
            else
            {
                MessageBox.Show("Перед началом работы необходимо подключить тренажер");
            }
        }

        private void buttonStopTraining_Click(object sender, EventArgs e)
        {
            StopDrawing();
        }


        // BUTTONS ON ARCHIVE SCREEN

        private void buttonPrint_Click(object sender, EventArgs e)
        {
            PrintDocument Document = new PrintDocument();
            Document.PrintPage += new PrintPageEventHandler(printDocument1_PrintPage);
            PrintPreviewDialog dlg = new PrintPreviewDialog();
            dlg.Document = Document;
            dlg.ShowDialog();
        }


        // FORM EVENTS

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            GetControlSizes();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            ThreadIncomingData.IsBackground = true;
            ThreadReadStream.IsBackground = true;
            ThreadSortData.IsBackground = true;
            ThreadWiFiStreamReader.IsBackground = true;
            ThreadSerialStreamReader.IsBackground = true;
            ThreadDrawHealthyECG.IsBackground = true;
            ThreadDrawHealthyECGLongTerm.IsBackground = true;
            ThreadDrawStreamData.IsBackground = true;

            ThreadSoundBeeps.IsBackground = true;

            if (CurrentPort != null)
            {
                if (CurrentPort.IsOpen)
                {
                    CurrentPort.Close();
                }
            }
        }

        private void tabPage2_Enter(object sender, EventArgs e)
        {
            //MessageBox.Show("ACTIVATED");
            ReadDB();
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Graphics graphics = e.Graphics;
            Font font = new Font("Tahoma", 16, FontStyle.Regular, GraphicsUnit.Point);
            SolidBrush solidbrush = new SolidBrush(Color.Black);

            for (int columnIndex = 0; columnIndex < dataGridViewArchive.Columns.Count; columnIndex++)
            {
                string value = dataGridViewArchive.Columns[columnIndex].HeaderText.ToString() + ": " + dataGridViewArchive[columnIndex, dataGridViewArchive.CurrentCell.RowIndex].Value.ToString();
                graphics.DrawString(value, font, solidbrush, 100, 100 + columnIndex * 40);
            }

        }
    }
}
