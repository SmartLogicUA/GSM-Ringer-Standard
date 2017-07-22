using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GsmRingerStandartConfig
{
    public partial class NewPasswordForm : Form
    {
        public NewPasswordForm()
        {
            InitializeComponent();
        }

        public string Password
        {
            get
            {
                return passwordBox.Text;
            }
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            if (passwordBox.Text.Trim().Length != 6)
                MessageBox.Show("Длинна пароля должна быть 6 цифр", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                this.DialogResult = DialogResult.OK;
        }
    }
}
