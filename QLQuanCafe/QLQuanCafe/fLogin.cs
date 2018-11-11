using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLQuanCafe
{
    public partial class fLogin : Form
    {
        public fLogin()
        {
            InitializeComponent();
        }
        public static class getUserName
        {
            public static string strUsername= string.Empty;
            public static string strDisplayname = string.Empty;
        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (!IsLogin(txtUserName.Text.Trim(),txtPassWord.Text.Trim()))
            {
                txtUserName.Clear();
                txtPassWord.Clear();
                txtUserName.Focus();
                MessageBox.Show("Sai tài khoản hoặc mật khẩu", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                getUserName.strUsername = txtUserName.Text;           
                fTableManager f = new fTableManager();
                this.Hide();
                f.ShowDialog();
                this.Show();
            }
        }

        public static string encodePass(string name)
        {
            byte[] temp = ASCIIEncoding.ASCII.GetBytes(name);//mã hóa chuỗi đưa vào
            byte[] hasData = new MD5CryptoServiceProvider().ComputeHash(temp);
            string hasPass = "";
            foreach (byte item in hasData)
            {
                hasPass += item;
            }
            return hasPass;
        }
        //Kiem tra Login
        public bool IsLogin(string username, string password)
        {
            using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
            {  
                Account acc = db.Accounts.FirstOrDefault(x => x.UserName.Equals(username) && x.PassWord.Equals(encodePass(password)));
                if (acc != null)
                {
                    return true;
                }
                else
                    return false;
            }

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void fLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn thoát không?", "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.OK)
            {
                e.Cancel = true;
            }
        }
    }
}
