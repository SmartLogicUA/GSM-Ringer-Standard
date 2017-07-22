using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GsmRingerStandartConfig
{
    public partial class GsmRingerProConfig : Form
    {
        const double WatchdogInterval = 3000D;

        //Dictionary<string, string> cardRecs;
        SerialPortListener listener;
        string functionResult;
        bool commandProccessing;
        bool multiCommand = false, errorFlag = false;

        ReadingUserCardsForm readForm;
        //CardAdminForm cardForm;
        VoidControlShort setControl;
        VoidControlString setControlString;
        
        public GsmRingerProConfig()
        {
            InitializeComponent();
        }

        public string PortName
        {
            get
            {
                return serialPort1.PortName;
            }
        }

        private void GsmRingerProConfig_Load(object sender, EventArgs e)
        {
            setControl = new VoidControlShort(SetControl);
            setControlString = new VoidControlString(SetControlString);
            if (System.IO.File.Exists("config.ini"))
            {
                string portName = System.IO.File.ReadAllText("config.ini");
                if ((new List<string>(System.IO.Ports.SerialPort.GetPortNames())).Contains(portName))
                {
                    serialPort1 = new System.IO.Ports.SerialPort(portName, 19200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                    serialPort1.NewLine = "\r\n";
                    serialPort1.Encoding = Encoding.UTF8;
                    listener = new SerialPortListener(serialPort1);
                }
            }

//            if (System.IO.File.Exists("users.cfg"))
//            {
//                try
//                {
//                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
//                    cardRecs = (Dictionary<string, string>)bf.Deserialize(System.IO.File.OpenRead("users.cfg"));
//                }
//#pragma warning disable 168
//                catch (System.Runtime.Serialization.SerializationException except)
//                {
//                    cardRecs = new Dictionary<string, string>();
//                }
//#pragma warning restore 168
//            }
//            else
//            {
//                cardRecs = new Dictionary<string, string>();
//            }
        }

        private void SendCommand(string cmd)
        {
            try
            {
                if (!serialPort1.IsOpen)
                {
                    serialPort1.Open();
                }
                serialPort1.WriteLine("$" + cmd + "*" + SerialPortListener.CalculateCRC(cmd));
            }
#pragma warning disable 168
            catch (NullReferenceException except)
            {
                MessageBox.Show("Сначала выберите порт", "Не выбран COM-порт", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#pragma warning restore 168
            catch
            {
                MessageBox.Show("Работа с выбраным портом невозможна", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetDeviceId()
        {
            commandProccessing = true;
            listener.DeviceIdReceived += new DeviceDataReceivedEventHandler(listener_DeviceIdReceived);
            SendCommand("PCHND");

            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.DeviceIdReceived -= listener_DeviceIdReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return "Error";
            }
            else
                return functionResult;
        }

        void listener_DeviceIdReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.DeviceIdReceived -= listener_DeviceIdReceived;
            commandProccessing = false;
        }

        private string GetDeviceVersion()
        {
            commandProccessing = true;
            listener.DeviceVersionReceived += new DeviceDataReceivedEventHandler(listener_DeviceVersionReceived);
            SendCommand("PCVER");
            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.DeviceVersionReceived -= listener_DeviceVersionReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return "Error";
            }
            else
                return functionResult;
        }

        void listener_DeviceVersionReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.DeviceVersionReceived -= listener_DeviceVersionReceived;
            commandProccessing = false;
        }

        private bool SetDefaults()
        {
            commandProccessing = true;
            listener.SetDefaultsOKReceived += new DeviceDataReceivedEventHandler(listener_SetDefaultsOKReceived);
            SendCommand("PCSFS");
            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.SetDefaultsOKReceived -= listener_SetDefaultsOKReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return false;
            }

            if (functionResult == "OK")
                return true;
            else
                return false;
        }

        void listener_SetDefaultsOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.SetDefaultsOKReceived -= listener_SetDefaultsOKReceived;
            commandProccessing = false;
        }

        private bool SetUserNumber(int userNumber, string phoneNumber)
        {
            commandProccessing = true;
            listener.SetUserNumberOKReceived += new DeviceDataReceivedEventHandler(listener_SetUserNumberOKReceived);
            SendCommand(string.Format("PCSUN {0}{1}", userNumber, phoneNumber));
            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.SetUserNumberOKReceived -= listener_SetUserNumberOKReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return false;
            }

            if (functionResult == "OK")
                return true;
            else
                return false;

        }

        private bool SetNewPassword(string pswd)
        {
            commandProccessing = true;
            listener.SetNewPasswordOKReceived += new DeviceDataReceivedEventHandler(listener_SetNewPasswordOKReceived);
            SendCommand(string.Format("PCNPW {0}", pswd));
            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.SetNewPasswordOKReceived -= listener_SetNewPasswordOKReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return false;
            }

            if (functionResult == "OK")
                return true;
            else
                return false;

        }

        void listener_SetNewPasswordOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.SetNewPasswordOKReceived -= listener_SetNewPasswordOKReceived;
            commandProccessing = false;
        }

        void listener_SetUserNumberOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.SetUserNumberOKReceived -= listener_SetUserNumberOKReceived;
            commandProccessing = false;
        }

        private string GetUserNumber(int userNumber)
        {
            commandProccessing = true;
            listener.GetUserNumberReceived += new DeviceDataReceivedEventHandler(listener_GetUserNumberReceived);
            SendCommand(string.Format("PCGUN {0}", userNumber));
            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.GetUserNumberReceived -= listener_GetUserNumberReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return "Error";
            }
            else
                return functionResult;
        }

        void listener_GetUserNumberReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message.Substring(1);
            listener.GetUserNumberReceived -= listener_GetUserNumberReceived;
            commandProccessing = false;
        }

        bool SetAlarmDevices(byte zoneNum, byte activateDevices, ushort timeOnRelay1, ushort timeOnCol1, ushort timeOffRelay1, ushort timeOffCol1)
        {
            commandProccessing = true;
            listener.SetAlarmDevicesOKReceived += new DeviceDataReceivedEventHandler(listener_SetAlarmDevicesOKReceived);
            SendCommand(string.Format("PCSTE {0}{1}{2}{3}{4}{5}", zoneNum.ToString("X2"), activateDevices.ToString("X2"), timeOnRelay1.ToString("X4"), timeOnCol1.ToString("X4"), timeOffRelay1.ToString("X4"), timeOffCol1.ToString("X4")));
            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.SetAlarmDevicesOKReceived -= listener_SetAlarmDevicesOKReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return false;
            }
            if (functionResult == "OK")
                return true;
            else
                return false;
        }

        void listener_SetAlarmDevicesOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.SetAlarmDevicesOKReceived -= listener_SetAlarmDevicesOKReceived;
            commandProccessing = false;
        }

        string GetAlarmDevices(byte zoneNum)
        {
            commandProccessing = true;
            listener.GetAlarmDevicesReceived += new DeviceDataReceivedEventHandler(listener_GetAlarmDevicesReceived);
            SendCommand(string.Format("PCGTE {0}", zoneNum.ToString("X2")));
            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.GetAlarmDevicesReceived -= listener_GetAlarmDevicesReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return "Error";
            }
            else
                return functionResult;
        }

        void listener_GetAlarmDevicesReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.GetAlarmDevicesReceived -= listener_GetAlarmDevicesReceived;
            commandProccessing = false;
        }

        bool SetUserCard(byte cardNum, string cardId)
        {
            commandProccessing = true;
            listener.SetUserCardOKReceived += new DeviceDataReceivedEventHandler(listener_SetUserCardOKReceived);
            SendCommand(string.Format("PCSCD {0}{1}", cardNum.ToString("X2"), cardId));
            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.SetUserCardOKReceived -= listener_SetUserCardOKReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return false;
            }
            if (functionResult == "OK")
                return true;
            else
                return false;
        }

        void listener_SetUserCardOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.SetUserCardOKReceived -= listener_SetUserCardOKReceived;
            commandProccessing = false;
        }

        string GetUserCard(byte cardNum)
        {
                commandProccessing = true;
                listener.GetUserCardReceived += new DeviceDataReceivedEventHandler(listener_GetUserCardReceived);
                SendCommand(string.Format("PCGCD {0}", cardNum.ToString("X2")));
                DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

                while (commandProccessing)
                {
                    if (DateTime.Now > watchdog)
                    {
                        listener.GetUserCardReceived -= listener_GetUserCardReceived;
                        break;
                    }
                }

                if (commandProccessing)
                {
                    commandProccessing = false;
                    return "FFFFFF";
                }
                else
                    return functionResult;
        }

        void listener_GetUserCardReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.GetUserCardReceived -= listener_GetUserCardReceived;
            commandProccessing = false;
        }

        bool SetActivationTime(byte time)
        {
            commandProccessing = true;
            listener.SetTimeActivationOKReceived += new DeviceDataReceivedEventHandler(listener_SetTimeActivationOKReceived);
            SendCommand(string.Format("PCSTA {0}", time.ToString("X2")));
            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.SetTimeActivationOKReceived -= listener_SetTimeActivationOKReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return false;
            }
            if (functionResult == "OK")
                return true;
            else
                return false;
        }

        void listener_SetTimeActivationOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.SetTimeActivationOKReceived -= listener_SetTimeActivationOKReceived;
            commandProccessing = false;
        }

        string GetActivationTime()
        {
            commandProccessing = true;
            listener.GetTimeActivationReceived += new DeviceDataReceivedEventHandler(listener_GetTimeActivationReceived);
            SendCommand("PCGTA");
            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.GetTimeActivationReceived -= listener_GetTimeActivationReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return "Error";
            }
            else
                return functionResult;
        }

        void listener_GetTimeActivationReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.GetTimeActivationReceived -= listener_GetTimeActivationReceived;
            commandProccessing = false;
        }

        bool SetDeactivationTime(byte time)
        {
            commandProccessing = true;
            listener.SetTimeDeactivationOKReceived += new DeviceDataReceivedEventHandler(listener_SetTimeDeactivationOKReceived);
            SendCommand(string.Format("PCSTD {0}", time.ToString("X2")));
            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.SetTimeDeactivationOKReceived -= listener_SetTimeDeactivationOKReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return false;
            }

            if (functionResult == "OK")
                return true;
            else
                return false;
        }

        void listener_SetTimeDeactivationOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.SetTimeDeactivationOKReceived -= listener_SetTimeDeactivationOKReceived;
            commandProccessing = false;
        }

        string GetDeactivationTime()
        {
            commandProccessing = true;
            listener.GetTimeDeactivationReceived += new DeviceDataReceivedEventHandler(listener_GetTimeDeactivationReceived);
            SendCommand("PCGTD");
            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.GetTimeDeactivationReceived -= listener_GetTimeDeactivationReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return "Error";
            }
            else
                return functionResult;
        }

        void listener_GetTimeDeactivationReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.GetTimeDeactivationReceived -= listener_GetTimeDeactivationReceived;
            commandProccessing = false;
        }

        bool SetCallParams(byte smsByte, byte callByte)
        {
            commandProccessing = true;
            listener.SetCallParamsOKReceived += new DeviceDataReceivedEventHandler(listener_SetCallParamsOKReceived);
            SendCommand(string.Format("PCSCS {0}{1}", smsByte.ToString("X2"), callByte.ToString("X2")));
            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.SetCallParamsOKReceived -= listener_SetCallParamsOKReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return false;
            }
            if (functionResult == "OK")
                return true;
            else
                return false;

        }

        void listener_SetCallParamsOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.SetCallParamsOKReceived -= listener_SetCallParamsOKReceived;
            commandProccessing = false;
        }

        string GetCallParams()
        {
            commandProccessing = true;
            listener.GetCallParamsReceived += new DeviceDataReceivedEventHandler(listener_GetCallParamsReceived);
            SendCommand("PCGCS");
            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.GetCallParamsReceived -= listener_GetCallParamsReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return "Error";
            }
            else
                return functionResult;
        }

        void listener_GetCallParamsReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.GetCallParamsReceived -= listener_GetCallParamsReceived;
            commandProccessing = false;
        }

        bool SetActiveZones(byte zones)
        {
            commandProccessing = true;
            listener.SetActiveZonesOKReceived += new DeviceDataReceivedEventHandler(listener_SetActiveZonesOKReceived);
            SendCommand(string.Format("PCSAZ {0}", zones.ToString("X2")));
            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.SetActiveZonesOKReceived -= listener_SetActiveZonesOKReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return false;
            }

            if (functionResult == "OK")
                return true;
            else
                return false;
        }

        void listener_SetActiveZonesOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.SetActiveZonesOKReceived -= listener_SetActiveZonesOKReceived;
            commandProccessing = false;
        }

        string GetActiveZones()
        {
            commandProccessing = true;
            listener.GetActiveZonesReceived += new DeviceDataReceivedEventHandler(listener_GetActiveZonesReceived);
            SendCommand("PCGAZ");
            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.GetActiveZonesReceived -= listener_GetActiveZonesReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return "Error";
            }
            else
                return functionResult;
        }

        void listener_GetActiveZonesReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.GetActiveZonesReceived -= listener_GetActiveZonesReceived;
            commandProccessing = false;
        }

        bool SetAlarmSources(byte sources)
        {
            commandProccessing = true;
            listener.SetAlarmSourcesOKReceived += new DeviceDataReceivedEventHandler(listener_SetAlarmSourcesOKReceived);
            SendCommand(string.Format("PCSON {0}", sources.ToString("X2")));
            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.SetAlarmSourcesOKReceived -= listener_SetAlarmSourcesOKReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return false;
            }
            if (functionResult == "OK")
                return true;
            else
                return false;
        }

        void listener_SetAlarmSourcesOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.SetAlarmSourcesOKReceived -= listener_SetAlarmSourcesOKReceived;
            commandProccessing = false;
        }

        string GetAlarmSources()
        {
            commandProccessing = true;
            listener.GetAlarmSourcesReceived += new DeviceDataReceivedEventHandler(listener_GetAlarmSourcesReceived);
            SendCommand("PCGON");
            DateTime watchdog = DateTime.Now.AddMilliseconds(WatchdogInterval);

            while (commandProccessing)
            {
                if (DateTime.Now > watchdog)
                {
                    listener.GetAlarmSourcesReceived -= listener_GetAlarmSourcesReceived;
                    break;
                }
            }

            if (commandProccessing)
            {
                commandProccessing = false;
                return "Error";
            }
            else
                return functionResult;
        }

        void listener_GetAlarmSourcesReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message;
            listener.GetAlarmSourcesReceived -= listener_GetAlarmSourcesReceived;
            commandProccessing = false;
        }

        private void GsmRingerProConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
//            try
//            {
//                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
//                bf.Serialize(System.IO.File.Open("users.cfg", System.IO.FileMode.Create), cardRecs);
//            }
//#pragma warning disable 168
//            catch (System.Runtime.Serialization.SerializationException except)
//            {
//            }
//            catch (System.UnauthorizedAccessException except)
//            {
//            }
//            catch (System.IO.IOException except)
//            {
//            }
//#pragma warning restore 168
        }

        //private void cardAdminBtn_Click(object sender, EventArgs e)
        //{
        //    readForm = new ReadingUserCardsForm();
        //    readForm.Show();
        //    cardRecsSyncronizer.RunWorkerAsync();
        //}

        //private void cardRecsSyncronizer_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    for (int i = 0; i < 20; i++)
        //    {
        //        string cardId = GetUserCard((byte)i);
        //        if (cardId == "FFFFFF")
        //        {
        //        }
        //        else if (!cardRecs.ContainsKey(cardId))
        //            cardRecs.Add(cardId, "Пользователь" + i.ToString());
        //    }
        //}

        //private void cardRecsSyncronizer_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    readForm.Close();
        //    readForm = null;
        //    cardForm = new CardAdminForm(cardRecs);
        //    if (cardForm.ShowDialog(this) == DialogResult.OK)
        //    {
        //        readForm = new ReadingUserCardsForm();
        //        readForm.Show();
        //        cardRecsWriter.RunWorkerAsync();
        //    }
        //}

        public SerialPortListener Listener
        {
            get
            {
                return listener;
            }
        }

        //private void cardRecsWriter_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    byte count = 0;
        //    foreach (KeyValuePair<string, string> element in cardRecs)
        //    {
        //        SetUserCard(count, element.Key);
        //        count++;
        //    }
        //    for (byte i = count; i < 20; i++)
        //    {
        //        SetUserCard(i, "FFFFFF");
        //    }
        //}

        //private void cardRecsWriter_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    readForm.Close();
        //    readForm = null;
        //}

        private void readCallParamsBtn_Click(object sender, EventArgs e)
        {
            string callParams = GetCallParams();
            byte buf = 0;
            if (byte.TryParse(callParams.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out buf))
            {
                if (0 == (buf & 1))
                    this.Invoke(setControl, smsUser1CheckBox, (ushort)0);
                else
                    this.Invoke(setControl, smsUser1CheckBox, (ushort)1);
                if (0 == (buf & 2))
                    this.Invoke(setControl, smsUser2CheckBox, (ushort)0);
                else
                    this.Invoke(setControl, smsUser2CheckBox, (ushort)1);
                if (0 == (buf & 4))
                    this.Invoke(setControl, smsUser3CheckBox, (ushort)0);
                else
                    this.Invoke(setControl, smsUser3CheckBox, (ushort)1);
                if (0 == (buf & 8))
                    this.Invoke(setControl, smsUser4CheckBox, (ushort)0);
                else
                    this.Invoke(setControl, smsUser4CheckBox, (ushort)1);
                if (0 == (buf & 16))
                    this.Invoke(setControl, smsUser5CheckBox, (ushort)0);
                else
                    this.Invoke(setControl, smsUser5CheckBox, (ushort)1);
            }
            if (byte.TryParse(callParams.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out buf))
            {
                if (0 == (buf & 1))
                    this.Invoke(setControl, callUser1CheckBox, (ushort)0);
                else
                    this.Invoke(setControl, callUser1CheckBox, (ushort)1);
                if (0 == (buf & 2))
                    this.Invoke(setControl, callUser2CheckBox, (ushort)0);
                else
                    this.Invoke(setControl, callUser2CheckBox, (ushort)1);
                if (0 == (buf & 4))
                    this.Invoke(setControl, callUser3CheckBox, (ushort)0);
                else
                    this.Invoke(setControl, callUser3CheckBox, (ushort)1);
                if (0 == (buf & 8))
                    this.Invoke(setControl, callUser4CheckBox, (ushort)0);
                else
                    this.Invoke(setControl, callUser4CheckBox, (ushort)1);
                if (0 == (buf & 16))
                    this.Invoke(setControl, callUser5CheckBox, (ushort)0);
                else
                    this.Invoke(setControl, callUser5CheckBox, (ushort)1);
            }
        }

        private void writeCallParamsBtn_Click(object sender, EventArgs e)
        {
            byte smsByte = 0, callByte = 0;
            if (smsUser1CheckBox.Checked)
                smsByte += 1;
            if (smsUser2CheckBox.Checked)
                smsByte += 2;
            if (smsUser3CheckBox.Checked)
                smsByte += 4;
            if (smsUser4CheckBox.Checked)
                smsByte += 8;
            if (smsUser5CheckBox.Checked)
                smsByte += 16;
            if (callUser1CheckBox.Checked)
                callByte += 1;
            if (callUser2CheckBox.Checked)
                callByte += 2;
            if (callUser3CheckBox.Checked)
                callByte += 4;
            if (callUser4CheckBox.Checked)
                callByte += 8;
            if (callUser5CheckBox.Checked)
                callByte += 16;
            if (!SetCallParams(smsByte, callByte))
            {
                if (multiCommand)
                    errorFlag = true;
                else
                    MessageBox.Show("Во время установки параметров оповещения произошла ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (!multiCommand)
                    MessageBox.Show("Параметры оповещения успешно установлены", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void readNum1Btn_Click(object sender, EventArgs e)
        {
            string num = GetUserNumber(0);
            if (num != "Error")
            {
                this.Invoke(setControlString, num1Box, num);
            }
        }

        private void writeNum1Btn_Click(object sender, EventArgs e)
        {
            string num = num1Box.Text.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            if (num.Length == 10)
            {
                if (!SetUserNumber(0, num))
                {
                    if (multiCommand)
                        errorFlag = true;
                    else
                        MessageBox.Show("Во время записи номера пользователя произошла ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (!multiCommand)
                        MessageBox.Show(String.Format("Номер {0} успешно установлен", 1), "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
                MessageBox.Show(String.Format("Номер {0} введен неправильно", 1), "Ошибка ввода номера", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void readNum2Btn_Click(object sender, EventArgs e)
        {
            string num = GetUserNumber(1);
            if (num != "Error")
            {
                this.Invoke(setControlString, num2Box, num);
            }
        }

        private void readNum3Btn_Click(object sender, EventArgs e)
        {
            string num = GetUserNumber(2);
            if (num != "Error")
            {
                this.Invoke(setControlString, num3Box, num);
            }
        }

        private void readNum4Btn_Click(object sender, EventArgs e)
        {
            string num = GetUserNumber(3);
            if (num != "Error")
            {
                this.Invoke(setControlString, num4Box, num);
            }
        }

        private void readNum5Btn_Click(object sender, EventArgs e)
        {
            string num = GetUserNumber(4);
            if (num != "Error")
            {
                this.Invoke(setControlString, num5Box, num);
            }
        }

        private void writeNum2Btn_Click(object sender, EventArgs e)
        {
            string num = num2Box.Text.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            if (num.Length == 10)
            {
                if (!SetUserNumber(1, num))
                {
                    if (multiCommand)
                        errorFlag = true;
                    else
                        MessageBox.Show("Во время записи номера пользователя произошла ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (!multiCommand)
                        MessageBox.Show(String.Format("Номер {0} успешно установлен", 2), "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
                MessageBox.Show(String.Format("Номер {0} введен неправильно", 2), "Ошибка ввода номера", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private void writeNum3Btn_Click(object sender, EventArgs e)
        {
            string num = num3Box.Text.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            if (num.Length == 10)
            {
                if (!SetUserNumber(2, num))
                {
                    if (multiCommand)
                        errorFlag = true;
                    else
                        MessageBox.Show("Во время записи номера пользователя произошла ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (!multiCommand)
                        MessageBox.Show(String.Format("Номер {0} успешно установлен", 3), "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
                MessageBox.Show(String.Format("Номер {0} введен неправильно", 3), "Ошибка ввода номера", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private void writeNum4Btn_Click(object sender, EventArgs e)
        {
            string num = num4Box.Text.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            if (num.Length == 10)
            {
                if (!SetUserNumber(3, num))
                {
                    if (multiCommand)
                        errorFlag = true;
                    else
                        MessageBox.Show("Во время записи номера пользователя произошла ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (!multiCommand)
                        MessageBox.Show(String.Format("Номер {0} успешно установлен", 4), "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
                MessageBox.Show(String.Format("Номер {0} введен неправильно", 4), "Ошибка ввода номера", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private void writeNum5Btn_Click(object sender, EventArgs e)
        {
            string num = num5Box.Text.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            if (num.Length == 10)
            {
                if (!SetUserNumber(4, num))
                {
                    if (multiCommand)
                        errorFlag = true;
                    else
                        MessageBox.Show("Во время записи номера пользователя произошла ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (!multiCommand)
                        MessageBox.Show(String.Format("Номер {0} успешно установле", 5), "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
                MessageBox.Show(String.Format("Номер {0} введен неправильно", 5), "Ошибка ввода номера", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private void readActivationTimeBtn_Click(object sender, EventArgs e)
        {
            byte time = 0;
            if (byte.TryParse(GetActivationTime(), System.Globalization.NumberStyles.HexNumber, null, out time))
            {
                this.Invoke(setControl, activationTimeBox, (ushort)time);
            }
            else
                MessageBox.Show("При считывании времени отсрочки активации произошла ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void writeActivationTimeBtn_Click(object sender, EventArgs e)
        {
            byte time;
            if (byte.TryParse(activationTimeBox.Text, out time))
            {
                if (time >= 1 && time <= 255)
                {
                    if (!SetActivationTime(time))
                    {
                        if (multiCommand)
                            errorFlag = true;
                        else
                            MessageBox.Show("Во время установки интервала отсрочки активации произошла ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        if (!multiCommand)
                            MessageBox.Show("Интервал отсрочки активации успешно установлен", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                    MessageBox.Show("Введите значение от 1 до 255", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
                MessageBox.Show("Время отсрочки активации не введено", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void readDeactivationTimeBtn_Click(object sender, EventArgs e)
        {
            byte time = 0;
            if (byte.TryParse(GetDeactivationTime(), System.Globalization.NumberStyles.HexNumber, null, out time))
            {
                this.Invoke(setControl, deactivationTimeBox, (ushort)time);
            }
            else
                MessageBox.Show("При считывании времени отсрочки оповещения произошла ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void writeDeactivationTimeBtn_Click(object sender, EventArgs e)
        {
            byte time;
            if (byte.TryParse(deactivationTimeBox.Text, out time))
            {
                if (time >= 1 && time <= 255)
                {
                    if (!SetDeactivationTime(time))
                    {
                        if (multiCommand)
                            errorFlag = true;
                        else
                            MessageBox.Show("Во время установки интервала отсрочки оповещения произошла ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        if (!multiCommand)
                            MessageBox.Show("Интервал отсрочки оповещения успешно установлен", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                    MessageBox.Show("Введите значение от 1 до 255", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
                MessageBox.Show("Время отсрочки оповещения не введено", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void readActiveZonesBtn_Click(object sender, EventArgs e)
        {
            byte zoneByte = 0;
            if (byte.TryParse(GetActiveZones(), System.Globalization.NumberStyles.HexNumber, null, out zoneByte))
            {
                if (0 == (zoneByte & 1))
                    this.Invoke(setControl, zone1EnableCheckBox, (ushort)0);
                else
                    this.Invoke(setControl, zone1EnableCheckBox, (ushort)1);
                if (0 == (zoneByte & 2))
                    this.Invoke(setControl, zone2EnableCheckBox, (ushort)0);
                else
                    this.Invoke(setControl, zone2EnableCheckBox, (ushort)1);
                if (0 == (zoneByte & 4))
                    this.Invoke(setControl, zone3EnableCheckBox, (ushort)0);
                else
                    this.Invoke(setControl, zone3EnableCheckBox, (ushort)1);
                if (0 == (zoneByte & 8))
                    this.Invoke(setControl, zone4EnableCheckBox, (ushort)0);
                else
                    this.Invoke(setControl, zone4EnableCheckBox, (ushort)1);
                //if (0 == (zoneByte & 16))
                //    this.Invoke(setControl, zone5EnableCheckBox, (ushort)0);
                //else
                //    this.Invoke(setControl, zone5EnableCheckBox, (ushort)1);
                //if (0 == (zoneByte & 32))
                //    this.Invoke(setControl, zone6EnableCheckBox, (ushort)0);
                //else
                //    this.Invoke(setControl, zone6EnableCheckBox, (ushort)1);
                //if (0 == (zoneByte & 64))
                //    this.Invoke(setControl, zone7EnableCheckBox, (ushort)0);
                //else
                //    this.Invoke(setControl, zone7EnableCheckBox, (ushort)1);
                //if (0 == (zoneByte & 128))
                //    this.Invoke(setControl, zone8EnableCheckBox, (ushort)0);
                //else
                //    this.Invoke(setControl, zone8EnableCheckBox, (ushort)1);
            }
            else
                MessageBox.Show("При считывании активных зон произошла ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void writeActiveZonesBtn_Click(object sender, EventArgs e)
        {
            byte zoneByte = 0;
            if (zone1EnableCheckBox.Checked)
                zoneByte += 1;
            if (zone2EnableCheckBox.Checked)
                zoneByte += 2;
            if (zone3EnableCheckBox.Checked)
                zoneByte += 4;
            if (zone4EnableCheckBox.Checked)
                zoneByte += 8;
            //if (zone5EnableCheckBox.Checked)
            //    zoneByte += 16;
            //if (zone6EnableCheckBox.Checked)
            //    zoneByte += 32;
            //if (zone7EnableCheckBox.Checked)
            //    zoneByte += 64;
            //if (zone8EnableCheckBox.Checked)
            //    zoneByte += 128;
            if (!SetActiveZones(zoneByte))
            {
                if (multiCommand)
                    errorFlag = true;
                else
                    MessageBox.Show("Во время установки активных зон произошла ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (!multiCommand)
                    MessageBox.Show("Активные зоны успешно установлены", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void sendUSSDBtn_Click(object sender, EventArgs e)
        {
            SendUSSDCommand(ussdBox.Text);
        }

        private void SendUSSDCommand(string cmd)
        {
            listener.USSDCommandReceived += new DeviceDataReceivedEventHandler(listener_USSDCommandReceived);
            SendCommand("PCUSD " + cmd.Replace('*', '@') + Encoding.UTF8.GetString(new byte[] { 0x00 }));
        }

        void listener_USSDCommandReceived(object sender, StringDataReceivedEventArgs e)
        {
            MessageBox.Show(e.Message.Replace('@', '*'), "USSD", MessageBoxButtons.OK, MessageBoxIcon.Information);
            listener.USSDCommandReceived -= listener_USSDCommandReceived;
        }

        private void readTabConfigBtn_Click(object sender, EventArgs e)
        {
            readForm = new ReadingUserCardsForm();
            readForm.Show();
            zoneConfigReader.RunWorkerAsync();
        }

        private void zoneConfigReader_DoWork(object sender, DoWorkEventArgs e)
        {
            
            TabConfig buf;
            
            //zone1
            string alarmStr = GetAlarmDevices(0);
            if (alarmStr != "Error")
            {
                buf = new TabConfig(alarmStr);
                if (buf.relay1)
                    this.Invoke(setControl, zone1Relay1ChkBox, (ushort)1);
                else
                    this.Invoke(setControl, zone1Relay1ChkBox, (ushort)0);
                //if (buf.relay2)
                //    this.Invoke(setControl, zone1Relay2ChkBox, (ushort)1);
                //else
                //    this.Invoke(setControl, zone1Relay2ChkBox, (ushort)0);
                if (buf.drain1)
                    this.Invoke(setControl, zone1Drain1ChkBox, (ushort)1);
                else
                    this.Invoke(setControl, zone1Drain1ChkBox, (ushort)0);
                //if (buf.drain2)
                //    this.Invoke(setControl, zone1Drain2ChkBox, (ushort)1);
                //else
                //    this.Invoke(setControl, zone1Drain2ChkBox, (ushort)0);
                this.Invoke(setControl, zone1Relay1OnTxtBox, buf.relay1On);
                this.Invoke(setControl, zone1Relay1OffTxtBox, buf.relay1Off);
                //this.Invoke(setControl, zone1Relay2OnTxtBox, buf.relay2On);
                //this.Invoke(setControl, zone1Relay2OffTxtBox, buf.relay2Off);
                this.Invoke(setControl, zone1Drain1OnTxtBox, buf.drain1On);
                this.Invoke(setControl, zone1Drain1OffTxtBox, buf.drain1Off);
                //this.Invoke(setControl, zone1Drain2OnTxtBox, buf.drain2On);
                //this.Invoke(setControl, zone1Drain2OffTxtBox, buf.drain2Off);
            }
            
            //zone2
            alarmStr = GetAlarmDevices(1);
            if (alarmStr != "Error")
            {
                buf = new TabConfig(alarmStr);
                if (buf.relay1)
                    this.Invoke(setControl, zone2Relay1ChkBox, (ushort)1);
                else
                    this.Invoke(setControl, zone2Relay1ChkBox, (ushort)0);
                //if (buf.relay2)
                //    this.Invoke(setControl, zone2Relay2ChkBox, (ushort)1);
                //else
                //    this.Invoke(setControl, zone2Relay2ChkBox, (ushort)0);
                if (buf.drain1)
                    this.Invoke(setControl, zone2Drain1ChkBox, (ushort)1);
                else
                    this.Invoke(setControl, zone2Drain1ChkBox, (ushort)0);
                //if (buf.drain2)
                //    this.Invoke(setControl, zone2Drain2ChkBox, (ushort)1);
                //else
                //    this.Invoke(setControl, zone2Drain2ChkBox, (ushort)0);
                this.Invoke(setControl, zone2Relay1OnTxtBox, buf.relay1On);
                this.Invoke(setControl, zone2Relay1OffTxtBox, buf.relay1Off);
                //this.Invoke(setControl, zone2Relay2OnTxtBox, buf.relay2On);
                //this.Invoke(setControl, zone2Relay2OffTxtBox, buf.relay2Off);
                this.Invoke(setControl, zone2Drain1OnTxtBox, buf.drain1On);
                this.Invoke(setControl, zone2Drain1OffTxtBox, buf.drain1Off);
                //this.Invoke(setControl, zone2Drain2OnTxtBox, buf.drain2On);
                //this.Invoke(setControl, zone2Drain2OffTxtBox, buf.drain2Off);
            }

            //zone3
            alarmStr = GetAlarmDevices(2);
            if (alarmStr != "Error")
            {
                buf = new TabConfig(alarmStr);
                if (buf.relay1)
                    this.Invoke(setControl, zone3Relay1ChkBox, (ushort)1);
                else
                    this.Invoke(setControl, zone3Relay1ChkBox, (ushort)0);
                //if (buf.relay2)
                //    this.Invoke(setControl, zone3Relay2ChkBox, (ushort)1);
                //else
                //    this.Invoke(setControl, zone3Relay2ChkBox, (ushort)0);
                if (buf.drain1)
                    this.Invoke(setControl, zone3Drain1ChkBox, (ushort)1);
                else
                    this.Invoke(setControl, zone3Drain1ChkBox, (ushort)0);
                //if (buf.drain2)
                //    this.Invoke(setControl, zone3Drain2ChkBox, (ushort)1);
                //else
                //    this.Invoke(setControl, zone3Drain2ChkBox, (ushort)0);
                this.Invoke(setControl, zone3Relay1OnTxtBox, buf.relay1On);
                this.Invoke(setControl, zone3Relay1OffTxtBox, buf.relay1Off);
                //this.Invoke(setControl, zone3Relay2OnTxtBox, buf.relay2On);
                //this.Invoke(setControl, zone3Relay2OffTxtBox, buf.relay2Off);
                this.Invoke(setControl, zone3Drain1OnTxtBox, buf.drain1On);
                this.Invoke(setControl, zone3Drain1OffTxtBox, buf.drain1Off);
                //this.Invoke(setControl, zone3Drain2OnTxtBox, buf.drain2On);
                //this.Invoke(setControl, zone3Drain2OffTxtBox, buf.drain2Off);
            }

            //zone4
            alarmStr = GetAlarmDevices(3);
            if (alarmStr != "Error")
            {
                buf = new TabConfig(alarmStr);
                if (buf.relay1)
                    this.Invoke(setControl, zone4Relay1ChkBox, (ushort)1);
                else
                    this.Invoke(setControl, zone4Relay1ChkBox, (ushort)0);
                //if (buf.relay2)
                //    this.Invoke(setControl, zone4Relay2ChkBox, (ushort)1);
                //else
                //    this.Invoke(setControl, zone4Relay2ChkBox, (ushort)0);
                if (buf.drain1)
                    this.Invoke(setControl, zone4Drain1ChkBox, (ushort)1);
                else
                    this.Invoke(setControl, zone4Drain1ChkBox, (ushort)0);
                //if (buf.drain2)
                //    this.Invoke(setControl, zone4Drain2ChkBox, (ushort)1);
                //else
                //    this.Invoke(setControl, zone4Drain2ChkBox, (ushort)0);
                this.Invoke(setControl, zone4Relay1OnTxtBox, buf.relay1On);
                this.Invoke(setControl, zone4Relay1OffTxtBox, buf.relay1Off);
                //this.Invoke(setControl, zone4Relay2OnTxtBox, buf.relay2On);
                //this.Invoke(setControl, zone4Relay2OffTxtBox, buf.relay2Off);
                this.Invoke(setControl, zone4Drain1OnTxtBox, buf.drain1On);
                this.Invoke(setControl, zone4Drain1OffTxtBox, buf.drain1Off);
                //this.Invoke(setControl, zone4Drain2OnTxtBox, buf.drain2On);
                //this.Invoke(setControl, zone4Drain2OffTxtBox, buf.drain2Off);
            }

            ////zone5
            //alarmStr = GetAlarmDevices(4);
            //if (alarmStr != "Error")
            //{
            //    buf = new TabConfig(alarmStr);
            //    if (buf.relay1)
            //        this.Invoke(setControl, zone5Relay1ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, zone5Relay1ChkBox, (ushort)0);
            //    if (buf.relay2)
            //        this.Invoke(setControl, zone5Relay2ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, zone5Relay2ChkBox, (ushort)0);
            //    if (buf.drain1)
            //        this.Invoke(setControl, zone5Drain1ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, zone5Drain1ChkBox, (ushort)0);
            //    if (buf.drain2)
            //        this.Invoke(setControl, zone5Drain2ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, zone5Drain2ChkBox, (ushort)0);
            //    this.Invoke(setControl, zone5Relay1OnTxtBox, buf.relay1On);
            //    this.Invoke(setControl, zone5Relay1OffTxtBox, buf.relay1Off);
            //    this.Invoke(setControl, zone5Relay2OnTxtBox, buf.relay2On);
            //    this.Invoke(setControl, zone5Relay2OffTxtBox, buf.relay2Off);
            //    this.Invoke(setControl, zone5Drain1OnTxtBox, buf.drain1On);
            //    this.Invoke(setControl, zone5Drain1OffTxtBox, buf.drain1Off);
            //    this.Invoke(setControl, zone5Drain2OnTxtBox, buf.drain2On);
            //    this.Invoke(setControl, zone5Drain2OffTxtBox, buf.drain2Off);
            //}

            ////zone6
            //alarmStr = GetAlarmDevices(5);
            //if (alarmStr != "Error")
            //{
            //    buf = new TabConfig(alarmStr);
            //    if (buf.relay1)
            //        this.Invoke(setControl, zone6Relay1ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, zone6Relay1ChkBox, (ushort)0);
            //    if (buf.relay2)
            //        this.Invoke(setControl, zone6Relay2ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, zone6Relay2ChkBox, (ushort)0);
            //    if (buf.drain1)
            //        this.Invoke(setControl, zone6Drain1ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, zone6Drain1ChkBox, (ushort)0);
            //    if (buf.drain2)
            //        this.Invoke(setControl, zone6Drain2ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, zone6Drain2ChkBox, (ushort)0);
            //    this.Invoke(setControl, zone6Relay1OnTxtBox, buf.relay1On);
            //    this.Invoke(setControl, zone6Relay1OffTxtBox, buf.relay1Off);
            //    this.Invoke(setControl, zone6Relay2OnTxtBox, buf.relay2On);
            //    this.Invoke(setControl, zone6Relay2OffTxtBox, buf.relay2Off);
            //    this.Invoke(setControl, zone6Drain1OnTxtBox, buf.drain1On);
            //    this.Invoke(setControl, zone6Drain1OffTxtBox, buf.drain1Off);
            //    this.Invoke(setControl, zone6Drain2OnTxtBox, buf.drain2On);
            //    this.Invoke(setControl, zone6Drain2OffTxtBox, buf.drain2Off);
            //}

            ////zone7
            //alarmStr = GetAlarmDevices(6);
            //if (alarmStr != "Error")
            //{
            //    buf = new TabConfig(alarmStr);
            //    if (buf.relay1)
            //        this.Invoke(setControl, zone7Relay1ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, zone7Relay1ChkBox, (ushort)0);
            //    if (buf.relay2)
            //        this.Invoke(setControl, zone7Relay2ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, zone7Relay2ChkBox, (ushort)0);
            //    if (buf.drain1)
            //        this.Invoke(setControl, zone7Drain1ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, zone7Drain1ChkBox, (ushort)0);
            //    if (buf.drain2)
            //        this.Invoke(setControl, zone7Drain2ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, zone7Drain2ChkBox, (ushort)0);
            //    this.Invoke(setControl, zone7Relay1OnTxtBox, buf.relay1On);
            //    this.Invoke(setControl, zone7Relay1OffTxtBox, buf.relay1Off);
            //    this.Invoke(setControl, zone7Relay2OnTxtBox, buf.relay2On);
            //    this.Invoke(setControl, zone7Relay2OffTxtBox, buf.relay2Off);
            //    this.Invoke(setControl, zone7Drain1OnTxtBox, buf.drain1On);
            //    this.Invoke(setControl, zone7Drain1OffTxtBox, buf.drain1Off);
            //    this.Invoke(setControl, zone7Drain2OnTxtBox, buf.drain2On);
            //    this.Invoke(setControl, zone7Drain2OffTxtBox, buf.drain2Off);
            //}

            ////zone8
            //alarmStr = GetAlarmDevices(7);
            //if (alarmStr != "Error")
            //{
            //    buf = new TabConfig(alarmStr);
            //    if (buf.relay1)
            //        this.Invoke(setControl, zone8Relay1ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, zone8Relay1ChkBox, (ushort)0);
            //    if (buf.relay2)
            //        this.Invoke(setControl, zone8Relay2ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, zone8Relay2ChkBox, (ushort)0);
            //    if (buf.drain1)
            //        this.Invoke(setControl, zone8Drain1ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, zone8Drain1ChkBox, (ushort)0);
            //    if (buf.drain2)
            //        this.Invoke(setControl, zone8Drain2ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, zone8Drain2ChkBox, (ushort)0);
            //    this.Invoke(setControl, zone8Relay1OnTxtBox, buf.relay1On);
            //    this.Invoke(setControl, zone8Relay1OffTxtBox, buf.relay1Off);
            //    this.Invoke(setControl, zone8Relay2OnTxtBox, buf.relay2On);
            //    this.Invoke(setControl, zone8Relay2OffTxtBox, buf.relay2Off);
            //    this.Invoke(setControl, zone8Drain1OnTxtBox, buf.drain1On);
            //    this.Invoke(setControl, zone8Drain1OffTxtBox, buf.drain1Off);
            //    this.Invoke(setControl, zone8Drain2OnTxtBox, buf.drain2On);
            //    this.Invoke(setControl, zone8Drain2OffTxtBox, buf.drain2Off);
            //}

            //button
            alarmStr = GetAlarmDevices(4);
            if (alarmStr != "Error")
            {
                buf = new TabConfig(alarmStr);
                if (buf.relay1)
                    this.Invoke(setControl, btnRelay1ChkBox, (ushort)1);
                else
                    this.Invoke(setControl, btnRelay1ChkBox, (ushort)0);
                //if (buf.relay2)
                //    this.Invoke(setControl, btnRelay2ChkBox, (ushort)1);
                //else
                //    this.Invoke(setControl, btnRelay2ChkBox, (ushort)0);
                if (buf.drain1)
                    this.Invoke(setControl, btnDrain1ChkBox, (ushort)1);
                else
                    this.Invoke(setControl, btnDrain1ChkBox, (ushort)0);
                //if (buf.drain2)
                //    this.Invoke(setControl, btnDrain2ChkBox, (ushort)1);
                //else
                //    this.Invoke(setControl, btnDrain2ChkBox, (ushort)0);
                this.Invoke(setControl, btnRelay1OnTxtBox, buf.relay1On);
                this.Invoke(setControl, btnRelay1OffTxtBox, buf.relay1Off);
                //this.Invoke(setControl, btnRelay2OnTxtBox, buf.relay2On);
                //this.Invoke(setControl, btnRelay2OffTxtBox, buf.relay2Off);
                this.Invoke(setControl, btnDrain1OnTxtBox, buf.drain1On);
                this.Invoke(setControl, btnDrain1OffTxtBox, buf.drain1Off);
                //this.Invoke(setControl, btnDrain2OnTxtBox, buf.drain2On);
                //this.Invoke(setControl, btnDrain2OffTxtBox, buf.drain2Off);
            }

            ////reader
            //alarmStr = GetAlarmDevices(9);
            //if (alarmStr != "Error")
            //{
            //    buf = new TabConfig(alarmStr);
            //    if (buf.relay1)
            //        this.Invoke(setControl, readerRelay1ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, readerRelay1ChkBox, (ushort)0);
            //    if (buf.relay2)
            //        this.Invoke(setControl, readerRelay2ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, readerRelay2ChkBox, (ushort)0);
            //    if (buf.drain1)
            //        this.Invoke(setControl, readerDrain1ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, readerDrain1ChkBox, (ushort)0);
            //    if (buf.drain2)
            //        this.Invoke(setControl, readerDrain2ChkBox, (ushort)1);
            //    else
            //        this.Invoke(setControl, readerDrain2ChkBox, (ushort)0);
            //    this.Invoke(setControl, readerRelay1OnTxtBox, buf.relay1On);
            //    this.Invoke(setControl, readerRelay1OffTxtBox, buf.relay1Off);
            //    this.Invoke(setControl, readerRelay2OnTxtBox, buf.relay2On);
            //    this.Invoke(setControl, readerRelay2OffTxtBox, buf.relay2Off);
            //    this.Invoke(setControl, readerDrain1OnTxtBox, buf.drain1On);
            //    this.Invoke(setControl, readerDrain1OffTxtBox, buf.drain1Off);
            //    this.Invoke(setControl, readerDrain2OnTxtBox, buf.drain2On);
            //    this.Invoke(setControl, readerDrain2OffTxtBox, buf.drain2Off);
            //}

            //dtmf
            alarmStr = GetAlarmDevices(5);
            if (alarmStr != "Error")
            {
                buf = new TabConfig(alarmStr);
                if (buf.relay1)
                    this.Invoke(setControl, dtmfRelay1ChkBox, (ushort)1);
                else
                    this.Invoke(setControl, dtmfRelay1ChkBox, (ushort)0);
                //if (buf.relay2)
                //    this.Invoke(setControl, dtmfRelay2ChkBox, (ushort)1);
                //else
                //    this.Invoke(setControl, dtmfRelay2ChkBox, (ushort)0);
                if (buf.drain1)
                    this.Invoke(setControl, dtmfDrain1ChkBox, (ushort)1);
                else
                    this.Invoke(setControl, dtmfDrain1ChkBox, (ushort)0);
                //if (buf.drain2)
                //    this.Invoke(setControl, dtmfDrain2ChkBox, (ushort)1);
                //else
                //    this.Invoke(setControl, dtmfDrain2ChkBox, (ushort)0);
                this.Invoke(setControl, dtmfRelay1OnTxtBox, buf.relay1On);
                this.Invoke(setControl, dtmfRelay1OffTxtBox, buf.relay1Off);
                //this.Invoke(setControl, dtmfRelay2OnTxtBox, buf.relay2On);
                //this.Invoke(setControl, dtmfRelay2OffTxtBox, buf.relay2Off);
                this.Invoke(setControl, dtmfDrain1OnTxtBox, buf.drain1On);
                this.Invoke(setControl, dtmfDrain1OffTxtBox, buf.drain1Off);
                //this.Invoke(setControl, dtmfDrain2OnTxtBox, buf.drain2On);
                //this.Invoke(setControl, dtmfDrain2OffTxtBox, buf.drain2Off);
            }
        }

        void SetControl(Control ctrl, ushort val)
        {
            if (ctrl is CheckBox)
            {
                if (0 == val)
                    (ctrl as CheckBox).Checked = false;
                else
                    (ctrl as CheckBox).Checked = true;
            }
            else if (ctrl is TextBox)
            {
                (ctrl as TextBox).Text = val.ToString();
            }
        }

        private void zoneConfigReader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            readForm.Close();
        }

        private void writeTabConfigBtn_Click(object sender, EventArgs e)
        {
            readForm = new ReadingUserCardsForm();
            readForm.Show();
            zoneConfigWriter.RunWorkerAsync();
        }

        private void zoneConfigWriter_DoWork(object sender, DoWorkEventArgs e)
        {
            bool error = false;
            ushort relay1On = 0, relay2On = 0, drain1On = 0, drain2On = 0, relay1Off = 0, relay2Off = 0, drain1Off = 0, drain2Off = 0;
            
            //zone1
            byte activeDevices = 0;
            if (zone1Relay1ChkBox.Checked)
                activeDevices += 1;
            //if (zone1Relay2ChkBox.Checked)
            //    activeDevices += 2;
            if (zone1Drain1ChkBox.Checked)
                activeDevices += 2;
            //if (zone1Drain2ChkBox.Checked)
            //    activeDevices += 8;
            ushort.TryParse(zone1Relay1OnTxtBox.Text, out relay1On);
            //ushort.TryParse(zone1Relay2OnTxtBox.Text, out relay2On);
            ushort.TryParse(zone1Relay1OffTxtBox.Text, out relay1Off);
            //ushort.TryParse(zone1Relay2OffTxtBox.Text, out relay2Off);
            ushort.TryParse(zone1Drain1OnTxtBox.Text, out drain1On);
            //ushort.TryParse(zone1Drain2OnTxtBox.Text, out drain2On);
            ushort.TryParse(zone1Drain1OffTxtBox.Text, out drain1Off);
            //ushort.TryParse(zone1Drain2OffTxtBox.Text, out drain2Off);

            if (!SetAlarmDevices(0, activeDevices, relay1On, drain1On,
                relay1Off, drain1Off))
            {
                if (multiCommand)
                    errorFlag = true;
                else
                    error = true;
            }
            relay1On = 0; relay2On = 0; drain1On = 0; drain2On = 0; relay1Off = 0; relay2Off = 0; drain1Off = 0; drain2Off = 0;

            //zone2
            activeDevices = 0;
            if (zone2Relay1ChkBox.Checked)
                activeDevices += 1;
            //if (zone2Relay2ChkBox.Checked)
            //    activeDevices += 2;
            if (zone2Drain1ChkBox.Checked)
                activeDevices += 2;
            //if (zone2Drain2ChkBox.Checked)
            //    activeDevices += 8;
            ushort.TryParse(zone2Relay1OnTxtBox.Text, out relay1On);
            //ushort.TryParse(zone2Relay2OnTxtBox.Text, out relay2On);
            ushort.TryParse(zone2Relay1OffTxtBox.Text, out relay1Off);
            //ushort.TryParse(zone2Relay2OffTxtBox.Text, out relay2Off);
            ushort.TryParse(zone2Drain1OnTxtBox.Text, out drain1On);
            //ushort.TryParse(zone2Drain2OnTxtBox.Text, out drain2On);
            ushort.TryParse(zone2Drain1OffTxtBox.Text, out drain1Off);
            //ushort.TryParse(zone2Drain2OffTxtBox.Text, out drain2Off);

            if (!SetAlarmDevices(1, activeDevices, relay1On, drain1On,
                relay1Off, drain1Off))
            {
                if (multiCommand)
                    errorFlag = true;
                else
                    error = true;
            }
            relay1On = 0; relay2On = 0; drain1On = 0; drain2On = 0; relay1Off = 0; relay2Off = 0; drain1Off = 0; drain2Off = 0;

            //zone3
            activeDevices = 0;
            if (zone3Relay1ChkBox.Checked)
                activeDevices += 1;
            //if (zone3Relay2ChkBox.Checked)
            //    activeDevices += 2;
            if (zone3Drain1ChkBox.Checked)
                activeDevices += 2;
            //if (zone3Drain2ChkBox.Checked)
            //    activeDevices += 8;
            ushort.TryParse(zone3Relay1OnTxtBox.Text, out relay1On);
            //ushort.TryParse(zone3Relay2OnTxtBox.Text, out relay2On);
            ushort.TryParse(zone3Relay1OffTxtBox.Text, out relay1Off);
            //ushort.TryParse(zone3Relay2OffTxtBox.Text, out relay2Off);
            ushort.TryParse(zone3Drain1OnTxtBox.Text, out drain1On);
            //ushort.TryParse(zone3Drain2OnTxtBox.Text, out drain2On);
            ushort.TryParse(zone3Drain1OffTxtBox.Text, out drain1Off);
            //ushort.TryParse(zone3Drain2OffTxtBox.Text, out drain2Off);

            if (!SetAlarmDevices(2, activeDevices, relay1On, drain1On,
                relay1Off, drain1Off))
            {
                if (multiCommand)
                    errorFlag = true;
                else
                    error = true;
            }
            relay1On = 0; relay2On = 0; drain1On = 0; drain2On = 0; relay1Off = 0; relay2Off = 0; drain1Off = 0; drain2Off = 0;

            //zone4
            activeDevices = 0;
            if (zone4Relay1ChkBox.Checked)
                activeDevices += 1;
            //if (zone4Relay2ChkBox.Checked)
            //    activeDevices += 2;
            if (zone4Drain1ChkBox.Checked)
                activeDevices += 2;
            //if (zone4Drain2ChkBox.Checked)
            //    activeDevices += 8;
            ushort.TryParse(zone4Relay1OnTxtBox.Text, out relay1On);
            //ushort.TryParse(zone4Relay2OnTxtBox.Text, out relay2On);
            ushort.TryParse(zone4Relay1OffTxtBox.Text, out relay1Off);
            //ushort.TryParse(zone4Relay2OffTxtBox.Text, out relay2Off);
            ushort.TryParse(zone4Drain1OnTxtBox.Text, out drain1On);
            //ushort.TryParse(zone4Drain2OnTxtBox.Text, out drain2On);
            ushort.TryParse(zone4Drain1OffTxtBox.Text, out drain1Off);
            //ushort.TryParse(zone4Drain2OffTxtBox.Text, out drain2Off);

            if (!SetAlarmDevices(3, activeDevices, relay1On, drain1On,
                relay1Off, drain1Off))
            {
                if (multiCommand)
                    errorFlag = true;
                else
                    error = true;
            }
            relay1On = 0; relay2On = 0; drain1On = 0; drain2On = 0; relay1Off = 0; relay2Off = 0; drain1Off = 0; drain2Off = 0;

            ////zone5
            //activeDevices = 0;
            //if (zone5Relay1ChkBox.Checked)
            //    activeDevices += 1;
            //if (zone5Relay2ChkBox.Checked)
            //    activeDevices += 2;
            //if (zone5Drain1ChkBox.Checked)
            //    activeDevices += 4;
            //if (zone5Drain2ChkBox.Checked)
            //    activeDevices += 8;
            //ushort.TryParse(zone5Relay1OnTxtBox.Text, out relay1On);
            //ushort.TryParse(zone5Relay2OnTxtBox.Text, out relay2On);
            //ushort.TryParse(zone5Relay1OffTxtBox.Text, out relay1Off);
            //ushort.TryParse(zone5Relay2OffTxtBox.Text, out relay2Off);
            //ushort.TryParse(zone5Drain1OnTxtBox.Text, out drain1On);
            //ushort.TryParse(zone5Drain2OnTxtBox.Text, out drain2On);
            //ushort.TryParse(zone5Drain1OffTxtBox.Text, out drain1Off);
            //ushort.TryParse(zone5Drain2OffTxtBox.Text, out drain2Off);

            //if (!SetAlarmDevices(4, activeDevices, relay1On, relay2On, drain1On, drain2On,
            //    relay1Off, relay2Off, drain1Off, drain2Off))
            //{
            //    if (multiCommand)
            //        errorFlag = true;
            //    else
            //        error = true;
            //}
            //relay1On = 0; relay2On = 0; drain1On = 0; drain2On = 0; relay1Off = 0; relay2Off = 0; drain1Off = 0; drain2Off = 0;

            ////zone6
            //activeDevices = 0;
            //if (zone6Relay1ChkBox.Checked)
            //    activeDevices += 1;
            //if (zone6Relay2ChkBox.Checked)
            //    activeDevices += 2;
            //if (zone6Drain1ChkBox.Checked)
            //    activeDevices += 4;
            //if (zone6Drain2ChkBox.Checked)
            //    activeDevices += 8;
            //ushort.TryParse(zone6Relay1OnTxtBox.Text, out relay1On);
            //ushort.TryParse(zone6Relay2OnTxtBox.Text, out relay2On);
            //ushort.TryParse(zone6Relay1OffTxtBox.Text, out relay1Off);
            //ushort.TryParse(zone6Relay2OffTxtBox.Text, out relay2Off);
            //ushort.TryParse(zone6Drain1OnTxtBox.Text, out drain1On);
            //ushort.TryParse(zone6Drain2OnTxtBox.Text, out drain2On);
            //ushort.TryParse(zone6Drain1OffTxtBox.Text, out drain1Off);
            //ushort.TryParse(zone6Drain2OffTxtBox.Text, out drain2Off);

            //if (!SetAlarmDevices(5, activeDevices, relay1On, relay2On, drain1On, drain2On,
            //    relay1Off, relay2Off, drain1Off, drain2Off))
            //{
            //    if (multiCommand)
            //        errorFlag = true;
            //    else
            //        error = true;
            //}
            //relay1On = 0; relay2On = 0; drain1On = 0; drain2On = 0; relay1Off = 0; relay2Off = 0; drain1Off = 0; drain2Off = 0;

            ////zone7
            //activeDevices = 0;
            //if (zone7Relay1ChkBox.Checked)
            //    activeDevices += 1;
            //if (zone7Relay2ChkBox.Checked)
            //    activeDevices += 2;
            //if (zone7Drain1ChkBox.Checked)
            //    activeDevices += 4;
            //if (zone7Drain2ChkBox.Checked)
            //    activeDevices += 8;
            //ushort.TryParse(zone7Relay1OnTxtBox.Text, out relay1On);
            //ushort.TryParse(zone7Relay2OnTxtBox.Text, out relay2On);
            //ushort.TryParse(zone7Relay1OffTxtBox.Text, out relay1Off);
            //ushort.TryParse(zone7Relay2OffTxtBox.Text, out relay2Off);
            //ushort.TryParse(zone7Drain1OnTxtBox.Text, out drain1On);
            //ushort.TryParse(zone7Drain2OnTxtBox.Text, out drain2On);
            //ushort.TryParse(zone7Drain1OffTxtBox.Text, out drain1Off);
            //ushort.TryParse(zone7Drain2OffTxtBox.Text, out drain2Off);

            //if (!SetAlarmDevices(6, activeDevices, relay1On, relay2On, drain1On, drain2On,
            //    relay1Off, relay2Off, drain1Off, drain2Off))
            //{
            //    if (multiCommand)
            //        errorFlag = true;
            //    else
            //        error = true;
            //}
            //relay1On = 0; relay2On = 0; drain1On = 0; drain2On = 0; relay1Off = 0; relay2Off = 0; drain1Off = 0; drain2Off = 0;

            ////zone8
            //activeDevices = 0;
            //if (zone8Relay1ChkBox.Checked)
            //    activeDevices += 1;
            //if (zone8Relay2ChkBox.Checked)
            //    activeDevices += 2;
            //if (zone8Drain1ChkBox.Checked)
            //    activeDevices += 4;
            //if (zone8Drain2ChkBox.Checked)
            //    activeDevices += 8;
            //ushort.TryParse(zone8Relay1OnTxtBox.Text, out relay1On);
            //ushort.TryParse(zone8Relay2OnTxtBox.Text, out relay2On);
            //ushort.TryParse(zone8Relay1OffTxtBox.Text, out relay1Off);
            //ushort.TryParse(zone8Relay2OffTxtBox.Text, out relay2Off);
            //ushort.TryParse(zone8Drain1OnTxtBox.Text, out drain1On);
            //ushort.TryParse(zone8Drain2OnTxtBox.Text, out drain2On);
            //ushort.TryParse(zone8Drain1OffTxtBox.Text, out drain1Off);
            //ushort.TryParse(zone8Drain2OffTxtBox.Text, out drain2Off);

            //if (!SetAlarmDevices(7, activeDevices, relay1On, relay2On, drain1On, drain2On,
            //    relay1Off, relay2Off, drain1Off, drain2Off))
            //{
            //    if (multiCommand)
            //        errorFlag = true;
            //    else
            //        error = true;
            //}
            //relay1On = 0; relay2On = 0; drain1On = 0; drain2On = 0; relay1Off = 0; relay2Off = 0; drain1Off = 0; drain2Off = 0;

            //button
            activeDevices = 0;
            if (btnRelay1ChkBox.Checked)
                activeDevices += 1;
            //if (btnRelay2ChkBox.Checked)
            //    activeDevices += 2;
            if (btnDrain1ChkBox.Checked)
                activeDevices += 2;
            //if (btnDrain2ChkBox.Checked)
            //    activeDevices += 8;
            ushort.TryParse(btnRelay1OnTxtBox.Text, out relay1On);
            //ushort.TryParse(btnRelay2OnTxtBox.Text, out relay2On);
            ushort.TryParse(btnRelay1OffTxtBox.Text, out relay1Off);
            //ushort.TryParse(btnRelay2OffTxtBox.Text, out relay2Off);
            ushort.TryParse(btnDrain1OnTxtBox.Text, out drain1On);
            //ushort.TryParse(btnDrain2OnTxtBox.Text, out drain2On);
            ushort.TryParse(btnDrain1OffTxtBox.Text, out drain1Off);
            //ushort.TryParse(btnDrain2OffTxtBox.Text, out drain2Off);

            if (!SetAlarmDevices(4, activeDevices, relay1On, drain1On,
                relay1Off, drain1Off))
            {
                if (multiCommand)
                    errorFlag = true;
                else
                    error = true;
            }
            relay1On = 0; relay2On = 0; drain1On = 0; drain2On = 0; relay1Off = 0; relay2Off = 0; drain1Off = 0; drain2Off = 0;

            ////reader
            //activeDevices = 0;
            //if (readerRelay1ChkBox.Checked)
            //    activeDevices += 1;
            //if (readerRelay2ChkBox.Checked)
            //    activeDevices += 2;
            //if (readerDrain1ChkBox.Checked)
            //    activeDevices += 4;
            //if (readerDrain2ChkBox.Checked)
            //    activeDevices += 8;
            //ushort.TryParse(readerRelay1OnTxtBox.Text, out relay1On);
            //ushort.TryParse(readerRelay2OnTxtBox.Text, out relay2On);
            //ushort.TryParse(readerRelay1OffTxtBox.Text, out relay1Off);
            //ushort.TryParse(readerRelay2OffTxtBox.Text, out relay2Off);
            //ushort.TryParse(readerDrain1OnTxtBox.Text, out drain1On);
            //ushort.TryParse(readerDrain2OnTxtBox.Text, out drain2On);
            //ushort.TryParse(readerDrain1OffTxtBox.Text, out drain1Off);
            //ushort.TryParse(readerDrain2OffTxtBox.Text, out drain2Off);

            //if (!SetAlarmDevices(9, activeDevices, relay1On, relay2On, drain1On, drain2On,
            //    relay1Off, relay2Off, drain1Off, drain2Off))
            //{
            //    if (multiCommand)
            //        errorFlag = true;
            //    else
            //        error = true;
            //}
            //relay1On = 0; relay2On = 0; drain1On = 0; drain2On = 0; relay1Off = 0; relay2Off = 0; drain1Off = 0; drain2Off = 0;

            //dtmf
            activeDevices = 0;
            if (dtmfRelay1ChkBox.Checked)
                activeDevices += 1;
            //if (dtmfRelay2ChkBox.Checked)
            //    activeDevices += 2;
            if (dtmfDrain1ChkBox.Checked)
                activeDevices += 2;
            //if (dtmfDrain2ChkBox.Checked)
            //    activeDevices += 8;
            ushort.TryParse(dtmfRelay1OnTxtBox.Text, out relay1On);
            //ushort.TryParse(dtmfRelay2OnTxtBox.Text, out relay2On);
            ushort.TryParse(dtmfRelay1OffTxtBox.Text, out relay1Off);
            //ushort.TryParse(dtmfRelay2OffTxtBox.Text, out relay2Off);
            ushort.TryParse(dtmfDrain1OnTxtBox.Text, out drain1On);
            //ushort.TryParse(dtmfDrain2OnTxtBox.Text, out drain2On);
            ushort.TryParse(dtmfDrain1OffTxtBox.Text, out drain1Off);
            //ushort.TryParse(dtmfDrain2OffTxtBox.Text, out drain2Off);

            if (!SetAlarmDevices(5, activeDevices, relay1On, drain1On,
                relay1Off, drain1Off))
            {
                if (multiCommand)
                    errorFlag = true;
                else
                    error = true;
            }
            if (e != null)
                e.Result = error;
        }

        private void zoneConfigWriter_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            readForm.Close();
            if (!multiCommand)
            {
                if ((bool)e.Result)
                {
                    MessageBox.Show("Во время установки параметров исполнительных устройств произошла ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                    MessageBox.Show("Параметры исполнительных устройств успешно установлены", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void readAlarmSourcesBtn_Click(object sender, EventArgs e)
        {
            byte alarmSources = byte.Parse(GetAlarmSources(), System.Globalization.NumberStyles.HexNumber);
            if (0 == (alarmSources & 1))
                this.Invoke(setControl, onGuardDtmfChkBox, (ushort)0);
            else
                this.Invoke(setControl, onGuardDtmfChkBox, (ushort)1);
            if (0 == (alarmSources & 2))
                this.Invoke(setControl, offGuardDtmfChkBox, (ushort)0);
            else
                this.Invoke(setControl, offGuardDtmfChkBox, (ushort)1);
            //if (0 == (alarmSources & 4))
            //    this.Invoke(setControl, onGuardReaderChkBox, (ushort)0);
            //else
            //    this.Invoke(setControl, onGuardReaderChkBox, (ushort)1);
            //if (0 == (alarmSources & 8))
            //    this.Invoke(setControl, offGuardReaderChkBox, (ushort)0);
            //else
            //    this.Invoke(setControl, offGuardReaderChkBox, (ushort)1);
            if (0 == (alarmSources & 4))
                this.Invoke(setControl, onGuardBtnChkBox, (ushort)0);
            else
                this.Invoke(setControl, onGuardBtnChkBox, (ushort)1);
            if (0 == (alarmSources & 8))
                this.Invoke(setControl, offGuardBtnChkBox, (ushort)0);
            else
                this.Invoke(setControl, offGuardBtnChkBox, (ushort)1);
        }

        private void writeAlarmSourcesBtn_Click(object sender, EventArgs e)
        {
            byte alarmSources = 0;
            if (onGuardDtmfChkBox.Checked)
                alarmSources += 1;
            if (offGuardDtmfChkBox.Checked)
                alarmSources += 2;
            //if (onGuardReaderChkBox.Checked)
            //    alarmSources += 4;
            //if (offGuardReaderChkBox.Checked)
            //    alarmSources += 8;
            if (onGuardBtnChkBox.Checked)
                alarmSources += 4;
            if (offGuardBtnChkBox.Checked)
                alarmSources += 8;
            if (!SetAlarmSources(alarmSources))
            {
                if (multiCommand)
                    errorFlag = true;
                else
                    MessageBox.Show("Во время установки параметров активации контроллера произошла ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (!multiCommand)
                    MessageBox.Show("Параметры активации контроллера успешно установлены", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void deviceInfoBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show(string.Format("{0}\r\nСерийный номер: {1}", GetDeviceVersion(), GetDeviceId()), "Информация о устройстве", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void readAllBtn_Click(object sender, EventArgs e)
        {
            readForm = new ReadingUserCardsForm();
            readForm.Show();
            readAllWorker.RunWorkerAsync();
        }

        private void readAllWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            readNum1Btn_Click(null, null);
            readNum2Btn_Click(null, null);
            readNum3Btn_Click(null, null);
            readNum4Btn_Click(null, null);
            readNum5Btn_Click(null, null);
            readCallParamsBtn_Click(null, null);
            readActivationTimeBtn_Click(null, null);
            readDeactivationTimeBtn_Click(null, null);
            readActiveZonesBtn_Click(null, null);
            readAlarmSourcesBtn_Click(null, null);
            zoneConfigReader_DoWork(null, null);
        }

        private void setDefaultsBtn_Click(object sender, EventArgs e)
        {
            if (SetDefaults())
                MessageBox.Show("Параметры по умолчанию успешно установлены", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SetControlString(Control ctrl, string txt)
        {
            if (ctrl is MaskedTextBox)
                (ctrl as MaskedTextBox).Text = txt;
            else
                throw new ArgumentException("The control could not be recognized");
        }

        private void readAllWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            readForm.Close();
        }

        private void writeAllBtn_Click(object sender, EventArgs e)
        {
            multiCommand = true;
            errorFlag = false;
            readForm = new ReadingUserCardsForm();
            readForm.Show();
            writeAllWorker.RunWorkerAsync();
        }

        private void writeAllWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            writeNum1Btn_Click(null, null);
            writeNum2Btn_Click(null, null);
            writeNum3Btn_Click(null, null);
            writeNum4Btn_Click(null, null);
            writeNum5Btn_Click(null, null);
            writeCallParamsBtn_Click(null, null);
            writeActivationTimeBtn_Click(null, null);
            writeDeactivationTimeBtn_Click(null, null);
            writeActiveZonesBtn_Click(null, null);
            writeAlarmSourcesBtn_Click(null, null);
            zoneConfigWriter_DoWork(null, null);
        }

        private void writeAllWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            readForm.Close();
            if (errorFlag)
                MessageBox.Show("Во время установки параметров произошла ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                MessageBox.Show("Конфигурация успешно установлена", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            multiCommand = false;
        }

        private void portConfigBtn_Click(object sender, EventArgs e)
        {
            PortSelectionForm portSelector = new PortSelectionForm();
            if (portSelector.ShowDialog(this) == DialogResult.OK)
            {
                if (serialPort1 == null)
                {
                    serialPort1 = new System.IO.Ports.SerialPort(portSelector.SelectedPort, 19200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                    serialPort1.NewLine = "\r\n";
                    serialPort1.Encoding = Encoding.UTF8;
                    listener = new SerialPortListener(serialPort1);
                }
                else
                {
                    if (serialPort1.PortName != portSelector.SelectedPort)
                    {
                        serialPort1 = new System.IO.Ports.SerialPort(portSelector.SelectedPort, 19200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                        serialPort1.NewLine = "\r\n";
                        serialPort1.Encoding = Encoding.UTF8;
                        listener = new SerialPortListener(serialPort1);
                    }
                }
                try
                {
                    serialPort1.Open();
                }
                catch
                {
                    MessageBox.Show("Работа с выбраным портом невозможна", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GsmRingerProConfig_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            (new AboutBox()).Show();
            e.Cancel = true;
        }

        private void changePasswordBtn_Click(object sender, EventArgs e)
        {
            NewPasswordForm passForm = new NewPasswordForm();
            if (passForm.ShowDialog() == DialogResult.OK)
            {
                if (SetNewPassword(passForm.Password))
                    MessageBox.Show("Новый пароль успешно установлен", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Во время установки пароля произошла ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    delegate void VoidControlShort(Control ctrl, ushort val);
    delegate void VoidControlString(Control ctrl, string txt);
    
    public class TabConfig
    {
        public bool relay1 = false;
        public bool relay2 = false;
        public bool drain1 = false;
        public bool drain2 = false;

        public ushort relay1On = 0;
        public ushort relay1Off = 0;
        public ushort relay2On = 0;
        public ushort relay2Off = 0;
        public ushort drain1On = 0;
        public ushort drain1Off = 0;
        public ushort drain2On = 0;
        public ushort drain2Off = 0;

        public TabConfig(string str)
        {
            byte activeDevices = byte.Parse(str.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            if (0 == (activeDevices & 1))
                relay1 = false;
            else
                relay1 = true;
            //if (0 == (activeDevices & 2))
            //    relay2 = false;
            //else
            //    relay2 = true;
            if (0 == (activeDevices & 2))
                drain1 = false;
            else
                drain1 = true;
            //if (0 == (activeDevices & 8))
            //    drain2 = false;
            //else
            //    drain2 = true;
            relay1On = ushort.Parse(str.Substring(2, 4), System.Globalization.NumberStyles.HexNumber);
            //relay2On = ushort.Parse(str.Substring(6, 4), System.Globalization.NumberStyles.HexNumber);
            drain1On = ushort.Parse(str.Substring(6, 4), System.Globalization.NumberStyles.HexNumber);
            //drain2On = ushort.Parse(str.Substring(14, 4), System.Globalization.NumberStyles.HexNumber);
            relay1Off = ushort.Parse(str.Substring(10, 4), System.Globalization.NumberStyles.HexNumber);
            //relay2Off = ushort.Parse(str.Substring(22, 4), System.Globalization.NumberStyles.HexNumber);
            drain1Off = ushort.Parse(str.Substring(14, 4), System.Globalization.NumberStyles.HexNumber);
            //drain2Off = ushort.Parse(str.Substring(30, 4), System.Globalization.NumberStyles.HexNumber);
        }
    }
}
