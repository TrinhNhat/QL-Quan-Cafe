using Microsoft.Reporting.WinForms;
using QLQuanCafe.QuanLyQuanCafeDataSetTableAdapters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLQuanCafe
{
    public partial class fReport : Form
    {
        public fReport()
        {
            InitializeComponent();
        }

        private void fReport_Load(object sender, EventArgs e)
        {
            DateTime today = DateTime.Now;
            dtpkFromDate.Value = new DateTime(today.Year, today.Month, 1);
            dtpkToDate.Value = dtpkFromDate.Value.AddMonths(1).AddDays(-1);
        }

        private void btnAddReport_Click(object sender, EventArgs e)
        {
            using (QLQuanCafeDataContext db = new QLQuanCafeDataContext())
            {
                var a = db.USP_RPBillByDate(Convert.ToDateTime(dtpkFromDate.Value.ToShortDateString()), Convert.ToDateTime(dtpkToDate.Value.ToShortDateString())).FirstOrDefault();
                
                if(a!=null)
                {                    
                    //  var table = from b in db.Bills
                    //                        join t in db.TableFoods on b.idTable equals t.id
                    //                        where b.DateCheckIn>= fdate && b.DateCheckOut<=tdate && b.status==1
                    //                        select new { b.id, t.name,b.discount,b.totalPrice };
                    this.USP_RPBillByDateTableAdapter.Fill(this.QuanLyQuanCafeDataSet.USP_RPBillByDate, dtpkFromDate.Value, dtpkToDate.Value);
                    rpBillByDate.RefreshReport();
                }
                else
                {
                    MessageBox.Show("Không có hóa đơn nào!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
               
        }
    }
}
