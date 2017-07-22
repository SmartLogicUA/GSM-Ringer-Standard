using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GsmRingerStandartConfig
{
    public partial class PortSelectionForm : Form
    {
        public PortSelectionForm()
        {
            InitializeComponent();
        }

        private void PortSelectionForm_Load(object sender, EventArgs e)
        {
            portNameBox.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            if (!string.IsNullOrEmpty((this.Owner as GsmRingerProConfig).PortName))
            {
                if (portNameBox.Items.Contains((this.Owner as GsmRingerProConfig).PortName))
                {
                    portNameBox.SelectedItem = (this.Owner as GsmRingerProConfig).PortName;
                }
            }
        }

        public string SelectedPort
        {
            get
            {
                return (portNameBox.SelectedItem as string);
            }
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            System.IO.File.WriteAllText("config.ini", portNameBox.SelectedItem as string);
        }
    }
}
