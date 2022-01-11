﻿using DevExpress.XtraEditors;
using QuanLyBilliard.BL;
using QuanLyBilliard.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyBilliard.GUI
{
    public partial class FrmSuDungDichVu : Form
    {
        #region khai báo
        float tongtien = 0;
        BackgroundWorker backgroundWorker1;
        BL_HoaDon blHoaDon;
        BL_Ban blBan;
        BL_LoaiThucPham blLoaiThucPham;
        BL_ThucPham blThucPham;
        BL_NhanVien blNhanVien;
        BL_KhachHang blKhachHang;
        BL_KhuyenMai blKhuyenMai;
        int idBanHienTai;
        const int TABLE_WIDTH = 70;
        const int TABLE_HEIGHT = 120;
        int quyen;
        #endregion

        /// <summary>
        /// Tạo hàm dựng cho form Sử dụng dịch vụ với tham số là quyền truyền vào
        /// </summary>
        /// <param name="quyen"></param>
        public FrmSuDungDichVu(int quyen)
        {
            InitializeComponent();
            blBan = new BL_Ban(this);
            blHoaDon = new BL_HoaDon(this);
            blLoaiThucPham = new BL_LoaiThucPham(this);
            blThucPham = new BL_ThucPham(this);
            blNhanVien = new BL_NhanVien(this);
            blKhachHang = new BL_KhachHang(this);
            blKhuyenMai = new BL_KhuyenMai(this);
            this.quyen = quyen;
            
        }
        /// <summary>
        /// Khởi chạy background worker - hiển thị bàn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmSuDungDichVu_Load(object sender, EventArgs e)
        {
            #region Khai báo cho background
            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.DoWork += BackgroundWorker1_DoWork;
            backgroundWorker1.ProgressChanged += BackgroundWorker1_ProgressChanged;
            if (backgroundWorker1.IsBusy != true)
            {
                // Start the asynchronous operation.
                backgroundWorker1.RunWorkerAsync();
            }
            #endregion
            
            HienThiBan();
            LoadLoaiThucPham();
            Enabel(false);
            dtBatDau.Enabled = false;
            btnBatDau.Enabled = false;
            //Phân quyền
            switch (quyen)
            {
                case BL.BL_Quyen.ADMIN:
                    QuyenAdmin();
                    break;
                case BL.BL_Quyen.QUANLY:
                    QuyenQuanLy();
                    break;
                case BL.BL_Quyen.THUNGAN:
                    QuyenThuNgan();
                    break;
                case BL.BL_Quyen.THUKHO:
                    QuyenThuKho();
                    break;
            }
            //Kiểm tra khuyến mãi
            double kmh = 0, kmn = 0;
            DataTable km = blKhuyenMai.XemKhuyenMai();
            DateTime homnay = DateTime.Now.Date;
            if (km.Rows.Count != 0)
            {
                foreach (DataRow row in km.Rows)
                {
                    if (homnay >= (DateTime)row["NGAYBATDAU"] && homnay <= (DateTime)row["NGAYKETTHUC"])
                    {
                        if (kmh < (double)row["GIAMGIAGIO"])
                        {
                            kmh = (double)row["GIAMGIAGIO"];
                        }
                        if (kmn < (double)row["GIAMGIATHUCPHAM"])
                        {
                            kmn = (double)row["GIAMGIATHUCPHAM"];
                        }
                    }
                }
                lbGiamGiaGio.Tag = kmh;
                lbGiamGiaNuoc.Tag = kmn;
            }
            
        
        }

        public void LoadLoaiThucPham()
        {
            List<LoaiThucPham> lst = blLoaiThucPham.LoadLoaiThucPham();
            foreach (LoaiThucPham food in lst)
            {
                treeView1.Nodes.Add(food.id.ToString(), food.tenloai);
                treeView1.NodeMouseClick += TreeView1_NodeMouseClick;
                treeView1.Tag = food;
            }
        }
        #region Event
        public void TreeView1_NodeMouseClick(object sender, System.Windows.Forms.TreeNodeMouseClickEventArgs e)
        {
            LoadThucPham(Int32.Parse(e.Node.Name));
            //blThucPham.getDuLieu(Int32.Parse(e.Node.Name));
            btnDaiDienThucPham.Tag = null;
            
        }

        private void LoadThucPham(int v)
        {
            DataTable result = blThucPham.getDuLieu(v);
            RefeshThucPham(result);
        }

        private void RefeshThucPham(DataTable result)
        {
            dgvThucPham.Rows.Clear();
            foreach(DataRow row in result.Rows)
            {
                dgvThucPham.Rows.Add(row["TENTHUCPHAM"], row["DVT"], row["GIABAN"], row["ID_THUCPHAM"]);
            }
        }


        #endregion

        #region Phân quyền
        private void QuyenThuKho()
        {
            txtGiamGiaNuoc.Enabled = false;
        }

        private void QuyenThuNgan()
        {
            txtGiamGiaNuoc.Enabled = false;
        }

        private void QuyenQuanLy()
        {
            txtGiamGiaNuoc.Enabled = false;
        }

        private void QuyenAdmin()
        {
            txtGiamGiaNuoc.Enabled = true;
        }
        #endregion

        /// <summary>
        /// Hiển thị tất cả các bàn có trong cơ sở dữ liệu
        /// </summary>
        public void HienThiBan()
        {
            //Xóa hết các bàn hiện tại để tải lại bàn mới
            flpBan.Controls.Clear();
            List<Ban> lst = blBan.LayBan();
            foreach (Ban table in lst)
            {
                // Tạo ra các button bàn, các thuộc tính của bàn như text và cách hiển thị màu của nó
                SimpleButton btn = new SimpleButton() { Width = TABLE_WIDTH, Height = TABLE_HEIGHT };
                
                //Set text
                string tenban;
                if (table.TrangThai)
                {
                    tenban = table.TenBan + "\n(ON)";
                }
                else tenban = table.TenBan + "\n(OFF)";
                btn.Text = tenban ;
                //SetColor
                btn.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
                btn.LookAndFeel.UseDefaultLookAndFeel = false;
                if (table.TrangThai)
                {
                    btn.Appearance.Image = Properties.Resources.off;
                }
                else btn.Appearance.Image = Properties.Resources.on;


                //Catch Event
                btn.Click += new EventHandler(btn_Click);
                btn.DoubleClick += Btn_DoubleClick;
                btn.Tag = table;

                // Add control (as button) in flowLayout
                flpBan.Controls.Add(btn);

            }
        }



        /// <summary>
        /// Bật bàn hoặc tắt bàn bằng cách double click vào bàn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_DoubleClick(object sender, EventArgs e)
        {
            Ban ban = ((sender as SimpleButton).Tag as Ban);
            if (!ban.TrangThai)
            {
                blBan.BatGio(ban.ID_Ban);
                HienThiBan();
                Enabel(true);
            }
            else
            {
                KetThuc();
            }
        }
        /// <summary>
        /// Hiển thị bàn đang click lên thanh detail ở bên phải
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Click(object sender, EventArgs e)
        {
            Ban ban = (sender as SimpleButton).Tag as Ban;
            lbTenBan.Text = ban.TenBan;
            //Lấy 1 button để lưu dữ liệu của 1 bàn khi click vào bàn
            btnDaiDienBan.Tag = ban;
            btnDaiDienBan.Text = ban.TenBan;
            // Khi đã click vào 1 bàn lấy được object bàn thì cũng phải lấy được object hoadon của nó luôn
            //Nếu bàn đã được bật thì mới lấy hóa đơn và show nó lên, còn không thì ko show gì cả
            if (ban.TrangThai)
            {
                float tongtien = 0f;
                DataTable dt = blBan.LayHoaDon(ban);
                HoaDon hoadon = new HoaDon(dt.Rows[0]);
                btnHoaDon.Tag = hoadon;
                btnHoaDon.Text = hoadon.ID_HoaDon.ToString();
                //Show các mặt hàng có trong hóa đơn và tính tổng tiền
                blHoaDon.ShowBill(hoadon, out tongtien);
                // Show số hóa đơn
                txtSoHD.Text = hoadon.ID_HoaDon.ToString();
                //Show ngày lập hóa đơn 
                DateTime NgayLapHoaDon = blBan.LayGioVao(ban.ID_Ban);
                dtpNgay.Text = NgayLapHoaDon.ToString();
                dtBatDau.Text = NgayLapHoaDon.TimeOfDay.ToString("hh");
                dtBatDau.Text += ":";
                dtBatDau.Text += NgayLapHoaDon.TimeOfDay.ToString("mm");
                //tổng tiền
                txtTienNuoc.Text = tongtien.ToString();
                txtTongCong.Text = tongtien.ToString();
                //Số lượng
                cbSoLuong.Text = "1";
                //Nhân viên
                cbNhanVien.DataSource = blNhanVien.LayNhanVien();
                cbNhanVien.DisplayMember = "TENNHANVIEN";
                cbNhanVien.ValueMember = "ID_NHANVIEN";
                //Khách hàng
                cbKhachHang.DataSource = blKhachHang.LayKhachHang();
                cbKhachHang.DisplayMember = "TENKHACHHANG";
                cbKhachHang.ValueMember = "ID_KHACHHANG";
                //Giảm giá
                if (lbGiamGiaGio.Tag == null)
                {
                    lbGiamGiaGio.Tag = "0";
                }
                double giamgio = Double.Parse(lbGiamGiaGio.Tag.ToString());
                if (hoadon.GiamGiaGio > giamgio)
                {
                    txtGiamGiaGio.Text = hoadon.GiamGiaGio.ToString();
                }else
                {
                    txtGiamGiaGio.Text = lbGiamGiaGio.Tag.ToString();
                }

                if (lbGiamGiaNuoc.Tag == null)
                {
                    lbGiamGiaNuoc.Tag = "0";
                }
                double giamnuoc = Double.Parse(lbGiamGiaNuoc.Tag.ToString());
                if (hoadon.GiamGiaThucPham > giamnuoc)
                {
                    txtGiamGiaNuoc.Text = hoadon.GiamGiaThucPham.ToString();
                }else
                {
                    txtGiamGiaNuoc.Text = lbGiamGiaNuoc.Tag.ToString();
                }
                Enabel(true);

            }
            else
            {
                btnHoaDon.Tag = null;
                Enabel(false);
                //Hiển thị giờ hiện tại của hệ thống.
                dtKetThuc.Text = DateTime.Now.TimeOfDay.ToString();
            }
        }
        /// <summary>
        /// Gán các giá trị enable của bàn bật (true) và tắt (false)
        /// </summary>
        /// <param name="v"></param>
        public void Enabel(bool v)
        {
            if (v)
            {
                //Enable
                dgvThucPhamTrongHoaDon.Enabled = true;
                btnBatDau.Enabled = false;
                dtBatDau.Enabled = false;
                btnKetThuc.Enabled = true;
                dtKetThuc.Enabled = true;
                panel4.Enabled = true;

            }
            else
            {
                dgvThucPhamTrongHoaDon.Enabled = false;
                txtSoGioChoi.Text = "";
                btnBatDau.Enabled = true;
                dtBatDau.Enabled = true;
                btnKetThuc.Enabled = false;
                dtKetThuc.Enabled = false;
                panel4.Enabled = false;
                //Xóa dữ liệu ở bàn đang bật đi
                txtSoHD.Text = "";
                // Xóa ngày giờ lập hóa đơn
                dtpNgay.Text = "";
                dtBatDau.Text = "";
                dgvThucPhamTrongHoaDon.Rows.Clear();
                dtBatDau.Text = DateTime.Now.TimeOfDay.ToString();
            }
        }

        /// <summary>
        /// Báo cáo của background woker - tính tổng giờ chơi hiển thị tại hóa đơn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// Không hiểu vì sao mà đối tượng bàn lấy được tất cả các giá trị nhưng không lấy được giờ vào, bắt buộc phải truy xuất đến sql
        /// </remarks>
        public void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            if ((btnDaiDienBan.Tag as Ban != null) && (btnDaiDienBan.Tag as Ban).TrangThai)
            {
                //Thời gian
                DateTime thoiGianHienTai = DateTime.Now;
                DateTime thoiGianBatBan = DateTime.Parse(dtpNgay.Text);
                int minutes = thoiGianHienTai.Minute - thoiGianBatBan.Minute;
                int hour = thoiGianHienTai.Hour - thoiGianBatBan.Hour;
                int day = thoiGianHienTai.Day - thoiGianBatBan.Day;
                float giamGiaGio = 0;
                if (txtGiamGiaGio.Text != "")
                {
                    giamGiaGio = float.Parse(txtGiamGiaGio.Text);
                }
                float giamGiaThucPham = 0;
                if (txtGiamGiaNuoc.Text != "")
                {
                    giamGiaThucPham = float.Parse(txtGiamGiaNuoc.Text);
                }
                
                //Làm tròn
                if (minutes < 0)
                {
                    //Phút sau nhỏ hơn phút trước
                    minutes += 60;
                    if (hour > 0)
                    {
                        hour--;
                    }else
                    {
                        hour = 24 + hour - 1;
                        day--;
                    }
                    ////Làm tròn phút để tính tiền
                    //if (minutes%10 >=5) minutes = (minutes/10+1)*10;
                }
               
                float tiengio = blBan.TinhTienGio(day,hour, minutes,giamGiaGio);
                txtTienGio.Text = tiengio.ToString();

                //Tiền nước
                blHoaDon.ShowBill((btnHoaDon.Tag as HoaDon), out tongtien);
                tongtien-= tongtien * giamGiaThucPham / 100;
                txtTienNuoc.Text = tongtien.ToString();
                //Hiển thị
                if (day > 0)
                {
                    hour += day * 24;
                }
                txtSoGioChoi.Text = hour.ToString() + ":" + minutes.ToString();
                
                txtTongCong.Text = (tiengio + float.Parse(txtTienNuoc.Text)).ToString();
            }
        }

        /// <summary>
        /// Hàm làm việc liên tục của background worker, đây như là một thread riêng biệt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            while (true)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    // Perform a time consuming operation and report progress.
                    System.Threading.Thread.Sleep(500);
                    worker.ReportProgress(1);
                }
            }
        }

        /// <summary>
        /// Bật giờ - tạo bill  - Hiển thị
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBatDau_Click(object sender, EventArgs e)
        {
            Ban ban = btnDaiDienBan.Tag as Ban;
            blBan.BatGio(ban.ID_Ban);
            HienThiBan();
            Enabel(true);
        }

        /// <summary>
        /// Kết thúc, nghỉ chơi, có thể thanh toán hoặc không
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnKetThuc_Click(object sender, EventArgs e)
        {
            KetThuc();
        }

        /// <summary>
        /// Khi bấm kết thúc hoặc click đúp chuột để tắt bàn
        /// </summary>
        public void KetThuc()
        {
            string nhanvien=null, khachhang = null;
            HoaDon hd = btnHoaDon.Tag as HoaDon;
            if (cbNhanVien.Items.Count != 0)
            {
                nhanvien = cbNhanVien.SelectedValue.ToString();
            }
            
            if (cbKhachHang.Items.Count != 0)
            {
                khachhang = cbKhachHang.SelectedValue.ToString();
            }
         
            
            string tiengio = txtTienGio.Text;
            string tienthucpham = txtTienNuoc.Text;
            (btnDaiDienBan.Tag as Ban).TrangThai = false;
            blBan.KetThuc(hd, nhanvien, khachhang,tiengio,tienthucpham);
            HienThiBan();
            Enabel(false);
            FrmHoaDon f = new FrmHoaDon(hd.ID_HoaDon, true);
            f.ShowDialog();
        }


        /// <summary>
        /// Thêm thực phẩm vào hóa đơn bằng nút thêm với số lượng ở combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnThem_Click(object sender, EventArgs e)
        {
            if (btnDaiDienThucPham.Tag == null)
            {
                MessageBox.Show("Bạn chưa chọn thực phẩm để thêm");
            }
            else if (cbSoLuong.Text == "0")
            {
                MessageBox.Show("Không thể thêm mặt hàng với số lượng là 0");
            }
            else
            {
                //Trước hết cần phải kiểm tra hàng đó có trong danh sách bill chưa, nếu có rồi thì update số lượng, còn chưa thì mới insert => kiểm tra và làm 1 lần trong csdl
                int id_hoadon = (btnHoaDon.Tag as HoaDon).ID_HoaDon;
                int soluong = Int32.Parse(cbSoLuong.Text);
                int id_thucpham = Int32.Parse(btnDaiDienThucPham.Tag as String);
                blHoaDon.ThemMatHang(id_hoadon, soluong, id_thucpham);
                blHoaDon.ShowBill((btnHoaDon.Tag as HoaDon), out tongtien);
                txtTienNuoc.Text = tongtien.ToString();
            }
            
        }


        /// <summary>
        /// Bắt sự kiện khi chọn thực phẩm để tương tác với nó, ở đây là thêm vào hóa đơn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            btnDaiDienThucPham.Tag = dgvThucPham.CurrentRow.Cells["ID_THUCPHAM"].Value.ToString();
        }


        /// <summary>
        /// Bắt sự kiện khi click vào thực phẩm đã có trong hóa đơn để tương tác với nó, ở đây là thay đổi số lượng hoặc xóa khỏi hóa đơn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = dgvThucPhamTrongHoaDon.CurrentRow;
            btnDaiDienHangHoaDon.Tag = row;
            btnDaiDienHangHoaDon.Text = row.Cells[4].Value.ToString();
        }
        /// <summary>
        /// Xóa thực phẩm ra khỏi hóa đơn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (btnDaiDienHangHoaDon.Tag == null)
            {
                MessageBox.Show("Bạn chưa chọn món thực phẩm cần xóa");
            }
            else
            {
                string idThucPham = (btnDaiDienHangHoaDon.Tag as DataGridViewRow).Cells[4].Value.ToString();
                string soluong = (btnDaiDienHangHoaDon.Tag as DataGridViewRow).Cells[2].Value.ToString();
                int idHoaDon = (btnHoaDon.Tag as HoaDon).ID_HoaDon;
                blHoaDon.XoaMatHang(idHoaDon, idThucPham, soluong);
                blHoaDon.ShowBill(btnHoaDon.Tag as HoaDon, out tongtien);
                txtTienNuoc.Text = tongtien.ToString();
                btnDaiDienHangHoaDon.Tag = null;
            }
        }

        /// <summary>
        /// Đổi số lượng thực phẩm có trong hóa đơn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnDoiSoLuong_Click(object sender, EventArgs e)
        {
            if (btnDaiDienHangHoaDon.Tag == null)
            {
                MessageBox.Show("Bạn chưa chọn món thực phẩm cần sửa số lượng");
            }
            else
            {
               
                string idThucPham = (btnDaiDienHangHoaDon.Tag as DataGridViewRow).Cells[4].Value.ToString();
                string soluong = (btnDaiDienHangHoaDon.Tag as DataGridViewRow).Cells[2].Value.ToString();
                int idHoaDon = (btnHoaDon.Tag as HoaDon).ID_HoaDon;
                //blHoaDon.DoiSoLuong(idHoaDon, idThucPham, soluong);

                FrmDoiSoLuong f = new FrmDoiSoLuong(idThucPham, soluong, idHoaDon);

                f.ShowDialog();
                
                blHoaDon.ShowBill(btnHoaDon.Tag as HoaDon, out tongtien);
                txtTienNuoc.Text = tongtien.ToString();
                btnDaiDienHangHoaDon.Tag = null;
            }

        }

        /// <summary>
        /// Tăng số lượng thực phẩm lên 1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void simpleButton2_Click(object sender, EventArgs e)
        {
            if (btnDaiDienHangHoaDon.Tag == null)
            {
                MessageBox.Show("Bạn chưa chọn món thực phẩm cần sửa số lượng");
            }
            else
            {
                int idThucPham = Convert.ToInt32((btnDaiDienHangHoaDon.Tag as DataGridViewRow).Cells[4].Value.ToString());
                int idHoaDon = (btnHoaDon.Tag as HoaDon).ID_HoaDon;
                int soluong = Convert.ToInt32((btnDaiDienHangHoaDon.Tag as DataGridViewRow).Cells[2].Value.ToString());

                blHoaDon.DoiSoLuong(idHoaDon, idThucPham,soluong+ 1);
                blHoaDon.ShowBill(btnHoaDon.Tag as HoaDon, out tongtien);
                txtTienNuoc.Text = tongtien.ToString();
                btnDaiDienHangHoaDon.Tag = null;
            }
        }

        /// <summary>
        /// Giảm số lượng thực phẩm xuống 1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void simpleButton4_Click(object sender, EventArgs e)
        {
            if (btnDaiDienHangHoaDon.Tag == null)
            {
                MessageBox.Show("Bạn chưa chọn món thực phẩm cần sửa số lượng");
            }
            else if (dgvThucPhamTrongHoaDon.CurrentRow.Cells[2].Value.ToString() == "1")
            {
                MessageBox.Show("Không thể giảm mặt hàng này vì số lượng còn 1");
            }
            else
            {
                int soluong = Convert.ToInt32((btnDaiDienHangHoaDon.Tag as DataGridViewRow).Cells[2].Value.ToString());

                int idThucPham = Convert.ToInt32((btnDaiDienHangHoaDon.Tag as DataGridViewRow).Cells[4].Value.ToString());
                int idHoaDon = (btnHoaDon.Tag as HoaDon).ID_HoaDon;
                blHoaDon.DoiSoLuong(idHoaDon, idThucPham, soluong-1);
                blHoaDon.ShowBill(btnHoaDon.Tag as HoaDon, out tongtien);
                txtTienNuoc.Text = tongtien.ToString();
                btnDaiDienHangHoaDon.Tag = null;
            }
        }


        /// <summary>
        /// In thử bill
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void simpleButton3_Click(object sender, EventArgs e)
        {
            if (btnHoaDon.Tag == null)
            {
                MessageBox.Show("Bạn chưa chọn bàn!!");
            }else
            {
                HoaDon hd = btnHoaDon.Tag as HoaDon;
                FrmHoaDon f = new FrmHoaDon(hd.ID_HoaDon, false);
                string nhanvien = cbNhanVien.SelectedValue.ToString();
                //Thử khách hàng
                string khachhang;
                if (cbKhachHang.Items.Count > 0)
                {
                    khachhang = cbKhachHang.SelectedValue.ToString();
                }else
                {
                    khachhang = "NULL";
                }
                
                blHoaDon.GanGiaTriInThuBill(hd.ID_HoaDon, nhanvien, khachhang);
                f.ShowDialog();
            }
            
        }
        /// <summary>
        /// Hiển thị các hóa đơn đã được tạo và đã kết thúc
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnThanhToan_Click(object sender, EventArgs e)
        {
            FrmThanhToan f = new FrmThanhToan();
            f.ShowDialog();
            
        }


        /// <summary>
        /// Không có ý nghĩa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChuyenBan_DoubleClick(object sender, EventArgs e)
        {
            MessageBox.Show("VL");
        }


        /// <summary>
        /// Chuyển bàn này sang bàn khác
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChuyenBan_Click(object sender, EventArgs e)
        {
            idBanHienTai = (btnDaiDienBan.Tag as Ban).ID_Ban;
            FrmChuyenBan f = new FrmChuyenBan(idBanHienTai);
            f.ShowDialog();
            HienThiBan();
        }
        /// <summary>
        /// Thêm món ăn vào hóa đơn bằng thao tác double click vào thực phẩm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (btnDaiDienBan.Text == "Đại diện bàn")
            {
                MessageBox.Show("Bạn phải chọn bàn trước khi gọi món");
            }
            else if (btnHoaDon.Text == "Đại diện hóa đơn")
            {
                MessageBox.Show("Bạn phải bật bàn trước khi gọi món");
            }else
            {
                btnDaiDienThucPham.Tag = dgvThucPham.CurrentRow.Cells["ID_THUCPHAM"].Value.ToString();
                int id_hoadon = (btnHoaDon.Tag as HoaDon).ID_HoaDon;
                int soluong = 1;
                int id_thucpham = Int32.Parse(btnDaiDienThucPham.Tag as String);
                blHoaDon.ThemMatHang(id_hoadon, soluong, id_thucpham);
                blHoaDon.ShowBill((btnHoaDon.Tag as HoaDon), out tongtien);
                txtTienNuoc.Text = tongtien.ToString();
            }
            
        }

        /// <summary>
        /// Tìm kiếm thực phẩm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            string keyword = txtTimKiem.Text;
            DataTable dt = blThucPham.TimThucPham(keyword);
            RefeshThucPham(dt);
            btnDaiDienThucPham.Tag = null;
        }

        private void txtGiamGiaGio_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void simpleButton5_Click(object sender, EventArgs e)
        {
            
            double giamGiaGio = Double.Parse(lbGiamGiaGio.Tag.ToString());
            try
            {
                double temp = Convert.ToDouble(txtGiamGiaGio.Text);
                if (temp <0 || temp > 100)
                {
                    txtGiamGiaGio.Text = giamGiaGio.ToString();
                    throw new Exception();
                }
                else if (temp > giamGiaGio)
                {
                    giamGiaGio = temp;
                }
                txtGiamGiaGio.Text = giamGiaGio.ToString();

            }
            catch(Exception)
            {
                MessageBox.Show("Chỉ khuyến mãi trong khoảng 0 - 100");
            }
            finally
            {
                int idHoaDon = (btnHoaDon.Tag as HoaDon).ID_HoaDon;
                int kq = blHoaDon.SetGiamGiaGio(idHoaDon, giamGiaGio);
                if (kq > 0)
                {
                    txtGiamGiaGio.ForeColor = Color.Black;
                }
                
            }
            
        }

        public void simpleButton6_Click(object sender, EventArgs e)
        {
            
            double giamGiaThucPham = Double.Parse(lbGiamGiaNuoc.Tag.ToString());
            try
            {
                double temp = Convert.ToDouble(txtGiamGiaNuoc.Text);
                if (temp <0  || temp > 100)
                {
                    txtGiamGiaNuoc.Text = giamGiaThucPham.ToString();
                    throw new Exception();
                } else if (temp > giamGiaThucPham)
                {
                    giamGiaThucPham = temp;
                }
                txtGiamGiaNuoc.Text = giamGiaThucPham.ToString();
            }
            catch (Exception)
            {
                MessageBox.Show("Giảm giá nằm trong khoảng 0-100");
            }
            finally
            {
                int idHoaDon = (btnHoaDon.Tag as HoaDon).ID_HoaDon;
                int kq = blHoaDon.SetGiamGiaThucPham(idHoaDon, giamGiaThucPham);
                if (kq > 0)
                {
                    txtGiamGiaNuoc.ForeColor = Color.Black;
                }
                
            }
            
        }

        private void cbSoLuong_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                MessageBox.Show("Sai định dạng ", "Thông Báo ");
            }
        }

        private void txtGiamGiaGio_KeyPress(object sender, KeyPressEventArgs e)
        {
            txtGiamGiaGio.ForeColor = Color.Red;
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                MessageBox.Show("Sai định dạng ", "Thông Báo ");
            }
        }

        private void txtGiamGiaNuoc_KeyPress(object sender, KeyPressEventArgs e)
        {
            txtGiamGiaNuoc.ForeColor = Color.Red;
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                MessageBox.Show("Sai định dạng ", "Thông Báo ");
            }
        }

        private void FrmSuDungDichVu_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void cbNhanVien_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void cbKhachHang_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }
    }
}
