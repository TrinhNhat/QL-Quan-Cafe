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
using static QLQuanCafe.fLogin;

namespace QLQuanCafe
{
 
    public partial class fAccountProfile : Form
    {
        public fAccountProfile()
        {
            InitializeComponent();
        }

        private void fAccountProfile_Load(object sender, EventArgs e)
        {
            ShowName();
        }
        public void ShowName()
        {
            using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
            {
                foreach (var u in db.Accounts)
                {
                    if (u.UserName == getUserName.strUsername)
                    {
                        txtUserName.Text = u.UserName.ToString();
                        txtDisplayName.Text = u.DisplayName.ToString();

                    }
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

       

        private void btnUpdate_Click(object sender, EventArgs e)
        {    
            if(!txtNewPass.Text.Equals(txtRePass.Text))
            {
                txtNewPass.Text = "";
                txtRePass.Text = "";
                txtNewPass.Focus();
                MessageBox.Show("Mật khẩu nhập lại không đúng", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
                {
                    
                    Account ac_old = db.Accounts.FirstOrDefault(x => x.UserName == txtUserName.Text && x.PassWord ==encodePass(txtPassWord.Text));
                    if(ac_old != null)
                    {
                        if(txtPassWord.Text == null || txtNewPass.Text == "")//update
                        {
                            ac_old.DisplayName = txtDisplayName.Text;
                            getUserName.strDisplayname = txtDisplayName.Text;    
                                          
                            db.SubmitChanges();
                            txtPassWord.Text = "";
                            txtPassWord.Focus();
                            if(updateAccount!=null)
                            {
                                updateAccount(this,new EventArgs());
                            }
                            MessageBox.Show("Cập nhật thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            ac_old.DisplayName = txtDisplayName.Text;
                            getUserName.strDisplayname = txtDisplayName.Text;
                            ac_old.PassWord = encodePass(txtNewPass.Text);
                            db.SubmitChanges();
                            if (updateAccount != null)
                            {
                                updateAccount(this, new EventArgs());
                            }
                            txtNewPass.Text = "";
                            txtRePass.Text = "";
                            txtPassWord.Text = "";
                            txtPassWord.Focus();
                           
                            MessageBox.Show("Cập nhật thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        txtNewPass.Text = "";
                        txtRePass.Text = "";
                        txtPassWord.Text = "";
                        txtPassWord.Focus();
                        MessageBox.Show("Vui lòng nhập đúng mật khẩu", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                        
                }
            }
        }
        //tạo event updateAccount
        private event EventHandler updateAccount;
        public event EventHandler UpdateAccount
        {
            add { updateAccount += value; }
            remove { updateAccount -= value; }
        }

    }
}
