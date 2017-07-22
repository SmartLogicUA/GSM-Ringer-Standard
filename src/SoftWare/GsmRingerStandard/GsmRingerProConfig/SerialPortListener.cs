using System;
using System.Collections.Generic;
using System.Text;

namespace GsmRingerStandartConfig
{
    public class SerialPortListener
    {
        System.IO.Ports.SerialPort port;

        public event DeviceDataReceivedEventHandler DeviceIdReceived;
        public event DeviceDataReceivedEventHandler DeviceVersionReceived;
        public event DeviceDataReceivedEventHandler SetDefaultsOKReceived;
        public event DeviceDataReceivedEventHandler ErrorReceived;
        public event DeviceDataReceivedEventHandler SetUserNumberOKReceived;
        public event DeviceDataReceivedEventHandler GetUserNumberReceived;
        public event DeviceDataReceivedEventHandler SetTimeActivationOKReceived;
        public event DeviceDataReceivedEventHandler GetTimeActivationReceived;
        public event DeviceDataReceivedEventHandler SetTimeDeactivationOKReceived;
        public event DeviceDataReceivedEventHandler GetTimeDeactivationReceived;
        public event DeviceDataReceivedEventHandler SetCallParamsOKReceived;
        public event DeviceDataReceivedEventHandler GetCallParamsReceived;
        public event DeviceDataReceivedEventHandler SetActiveZonesOKReceived;
        public event DeviceDataReceivedEventHandler GetActiveZonesReceived;
        public event DeviceDataReceivedEventHandler SetAlarmSourcesOKReceived;
        public event DeviceDataReceivedEventHandler GetAlarmSourcesReceived;
        public event DeviceDataReceivedEventHandler CardIdReceived;
        public event DeviceDataReceivedEventHandler SetAlarmDevicesOKReceived;
        public event DeviceDataReceivedEventHandler GetAlarmDevicesReceived;
        public event DeviceDataReceivedEventHandler SetUserCardOKReceived;
        public event DeviceDataReceivedEventHandler GetUserCardReceived;
        public event DeviceDataReceivedEventHandler USSDCommandReceived;
        public event DeviceDataReceivedEventHandler SetNewPasswordOKReceived;

        public SerialPortListener(System.IO.Ports.SerialPort port)
        {
            this.port = port;
            this.port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(port_DataReceived);
        }

        protected virtual void OnSetNewPasswordOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (SetNewPasswordOKReceived != null)
                SetNewPasswordOKReceived(sender, e);
        }
        
        protected virtual void OnUSSDCommandReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (USSDCommandReceived != null)
                USSDCommandReceived(sender, e);
        }
        
        protected virtual void OnGetUserCardReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (GetUserCardReceived != null)
                GetUserCardReceived(sender, e);
        }
        
        protected virtual void OnSetUserCardOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (SetUserCardOKReceived != null)
                SetUserCardOKReceived(sender, e);
        }
        
        protected virtual void OnGetAlarmDevicesReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (GetAlarmDevicesReceived != null)
                GetAlarmDevicesReceived(sender, e);
        }
        
        protected virtual void OnSetAlarmDevicesOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (SetAlarmDevicesOKReceived != null)
                SetAlarmDevicesOKReceived(sender, e);
        }
        
        protected virtual void OnCardIdReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (CardIdReceived != null)
                CardIdReceived(sender, e);
        }
        
        protected virtual void OnGetAlarmSourcesReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (GetAlarmSourcesReceived != null)
                GetAlarmSourcesReceived(sender, e);
        }
        
        protected virtual void OnSetAlarmSourcesOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (SetAlarmSourcesOKReceived != null)
                SetAlarmSourcesOKReceived(sender, e);
        }
        
        protected virtual void OnGetActiveZonesReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (GetActiveZonesReceived != null)
                GetActiveZonesReceived(sender, e);
        }
        
        protected virtual void OnSetActiveZonesOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (SetActiveZonesOKReceived != null)
                SetActiveZonesOKReceived(sender, e);
        }
        
        protected virtual void OnSetTimeActivationOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (SetTimeActivationOKReceived != null)
                SetTimeActivationOKReceived(sender, e);
        }

        protected virtual void OnGetTimeActivationReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (GetTimeActivationReceived != null)
                GetTimeActivationReceived(sender, e);
        }
        
        protected virtual void OnGetCallParamsReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (GetCallParamsReceived != null)
                GetCallParamsReceived(sender, e);
        }

        protected virtual void OnSetCallParamsOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (SetCallParamsOKReceived != null)
                SetCallParamsOKReceived(sender, e);
        }

        protected virtual void OnGetTimeDeactivationReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (GetTimeDeactivationReceived != null)
                GetTimeDeactivationReceived(sender, e);
        }

        protected virtual void OnSetTimeDeactivationOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (SetTimeDeactivationOKReceived != null)
                SetTimeDeactivationOKReceived(sender, e);
        }
        
        protected virtual void OnErrorReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (ErrorReceived != null)
                ErrorReceived(sender, e);
        }

        protected virtual void OnSetUserNumberOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (SetUserNumberOKReceived != null)
                SetUserNumberOKReceived(sender, e);
        }

        protected virtual void OnGetUserNumberReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (GetUserNumberReceived != null)
                GetUserNumberReceived(sender, e);
        }

        protected virtual void OnSetDefaultsOKReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (SetDefaultsOKReceived != null)
                SetDefaultsOKReceived(sender, e);
        }

        protected virtual void OnDeviceVersionReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (DeviceVersionReceived != null)
                DeviceVersionReceived(sender, e);
        }

        protected virtual void OnDeviceIdReceived(object sender, StringDataReceivedEventArgs e)
        {
            if (DeviceIdReceived != null)
                DeviceIdReceived(sender, e);
        }

        void port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                port.ReadTo("$");
                string message = port.ReadLine();

                //byte[] buf = new byte[port.BytesToRead];
                //port.Read(buf, 0, buf.Length);
                //string message = Encoding.UTF8.GetString(buf).Replace("$", "").Replace("\r\n", "");

                //byte curr;
                //while (0x24 != port.ReadByte())
                //{
                //}
                //List<byte> bytes = new List<byte>();
                //while (0x0D != (curr = (byte)port.ReadByte()))
                //{
                //    bytes.Add(curr);
                //}
                //string message = Encoding.UTF8.GetString(bytes.ToArray());

                if (IsCorrectCRC(message))
                {
                    message = message.Substring(0, message.Length - 3);
                    switch (message.Substring(0, 5))
                    {
                        case "SYHND":
                            OnDeviceIdReceived(this, new StringDataReceivedEventArgs(message.Substring(6)));
                            break;
                        case "SYVER":
                            OnDeviceVersionReceived(this, new StringDataReceivedEventArgs(message.Substring(6)));
                            break;
                        case "SYSFS":
                            if (message.Substring(6, 2) == "OK")
                                OnSetDefaultsOKReceived(this, new StringDataReceivedEventArgs("OK"));
                            else
                                OnErrorReceived(this, new StringDataReceivedEventArgs("Во время установки параметров по умолчанию произошла ошибка"));
                            break;
                        case "SYSUN":
                            if (message.Substring(6, 2) == "OK")
                                OnSetUserNumberOKReceived(this, new StringDataReceivedEventArgs("OK"));
                            else
                                OnErrorReceived(this, new StringDataReceivedEventArgs("Во время установки номера пользователя произошла ошибка"));
                            break;
                        case "SYGUN":
                            OnGetUserNumberReceived(this, new StringDataReceivedEventArgs(message.Substring(6)));
                            break;
                        case "SYSTA":
                            if (message.Substring(6, 2) == "OK")
                                OnSetTimeActivationOKReceived(this, new StringDataReceivedEventArgs("OK"));
                            else
                                OnErrorReceived(this, new StringDataReceivedEventArgs("Во время установки времени активации сигнализации произошла ошибка"));
                            break;
                        case "SYGTA":
                            OnGetTimeActivationReceived(this, new StringDataReceivedEventArgs(message.Substring(6)));
                            break;
                        case "SYSTD":
                            if (message.Substring(6, 2) == "OK")
                                OnSetTimeDeactivationOKReceived(this, new StringDataReceivedEventArgs("OK"));
                            else
                                OnErrorReceived(this, new StringDataReceivedEventArgs("Во время установки времени деактивации сигнализации произошла ошибка"));
                            break;
                        case "SYGTD":
                            OnGetTimeDeactivationReceived(this, new StringDataReceivedEventArgs(message.Substring(6)));
                            break;
                        case "SYSCS":
                            if (message.Substring(6, 2) == "OK")
                                OnSetCallParamsOKReceived(this, new StringDataReceivedEventArgs("OK"));
                            else
                                OnErrorReceived(this, new StringDataReceivedEventArgs("Во время установки параметров оповещения произошла ошибка"));
                            break;
                        case "SYGCS":
                            OnGetCallParamsReceived(this, new StringDataReceivedEventArgs(message.Substring(6)));
                            break;
                        case "SYSAZ":
                            if (message.Substring(6, 2) == "OK")
                                OnSetActiveZonesOKReceived(this, new StringDataReceivedEventArgs("OK"));
                            else
                                OnErrorReceived(this, new StringDataReceivedEventArgs("Во время установки активных зон произошла ошибка"));
                            break;
                        case "SYGAZ":
                            OnGetActiveZonesReceived(this, new StringDataReceivedEventArgs(message.Substring(6)));
                            break;
                        case "SYSON":
                            if (message.Substring(6, 2) == "OK")
                                OnSetAlarmSourcesOKReceived(this, new StringDataReceivedEventArgs("OK"));
                            else
                                OnErrorReceived(this, new StringDataReceivedEventArgs("Во время установки источников постановки и снятия сигнализации произошла ошибка"));
                            break;
                        case "SYGON":
                            GetAlarmSourcesReceived(this, new StringDataReceivedEventArgs(message.Substring(6)));
                            break;
                        case "SYCRD":
                            OnCardIdReceived(this, new StringDataReceivedEventArgs(message.Substring(6)));
                            break;
                        case "SYSTE":
                            if (message.Substring(6, 2) == "OK")
                                OnSetAlarmDevicesOKReceived(this, new StringDataReceivedEventArgs("OK"));
                            else
                                OnErrorReceived(this, new StringDataReceivedEventArgs("Во время установки параметров исполнительных устройств произошла ошибка"));
                            break;
                        case "SYGTE":
                            OnGetAlarmDevicesReceived(this, new StringDataReceivedEventArgs(message.Substring(6)));
                            break;
                        case "SYSCD":
                            if (message.Substring(6, 2) == "OK")
                                OnSetUserCardOKReceived(this, new StringDataReceivedEventArgs("OK"));
                            else
                                OnErrorReceived(this, new StringDataReceivedEventArgs("Во время записи карточки пользователя произошла ошибка произошла ошибка"));
                            break;
                        case "SYNPW":
                            if (message.Substring(6, 2) == "OK")
                                OnSetNewPasswordOKReceived(this, new StringDataReceivedEventArgs("OK"));
                            else
                                OnErrorReceived(this, new StringDataReceivedEventArgs("Во время установки пароля произошла ошибка"));
                            break;
                        case "SYGCD":
                            OnGetUserCardReceived(this, new StringDataReceivedEventArgs(message.Substring(6)));
                            break;
                        case "SYUSD":
                            OnUSSDCommandReceived(this, new StringDataReceivedEventArgs(message.Substring(6)));
                            break;
                        default:
                            OnErrorReceived(this, new StringDataReceivedEventArgs("Устройство вернуло неверные данные"));
                            break;
                    }
                }
                else
                {
                    OnErrorReceived(this, new StringDataReceivedEventArgs("Устройство вернуло неверные данные"));
                }
            }
#pragma warning disable 168
            catch (Exception except)
            {
            }
#pragma warning restore 168
        }

        public static string CalculateCRC(string data)
        {
            byte crc = 0;
            foreach (byte b in Encoding.UTF8.GetBytes(data))
            {
                crc ^= b;
            }
            return crc.ToString("X2");
        }

        static bool IsCorrectCRC(string message)
        {
            if (CalculateCRC(message.Substring(0, message.Length - 3)) == message.Substring(message.Length - 2, 2))
                return true;
            else
                return false;
        }
    }

    public class StringDataReceivedEventArgs : EventArgs
    {
        string message = "";

        public StringDataReceivedEventArgs(string message)
        {
            this.message = message;
        }

        public string Message
        {
            get
            {
                return message;
            }
        }
    }

    public delegate void DeviceDataReceivedEventHandler(object sender, StringDataReceivedEventArgs e);
}
