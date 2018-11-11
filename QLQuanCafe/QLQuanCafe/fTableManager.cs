using DevComponents.DotNetBar;
using QLQuanCafe.DAO;
using QLQuanCafe.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QLQuanCafe.fLogin;

namespace QLQuanCafe
{
    public partial class fTableManager : Form
    {
       
        public fTableManager()
        {
            InitializeComponent();

        }

        #region MeThod

        private void fTableManager_Load(object sender, EventArgs e)
        {
            timer1.Start();
            lbngay.Text = DateTime.Now.ToShortDateString();
            LoadTable();
            LoadCategory();
            LoadComboboxTable(cbSwitchTable);
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
                        lbDisplayName.Text = u.DisplayName.ToString();
                        getUserName.strDisplayname = u.DisplayName.ToString();
                        thôngTinTàiKhoảnToolStripMenuItem.Text += " (" + getUserName.strDisplayname + ")";
                        if (u.Type == 1)
                        {
                            lbAccountType.Text = "Admin";
                            adminToolStripMenuItem.Enabled = true;
                        }                                                                            
                        else
                        {
                            lbAccountType.Text = "Nhân viên";
                            adminToolStripMenuItem.Enabled = false;
                        }
                            
                    }
                }
            }
        }

        public void LoadTable()
        {
            flpTable.Controls.Clear();

            List<Table> tableList = TableDAO.Instance.LoadTableList();
            foreach (Table item in tableList)
            {
                Button btn = new Button() { Width = TableDAO.TableWidth, Height = TableDAO.TableHeight };
                btn.Text = item.Name + Environment.NewLine + item.Status;
                btn.FlatStyle = FlatStyle.Flat;
                btn.ForeColor = Color.MidnightBlue;
                // thông tin control gắn vó thẻ, kiểu obj
                btn.Tag = item;
                btn.Click += btn_Click;

                switch (item.Status)
                {
                    case "Trống":
                        btn.BackColor = Color.LightSkyBlue;
                        btn.Font = new Font("Times New Roman", 10, FontStyle.Regular);
                        break;
                    default:
                        btn.BackColor = Color.FromArgb(192, 255, 255);
                        btn.Font = new Font("Times New Roman", 10, FontStyle.Bold);
                        break;
                }

                flpTable.Controls.Add(btn);
            }
        }

        public void LoadComboboxTable(ComboBox cb)
        {
            cb.DataSource = TableDAO.Instance.LoadTableList();
            cb.DisplayMember = "Name";
            cb.ValueMember = "ID";
        }

        double finailtotalPrice = 0;
        public void ShowBill(int id)
        {
            using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
            {
                var showlv = db.USP_GetBillUncheckByTableID(idtb); //gọi store trong sql
                lvBill.Items.Clear();
                // c1 Add ListView dùng mảng
                //string[] arr = new string[4];
                //foreach (var i in showlv)
                //{
                //    arr[0] = i.Tên_món + "";
                //    arr[1] = i.Số_lượng + "";
                //    arr[2] = i.Đơn_giá + "";
                //    arr[3] = i.Thành_tiền + "";
                //    ListViewItem itm = new ListViewItem(arr);
                //    lvBill.Items.Add(itm);
                //}

                //c2
                double totalPrice = 0;
                foreach (var i in showlv)
                {
                    ListViewItem lvItem = new ListViewItem(i.Tên_món.ToString());
                    lvItem.SubItems.Add(i.Số_lượng.ToString());
                    lvItem.SubItems.Add(i.Đơn_giá.ToString());
                    lvItem.SubItems.Add(i.Thành_tiền.ToString());
                    totalPrice += i.Thành_tiền;
                    lvBill.Items.Add(lvItem);
                }
                //chuyển culture sang việt nam
                finailtotalPrice = totalPrice;
                CultureInfo culture = new CultureInfo("vi-VN");
                lbTotalPrice.Text = totalPrice.ToString("c", culture);
                lbPriceDefault.Text = totalPrice.ToString("c", culture);
                nmDiscount.Value = 0;
                LoadComboboxTable(cbSwitchTable);
            }

        }
        

        public void LoadCategory()
        {
            using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
            {
                cbCategory.DataSource = db.FoodCategories;
                cbCategory.DisplayMember = "name";
                cbCategory.ValueMember = "id";


            }
        }

        public void LoadFoodListByCategoryID()
        {
            using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
            {
                //string idFoodCategory = cbCategory.SelectedValue.ToString();
                // lấy ra id được chọn ở combobox Category
                string idFoodCategory = ((FoodCategory)cbCategory.SelectedItem).id.ToString();
                var query = from u in db.Foods
                            where u.idCategory == Convert.ToInt16(idFoodCategory)
                            select u;
                cbFood.DataSource = query;
                cbFood.DisplayMember = "name";
                cbFood.ValueMember = "id";

            }


        }

        #endregion

        #region Events

        int idtb;
        private void btn_Click(object sender, EventArgs e)
        {
            int tableID = ((sender as Button).Tag as Table).ID; // idtable
            idtb = tableID;
            lvBill.Tag = (sender as Button).Tag; // Lưu tag của lv khi click vào table bất kì
            ShowBill(tableID);
            lbtable.Text = ((sender as Button).Tag as Table).Name;
        }

        private void đăngXuấtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void thôngTinCáNhânToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fAccountProfile f = new fAccountProfile();
            f.UpdateAccount += F_UpdateAccount;//gọi sự kiện F_UpdateAccount bên form fAccountProfile
            f.ShowDialog();
        }

        private void F_UpdateAccount(object sender, EventArgs e)
        {
            thôngTinTàiKhoảnToolStripMenuItem.Text = "Thông tin tài khoản (" + getUserName.strDisplayname + ")";
            lbDisplayName.Text = getUserName.strDisplayname;
        }

        private void adminToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fAdmin f = new fAdmin();
            f.AddFood += F_AddFood;
            f.EditFood += F_EditFood;
            f.DeleteFood += F_DeleteFood;
            f.AddCategory += F_AddCategory;
            f.EditCategory += F_EditCategory;
            f.DeleteCategory += F_DeleteCategory;
            f.AddTable += F_AddTable;
            f.EditTable += F_EditTable;
            f.DeleteTable += F_DeleteTable;
            f.ShowDialog();
        }

        private void F_DeleteTable(object sender, EventArgs e)
        {
            LoadTable();
            if (lvBill.Tag != null)
                ShowBill((lvBill.Tag as Table).ID);
        }

        private void F_EditTable(object sender, EventArgs e)
        {
            LoadTable();
            if (lvBill.Tag != null)
                ShowBill((lvBill.Tag as Table).ID);
        }

        private void F_AddTable(object sender, EventArgs e)
        {
            LoadTable();
            if (lvBill.Tag != null)
                ShowBill((lvBill.Tag as Table).ID);
        }

        private void F_DeleteCategory(object sender, EventArgs e)
        {
            LoadCategory();
            if (lvBill.Tag != null)
                ShowBill((lvBill.Tag as Table).ID);
        }

        private void F_EditCategory(object sender, EventArgs e)
        {
            LoadCategory();
            if (lvBill.Tag != null)
                ShowBill((lvBill.Tag as Table).ID);
        }

        private void F_AddCategory(object sender, EventArgs e)
        {
            LoadCategory();
            if (lvBill.Tag != null)
                ShowBill((lvBill.Tag as Table).ID);
        }

        private void F_DeleteFood(object sender, EventArgs e)
        {
            LoadFoodListByCategoryID();
            if (lvBill.Tag != null)
                ShowBill((lvBill.Tag as Table).ID);
        }

        private void F_EditFood(object sender, EventArgs e)
        {
            LoadFoodListByCategoryID();
            if (lvBill.Tag != null)
                ShowBill((lvBill.Tag as Table).ID);
        }

        private void F_AddFood(object sender, EventArgs e)
        {
            LoadFoodListByCategoryID();
            if (lvBill.Tag != null)
                ShowBill((lvBill.Tag as Table).ID);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lbtime.Text = DateTime.Now.ToLongTimeString();
        }

        private void cbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadFoodListByCategoryID();
        }

        private void btnAddFood_Click(object sender, EventArgs e)
        {
            Table table = lvBill.Tag as Table;
            if (table == null)
            {
                MessageBox.Show("Vui lòng chọn bàn cần thêm!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
           
            try
            {
                using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
                {
                    // Check bảng Bill mà idTable của Bảng TableFood = idTable của bảng Bill và Bill.status =0            
                    var CheckBill = db.USP_GetIDBillUncheckByTableID(idtb).FirstOrDefault(); // check bill có tồn tại hay k
                    //var result = from g in db.Bills
                    //             from h in db.TableFoods
                    //             where g.idTable == h.id && g.idTable == idtb && g.status == 0
                    //             select new
                    //             {
                    //                 g.id,
                    //                 h.name
                    //             };
                    //var CheckBill = result.FirstOrDefault();
                    string nameTable = table.Name.ToString();                   
                    string z = ((Food)cbFood.SelectedItem).name.ToString();  //get name combobox
                    if (CheckBill != null) //có bill thêm bớt món
                    {
                        int idbill = CheckBill.id; // lấy ra idbill                       
                        BillInfo binfo = db.BillInfos.FirstOrDefault(x => x.idBill == idbill && x.idFood == (int)cbFood.SelectedValue);
                        if (binfo != null)  // có food trong bill info if count <0 delete billinfo
                        {
                            int newcount = binfo.count + nmFoodCount.Value;
                            if (newcount > 0)  //update count food
                            {
                                binfo.count = newcount;
                                db.SubmitChanges();
                                MessageBox.Show(nameTable + ": Đã cập nhật sô lượng món " + z, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else  //deletebillinfo
                            {
                                db.BillInfos.DeleteOnSubmit(binfo);
                                db.SubmitChanges();
                                MessageBox.Show(nameTable + ": Bạn vừa xóa món " + z, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else //add food mới
                        {
                            TableFood tb = db.TableFoods.FirstOrDefault(x => x.id == idtb);
                            if (tb != null)
                            {
                                tb.status = "Có người";
                                db.SubmitChanges();
                            }
                            BillInfo b = new BillInfo();
                            b.idBill = idbill;
                            b.idFood = (int)cbFood.SelectedValue;
                            if (nmFoodCount.Value <= 0)
                                b.count = 1;
                            else
                                b.count = Convert.ToInt16(nmFoodCount.Value.ToString());
                            db.BillInfos.InsertOnSubmit(b);
                            db.SubmitChanges();
                            MessageBox.Show(nameTable + ": Bạn vừa thêm món " + z, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else  // add newwbil billinnfo
                    {
                        Bill bill_new = new Bill();
                        bill_new.DateCheckIn = Convert.ToDateTime(lbngay.Text);
                        bill_new.idTable = idtb;
                        bill_new.status = 0;
                        bill_new.discount = 0;
                        db.Bills.InsertOnSubmit(bill_new);
                        db.SubmitChanges();

                        // bàn đang trống chuyển thành có người update TableFood
                        TableFood tb = db.TableFoods.FirstOrDefault(x => x.id == idtb);
                        if (tb != null)
                        {
                            tb.status = "Có người";
                            db.SubmitChanges();
                        }

                        //insert billInfo
                        int maxid = db.Bills.Select(x => x.id).Max(); // lấy id max của bill
                        BillInfo bi = new BillInfo();
                        bi.idBill = maxid;
                        bi.idFood = Convert.ToInt16(cbFood.SelectedValue.ToString());
                        if (nmFoodCount.Value <= 0)
                            bi.count = 1;
                        else
                            bi.count = Convert.ToInt16(nmFoodCount.Value.ToString());
                        db.BillInfos.InsertOnSubmit(bi);
                        db.SubmitChanges();
                        MessageBox.Show(nameTable + ": Bạn vừa thêm món " + z, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }


                }
                ShowBill(table.ID);
                LoadTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }         
        }
     
        private void btnCheckOut_Click(object sender, EventArgs e)
        {
            Table table = lvBill.Tag as Table;
            if (table == null)
            {
                MessageBox.Show("Vui lòng chọn bàn cần thanh toán!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
            {
                var checkBill = db.USP_GetIDBillUncheckByTableID(idtb).FirstOrDefault();
              
                if (checkBill != null)
                {
                    if (MessageBox.Show(string.Format("Bạn có chắc muốn thanh toán cho {0}\nTổng tiền: {1}\nGiảm giá: {2}% \nTổng cộng: {3}",table.Name,lbPriceDefault.Text,nmDiscount.Value.ToString(),lbTotalPrice.Text ), "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        int discount = (int)nmDiscount.Value;
                        int idBill = checkBill.id;
                        Bill bi = db.Bills.FirstOrDefault(x => x.id == idBill);
                        bi.DateCheckOut = Convert.ToDateTime(lbngay.Text);
                        bi.status = 1;
                        bi.discount = discount;
                        //totalprice  string --> double
                        string z = lbTotalPrice.Text.Substring(0, lbTotalPrice.Text.Length - 5).Replace(".", "");
                        double total = Convert.ToDouble(z);                      
                        bi.totalPrice = total;
                        db.SubmitChanges();

                        TableFood tb = db.TableFoods.FirstOrDefault(x => x.id == idtb);
                        if (tb != null)
                        {
                            tb.status = "Trống";
                            db.SubmitChanges();
                        }
                        LoadTable();
                        ShowBill(idtb);
                    }
                }
                else
                {
                    MessageBox.Show(table.Name+ " chưa gọi món!!! \nKHÔNG THỂ THANH TOÁN", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        private void nmDiscount_ValueChanged_1(object sender, EventArgs e)
        {
            double finaltotal = finailtotalPrice - (finailtotalPrice / 100) * (int)nmDiscount.Value;
            CultureInfo culture = new CultureInfo("vi-VN");
            lbTotalPrice.Text = finaltotal.ToString("c", culture);
        }

        private void btnSwitchTable_Click(object sender, EventArgs e)
        {
            Table table = lvBill.Tag as Table;
            if (table == null)
            {
                MessageBox.Show("Vui lòng chọn bàn cần chuyển!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int id1 = (lvBill.Tag as Table).ID;
            int id2 = (cbSwitchTable.SelectedItem as Table).ID;
            string a = (lvBill.Tag as Table).Status;
            string b = (cbSwitchTable.SelectedItem as Table).Status;
            //MessageBox.Show(a+ ""+b);

            if (MessageBox.Show(string.Format("Bạn có thật sự muốn chuyển {0} qua {1}", (lvBill.Tag as Table).Name, (cbSwitchTable.SelectedItem as Table).Name), "Thông báo", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                //if(a == "Trống")
                //{
                //    MessageBox.Show((lvBill.Tag as Table).Name + " không có người!!! \nKHÔNG THỂ CHUYỂN", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //}
                //else
                //{
                    TableDAO.Instance.SwitchTable(id1, id2);
                    LoadTable();
                    ShowBill(id1);
                //}
                
            }
        }
        private void btnGatherTable_Click(object sender, EventArgs e)
        {
            Table table = lvBill.Tag as Table;
            if (table == null)
            {
                MessageBox.Show("Vui lòng chọn bàn cần gộp!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int id1 = (lvBill.Tag as Table).ID;
            int id2 = (cbSwitchTable.SelectedItem as Table).ID;

            if (MessageBox.Show(string.Format("Bạn có thật sự muốn gộp {0} và {1}", (lvBill.Tag as Table).Name, (cbSwitchTable.SelectedItem as Table).Name), "Thông báo", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                //if(a == "Trống")
                //{
                //    MessageBox.Show((lvBill.Tag as Table).Name + " không có người!!! \nKHÔNG THỂ CHUYỂN", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //}
                //else
                //{
                TableDAO.Instance.GatherTable(id1, id2);
                LoadTable();
                ShowBill(id1);
                //}

            }
        }

        private void báoCáoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fReport f = new fReport();
            f.ShowDialog();
        }



        #endregion

       
    }


}

