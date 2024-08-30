using QuanLyNhaHang.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVien.Controllers
{
    public class HoaDonController : Controller
    {
        DatabaseQuanLyNhaHang db = new DatabaseQuanLyNhaHang();
        [HttpGet]
        public ActionResult ThongTinHoaDon(int iMaBan)
        {
            ViewBag.LoaiMonAn = db.LoaiMonAn.ToList(); // Lấy danh mục món ăn
            ViewBag.DanhSachMonAn = db.MonAn.ToList(); // Lấy danh sách món ăn
            ViewBag.TatCa = db.MonAn.Count();
            ViewBag.Ban = db.Ban.ToList();
            /* có 2 trường hợp: 
            TH1: Bàn đang trống thì sẽ tạo đơn hàng mới
            TH2: Bàn đã có người thì vào xem hóa đơn
             */
            // xem bàn đó đã có khách hay chưa? 
            var ban = db.Ban.SingleOrDefault(n => n.MaBan == iMaBan && n.TinhTrang == 0); // 0: bàn trống
            if (ban != null) // nếu bàn trống
            {
                ban.TinhTrang = 1;
                db.SaveChanges();

                //mà bàn đang trống => hóa đơn cũng trống => tạo mới hóa đơn
                HoaDon hd = new HoaDon();
                hd.TenKhachHang = "Restaurant";
                hd.SDTKhachHang = "0123456789";
                hd.TongHoaDon = 1;
                hd.NgayTao = DateTime.Now;
                hd.TongTien = 0;
                hd.TrangThai = 1; // 1: chưa thanh toán
                hd.MaBan_id = iMaBan;
                db.HoaDon.Add(hd);
                db.SaveChanges();
                var hoaDonTrong = db.HoaDon.SingleOrDefault(n => n.MaBan_id == iMaBan && n.TrangThai == 1); //bàn đã có hóa đơn chưa thanh toán 


                #region Order khăn lạnh
                MonAn monAn = db.MonAn.SingleOrDefault(n => n.MaMonAn == 1);
                ChiTietHoaDon cthd = new ChiTietHoaDon();
                cthd.MaHoaDon_id = hoaDonTrong.MaHoaDon;
                cthd.MaMonAn_id = 1;
                cthd.SoLuongMua = ban.SoGhe;
                cthd.ThanhTien = (double)((cthd.SoLuongMua) * monAn.DonGia);
                db.ChiTietHoaDon.Add(cthd);
                // thêm số lượng đã bán trong bảng Món Ăn
                var monAn_SLDaBan = db.MonAn.SingleOrDefault(n => n.MaMonAn == monAn.MaMonAn);
                monAn_SLDaBan.SoLuongDaBan = monAn_SLDaBan.SoLuongDaBan + ban.SoGhe;
                //db.SaveChanges();
                //Ghi vào lịch sử gọi món
                LichSuGoiMon ls = new LichSuGoiMon();
                ls.SoLuongMua = ban.SoGhe;
                ls.SoLuongTra = 0;
                ls.ThoiGianGoi = DateTime.Now;
                ls.ThoiGianTra = null;
                ls.MaHoaDon_id = hoaDonTrong.MaHoaDon;
                ls.MaMonAn_id = monAn.MaMonAn;
                db.LichSuGoiMon.Add(ls);
                db.SaveChanges();
                #endregion


                var listMonAnKhachChon = db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == hoaDonTrong.MaHoaDon).ToList();
                ViewBag.MonAnKhachChon = listMonAnKhachChon;

                if (ban.Vip == 1)// Nếu bàn vip
                {
                    ViewBag.TongTienMonAn = TongTienOrder(hoaDonTrong.MaHoaDon, 10, 0, 0, 0);
                }
                else
                {
                    ViewBag.TongTienMonAn = TongTienOrder(hoaDonTrong.MaHoaDon, 0, 0, 0, 0);
                }
                ViewBag.SoLuongMonAn = SoLuongOrder(hoaDonTrong.MaHoaDon);

                return View(hoaDonTrong);
            }
            else // bàn đã có => có hóa đơn nhưng chưa gọi món
            {
                var hoaDon = db.HoaDon.SingleOrDefault(n => n.MaBan_id == iMaBan && n.TrangThai == 1); //bàn đã có hóa đơn chưa thanh toán 
                // Hiển thị danh mục order (tạo hóa đơn nhưng chưa order)
                if (hoaDon != null)
                {
                    var listMonAnKhachChon = db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == hoaDon.MaHoaDon).ToList();
                    var banVip = db.Ban.SingleOrDefault(n => n.MaBan == hoaDon.MaBan_id);
                    ViewBag.MonAnKhachChon = listMonAnKhachChon;
                    if (banVip.Vip == 1)// Nếu bàn vip
                    {
                        ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, 10, 0, 0, 0);
                    }
                    else
                    {
                        ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, 0, 0, 0, 0);
                    }
                    ViewBag.SoLuongMonAn = SoLuongOrder(hoaDon.MaHoaDon);
                }
                return View(hoaDon);
            }
        }
        [HttpGet]
        public ActionResult ThongTinHoaDonMonAn(int iMaBan, int iMaHoaDon, int iMaLMA)
        {
            ViewBag.LoaiMonAn = db.LoaiMonAn.ToList(); // Lấy danh mục món ăn
            ViewBag.DanhSachMonAn = db.MonAn.Where(n => n.MaLMA_id == iMaLMA).ToList();
            ViewBag.TatCa = db.MonAn.Count();
            ViewBag.Ban = db.Ban.ToList();
            #region Tìm ra hóa đơn
            var hoaDon = db.HoaDon.SingleOrDefault(n => n.MaBan_id == iMaBan & n.MaHoaDon == iMaHoaDon);
            var listMonAnKhachChon = db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == hoaDon.MaHoaDon).ToList();
            ViewBag.MonAnKhachChon = listMonAnKhachChon;
            var ban = db.Ban.SingleOrDefault(n => n.MaBan == hoaDon.MaBan_id);
            if (ban.Vip == 1)// Nếu bàn vip
            {
                ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, 10, 0, 0, 0);
            }
            else
            {
                ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, 0, 0, 0, 0);
            }
            ViewBag.SoLuongMonAn = SoLuongOrder(hoaDon.MaHoaDon);
            #endregion

            return View(hoaDon);
        }

        public ActionResult Order(int iMaHoaDon, int iMaMonAn, string strURL, FormCollection f) // Order = thêm món ăn vào hóa đơn
        {
            int soLuongOrder = int.Parse(f["txtSoLuongThem"].ToString());
            int kiemTra = 0;
            #region Kiểm tra trong kho có đáp ứng đủ k
            // lấy danh sách chi tiết sản phẩm
            var listCTSP = db.ChiTietSanPham.Where(n => n.MaMonAn_id == iMaMonAn && n.Tru == 1);
            if (listCTSP != null)
            {
                foreach (var item in listCTSP)
                {
                    // lấy số lượng trong kho
                    var nguyenLieu = db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
                    if (nguyenLieu.SoLuongHienCon >= (item.SoLuongDung * soLuongOrder)) // đáp ứng
                    {
                        // Thỏa mãn Kiemtra =0 sẽ chạy dòng 158
                    }
                    else // k đáp ứng
                    {
                        kiemTra = 1;
                    }
                }
            }
            #endregion

            if (kiemTra == 0)
            {
                #region Tính Toán
                // Lấy giá món ăn
                MonAn giaMonAn = db.MonAn.SingleOrDefault(n => n.MaMonAn == iMaMonAn);
                var monAn = db.ChiTietHoaDon.SingleOrDefault(n => n.MaHoaDon_id == iMaHoaDon && n.MaMonAn_id == iMaMonAn);
                if (monAn == null) // món ăn chưa tồn tại trong list khách gọi
                {
                    ChiTietHoaDon cthd = new ChiTietHoaDon();
                    cthd.MaHoaDon_id = iMaHoaDon;
                    cthd.MaMonAn_id = iMaMonAn;
                    cthd.SoLuongMua = soLuongOrder;
                    cthd.ThanhTien = (double)((cthd.SoLuongMua) * giaMonAn.DonGia);
                    cthd.NgayGoi = DateTime.Now;
                    db.ChiTietHoaDon.Add(cthd);
                    // thêm số lượng đã bán trong bảng Món Ăn
                    giaMonAn.SoLuongDaBan = giaMonAn.SoLuongDaBan + soLuongOrder;
                    db.SaveChanges();

                    //Ghi vào lịch sử gọi món
                    LichSuGoiMon ls = new LichSuGoiMon();
                    ls.SoLuongMua = soLuongOrder;
                    ls.SoLuongTra = 0;
                    ls.ThoiGianGoi = DateTime.Now;
                    ls.ThoiGianTra = null;
                    ls.MaHoaDon_id = iMaHoaDon;
                    ls.MaMonAn_id = iMaMonAn;
                    db.LichSuGoiMon.Add(ls);
                    db.SaveChanges();

                    #region Trừ nguyên liệu trong kho và lưu vào phiếu xuất kho
                    //B1: tìm món ăn
                    // B2: Lọc ra chi tiết sản phẩm có giá trị = 1
                    var listChitietSanPham = db.ChiTietSanPham.Where(n => n.MaMonAn_id == giaMonAn.MaMonAn && n.Tru == 1);
                    foreach (var item in listChitietSanPham)
                    {
                        // lấy ra nguyên liệu tương ứng
                        var nguyenLieu = db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
                        // kiểm tra số lượng dùng <= số lượng hiện còn trong kho k
                        //if (nguyenLieu.SoLuongHienCon >= (item.SoLuongDung * soLuongOrder)) // đáp ứng
                        //{
                        nguyenLieu.SoLuongHienCon = (nguyenLieu.SoLuongHienCon - (item.SoLuongDung * soLuongOrder));
                        //}
                        //else // k đáp ứng
                        //{
                        //    return RedirectToAction("KhoKhongDapUng", "ERROR");
                        //}
                    }
                    db.SaveChanges();

                    // tạo phiếu xuất
                    double? tongTien = 0;
                    XuatKho xk = new XuatKho();
                    xk.NgayXuat = DateTime.Now;
                    db.XuatKho.Add(xk);
                    db.SaveChanges();
                    // lưu nguyên liệu vào nguyên liệu xuất
                    var listNguyenLieu = db.ChiTietSanPham.Where(n => n.MaMonAn_id == giaMonAn.MaMonAn);
                    // lấy mã xuất kho
                    var xuatKho = db.XuatKho.OrderByDescending(n => n.MaXuatKho).FirstOrDefault();
                    foreach (var item in listNguyenLieu)
                    {
                        // lấy ra nguyên liệu tương ứng => tìm đc đơn giá
                        var nguyenLieu = db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
                        NguyenLieuXuat nlx = new NguyenLieuXuat();
                        nlx.MaXuatKho_id = xuatKho.MaXuatKho;
                        nlx.MaNguyenLieu_id = item.MaNguyenLieu_id;
                        nlx.SoLuongXuat = item.SoLuongDung;
                        db.NguyenLieuXuat.Add(nlx);
                    }
                    db.SaveChanges();
                    db.SaveChanges();
                    #endregion

                    return Redirect(strURL);
                }
                else // Món ăn đã tồn tại (ví dụ: Trước đó khách gọi món lẩu, giờ cũng gọi thêm 1 suất nữa)
                {
                    monAn.SoLuongMua = monAn.SoLuongMua + soLuongOrder;
                    monAn.ThanhTien = (double)((monAn.SoLuongMua) * giaMonAn.DonGia);
                    db.SaveChanges();

                    //Ghi vào lịch sử gọi món
                    LichSuGoiMon ls = new LichSuGoiMon();
                    ls.SoLuongMua = 1;
                    ls.SoLuongTra = 0;
                    ls.ThoiGianGoi = DateTime.Now;
                    ls.ThoiGianTra = null;
                    ls.MaHoaDon_id = iMaHoaDon;
                    ls.MaMonAn_id = iMaMonAn;
                    db.LichSuGoiMon.Add(ls);
                    // thêm số lượng đã bán trong bảng Món Ăn
                    giaMonAn.SoLuongDaBan = giaMonAn.SoLuongDaBan + soLuongOrder;
                    db.SaveChanges();

                    #region Trừ nguyên liệu trong kho và lưu vào phiếu xuất kho
                    //B1: tìm món ăn
                    // B2: Lọc ra chi tiết sản phẩm có giá trị = 1
                    var listChitietSanPham = db.ChiTietSanPham.Where(n => n.MaMonAn_id == giaMonAn.MaMonAn && n.Tru == 1);
                    foreach (var item in listChitietSanPham)
                    {
                        // lấy ra nguyên liệu tương ứng
                        var nguyenLieu = db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
                        // kiểm tra số lượng dùng <= số lượng hiện còn trong kho k
                        //if (nguyenLieu.SoLuongHienCon >= (item.SoLuongDung * soLuongOrder)) // đáp ứng
                        //{
                        nguyenLieu.SoLuongHienCon = (nguyenLieu.SoLuongHienCon - item.SoLuongDung * soLuongOrder);
                        //}
                        //else // k đáp ứng
                        //{
                        //    return RedirectToAction("KhoKhongDapUng", "ERROR");
                        //}
                    }
                    db.SaveChanges();

                    // tạo phiếu xuất
                    double? tongTien = 0;
                    XuatKho xk = new XuatKho();
                    xk.NgayXuat = DateTime.Now;
                    db.XuatKho.Add(xk);
                    db.SaveChanges();
                    // lưu nguyên liệu vào nguyên liệu xuất
                    var listNguyenLieu = db.ChiTietSanPham.Where(n => n.MaMonAn_id == giaMonAn.MaMonAn);
                    // lấy mã xuất kho
                    var xuatKho = db.XuatKho.OrderByDescending(n => n.MaXuatKho).FirstOrDefault();
                    foreach (var item in listNguyenLieu)
                    {
                        // lấy ra nguyên liệu tương ứng => tìm đc đơn giá
                        var nguyenLieu = db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
                        NguyenLieuXuat nlx = new NguyenLieuXuat();
                        nlx.MaXuatKho_id = xuatKho.MaXuatKho;
                        nlx.MaNguyenLieu_id = item.MaNguyenLieu_id;
                        nlx.SoLuongXuat = item.SoLuongDung;
                        db.NguyenLieuXuat.Add(nlx);
                    }
                    db.SaveChanges();
                    db.SaveChanges();
                    #endregion

                    return Redirect(strURL);
                }
                #endregion
            }
            else
            {
                return RedirectToAction("KhoKhongDapUng", "Error");
            }
        }
        public ActionResult CapNhatSoLuong(int iMaHoaDon, int iMaMonAn, string strURL, FormCollection f)
        {
            int soLuong = int.Parse(f["txtSoLuongMua"].ToString());
            var monAn = db.ChiTietHoaDon.SingleOrDefault(n => n.MaHoaDon_id == iMaHoaDon && n.MaMonAn_id == iMaMonAn);
            // Lấy giá món ăn
            MonAn giaMonAn = db.MonAn.SingleOrDefault(n => n.MaMonAn == iMaMonAn);
            if (soLuong > 0)
            {
                //Ghi vào lịch sử gọi món. Có 2 trường hợp. cập nhật tăng => thêm món. Cập nhật giảm => hủy món
                if (soLuong > monAn.SoLuongMua) // cập nhật thêm số lượng món ăn
                {
                    int kiemTra = 0;
                    // lấy danh sách chi tiết sản phẩm
                    var listCTSP = db.ChiTietSanPham.Where(n => n.MaMonAn_id == iMaMonAn && n.Tru == 1);
                    if (listCTSP != null)
                    {
                        foreach (var item in listCTSP)
                        {
                            // lấy số lượng trong kho
                            var nguyenLieu = db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
                            if (nguyenLieu.SoLuongHienCon >= (item.SoLuongDung * soLuong)) // đáp ứng
                            {
                                //
                            }
                            else // k đáp ứng
                            {
                                kiemTra = 1;
                            }
                        }
                    }
                    if (kiemTra == 0) // thỏa mãn
                    {
                        LichSuGoiMon ls = new LichSuGoiMon();
                        ls.SoLuongMua = soLuong - monAn.SoLuongMua;
                        //ls.SoLuongTra = 0;
                        ls.ThoiGianGoi = DateTime.Now;
                        //ls.ThoiGianTra = null;
                        ls.MaHoaDon_id = iMaHoaDon;
                        ls.MaMonAn_id = iMaMonAn;
                        db.LichSuGoiMon.Add(ls);
                        db.SaveChanges();

                        #region Trừ nguyên liệu trong kho và lưu vào phiếu xuất kho
                        //B1: tìm món ăn
                        // B2: Lọc ra chi tiết sản phẩm có giá trị = 1
                        var listChitietSanPham = db.ChiTietSanPham.Where(n => n.MaMonAn_id == giaMonAn.MaMonAn && n.Tru == 1);
                        foreach (var item in listChitietSanPham)
                        {
                            // lấy ra nguyên liệu tương ứng
                            var nguyenLieu = db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
                            // kiểm tra số lượng dùng <= số lượng hiện còn trong kho k
                            if (nguyenLieu.SoLuongHienCon >= (item.SoLuongDung * soLuong)) // đáp ứng
                            {
                                nguyenLieu.SoLuongHienCon = (nguyenLieu.SoLuongHienCon - (item.SoLuongDung * soLuong));
                            }
                            else // k đáp ứng
                            {
                                return RedirectToAction("KhoKhongDapUng", "ERROR");
                            }
                        }
                        db.SaveChanges();

                        // tạo phiếu xuất
                        XuatKho xk = new XuatKho();
                        xk.NgayXuat = DateTime.Now;
                        db.XuatKho.Add(xk);
                        db.SaveChanges();
                        // lưu nguyên liệu vào nguyên liệu xuất
                        var listNguyenLieu = db.ChiTietSanPham.Where(n => n.MaMonAn_id == giaMonAn.MaMonAn);
                        // lấy mã xuất kho
                        var xuatKho = db.XuatKho.OrderByDescending(n => n.MaXuatKho).FirstOrDefault();
                        foreach (var item in listNguyenLieu)
                        {
                            // lấy ra nguyên liệu tương ứng => tìm đc đơn giá
                            var nguyenLieu = db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
                            NguyenLieuXuat nlx = new NguyenLieuXuat();
                            nlx.MaXuatKho_id = xuatKho.MaXuatKho;
                            nlx.MaNguyenLieu_id = item.MaNguyenLieu_id;
                            nlx.SoLuongXuat = item.SoLuongDung;
                            db.NguyenLieuXuat.Add(nlx);
                        }
                        db.SaveChanges();
                        db.SaveChanges();
                        #endregion
                    }
                    else
                    {
                        //ViewBag.ThongBao = "Kho không đáp ứng nguyên liệu cho món ăn: " + giaMonAn.TenMonAn + " Số lượng = " + soLuong;
                        //return View();
                        return RedirectToAction("KhoKhongDapUng", "Error");
                    }
                }
                if (soLuong < monAn.SoLuongMua) // trả món
                {
                    LichSuGoiMon ls = new LichSuGoiMon();
                    //ls.SoLuongMua = 0;
                    ls.SoLuongTra = monAn.SoLuongMua - soLuong;
                    //ls.ThoiGianGoi = null;
                    ls.ThoiGianTra = DateTime.Now;
                    ls.MaHoaDon_id = iMaHoaDon;
                    ls.MaMonAn_id = iMaMonAn;
                    db.LichSuGoiMon.Add(ls);
                    db.SaveChanges();
                }
                monAn.SoLuongMua = soLuong;
                monAn.ThanhTien = (double)(monAn.SoLuongMua * monAn.MonAn.DonGia);

            }
            else // nếu số lượng bằng 0 thì xóa đi
            {
                //Ghi vào lịch sử gọi món xóa
                LichSuGoiMon ls = new LichSuGoiMon();
                //ls.SoLuongMua = 0;
                ls.SoLuongTra = monAn.SoLuongMua;
                //ls.ThoiGianGoi = DateTime.Now;
                ls.ThoiGianTra = DateTime.Now;
                ls.MaHoaDon_id = iMaHoaDon;
                ls.MaMonAn_id = iMaMonAn;
                db.LichSuGoiMon.Add(ls);
                db.SaveChanges();

                db.ChiTietHoaDon.Remove(monAn);
            }
            // lấy hóa đơn
            var hoaDon = db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
            var ban = db.Ban.SingleOrDefault(n => n.MaBan == hoaDon.MaBan_id);
            if (ban.Vip == 1)// Nếu bàn vip
            {
                ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, 10, 0, 0, 0);
            }
            else
            {
                ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, 0, 0, 0, 0);
            }

            db.SaveChanges();

            return Redirect(strURL);
        }
        public ActionResult XoaMonAn(int iMaHoaDon, int iMaMonAn, string strURL)
        {
            var monAn = db.ChiTietHoaDon.SingleOrDefault(n => n.MaHoaDon_id == iMaHoaDon && n.MaMonAn_id == iMaMonAn);
            //Ghi vào lịch sử gọi món xóa
            LichSuGoiMon ls = new LichSuGoiMon();
            //ls.SoLuongMua = 0;
            ls.SoLuongTra = monAn.SoLuongMua;
            //ls.ThoiGianGoi = DateTime.Now;
            ls.ThoiGianTra = DateTime.Now;
            ls.MaHoaDon_id = iMaHoaDon;
            ls.MaMonAn_id = iMaMonAn;
            db.LichSuGoiMon.Add(ls);
            db.ChiTietHoaDon.Remove(monAn);
            db.SaveChanges();
            return Redirect(strURL);
        }
        public ActionResult ThanhToan(int iMaHoaDon, FormCollection f)
        {
            float giamGiaKhachHang = 0;
            float giamGiaVND = float.Parse(f["txtGiamGiaVND"].ToString()); // Giảm giá vnđ
            float giamGiaPhanTram = float.Parse(f["txtGiamGiaPhanTram"].ToString()); // Giảm giá theo %
            try
            {
                giamGiaKhachHang = float.Parse(f["txtGiamGiaKhachHang"].ToString()); // Giảm giá khách hàng tiềm năng
            }
            catch
            {
            }

            HoaDon hoaDon = db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
            Ban ban = db.Ban.SingleOrDefault(n => n.MaBan == hoaDon.MaBan_id);
            if (hoaDon != null)
            {

                hoaDon.TrangThai = 0; // trạng thái trong đơn hàng = 0 là đã thanh toán
                if (ban.Vip == 1)// Nếu bàn vip
                {
                    ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, 10, giamGiaVND, giamGiaPhanTram, giamGiaKhachHang);
                }
                else
                {
                    ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, 0, giamGiaVND, giamGiaPhanTram, giamGiaKhachHang);
                }
                hoaDon.TongTien = ViewBag.TongTienMonAn;
                hoaDon.NgayThanhToan = DateTime.Now;
                db.SaveChanges();
            }
            if (ban != null)
            {
                ban.TinhTrang = 0;
                db.SaveChanges();
            }
            //return RedirectToAction("Index", "Home");
            return RedirectToAction("ThanhCong", "Error");

        }
        public ActionResult HuyHoaDon(int iMaHoaDon)
        {
            HoaDon hoaDon = db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
            Ban ban = db.Ban.SingleOrDefault(n => n.MaBan == hoaDon.MaBan_id);
            if (ban != null)
            {
                ban.TinhTrang = 0;
                db.SaveChanges();
            }
            // muốn xóa hóa đơn phải xóa trong lịch sử gọi món
            var history = db.LichSuGoiMon.Where(n => n.MaHoaDon_id == iMaHoaDon).ToList();
            foreach (var item in history)
            {
                db.LichSuGoiMon.Remove(item);
            }
            db.HoaDon.Remove(hoaDon);
            db.SaveChanges();

            return RedirectToAction("Index", "Home");
        }
        private int SoLuongOrder(int iMaHoaDon)
        {

            int COUNT = 0;
            var slMonAn = db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == iMaHoaDon);
            if (slMonAn.Count() != 0)
            {
                COUNT = slMonAn.Sum(n => n.SoLuongMua);
            }
            return COUNT;
        }

        // Tổng tiền gồm có: phí phòng vip, giảm giá vnd, giảm giá phần trăm, 
        private double TongTienOrder(int iMaHoaDon, float fPhibanVip, float fMaGiamGiaVND, float fGiamGiaPhanTram, float fGiamGiaKhachHang)
        {
            double TOTAL = 0; // Tổng tiền bằng 0 
            var ttMonAn = db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == iMaHoaDon).ToList(); // Tìm được món ăn thuộc hóa đơn
            var hoaDon = db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon); // Lấy ra được hóa đơn của mình
            var ban = db.Ban.SingleOrDefault(n => n.MaBan == hoaDon.MaBan_id); // Lấy bàn 

            if (ban.Vip == 1) // 1 là bàn vip
            {
                if (ttMonAn.Count() != 0) // đếm bao nhiêu món 
                {
                    TOTAL = ttMonAn.Sum(n => n.ThanhTien) + (ttMonAn.Sum(n => n.ThanhTien) * 10 / 100) - fMaGiamGiaVND - fGiamGiaKhachHang - (ttMonAn.Sum(n => n.ThanhTien) * fGiamGiaPhanTram / 100);

                    if (TOTAL < 0) // Giảm giá quá mức thì lỗ 
                    {
                        TOTAL = 0;
                    }
                }
            }
            else // Không phải bàn VIP
            {
                if (ttMonAn.Count() != 0)
                {
                    TOTAL = ttMonAn.Sum(n => n.ThanhTien) - fMaGiamGiaVND - fGiamGiaKhachHang - (ttMonAn.Sum(n => n.ThanhTien) * fGiamGiaPhanTram / 100);
                }
            }
            return TOTAL;
        }
        #region Sửa Xóa Hóa Đơn
        // danh sách hóa đơn
        public ActionResult DanhSachHoaDon()
        {
            var listHoaDon = db.HoaDon.OrderByDescending(n => n.MaHoaDon).ToList();
            return View(listHoaDon);
        }
        // xem chi tiết
        public ActionResult ChiTietHoaDon(int iMaHoaDon)
        {
            var chiTiet = db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
            // Lấy danh sách món ăn đã gọi trong bản chi tiết
            var listOrder = db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == iMaHoaDon).ToList();
            ViewBag.ListOrder = listOrder;
            // Lấy thời gian gọi món sắp xếp giảm gần theo thời gian đặt / mã hóa đơn
            var listHistory = db.LichSuGoiMon.Where(n => n.MaHoaDon_id == iMaHoaDon).ToList().OrderByDescending(n => n.MaLichSu);
            ViewBag.ListHistory = listHistory;
            return View(chiTiet);
        }
        public ActionResult XoaHoaDons(int iMaHoaDon)
        {
            var chiTiet = db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
            if (chiTiet == null)
            {
                return RedirectToAction("XoaHoaDon", "Error");
            }
            HoaDon hoaDon = db.HoaDon.Find(iMaHoaDon);
            if (hoaDon == null)
            {
                return HttpNotFound();
            }
            return View(hoaDon);
        }
        // xóa hóa đơn
        public ActionResult XoaHoaDon(int iMaHoaDon)
        {
            var hoaDon = db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
            // Kiểm tra hóa đơn đã thanh toán mới được phép xóa
            if (hoaDon.TrangThai == 0)
            {
                // xóa danh sách món ăn trong hóa đơn trước
                var monAn = db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == iMaHoaDon).ToList();
                foreach (var item in monAn)
                {
                    db.ChiTietHoaDon.Remove(item);
                    db.SaveChanges();
                }
                // xóa trong danh sách lịch sử gọi món
                var lichSu = db.LichSuGoiMon.Where(n => n.MaHoaDon_id == iMaHoaDon).ToList();
                foreach (var item in lichSu)
                {
                    db.LichSuGoiMon.Remove(item);
                    db.SaveChanges();
                }
                db.HoaDon.Remove(hoaDon);
                db.SaveChanges();
                return RedirectToAction("DanhSachHoaDon", "HoaDon");
            }
            else
            {
                return RedirectToAction("XoaHoaDon", "Error");
            }
        }
        #endregion
        // Chuyển bàn
        public ActionResult ChuyenBan(int iMaHoaDon, FormCollection f)
        {
            int banMoi = int.Parse(f["Ban"].ToString());

            // cho trạng thái bài thuộc hóa đơn về bằng 0
            var hoaDonCu = db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
            var ban = db.Ban.SingleOrDefault(n => n.MaBan == hoaDonCu.MaBan_id);
            ban.TinhTrang = 0;
            //db.SaveChanges();

            //Kiểm tra bàn mới này đã có khách nào đang ngồi hay k
            var checkBan = db.Ban.SingleOrDefault(n => n.MaBan == banMoi);
            if (checkBan.TinhTrang == 0) // chưa có khách => cập nhật lại bàn mới
            {
                hoaDonCu.MaBan_id = banMoi;
                checkBan.TinhTrang = 1;
                db.SaveChanges();

                // 
                #region Tính lại tổng tiền cho hóa đơn cũ
                //if (checkBan.Vip == 1)// Nếu bàn vip
                //{
                //    ViewBag.TongTienMonAn = TongTienOrder(hoaDonBanMoi.MaHoaDon, 10, 0);
                //}
                //else
                //{
                //    ViewBag.TongTienMonAn = TongTienOrder(hoaDonBanMoi.MaHoaDon, 0, 0);
                //}
                #endregion
            }
            else // đã có khách => đã có hóa đơn
            {
                // lấy ra hóa đơn thuộc bàn cao nhất
                var hoaDonBanMoi = db.HoaDon.Where(n => n.MaBan_id == banMoi).OrderByDescending(n => n.MaHoaDon).FirstOrDefault();
                var chiTietMonAnMoi = db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == hoaDonBanMoi.MaHoaDon).ToList();
                // lấy ra danh sách món add vào sanh sách món mới()
                var chiTietMonAnCu = db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == hoaDonCu.MaHoaDon);
                if (chiTietMonAnCu != null) // đã gọi món => có lịch sử
                {
                    foreach (var item in chiTietMonAnCu)
                    {
                        foreach (var items in chiTietMonAnMoi)
                        {
                            if (item.MaMonAn_id != items.MaMonAn_id) // hóa đơn k trùng món 
                            {
                                ChiTietHoaDon cthd = new ChiTietHoaDon();
                                cthd.MaHoaDon_id = hoaDonBanMoi.MaHoaDon;
                                cthd.MaMonAn_id = item.MaMonAn_id;
                                cthd.SoLuongMua = item.SoLuongMua;
                                cthd.ThanhTien = item.ThanhTien;
                                db.ChiTietHoaDon.Add(cthd);
                                //db.SaveChanges();
                            }
                            else // trùng món => + số lượng
                            {
                                var monAnMoi = db.ChiTietHoaDon.SingleOrDefault(n => n.MaMonAn_id == items.MaMonAn_id & n.MaHoaDon_id == hoaDonBanMoi.MaHoaDon);
                                monAnMoi.SoLuongMua += item.SoLuongMua;
                            }
                        }
                    }

                    //đưa lịch sử hóa đơn cũ vào lịch sử hóa đơn mới. sau khi thực hiện xóa lịch sử củ đi
                    //lấy lịch sử món ăn bàn cũ và mới
                    var lichSuBanCu = db.LichSuGoiMon.Where(n => n.MaHoaDon_id == hoaDonCu.MaHoaDon).ToList();
                    var lichSuBanMoi = db.LichSuGoiMon.Where(n => n.MaHoaDon_id == hoaDonBanMoi.MaHoaDon).ToList();
                    foreach (var item in lichSuBanCu)
                    {
                        LichSuGoiMon history = new LichSuGoiMon();
                        history.SoLuongMua = item.SoLuongMua;
                        history.SoLuongTra = item.SoLuongTra;
                        history.ThoiGianGoi = item.ThoiGianGoi;
                        history.ThoiGianTra = item.ThoiGianTra;
                        history.MaHoaDon_id = hoaDonBanMoi.MaHoaDon;
                        history.MaMonAn_id = item.MaMonAn_id;
                        db.LichSuGoiMon.Add(history);
                        db.LichSuGoiMon.Remove(item);
                    }
                    db.HoaDon.Remove(hoaDonCu);
                    db.SaveChanges();
                    #region Tính lại tổng tiền
                    if (checkBan.Vip == 1)// Nếu bàn vip
                    {
                        ViewBag.TongTienMonAn = TongTienOrder(hoaDonBanMoi.MaHoaDon, 10, 0, 0, 0);
                    }
                    else
                    {
                        ViewBag.TongTienMonAn = TongTienOrder(hoaDonBanMoi.MaHoaDon, 0, 0, 0, 0);
                    }
                    #endregion
                }
                return RedirectToAction("Index","Home");
            }
            return RedirectToAction("Index", "Home");
        }
        //Cập nhật lại tên khách hàng
        public ActionResult CapNhatTenKhachHang(int iMaHoaDon, string strURL, FormCollection f)
        {
            string tenkhachhang = f["txtTenKhachhang"].ToString();
            string dienThoai = f["txtSoDienThoai"].ToString();

            var hoaDon = db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
            hoaDon.TenKhachHang = tenkhachhang;
            hoaDon.SDTKhachHang = dienThoai;

            #region Kiểm tra số điện thoại khách hàng đã tồn tại chưa
            var demSoHoaDon = db.HoaDon.Where(n => n.SDTKhachHang == dienThoai).OrderByDescending(n => n.MaHoaDon).FirstOrDefault();
            if (demSoHoaDon != null)
            {
                hoaDon.TongHoaDon = demSoHoaDon.TongHoaDon + 1;
                hoaDon.TenKhachHang = demSoHoaDon.TenKhachHang;
            }
            else
            {
                hoaDon.TongHoaDon = 1;
            }
            #endregion
            db.SaveChanges();
            return Redirect(strURL);
        }
        public ActionResult KhachHang()
        {
            var listKhachHang = db.HoaDon.OrderByDescending(n => n.NgayTao).ToList();
            return View(listKhachHang);
        }
    }
}
