using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QLQuanCafe.fLogin;

namespace QLQuanCafe
{
    public partial class fAdmin : Form
    {
        public fAdmin()
        {
            InitializeComponent();
        }


        #region  Method
        public void LoadCategory()
        {
            using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
            {
                var data = db.FoodCategories;
                dtgvCategory.DataSource = data;
            }
        }

        public void LoadAccountType()
        {
            using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
            {
                cbAccountType.DataSource = db.AccountTypes;
                cbAccountType.DisplayMember = "name";
                cbAccountType.ValueMember = "type";
            }
        }
        public void LoadAccount()
        {
            using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
            {
                int type = ((AccountType)cbAccountType.SelectedItem).Type;
                var result = from a in db.Accounts
                             join t in db.AccountTypes on a.Type equals t.Type
                             where a.Type == type
                             select new
                             {
                                 username = a.UserName,
                                 displayname = a.DisplayName,
                                 type = t.Name
                             };
                dtgvAccount.DataSource = result;
            }
        }

        public void LoadTable()
        {
            using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
            {
                var result = from a in db.TableFoods
                             where a.status == cbTableStatus.SelectedItem.ToString()
                             select a;

                dtgvTable.DataSource = result;
            }
        }

        public void LoadListFood()
        {
            using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
            {
                int idFoodCategory = ((FoodCategory)cbFoodCategory.SelectedItem).id;
                var query = db.USP_GetFoodList(idFoodCategory);
                dtgvFood.DataSource = query;
            }
        }
        public void LoadCategoryFood()
        {
            using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
            {
                cbFoodCategory.DataSource = db.FoodCategories;
                cbFoodCategory.DisplayMember = "name";
                cbFoodCategory.ValueMember = "id";
            }
        }
        public void LoadStatusTable()
        {
            using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
            {
                cbTableStatus.Items.Add("Trống");
                cbTableStatus.Items.Add("Có người");
                cbTableStatus.SelectedItem = "Trống";
            }
        }

        private void fAdmin_Load(object sender, EventArgs e)
        {
            LoadStatusTable();
            LoadCategory();
            LoadAccountType();
            LoadCategoryFood();
            LoadDateTimePickerBill();
            LoadViewBill();
        }

        public void LoadViewBill()
        {
            using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
            {
                var table = db.USP_GetListBillByDate(Convert.ToDateTime(dtpkFromDate.Value.ToShortDateString()), Convert.ToDateTime(dtpkToDate.Value.ToShortDateString()));
                dtgvBill.DataSource = table;
            }
        }

        public void LoadDateTimePickerBill()
        {
            DateTime today = DateTime.Now;
            dtpkFromDate.Value = new DateTime(today.Year, today.Month, 1);
            dtpkToDate.Value = dtpkFromDate.Value.AddMonths(1).AddDays(-1);
        }

        #endregion

        #region  Events
        private void btnViewBill_Click(object sender, EventArgs e)
        {
            LoadViewBill();
        }

        private void cbFoodCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadListFood();
        }

        private void dtgvFood_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int dong = e.RowIndex;
            try
            {
                txtFoodID.Text = dtgvFood.Rows[dong].Cells[0].Value.ToString();
                txtFoodName.Text = dtgvFood.Rows[dong].Cells[1].Value.ToString();
                txtFoodPrice.Text = dtgvFood.Rows[dong].Cells[2].Value.ToString();
                using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
                {
                    var result = db.Foods.FirstOrDefault(x => x.id == Convert.ToInt16(txtFoodID.Text));

                    if (result != null)
                    {
                        byte[] arr = result.images.ToArray();
                        if (arr != null)
                        {
                            MemoryStream ms = new MemoryStream(arr);
                            picBoxImageFood.Image = Image.FromStream(ms);

                        }
                        else
                        {
                            picBoxImageFood.Image = null;
                            PathImage = null;
                        }
                    }
                    else
                    {
                        picBoxImageFood.Image = null;
                        PathImage = null;
                    }
                }
            }
            catch
            {
                picBoxImageFood.Image = null;
                PathImage = null;
            }
        }

        private void txtFoodPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void btnShowFood_Click(object sender, EventArgs e)
        {
            using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
            {
                var result = from f in db.Foods
                             join fc in db.FoodCategories on f.idCategory equals fc.id
                             select new
                             {
                                 id = f.id,
                                 name = f.name,
                                 price = f.price,
                             };
                dtgvFood.DataSource = result;
            }
        }

        private void btnAddFood_Click(object sender, EventArgs e)
        {
            try
            {
                using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
                {
                    if (txtFoodName.Text == "" || txtFoodPrice.Text == "")
                    {
                        MessageBox.Show("Vui lòng điền đầy đủ thông tin!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtFoodName.Focus();
                    }
                    else
                    {
                        if (MessageBox.Show("Bạn có muốn thêm món " + txtFoodName.Text + " ?", "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                        {
                            Food f = new Food();
                            f.name = txtFoodName.Text;
                            f.idCategory = Convert.ToInt16(cbFoodCategory.SelectedValue.ToString());
                            f.price = Convert.ToDouble(txtFoodPrice.Text);
                            if (PathImage != null)
                            {
                                Image img = Image.FromFile(PathImage);
                                MemoryStream ms = new MemoryStream();
                                img.Save(ms, img.RawFormat);
                                f.images = ms.ToArray();
                            }
                            db.Foods.InsertOnSubmit(f);
                            db.SubmitChanges();
                            LoadListFood();
                            addFood(this, new EventArgs());
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }

        private void btnDeleteFood_Click(object sender, EventArgs e)
        {
            try
            {
                using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
                {
                    if (txtFoodID.Text == "")
                    {
                        MessageBox.Show("Vui lòng chọn món cần xóa!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        Food f = db.Foods.FirstOrDefault(x => x.id == Convert.ToInt16(txtFoodID.Text));
                        if (f != null)
                        {
                            if (MessageBox.Show("Bạn có muốn xóa món " + txtFoodName.Text + " ?", "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                            {
                                BillInfo b = db.BillInfos.FirstOrDefault(x => x.idFood == Convert.ToInt16(txtFoodID.Text));
                                if (b != null)
                                {
                                    db.BillInfos.DeleteOnSubmit(b);
                                    db.SubmitChanges();
                                }
                                db.Foods.DeleteOnSubmit(f);
                                db.SubmitChanges();
                                LoadListFood();
                                txtFoodID.Text = "";
                                txtFoodName.Text = "";
                                txtFoodPrice.Text = "";
                                txtFoodName.Focus();
                                picBoxImageFood.Image = null;
                                deleteFood(this, new EventArgs());
                            }

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }

        private void btnEditFood_Click(object sender, EventArgs e)
        {
            try
            {
                using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
                {
                    if (txtFoodID.Text == "")
                    {
                        MessageBox.Show("Vui lòng chọn món cần cập nhật!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        Food f = db.Foods.FirstOrDefault(x => x.id == Convert.ToInt16(txtFoodID.Text));
                        if (f != null)
                        {
                            if (MessageBox.Show("Bạn có muốn cập nhật món " + txtFoodName.Text + " ?", "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                            {
                                f.name = txtFoodName.Text;
                                f.idCategory = Convert.ToInt16(cbFoodCategory.SelectedValue.ToString());
                                f.price = Convert.ToDouble(txtFoodPrice.Text);
                                if (PathImage != null)
                                {
                                    Image img = Image.FromFile(PathImage);
                                    MemoryStream ms = new MemoryStream();
                                    img.Save(ms, img.RawFormat);
                                    f.images = ms.ToArray();
                                }
                                db.SubmitChanges();
                                LoadListFood();
                                editFood(this, new EventArgs());
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }

        private void txtSearchFoodName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
                {
                    var result = db.USP_SearchFoodList(txtSearchFoodName.Text);
                    dtgvFood.DataSource = result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }
        private void btnRetype_Click(object sender, EventArgs e)
        {
            txtSearchFoodName.Clear();
            txtSearchFoodName.Focus();
        }

        private void cbAccountType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadAccount();
        }

        private void dtgvCategory_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int dong = e.RowIndex;
            try
            {
                txtCategoryID.Text = dtgvCategory.Rows[dong].Cells[0].Value.ToString();
                txtCategoryName.Text = dtgvCategory.Rows[dong].Cells[1].Value.ToString();
            }
            catch
            { }
        }

        private void btnAddCategory_Click(object sender, EventArgs e)
        {
            try
            {
                using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
                {
                    if (txtCategoryName.Text == "")
                    {
                        MessageBox.Show("Vui lòng điền đầy đủ thông tin!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtCategoryName.Focus();
                    }
                    else
                    {
                        if (MessageBox.Show("Bạn có muốn thêm doanh mục " + txtCategoryName.Text + " ?", "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                        {
                            FoodCategory f = new FoodCategory();
                            f.name = txtCategoryName.Text;
                            db.FoodCategories.InsertOnSubmit(f);
                            db.SubmitChanges();
                            LoadCategory();
                            addCategory(this, new EventArgs());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }

        private void btnEditCategory_Click(object sender, EventArgs e)
        {
            try
            {
                using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
                {
                    if (txtCategoryID.Text == "")
                    {
                        MessageBox.Show("Vui lòng chọn doanh mục cần cập nhật!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        FoodCategory f = db.FoodCategories.FirstOrDefault(x => x.id == Convert.ToInt16(txtCategoryID.Text));
                        if (f != null)
                        {
                            if (MessageBox.Show("Bạn có muốn cập nhật doanh mục " + txtCategoryName.Text + " ?", "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                            {
                                f.name = txtCategoryName.Text;
                                db.SubmitChanges();
                                LoadCategory();
                                editCategory(this, new EventArgs());
                            }

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }

        private void btnDeleteCategory_Click(object sender, EventArgs e)
        {
            try
            {
                using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
                {
                    if (txtCategoryID.Text == "")
                    {
                        MessageBox.Show("Vui lòng chọn doanh mục cần xóa!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        FoodCategory f = db.FoodCategories.FirstOrDefault(x => x.id == Convert.ToInt16(txtCategoryID.Text));
                        if (f != null)
                        {
                            if (MessageBox.Show("Bạn có muốn xóa doanh mục " + txtCategoryName.Text + " ?", "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                            {
                                db.FoodCategories.DeleteOnSubmit(f);
                                db.SubmitChanges();
                                LoadCategory();
                                txtCategoryID.Text = "";
                                txtCategoryName.Text = "";
                                txtCategoryName.Focus();
                                deleteCategory(this, new EventArgs());
                            }

                        }
                    }

                }
            }
            catch
            {
                MessageBox.Show("Vui lòng xóa những món trong doanh mục " + txtCategoryName.Text, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnShowAccount_Click(object sender, EventArgs e)
        {
            using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
            {
                var result = from a in db.Accounts
                             join t in db.AccountTypes on a.Type equals t.Type
                             select new
                             {
                                 username = a.UserName,
                                 displayname = a.DisplayName,
                                 type = t.Name
                             };
                dtgvAccount.DataSource = result;
            }
        }

        private void btnAddAccount_Click(object sender, EventArgs e)
        {
            try
            {
                using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
                {
                    if (txtDisplayName.Text == "" || txtAccountName.Text == "")
                    {
                        MessageBox.Show("Vui lòng điền đầy đủ thông tin!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtAccountName.Focus();
                    }
                    else
                    {
                        Account acc_old = db.Accounts.FirstOrDefault(x => x.UserName == txtAccountName.Text);
                        if (acc_old != null)
                        {
                            MessageBox.Show("Tên đăng nhập đã tồn tại!!\nVui lòng chọn tên khác", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            txtAccountName.Clear();
                            txtAccountName.Focus();
                        }
                        else
                        {
                            if (MessageBox.Show("Bạn có muốn thêm tài khoản " + txtAccountName.Text + " ?", "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                            {
                                //Mã hóa pass
                                byte[] temp = ASCIIEncoding.ASCII.GetBytes("0");//khi thêm tài khoản mk default =0
                                byte[] hasData = new MD5CryptoServiceProvider().ComputeHash(temp);
                                string hasPass = "";
                                foreach (byte item in hasData)
                                {
                                    hasPass += item;
                                }
                                Account a = new Account();
                                a.UserName = txtAccountName.Text;
                                a.DisplayName = txtDisplayName.Text;
                                a.Type = Convert.ToInt16(cbAccountType.SelectedValue.ToString());
                                a.PassWord = hasPass;
                                db.Accounts.InsertOnSubmit(a);
                                db.SubmitChanges();
                                LoadAccount();
                                txtDisplayName.Clear();
                                txtAccountName.Clear();
                                txtAccountName.Focus();
                                MessageBox.Show("Mật khẩu mặc định của tài khoản là 0 \nVui lòng nhớ đổi mật khẩu cho lần đăng nhập kế tiếp!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }

        private void btnDeleteAccount_Click(object sender, EventArgs e)
        {
            try
            {
                using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
                {
                    if (txtDisplayName.Text == "" || txtAccountName.Text == "")
                    {
                        MessageBox.Show("Vui lòng chọn tài khoản cần xóa!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtAccountName.Focus();
                    }
                    else
                    {
                        if (txtAccountName.Text == getUserName.strUsername)
                        {
                            MessageBox.Show("Tài khoản đang đăng nhặp\nKhông thể xóa!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            Account acc_old = db.Accounts.FirstOrDefault(x => x.UserName == txtAccountName.Text);
                            if (acc_old != null)
                            {
                                if (MessageBox.Show("Bạn có muốn xóa tài khoản " + txtAccountName.Text + " ?", "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                                {
                                    db.Accounts.DeleteOnSubmit(acc_old);
                                    db.SubmitChanges();
                                    LoadAccount();
                                    txtDisplayName.Clear();
                                    txtAccountName.Clear();
                                    txtAccountName.Focus();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Tên đăng nhập không tồn tại\nKhông thể xóa!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                txtAccountName.Focus();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }

        private void btnEditAccount_Click(object sender, EventArgs e)
        {
            try
            {
                using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
                {
                    if (txtDisplayName.Text == "" || txtAccountName.Text == "")
                    {
                        MessageBox.Show("Vui lòng chọn tài khoản cần cập nhật!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtAccountName.Focus();
                    }
                    else
                    {
                        Account acc_old = db.Accounts.FirstOrDefault(x => x.UserName == txtAccountName.Text);
                        if (acc_old != null)
                        {
                            if (MessageBox.Show("Bạn có muốn cập nhật tài khoản " + txtAccountName.Text + " ?", "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                            {
                                acc_old.DisplayName = txtDisplayName.Text;
                                acc_old.Type = Convert.ToInt16(cbAccountType.SelectedValue.ToString());
                                db.SubmitChanges();
                                LoadAccount();
                                txtDisplayName.Clear();
                                txtAccountName.Clear();
                                txtAccountName.Focus();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Tên đăng nhập không tồn tại\nKhông thể cập nhập!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            txtAccountName.Focus();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }

        private void btnResetPass_Click(object sender, EventArgs e)
        {
            try
            {
                using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
                {
                    if (txtDisplayName.Text == "" || txtAccountName.Text == "")
                    {
                        MessageBox.Show("Vui lòng chọn tài khoản cần đặt lại mật khẩu!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtAccountName.Focus();
                    }
                    else
                    {
                        Account acc_old = db.Accounts.FirstOrDefault(x => x.UserName == txtAccountName.Text);
                        if (acc_old != null)
                        {
                            if (MessageBox.Show("Bạn có muốn đặt lại mật khẩu bằng 0 cho tài khoản " + txtAccountName.Text + " ?", "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                            {
                                acc_old.PassWord = encodePass("0");
                                db.SubmitChanges();
                                LoadAccount();
                                txtDisplayName.Clear();
                                txtAccountName.Clear();
                                txtAccountName.Focus();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Tên đăng nhập không tồn tại\nKhông thể đặt lại mật khẩu!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            txtAccountName.Focus();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }

        private void dtgvAccount_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int dong = e.RowIndex;
            try
            {
                txtAccountName.Text = dtgvAccount.Rows[dong].Cells[0].Value.ToString();
                txtDisplayName.Text = dtgvAccount.Rows[dong].Cells[1].Value.ToString();
            }
            catch
            { }
        }

        private void cbTableStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTable();
        }

        private void dtgvTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int dong = e.RowIndex;
            try
            {
                txtTableID.Text = dtgvTable.Rows[dong].Cells[0].Value.ToString();
                txtTableName.Text = dtgvTable.Rows[dong].Cells[1].Value.ToString();
            }
            catch
            { }
        }

        private void btnShowTable_Click(object sender, EventArgs e)
        {
            using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
            {
                var result = from a in db.TableFoods
                             select a;
                dtgvTable.DataSource = result;
            }
        }

        private void btnAddTable_Click(object sender, EventArgs e)
        {
            try
            {
                using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
                {
                    if (txtTableName.Text == "")
                    {
                        MessageBox.Show("Vui lòng điền đầy đủ thông tin!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtTableName.Focus();
                    }
                    else
                    {
                        if (MessageBox.Show("Bạn có muốn thêm " + txtTableName.Text + " ?", "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                        {
                            TableFood t = new TableFood();
                            t.name = txtTableName.Text;
                            t.status = "Trống";
                            db.TableFoods.InsertOnSubmit(t);
                            db.SubmitChanges();
                            LoadTable();
                            addTable(this, new EventArgs());
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }

        private void btnDeleteTable_Click(object sender, EventArgs e)
        {
            try
            {
                using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
                {
                    if (txtTableID.Text == "")
                    {
                        MessageBox.Show("Vui lòng chọn bàn cần xóa!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtTableName.Focus();
                    }
                    else
                    {
                        TableFood t = db.TableFoods.FirstOrDefault(x => x.id == Convert.ToInt16(txtTableID.Text));
                        if (t != null)
                        {
                            if (MessageBox.Show("Bạn có muốn xóa " + txtTableName.Text + " ?", "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                            {
                                var b = from u in db.Bills
                                        where u.idTable == Convert.ToInt16(txtTableID.Text)
                                        select u;
                                foreach (var i in b)
                                {
                                    int idbill = i.id;
                                    BillInfo bi = db.BillInfos.FirstOrDefault(x => x.idBill == idbill);
                                    if (bi != null)
                                    {
                                        db.BillInfos.DeleteOnSubmit(bi);
                                        db.SubmitChanges();
                                    }
                                    db.Bills.DeleteOnSubmit(i);
                                    db.SubmitChanges();
                                }
                                db.TableFoods.DeleteOnSubmit(t);
                                db.SubmitChanges();
                                LoadTable();
                                txtTableID.Text = "";
                                txtTableName.Text = "";
                                txtTableName.Focus();
                                deleteTable(this, new EventArgs());
                            }

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }
        string PathImage ;
        private void btnEditTable_Click(object sender, EventArgs e)
        {
            try
            {
                using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
                {
                    if (txtTableID.Text == "")
                    {
                        MessageBox.Show("Vui lòng chọn bàn cần cập nhật!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtTableName.Focus();
                    }
                    else
                    {
                        TableFood t = db.TableFoods.FirstOrDefault(x => x.id == Convert.ToInt16(txtTableID.Text));
                        if (t != null)
                        {
                            if (MessageBox.Show("Bạn có muốn cập nhật " + txtTableName.Text + " ?", "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                            {
                                t.name = txtTableName.Text;
                                db.SubmitChanges();
                                LoadTable();
                                editTable(this, new EventArgs());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }
        private void btnChooseImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp; *.png)|*.jpg; *.jpeg; *.gif; *.bmp; *.png";
            if (open.ShowDialog() == DialogResult.OK)
            {
                picBoxImageFood.Image = new Bitmap(open.FileName);
                PathImage = open.FileName;
            }
        }

        private event EventHandler addFood;
        public event EventHandler AddFood
        {
            add { addFood += value; }
            remove { addFood -= value; }
        }
        private event EventHandler deleteFood;
        public event EventHandler DeleteFood
        {
            add { deleteFood += value; }
            remove { deleteFood -= value; }
        }
        private event EventHandler editFood;
        public event EventHandler EditFood
        {
            add { editFood += value; }
            remove { editFood -= value; }
        }
        private event EventHandler addCategory;
        public event EventHandler AddCategory
        {
            add { addCategory += value; }
            remove { addCategory -= value; }
        }
        private event EventHandler deleteCategory;
        public event EventHandler DeleteCategory
        {
            add { deleteCategory += value; }
            remove { deleteCategory -= value; }
        }
        private event EventHandler editCategory;
        public event EventHandler EditCategory
        {
            add { editCategory += value; }
            remove { editCategory -= value; }
        }
        private event EventHandler addTable;
        public event EventHandler AddTable
        {
            add { addTable += value; }
            remove { addTable -= value; }
        }
        private event EventHandler editTable;
        public event EventHandler EditTable
        {
            add { editTable += value; }
            remove { editTable -= value; }
        }
        private event EventHandler deleteTable;
        public event EventHandler DeleteTable
        {
            add { deleteTable += value; }
            remove { deleteTable -= value; }
        }


        #endregion


    }
}
