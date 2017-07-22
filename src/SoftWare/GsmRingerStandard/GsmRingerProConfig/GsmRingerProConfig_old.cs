using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GsmRingerProConfig
{
    public partial class GsmRingerProConfig : Form
    {
        //const int NumOfUsers = 20;

        Dictionary<string, string> cardRecs;
        SerialPortListener listener;
        string functionResult;
        bool commandProccessing;
        int previousTabIndex;

        CheckBox relay1CheckBox, relay2CheckBox, openDrain1CheckBox, openDrain2CheckBox;
        TextBox relay1OnTxt, relay2OnTxt, relay1OffTxt, relay2OffTxt, openDrain1OnTxt, openDrain2OnTxt, openDrain1OffTxt, openDrain2OffTxt;

        ReadingUserCardsForm readForm;
        CardAdminForm cardForm;
        VoidControlShort setControl;
        
        public GsmRingerProConfig()
        {
            InitializeComponent();
        }

        private void GsmRingerProConfig_Load(object sender, EventArgs e)
        {
            setControl = new VoidControlShort(SetControl);
            portNameBox.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            if (System.IO.File.Exists("config.ini"))
            {
                string portName = System.IO.File.ReadAllText("config.ini");
                if (portNameBox.Items.Contains(portName))
                {
                    portNameBox.SelectedItem = portName;
                    serialPort1 = new System.IO.Ports.SerialPort((string)portNameBox.SelectedItem, 9600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                    serialPort1.NewLine = "\r\n";
                    serialPort1.Encoding = Encoding.UTF8;
                    listener = new SerialPortListener(serialPort1);
                }
            }

            if (System.IO.File.Exists("users.cfg"))
            {
                try
                {
                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    cardRecs = (Dictionary<string, string>)bf.Deserialize(System.IO.File.OpenRead("users.cfg"));
                }
#pragma warning disable 168
                catch (System.Runtime.Serialization.SerializationException except)
                {
                    cardRecs = new Dictionary<string, string>();
                }
#pragma warning restore 168
            }
            else
            {
                cardRecs = new Dictionary<string, string>();
            }
        }

        private void SendCommand(string cmd)
        {
            try
            {
                if (!serialPort1.IsOpen)
                    serialPort1.Open();
                serialPort1.WriteLine("$" + cmd + "*" + SerialPortListener.CalculateCRC(cmd));
            }
#pragma warning disable 168
            catch (NullReferenceException except)
            {
                MessageBox.Show("Сначала выберите порт", "Не выбран COM-порт", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#pragma warning restore 168
        }

        private void portNameBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.IO.File.WriteAllText("config.ini", (string)portNameBox.SelectedItem);
            serialPort1 = new System.IO.Ports.SerialPort((string)portNameBox.SelectedItem, 9600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
            serialPort1.Encoding = Encoding.UTF8;
            listener = new SerialPortListener(serialPort1);
        }

        private void InitializeDelegates()
        {
        }

        private string GetDeviceId()
        {
            commandProccessing = true;
            listener.DeviceIdReceived += new DeviceDataReceivedEventHandler(listener_DeviceIdReceived);
            SendCommand("PCHND");
            
            while (commandProccessing)
            {
            }

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
            while (commandProccessing)
            {
            }

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
            while (commandProccessing)
            {
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
            while (commandProccessing)
            {
            }

            if (functionResult == "OK")
                return true;
            else
                return false;

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
            while (commandProccessing)
            {
            }
            return functionResult;
        }

        void listener_GetUserNumberReceived(object sender, StringDataReceivedEventArgs e)
        {
            functionResult = e.Message.Substring(1);
            listener.GetUserNumberReceived -= listener_GetUserNumberReceived;
            commandProccessing = false;
        }

        bool SetAlarmDevices(byte zoneNum, byte activateDevices, short timeOnRelay1, short timeOnRelay2, short timeOnCol1, short timeOnCol2, short timeOffRelay1, short timeOffRelay2, short timeOffCol1, short timeOffCol2)
        {
            commandProccessing = true;
            listener.SetAlarmDevicesOKReceived += new DeviceDataReceivedEventHandler(listener_SetAlarmDevicesOKReceived);
            SendCommand(string.Format("PCSTE {0}{1}{2}{3}{4}{5}{6}{7}{8}{9}", zoneNum.ToString("X2"), activateDevices.ToString("X2"), timeOnRelay1.ToString("X4"), timeOnRelay2.ToString("X4"), timeOnCol1.ToString("X4"), timeOnCol2.ToString("X4"), timeOffRelay1.ToString("X4"), timeOffRelay2.ToString("X4"), timeOffCol1.ToString("X4"), timeOffCol2.ToString("X4")));
            while (commandProccessing)
            {
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
            while(commandProccessing)
            {
            }

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
            while (commandProccessing)
            {
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
            while (commandProccessing)
            {
            }

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
            while (commandProccessing)
            {
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
            while (commandProccessing)
            {
            }

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
            while (commandProccessing)
            {
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
            while (commandProccessing)
            {
            }

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
            while (commandProccessing)
            {
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
            while (commandProccessing)
            {
            }

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
            while (commandProccessing)
            {
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
            while (commandProccessing)
            {
            }

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
            while (commandProccessing)
            {
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
            while (commandProccessing)
            {
            }

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
            try
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bf.Serialize(System.IO.File.Open("users.cfg", System.IO.FileMode.Create), cardRecs);
            }
#pragma warning disable 168
            catch (System.Runtime.Serialization.SerializationException except)
            {
            }
            catch (System.UnauthorizedAccessException except)
            {
            }
            catch (System.IO.IOException except)
            {
            }
#pragma warning restore 168
        }

        private void cardAdminBtn_Click(object sender, EventArgs e)
        {
            readForm = new ReadingUserCardsForm();
            readForm.Show();
            cardRecsSyncronizer.RunWorkerAsync();
        }

        private void cardRecsSyncronizer_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < 20; i++)
            {
                string cardId = GetUserCard((byte)i);
                if (cardId == "FFFFFF")
                {
                }
                else if (!cardRecs.ContainsKey(cardId))
                    cardRecs.Add(cardId, "Пользователь" + i.ToString());
            }
        }

        private void cardRecsSyncronizer_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            readForm.Close();
            readForm = null;
            cardForm = new CardAdminForm(cardRecs);
            if (cardForm.ShowDialog(this) == DialogResult.OK)
            {
                readForm = new ReadingUserCardsForm();
                readForm.Show();
                cardRecsWriter.RunWorkerAsync();
            }
        }

        public SerialPortListener Listener
        {
            get
            {
                return listener;
            }
        }

        private void cardRecsWriter_DoWork(object sender, DoWorkEventArgs e)
        {
            byte count = 0;
            foreach (KeyValuePair<string, string> element in cardRecs)
            {
                SetUserCard(count, element.Key);
                count++;
            }
            for (byte i = count; i < 20; i++)
            {
                SetUserCard(i, "FFFFFF");
            }
        }

        private void cardRecsWriter_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            readForm.Close();
            readForm = null;
        }

        private void readCallParamsBtn_Click(object sender, EventArgs e)
        {
            string callParams = GetCallParams();
            byte buf = 0;
            if (byte.TryParse(callParams.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out buf))
            {
                if (0 == (buf & 1))
                    smsUser1CheckBox.Checked = false;
                else
                    smsUser1CheckBox.Checked = true;
                if (0 == (buf & 2))
                    smsUser2CheckBox.Checked = false;
                else
                    smsUser2CheckBox.Checked = true;
                if (0 == (buf & 4))
                    smsUser3CheckBox.Checked = false;
                else
                    smsUser3CheckBox.Checked = true;
                if (0 == (buf & 8))
                    smsUser4CheckBox.Checked = false;
                else
                    smsUser4CheckBox.Checked = true;
                if (0 == (buf & 16))
                    smsUser5CheckBox.Checked = false;
                else
                    smsUser5CheckBox.Checked = true;
            }
            if (byte.TryParse(callParams.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out buf))
            {
                if (0 == (buf & 1))
                    callUser1CheckBox.Checked = false;
                else
                    callUser1CheckBox.Checked = true;
                if (0 == (buf & 2))
                    callUser2CheckBox.Checked = false;
                else
                    callUser2CheckBox.Checked = true;
                if (0 == (buf & 4))
                    callUser3CheckBox.Checked = false;
                else
                    callUser3CheckBox.Checked = true;
                if (0 == (buf & 8))
                    callUser4CheckBox.Checked = false;
                else
                    callUser4CheckBox.Checked = true;
                if (0 == (buf & 16))
                    callUser5CheckBox.Checked = false;
                else
                    callUser5CheckBox.Checked = true;
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
                MessageBox.Show("Во время установки параметров оповещения произошла ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void readNum1Btn_Click(object sender, EventArgs e)
        {
            num1Box.Text = GetUserNumber(0);
        }

        private void writeNum1Btn_Click(object sender, EventArgs e)
        {
            string num = num1Box.Text.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            if (num.Length == 10)
                SetUserNumber(0, num);
            else
                MessageBox.Show(String.Format("Номер {0} введен неправильно", 1), "Ошибка ввода номера", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void readNum2Btn_Click(object sender, EventArgs e)
        {
            num2Box.Text = GetUserNumber(1);
        }

        private void readNum3Btn_Click(object sender, EventArgs e)
        {
            num3Box.Text = GetUserNumber(2);
        }

        private void readNum4Btn_Click(object sender, EventArgs e)
        {
            num4Box.Text = GetUserNumber(3);
        }

        private void readNum5Btn_Click(object sender, EventArgs e)
        {
            num5Box.Text = GetUserNumber(4);
        }

        private void writeNum2Btn_Click(object sender, EventArgs e)
        {
            string num = num2Box.Text.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            if (num.Length == 10)
                SetUserNumber(1, num);
            else
                MessageBox.Show(String.Format("Номер {0} введен неправильно", 2), "Ошибка ввода номера", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private void writeNum3Btn_Click(object sender, EventArgs e)
        {
            string num = num3Box.Text.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            if (num.Length == 10)
                SetUserNumber(2, num);
            else
                MessageBox.Show(String.Format("Номер {0} введен неправильно", 3), "Ошибка ввода номера", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private void writeNum4Btn_Click(object sender, EventArgs e)
        {
            string num = num4Box.Text.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            if (num.Length == 10)
                SetUserNumber(3, num);
            else
                MessageBox.Show(String.Format("Номер {0} введен неправильно", 4), "Ошибка ввода номера", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private void writeNum5Btn_Click(object sender, EventArgs e)
        {
            string num = num5Box.Text.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            if (num.Length == 10)
                SetUserNumber(4, num);
            else
                MessageBox.Show(String.Format("Номер {0} введен неправильно", 5), "Ошибка ввода номера", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private void readActivationTimeBtn_Click(object sender, EventArgs e)
        {
            byte time = 0;
            if (byte.TryParse(GetActivationTime(), System.Globalization.NumberStyles.HexNumber, null, out time))
            {
                activationTimeBox.Text = time.ToString();
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
                    SetActivationTime(time);
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
                deactivationTimeBox.Text = time.ToString();
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
                    SetDeactivationTime(time);
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
                    zone1EnableCheckBox.Checked = false;
                else
                    zone1EnableCheckBox.Checked = true;
                if (0 == (zoneByte & 2))
                    zone2EnableCheckBox.Checked = false;
                else
                    zone2EnableCheckBox.Checked = true;
                if (0 == (zoneByte & 4))
                    zone3EnableCheckBox.Checked = false;
                else
                    zone3EnableCheckBox.Checked = true;
                if (0 == (zoneByte & 8))
                    zone4EnableCheckBox.Checked = false;
                else
                    zone4EnableCheckBox.Checked = true;
                if (0 == (zoneByte & 16))
                    zone5EnableCheckBox.Checked = false;
                else
                    zone5EnableCheckBox.Checked = true;
                if (0 == (zoneByte & 32))
                    zone6EnableCheckBox.Checked = false;
                else
                    zone6EnableCheckBox.Checked = true;
                if (0 == (zoneByte & 64))
                    zone7EnableCheckBox.Checked = false;
                else
                    zone7EnableCheckBox.Checked = true;
                if (0 == (zoneByte & 128))
                    zone8EnableCheckBox.Checked = false;
                else
                    zone8EnableCheckBox.Checked = true;
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
            if (zone5EnableCheckBox.Checked)
                zoneByte += 16;
            if (zone6EnableCheckBox.Checked)
                zoneByte += 32;
            if (zone7EnableCheckBox.Checked)
                zoneByte += 64;
            if (zone8EnableCheckBox.Checked)
                zoneByte += 128;
            if (!SetActiveZones(zoneByte))
                MessageBox.Show("Error");
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
            buf = new TabConfig(GetAlarmDevices(0));
            if (buf.relay1)
                this.Invoke(setControl, zone1Relay1ChkBox, (short)1);
            else
                this.Invoke(setControl, zone1Relay1ChkBox, (short)0);
            if (buf.relay2)
                this.Invoke(setControl, zone1Relay2ChkBox, (short)1);
            else
                this.Invoke(setControl, zone1Relay2ChkBox, (short)0);
            if (buf.drain1)
                this.Invoke(setControl, zone1Drain1ChkBox, (short)1);
            else
                this.Invoke(setControl, zone1Drain1ChkBox, (short)0);
            if (buf.drain2)
                this.Invoke(setControl, zone1Drain2ChkBox, (short)1);
            else
                this.Invoke(setControl, zone1Drain2ChkBox, (short)0);
            this.Invoke(setControl, zone1Relay1OnTxtBox, buf.relay1On);
            this.Invoke(setControl, zone1Relay1OffTxtBox, buf.relay1Off);
            this.Invoke(setControl, zone1Relay2OnTxtBox, buf.relay2On);
            this.Invoke(setControl, zone1Relay2OffTxtBox, buf.relay2Off);
            this.Invoke(setControl, zone1Drain1OnTxtBox, buf.drain1On);
            this.Invoke(setControl, zone1Drain1OffTxtBox, buf.drain1Off);
            this.Invoke(setControl, zone1Drain2OnTxtBox, buf.drain2On);
            this.Invoke(setControl, zone1Drain2OffTxtBox, buf.drain2Off);
            
            //zone2
            buf = new TabConfig(GetAlarmDevices(1));
            if (buf.relay1)
                this.Invoke(setControl, zone2Relay1ChkBox, (short)1);
            else
                this.Invoke(setControl, zone2Relay1ChkBox, (short)0);
            if (buf.relay2)
                this.Invoke(setControl, zone2Relay2ChkBox, (short)1);
            else
                this.Invoke(setControl, zone2Relay2ChkBox, (short)0);
            if (buf.drain1)
                this.Invoke(setControl, zone2Drain1ChkBox, (short)1);
            else
                this.Invoke(setControl, zone2Drain1ChkBox, (short)0);
            if (buf.drain2)
                this.Invoke(setControl, zone2Drain2ChkBox, (short)1);
            else
                this.Invoke(setControl, zone2Drain2ChkBox, (short)0);
            this.Invoke(setControl, zone2Relay1OnTxtBox, buf.relay1On);
            this.Invoke(setControl, zone2Relay1OffTxtBox, buf.relay1Off);
            this.Invoke(setControl, zone2Relay2OnTxtBox, buf.relay2On);
            this.Invoke(setControl, zone2Relay2OffTxtBox, buf.relay2Off);
            this.Invoke(setControl, zone2Drain1OnTxtBox, buf.drain1On);
            this.Invoke(setControl, zone2Drain1OffTxtBox, buf.drain1Off);
            this.Invoke(setControl, zone2Drain2OnTxtBox, buf.drain2On);
            this.Invoke(setControl, zone2Drain2OffTxtBox, buf.drain2Off);

            //zone3
            buf = new TabConfig(GetAlarmDevices(2));
            if (buf.relay1)
                this.Invoke(setControl, zone3Relay1ChkBox, (short)1);
            else
                this.Invoke(setControl, zone3Relay1ChkBox, (short)0);
            if (buf.relay2)
                this.Invoke(setControl, zone3Relay2ChkBox, (short)1);
            else
                this.Invoke(setControl, zone3Relay2ChkBox, (short)0);
            if (buf.drain1)
                this.Invoke(setControl, zone3Drain1ChkBox, (short)1);
            else
                this.Invoke(setControl, zone3Drain1ChkBox, (short)0);
            if (buf.drain2)
                this.Invoke(setControl, zone3Drain2ChkBox, (short)1);
            else
                this.Invoke(setControl, zone3Drain2ChkBox, (short)0);
            this.Invoke(setControl, zone3Relay1OnTxtBox, buf.relay1On);
            this.Invoke(setControl, zone3Relay1OffTxtBox, buf.relay1Off);
            this.Invoke(setControl, zone3Relay2OnTxtBox, buf.relay2On);
            this.Invoke(setControl, zone3Relay2OffTxtBox, buf.relay2Off);
            this.Invoke(setControl, zone3Drain1OnTxtBox, buf.drain1On);
            this.Invoke(setControl, zone3Drain1OffTxtBox, buf.drain1Off);
            this.Invoke(setControl, zone3Drain2OnTxtBox, buf.drain2On);
            this.Invoke(setControl, zone3Drain2OffTxtBox, buf.drain2Off);

            //zone4
            buf = new TabConfig(GetAlarmDevices(3));
            if (buf.relay1)
                this.Invoke(setControl, zone4Relay1ChkBox, (short)1);
            else
                this.Invoke(setControl, zone4Relay1ChkBox, (short)0);
            if (buf.relay2)
                this.Invoke(setControl, zone4Relay2ChkBox, (short)1);
            else
                this.Invoke(setControl, zone4Relay2ChkBox, (short)0);
            if (buf.drain1)
                this.Invoke(setControl, zone4Drain1ChkBox, (short)1);
            else
                this.Invoke(setControl, zone4Drain1ChkBox, (short)0);
            if (buf.drain2)
                this.Invoke(setControl, zone4Drain2ChkBox, (short)1);
            else
                this.Invoke(setControl, zone4Drain2ChkBox, (short)0);
            this.Invoke(setControl, zone4Relay1OnTxtBox, buf.relay1On);
            this.Invoke(setControl, zone4Relay1OffTxtBox, buf.relay1Off);
            this.Invoke(setControl, zone4Relay2OnTxtBox, buf.relay2On);
            this.Invoke(setControl, zone4Relay2OffTxtBox, buf.relay2Off);
            this.Invoke(setControl, zone4Drain1OnTxtBox, buf.drain1On);
            this.Invoke(setControl, zone4Drain1OffTxtBox, buf.drain1Off);
            this.Invoke(setControl, zone4Drain2OnTxtBox, buf.drain2On);
            this.Invoke(setControl, zone4Drain2OffTxtBox, buf.drain2Off);

            //zone5
            buf = new TabConfig(GetAlarmDevices(4));
            if (buf.relay1)
                this.Invoke(setControl, zone5Relay1ChkBox, (short)1);
            else
                this.Invoke(setControl, zone5Relay1ChkBox, (short)0);
            if (buf.relay2)
                this.Invoke(setControl, zone5Relay2ChkBox, (short)1);
            else
                this.Invoke(setControl, zone5Relay2ChkBox, (short)0);
            if (buf.drain1)
                this.Invoke(setControl, zone5Drain1ChkBox, (short)1);
            else
                this.Invoke(setControl, zone5Drain1ChkBox, (short)0);
            if (buf.drain2)
                this.Invoke(setControl, zone5Drain2ChkBox, (short)1);
            else
                this.Invoke(setControl, zone5Drain2ChkBox, (short)0);
            this.Invoke(setControl, zone5Relay1OnTxtBox, buf.relay1On);
            this.Invoke(setControl, zone5Relay1OffTxtBox, buf.relay1Off);
            this.Invoke(setControl, zone5Relay2OnTxtBox, buf.relay2On);
            this.Invoke(setControl, zone5Relay2OffTxtBox, buf.relay2Off);
            this.Invoke(setControl, zone5Drain1OnTxtBox, buf.drain1On);
            this.Invoke(setControl, zone5Drain1OffTxtBox, buf.drain1Off);
            this.Invoke(setControl, zone5Drain2OnTxtBox, buf.drain2On);
            this.Invoke(setControl, zone5Drain2OffTxtBox, buf.drain2Off);

            //zone6
            buf = new TabConfig(GetAlarmDevices(5));
            if (buf.relay1)
                this.Invoke(setControl, zone6Relay1ChkBox, (short)1);
            else
                this.Invoke(setControl, zone6Relay1ChkBox, (short)0);
            if (buf.relay2)
                this.Invoke(setControl, zone6Relay2ChkBox, (short)1);
            else
                this.Invoke(setControl, zone6Relay2ChkBox, (short)0);
            if (buf.drain1)
                this.Invoke(setControl, zone6Drain1ChkBox, (short)1);
            else
                this.Invoke(setControl, zone6Drain1ChkBox, (short)0);
            if (buf.drain2)
                this.Invoke(setControl, zone6Drain2ChkBox, (short)1);
            else
                this.Invoke(setControl, zone6Drain2ChkBox, (short)0);
            this.Invoke(setControl, zone6Relay1OnTxtBox, buf.relay1On);
            this.Invoke(setControl, zone6Relay1OffTxtBox, buf.relay1Off);
            this.Invoke(setControl, zone6Relay2OnTxtBox, buf.relay2On);
            this.Invoke(setControl, zone6Relay2OffTxtBox, buf.relay2Off);
            this.Invoke(setControl, zone6Drain1OnTxtBox, buf.drain1On);
            this.Invoke(setControl, zone6Drain1OffTxtBox, buf.drain1Off);
            this.Invoke(setControl, zone6Drain2OnTxtBox, buf.drain2On);
            this.Invoke(setControl, zone6Drain2OffTxtBox, buf.drain2Off);

            //zone7
            buf = new TabConfig(GetAlarmDevices(6));
            if (buf.relay1)
                this.Invoke(setControl, zone7Relay1ChkBox, (short)1);
            else
                this.Invoke(setControl, zone7Relay1ChkBox, (short)0);
            if (buf.relay2)
                this.Invoke(setControl, zone7Relay2ChkBox, (short)1);
            else
                this.Invoke(setControl, zone7Relay2ChkBox, (short)0);
            if (buf.drain1)
                this.Invoke(setControl, zone7Drain1ChkBox, (short)1);
            else
                this.Invoke(setControl, zone7Drain1ChkBox, (short)0);
            if (buf.drain2)
                this.Invoke(setControl, zone7Drain2ChkBox, (short)1);
            else
                this.Invoke(setControl, zone7Drain2ChkBox, (short)0);
            this.Invoke(setControl, zone7Relay1OnTxtBox, buf.relay1On);
            this.Invoke(setControl, zone7Relay1OffTxtBox, buf.relay1Off);
            this.Invoke(setControl, zone7Relay2OnTxtBox, buf.relay2On);
            this.Invoke(setControl, zone7Relay2OffTxtBox, buf.relay2Off);
            this.Invoke(setControl, zone7Drain1OnTxtBox, buf.drain1On);
            this.Invoke(setControl, zone7Drain1OffTxtBox, buf.drain1Off);
            this.Invoke(setControl, zone7Drain2OnTxtBox, buf.drain2On);
            this.Invoke(setControl, zone7Drain2OffTxtBox, buf.drain2Off);

            //zone8
            buf = new TabConfig(GetAlarmDevices(7));
            if (buf.relay1)
                this.Invoke(setControl, zone8Relay1ChkBox, (short)1);
            else
                this.Invoke(setControl, zone8Relay1ChkBox, (short)0);
            if (buf.relay2)
                this.Invoke(setControl, zone8Relay2ChkBox, (short)1);
            else
                this.Invoke(setControl, zone8Relay2ChkBox, (short)0);
            if (buf.drain1)
                this.Invoke(setControl, zone8Drain1ChkBox, (short)1);
            else
                this.Invoke(setControl, zone8Drain1ChkBox, (short)0);
            if (buf.drain2)
                this.Invoke(setControl, zone8Drain2ChkBox, (short)1);
            else
                this.Invoke(setControl, zone8Drain2ChkBox, (short)0);
            this.Invoke(setControl, zone8Relay1OnTxtBox, buf.relay1On);
            this.Invoke(setControl, zone8Relay1OffTxtBox, buf.relay1Off);
            this.Invoke(setControl, zone8Relay2OnTxtBox, buf.relay2On);
            this.Invoke(setControl, zone8Relay2OffTxtBox, buf.relay2Off);
            this.Invoke(setControl, zone8Drain1OnTxtBox, buf.drain1On);
            this.Invoke(setControl, zone8Drain1OffTxtBox, buf.drain1Off);
            this.Invoke(setControl, zone8Drain2OnTxtBox, buf.drain2On);
            this.Invoke(setControl, zone8Drain2OffTxtBox, buf.drain2Off);

            //button
            buf = new TabConfig(GetAlarmDevices(8));
            if (buf.relay1)
                this.Invoke(setControl, btnRelay1ChkBox, (short)1);
            else
                this.Invoke(setControl, btnRelay1ChkBox, (short)0);
            if (buf.relay2)
                this.Invoke(setControl, btnRelay2ChkBox, (short)1);
            else
                this.Invoke(setControl, btnRelay2ChkBox, (short)0);
            if (buf.drain1)
                this.Invoke(setControl, btnDrain1ChkBox, (short)1);
            else
                this.Invoke(setControl, btnDrain1ChkBox, (short)0);
            if (buf.drain2)
                this.Invoke(setControl, btnDrain2ChkBox, (short)1);
            else
                this.Invoke(setControl, btnDrain2ChkBox, (short)0);
            this.Invoke(setControl, btnRelay1OnTxtBox, buf.relay1On);
            this.Invoke(setControl, btnRelay1OffTxtBox, buf.relay1Off);
            this.Invoke(setControl, btnRelay2OnTxtBox, buf.relay2On);
            this.Invoke(setControl, btnRelay2OffTxtBox, buf.relay2Off);
            this.Invoke(setControl, btnDrain1OnTxtBox, buf.drain1On);
            this.Invoke(setControl, btnDrain1OffTxtBox, buf.drain1Off);
            this.Invoke(setControl, btnDrain2OnTxtBox, buf.drain2On);
            this.Invoke(setControl, btnDrain2OffTxtBox, buf.drain2Off);

            //reader
            buf = new TabConfig(GetAlarmDevices(9));
            if (buf.relay1)
                this.Invoke(setControl, readerRelay1ChkBox, (short)1);
            else
                this.Invoke(setControl, readerRelay1ChkBox, (short)0);
            if (buf.relay2)
                this.Invoke(setControl, readerRelay2ChkBox, (short)1);
            else
                this.Invoke(setControl, readerRelay2ChkBox, (short)0);
            if (buf.drain1)
                this.Invoke(setControl, readerDrain1ChkBox, (short)1);
            else
                this.Invoke(setControl, readerDrain1ChkBox, (short)0);
            if (buf.drain2)
                this.Invoke(setControl, readerDrain2ChkBox, (short)1);
            else
                this.Invoke(setControl, readerDrain2ChkBox, (short)0);
            this.Invoke(setControl, readerRelay1OnTxtBox, buf.relay1On);
            this.Invoke(setControl, readerRelay1OffTxtBox, buf.relay1Off);
            this.Invoke(setControl, readerRelay2OnTxtBox, buf.relay2On);
            this.Invoke(setControl, readerRelay2OffTxtBox, buf.relay2Off);
            this.Invoke(setControl, readerDrain1OnTxtBox, buf.drain1On);
            this.Invoke(setControl, readerDrain1OffTxtBox, buf.drain1Off);
            this.Invoke(setControl, readerDrain2OnTxtBox, buf.drain2On);
            this.Invoke(setControl, readerDrain2OffTxtBox, buf.drain2Off);

            //dtmf
            buf = new TabConfig(GetAlarmDevices(10));
            if (buf.relay1)
                this.Invoke(setControl, dtmfRelay1ChkBox, (short)1);
            else
                this.Invoke(setControl, dtmfRelay1ChkBox, (short)0);
            if (buf.relay2)
                this.Invoke(setControl, dtmfRelay2ChkBox, (short)1);
            else
                this.Invoke(setControl, dtmfRelay2ChkBox, (short)0);
            if (buf.drain1)
                this.Invoke(setControl, dtmfDrain1ChkBox, (short)1);
            else
                this.Invoke(setControl, dtmfDrain1ChkBox, (short)0);
            if (buf.drain2)
                this.Invoke(setControl, dtmfDrain2ChkBox, (short)1);
            else
                this.Invoke(setControl, dtmfDrain2ChkBox, (short)0);
            this.Invoke(setControl, dtmfRelay1OnTxtBox, buf.relay1On);
            this.Invoke(setControl, dtmfRelay1OffTxtBox, buf.relay1Off);
            this.Invoke(setControl, dtmfRelay2OnTxtBox, buf.relay2On);
            this.Invoke(setControl, dtmfRelay2OffTxtBox, buf.relay2Off);
            this.Invoke(setControl, dtmfDrain1OnTxtBox, buf.drain1On);
            this.Invoke(setControl, dtmfDrain1OffTxtBox, buf.drain1Off);
            this.Invoke(setControl, dtmfDrain2OnTxtBox, buf.drain2On);
            this.Invoke(setControl, dtmfDrain2OffTxtBox, buf.drain2Off);
        }

        void SetControl(Control ctrl, short val)
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
            //zone1
            byte activeDevices = 0;
            if (zone1Relay1ChkBox.Checked)
                activeDevices += 1;
            if (zone1Relay2ChkBox.Checked)
                activeDevices += 2;
            if (zone1Drain1ChkBox.Checked)
                activeDevices += 4;
            if (zone1Drain2ChkBox.Checked)
                activeDevices += 8;
            SetAlarmDevices(0, activeDevices, short.Parse(zone1Relay1OnTxtBox.Text), short.Parse(zone1Relay2OnTxtBox.Text),
                short.Parse(zone1Drain1OnTxtBox.Text), short.Parse(zone1Drain2OnTxtBox.Text),
                short.Parse(zone1Relay1OffTxtBox.Text), short.Parse(zone1Relay2OffTxtBox.Text),
                short.Parse(zone1Drain1OffTxtBox.Text), short.Parse(zone1Drain2OffTxtBox.Text));

            //zone2
            activeDevices = 0;
            if (zone2Relay1ChkBox.Checked)
                activeDevices += 1;
            if (zone2Relay2ChkBox.Checked)
                activeDevices += 2;
            if (zone2Drain1ChkBox.Checked)
                activeDevices += 4;
            if (zone2Drain2ChkBox.Checked)
                activeDevices += 8;
            SetAlarmDevices(1, activeDevices, short.Parse(zone2Relay1OnTxtBox.Text), short.Parse(zone2Relay2OnTxtBox.Text),
                short.Parse(zone2Drain1OnTxtBox.Text), short.Parse(zone2Drain2OnTxtBox.Text),
                short.Parse(zone2Relay1OffTxtBox.Text), short.Parse(zone2Relay2OffTxtBox.Text),
                short.Parse(zone2Drain1OffTxtBox.Text), short.Parse(zone2Drain2OffTxtBox.Text));

            //zone3
            activeDevices = 0;
            if (zone3Relay1ChkBox.Checked)
                activeDevices += 1;
            if (zone3Relay2ChkBox.Checked)
                activeDevices += 2;
            if (zone3Drain1ChkBox.Checked)
                activeDevices += 4;
            if (zone3Drain2ChkBox.Checked)
                activeDevices += 8;
            SetAlarmDevices(2, activeDevices, short.Parse(zone3Relay1OnTxtBox.Text), short.Parse(zone3Relay2OnTxtBox.Text),
                short.Parse(zone3Drain1OnTxtBox.Text), short.Parse(zone3Drain2OnTxtBox.Text),
                short.Parse(zone3Relay1OffTxtBox.Text), short.Parse(zone3Relay2OffTxtBox.Text),
                short.Parse(zone3Drain1OffTxtBox.Text), short.Parse(zone3Drain2OffTxtBox.Text));

            //zone4
            activeDevices = 0;
            if (zone4Relay1ChkBox.Checked)
                activeDevices += 1;
            if (zone4Relay2ChkBox.Checked)
                activeDevices += 2;
            if (zone4Drain1ChkBox.Checked)
                activeDevices += 4;
            if (zone4Drain2ChkBox.Checked)
                activeDevices += 8;
            SetAlarmDevices(3, activeDevices, short.Parse(zone4Relay1OnTxtBox.Text), short.Parse(zone4Relay2OnTxtBox.Text),
                short.Parse(zone4Drain1OnTxtBox.Text), short.Parse(zone4Drain2OnTxtBox.Text),
                short.Parse(zone4Relay1OffTxtBox.Text), short.Parse(zone4Relay2OffTxtBox.Text),
                short.Parse(zone4Drain1OffTxtBox.Text), short.Parse(zone4Drain2OffTxtBox.Text));

            //zone5
            activeDevices = 0;
            if (zone5Relay1ChkBox.Checked)
                activeDevices += 1;
            if (zone5Relay2ChkBox.Checked)
                activeDevices += 2;
            if (zone5Drain1ChkBox.Checked)
                activeDevices += 4;
            if (zone5Drain2ChkBox.Checked)
                activeDevices += 8;
            SetAlarmDevices(4, activeDevices, short.Parse(zone5Relay1OnTxtBox.Text), short.Parse(zone5Relay2OnTxtBox.Text),
                short.Parse(zone5Drain1OnTxtBox.Text), short.Parse(zone5Drain2OnTxtBox.Text),
                short.Parse(zone5Relay1OffTxtBox.Text), short.Parse(zone5Relay2OffTxtBox.Text),
                short.Parse(zone5Drain1OffTxtBox.Text), short.Parse(zone5Drain2OffTxtBox.Text));

            //zone6
            activeDevices = 0;
            if (zone6Relay1ChkBox.Checked)
                activeDevices += 1;
            if (zone6Relay2ChkBox.Checked)
                activeDevices += 2;
            if (zone6Drain1ChkBox.Checked)
                activeDevices += 4;
            if (zone6Drain2ChkBox.Checked)
                activeDevices += 8;
            SetAlarmDevices(5, activeDevices, short.Parse(zone6Relay1OnTxtBox.Text), short.Parse(zone6Relay2OnTxtBox.Text),
                short.Parse(zone6Drain1OnTxtBox.Text), short.Parse(zone6Drain2OnTxtBox.Text),
                short.Parse(zone6Relay1OffTxtBox.Text), short.Parse(zone6Relay2OffTxtBox.Text),
                short.Parse(zone6Drain1OffTxtBox.Text), short.Parse(zone6Drain2OffTxtBox.Text));

            //zone7
            activeDevices = 0;
            if (zone7Relay1ChkBox.Checked)
                activeDevices += 1;
            if (zone7Relay2ChkBox.Checked)
                activeDevices += 2;
            if (zone7Drain1ChkBox.Checked)
                activeDevices += 4;
            if (zone7Drain2ChkBox.Checked)
                activeDevices += 8;
            SetAlarmDevices(6, activeDevices, short.Parse(zone7Relay1OnTxtBox.Text), short.Parse(zone7Relay2OnTxtBox.Text),
                short.Parse(zone7Drain1OnTxtBox.Text), short.Parse(zone7Drain2OnTxtBox.Text),
                short.Parse(zone7Relay1OffTxtBox.Text), short.Parse(zone7Relay2OffTxtBox.Text),
                short.Parse(zone7Drain1OffTxtBox.Text), short.Parse(zone7Drain2OffTxtBox.Text));

            //zone8
            activeDevices = 0;
            if (zone8Relay1ChkBox.Checked)
                activeDevices += 1;
            if (zone8Relay2ChkBox.Checked)
                activeDevices += 2;
            if (zone8Drain1ChkBox.Checked)
                activeDevices += 4;
            if (zone8Drain2ChkBox.Checked)
                activeDevices += 8;
            SetAlarmDevices(7, activeDevices, short.Parse(zone8Relay1OnTxtBox.Text), short.Parse(zone8Relay2OnTxtBox.Text),
                short.Parse(zone8Drain1OnTxtBox.Text), short.Parse(zone8Drain2OnTxtBox.Text),
                short.Parse(zone8Relay1OffTxtBox.Text), short.Parse(zone8Relay2OffTxtBox.Text),
                short.Parse(zone8Drain1OffTxtBox.Text), short.Parse(zone8Drain2OffTxtBox.Text));

            //button
            activeDevices = 0;
            if (btnRelay1ChkBox.Checked)
                activeDevices += 1;
            if (btnRelay2ChkBox.Checked)
                activeDevices += 2;
            if (btnDrain1ChkBox.Checked)
                activeDevices += 4;
            if (btnDrain2ChkBox.Checked)
                activeDevices += 8;
            SetAlarmDevices(8, activeDevices, short.Parse(btnRelay1OnTxtBox.Text), short.Parse(btnRelay2OnTxtBox.Text),
                short.Parse(btnDrain1OnTxtBox.Text), short.Parse(btnDrain2OnTxtBox.Text),
                short.Parse(btnRelay1OffTxtBox.Text), short.Parse(btnRelay2OffTxtBox.Text),
                short.Parse(btnDrain1OffTxtBox.Text), short.Parse(btnDrain2OffTxtBox.Text));

            //reader
            activeDevices = 0;
            if (readerRelay1ChkBox.Checked)
                activeDevices += 1;
            if (readerRelay2ChkBox.Checked)
                activeDevices += 2;
            if (readerDrain1ChkBox.Checked)
                activeDevices += 4;
            if (readerDrain2ChkBox.Checked)
                activeDevices += 8;
            SetAlarmDevices(9, activeDevices, short.Parse(readerRelay1OnTxtBox.Text), short.Parse(readerRelay2OnTxtBox.Text),
                short.Parse(readerDrain1OnTxtBox.Text), short.Parse(readerDrain2OnTxtBox.Text),
                short.Parse(readerRelay1OffTxtBox.Text), short.Parse(readerRelay2OffTxtBox.Text),
                short.Parse(readerDrain1OffTxtBox.Text), short.Parse(readerDrain2OffTxtBox.Text));

            //dtmf
            activeDevices = 0;
            if (dtmfRelay1ChkBox.Checked)
                activeDevices += 1;
            if (dtmfRelay2ChkBox.Checked)
                activeDevices += 2;
            if (dtmfDrain1ChkBox.Checked)
                activeDevices += 4;
            if (dtmfDrain2ChkBox.Checked)
                activeDevices += 8;
            SetAlarmDevices(10, activeDevices, short.Parse(dtmfRelay1OnTxtBox.Text), short.Parse(dtmfRelay2OnTxtBox.Text),
                short.Parse(dtmfDrain1OnTxtBox.Text), short.Parse(dtmfDrain2OnTxtBox.Text),
                short.Parse(dtmfRelay1OffTxtBox.Text), short.Parse(dtmfRelay2OffTxtBox.Text),
                short.Parse(dtmfDrain1OffTxtBox.Text), short.Parse(dtmfDrain2OffTxtBox.Text));
        }

        private void zoneConfigWriter_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            readForm.Close();
        }
    }

    delegate void VoidControlShort(Control ctrl, short val);
    
    public class TabConfig
    {
        public bool relay1 = false;
        public bool relay2 = false;
        public bool drain1 = false;
        public bool drain2 = false;

        public short relay1On = 0;
        public short relay1Off = 0;
        public short relay2On = 0;
        public short relay2Off = 0;
        public short drain1On = 0;
        public short drain1Off = 0;
        public short drain2On = 0;
        public short drain2Off = 0;

        public TabConfig(string str)
        {
            byte activeDevices = byte.Parse(str.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            if (0 == (activeDevices & 1))
                relay1 = false;
            else
                relay1 = true;
            if (0 == (activeDevices & 2))
                relay2 = false;
            else
                relay2 = true;
            if (0 == (activeDevices & 4))
                drain1 = false;
            else
                drain1 = true;
            if (0 == (activeDevices & 8))
                drain2 = false;
            else
                drain2 = true;
            relay1On = short.Parse(str.Substring(2, 4), System.Globalization.NumberStyles.HexNumber);
            relay2On = short.Parse(str.Substring(6, 4), System.Globalization.NumberStyles.HexNumber);
            drain1On = short.Parse(str.Substring(10, 4), System.Globalization.NumberStyles.HexNumber);
            drain2On = short.Parse(str.Substring(14, 4), System.Globalization.NumberStyles.HexNumber);
            relay1Off = short.Parse(str.Substring(18, 4), System.Globalization.NumberStyles.HexNumber);
            relay2Off = short.Parse(str.Substring(22, 4), System.Globalization.NumberStyles.HexNumber);
            drain1Off = short.Parse(str.Substring(24, 4), System.Globalization.NumberStyles.HexNumber);
            drain2Off = short.Parse(str.Substring(30, 4), System.Globalization.NumberStyles.HexNumber);
        }
    }
}
