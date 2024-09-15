using QuanLyNhaHang.Common.Const;
using QuanLyNhaHang.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVien.Controllers
{
    public class HoaDonController : Controller
    {
        private readonly DatabaseQuanLyNhaHang _db;

        public HoaDonController(DatabaseQuanLyNhaHang db)
        {
            _db = db;
        }

        private string GetMaDoanhNghiepFromCookie()
        {
            var cookie = Request.Cookies["UserLogin"];
            return cookie?["MaDoanhNghiep"];
        }


        [HttpGet]
        public ActionResult ThongTinHoaDon(int iMaBan)
        {
            try
            {
                string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

                // Kiểm tra bàn có hợp lệ không
                var checkBan = _db.Ban.SingleOrDefault(n => n.MaBan == iMaBan && n.MaDoanhNghiep_id == sMaDoanhNghiep);
                if (checkBan == null)
                {
                    TempData["ToastMessage"] = "error|Bàn không hợp lệ";
                    return RedirectToAction("Index", "Home");
                }

                var loaiMonAn = _db.LoaiMonAn.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).ToList();
                var monAn = _db.MonAn.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).ToList();
                var banTask = _db.Ban.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).ToList();

                ViewBag.LoaiMonAn = loaiMonAn;
                ViewBag.DanhSachMonAn = monAn;
                ViewBag.Ban = banTask;

                if (iMaBan == Const.MangDi) // Khách mang đi
                {
                    ViewBag.MangDi = Const.MangDi;

                    // Kiểm tra hóa đơn mang đi có trạng thái là chưa thanh toán
                    HoaDon hoaDon = _db.HoaDon.FirstOrDefault(n => n.MaBan_id == Const.MangDi && n.TrangThai == Const.ChuaThanhToan);

                    if (hoaDon == null)
                    {
                        var HoaDon = new HoaDon
                        {
                            TenKhachHang = "",
                            SDTKhachHang = "",
                            DiaChiKhachHang = "",
                            NgayTao = DateTime.Now,
                            TongTien = 0,
                            TrangThai = Const.ChuaThanhToan,
                            MaBan_id = Const.MangDi,
                            MaDoanhNghiep_id = sMaDoanhNghiep
                        };

                        _db.HoaDon.Add(HoaDon);
                        _db.SaveChanges();

                        ViewBag.MonAnKhachChon = 0;
                        ViewBag.TongTienMonAn = 0;
                        ViewBag.SoLuongMonAn = 0;

                        return View(HoaDon);
                    }

                    // Tính toán tổng tiền và số lượng món ăn
                    ViewBag.MonAnKhachChon = _db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == hoaDon.MaHoaDon).ToList();
                    ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, 0, 0, 0);
                    ViewBag.SoLuongMonAn = SoLuongOrder(hoaDon.MaHoaDon);

                    return View(hoaDon);
                }
                else // Khách ngồi tại bàn
                {
                    ViewBag.MangDi = Const.TaiBan;

                    // Kiểm tra tình trạng của bàn
                    if (checkBan.TinhTrang == Const.KhongCoNguoi)
                    {
                        checkBan.TinhTrang = Const.CoNguoi;
                        _db.SaveChanges();

                        // Tạo hóa đơn mới
                        var hoaDonNew = new HoaDon
                        {
                            TenKhachHang = "Ẩn Danh",
                            SDTKhachHang = "0123456789",
                            DiaChiKhachHang = "",
                            NgayTao = DateTime.Now,
                            TongTien = 0,
                            TrangThai = Const.ChuaThanhToan,
                            MaBan_id = iMaBan,
                            MaDoanhNghiep_id = sMaDoanhNghiep
                        };

                        _db.HoaDon.Add(hoaDonNew);
                        _db.SaveChanges();

                        ViewBag.MonAnKhachChon = 0;
                        ViewBag.TongTienMonAn = 0;
                        ViewBag.SoLuongMonAn = 0;

                        return View(hoaDonNew);
                    }
                    else
                    {
                        // Lấy hóa đơn của bàn đã có người
                        var hoaDon = _db.HoaDon.SingleOrDefault(n => n.MaBan_id == iMaBan &&
                                                                n.TrangThai == Const.ChuaThanhToan &&
                                                                n.MaDoanhNghiep_id == sMaDoanhNghiep);
                        if (hoaDon == null)
                        {
                            checkBan.TinhTrang = Const.KhongCoNguoi;
                            _db.SaveChanges();

                            TempData["ToastMessage"] = "error|Hóa đơn không tồn tại";
                            return RedirectToAction("DanhSachBan", "Ban");
                        }

                        // Tính toán tổng tiền và số lượng món ăn
                        ViewBag.MonAnKhachChon = _db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == hoaDon.MaHoaDon).ToList();
                        ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, 0, 0, 0);
                        ViewBag.SoLuongMonAn = SoLuongOrder(hoaDon.MaHoaDon);

                        return View(hoaDon);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Tạo hóa đơn thất bại.";
                return RedirectToAction("DanhSachBan", "Ban");
            }
        }

        [HttpGet]
        public ActionResult ThongTinHoaDonMonAn(int iMaBan, int iMaHoaDon, int iMaLMA)
        {
            ViewBag.LoaiMonAn = _db.LoaiMonAn.ToList(); // Lấy danh mục món ăn
            ViewBag.DanhSachMonAn = _db.MonAn.Where(n => n.MaLMA_id == iMaLMA).ToList();
            ViewBag.MonAn = _db.MonAn.Count();
            ViewBag.Ban = _db.Ban.ToList();
            #region Tìm ra hóa đơn
            var hoaDon = _db.HoaDon.SingleOrDefault(n => n.MaBan_id == iMaBan & n.MaHoaDon == iMaHoaDon);
            var listMonAnKhachChon = _db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == hoaDon.MaHoaDon).ToList();
            ViewBag.MonAnKhachChon = listMonAnKhachChon;
            var ban = _db.Ban.SingleOrDefault(n => n.MaBan == hoaDon.MaBan_id);
            if (ban.Vip == 1)// Nếu bàn vip
            {
                //ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, 10, 0, 0, 0);
            }
            else
            {
                //ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, 0, 0, 0, 0);
            }
            //ViewBag.SoLuongMonAn = SoLuongOrder(hoaDon.MaHoaDon);
            #endregion

            return View(hoaDon);
        }

        public ActionResult Order(int iMaHoaDon, int iMaMonAn, string strURL, FormCollection f) // Order = thêm món ăn vào hóa đơn
        {
            try
            {
                string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

                int soLuongOrder = int.Parse(f["txtSoLuongThem"].ToString());

                #region Tính Toán
                // Lấy giá món ăn
                MonAn giaMonAn = _db.MonAn.SingleOrDefault(n => n.MaMonAn == iMaMonAn);
                var monAn = _db.ChiTietHoaDon.SingleOrDefault(n => n.MaHoaDon_id == iMaHoaDon && n.MaMonAn_id == iMaMonAn);
                if (monAn == null) // món ăn chưa tồn tại trong list khách gọi
                {
                    ChiTietHoaDon cthd = new ChiTietHoaDon();
                    cthd.MaHoaDon_id = iMaHoaDon;
                    cthd.MaMonAn_id = iMaMonAn;
                    cthd.SoLuongMua = soLuongOrder;
                    cthd.ThanhTien = (double)((cthd.SoLuongMua) * giaMonAn.DonGia);
                    cthd.NgayGoi = DateTime.Now;

                    _db.ChiTietHoaDon.Add(cthd);
                    // thêm số lượng đã bán trong bảng Món Ăn
                    giaMonAn.SoLuongDaBan = giaMonAn.SoLuongDaBan + soLuongOrder;
                    _db.SaveChanges();

                    //Ghi vào lịch sử gọi món
                    LichSuGoiMon ls = new LichSuGoiMon();
                    ls.SoLuongMua = soLuongOrder;
                    ls.SoLuongTra = 0;
                    ls.ThoiGianGoi = DateTime.Now;
                    ls.ThoiGianTra = null;
                    ls.MaHoaDon_id = iMaHoaDon;
                    ls.MaMonAn_id = iMaMonAn;
                    _db.LichSuGoiMon.Add(ls);
                    _db.SaveChanges();
                }
                else // Món ăn đã tồn tại (ví dụ: Trước đó khách gọi món lẩu, giờ cũng gọi thêm 1 suất nữa)
                {
                    monAn.SoLuongMua = monAn.SoLuongMua + soLuongOrder;
                    monAn.ThanhTien = (double)((monAn.SoLuongMua) * giaMonAn.DonGia);
                    _db.SaveChanges();

                    //Ghi vào lịch sử gọi món
                    LichSuGoiMon ls = new LichSuGoiMon();
                    ls.SoLuongMua = 1;
                    ls.SoLuongTra = 0;
                    ls.ThoiGianGoi = DateTime.Now;
                    ls.ThoiGianTra = null;
                    ls.MaHoaDon_id = iMaHoaDon;
                    ls.MaMonAn_id = iMaMonAn;
                    _db.LichSuGoiMon.Add(ls);
                    // thêm số lượng đã bán trong bảng Món Ăn
                    giaMonAn.SoLuongDaBan = giaMonAn.SoLuongDaBan + soLuongOrder;
                    _db.SaveChanges();
                }

                TempData["ToastMessage"] = "success|Thêm món ăn thành công.";
                return Redirect(strURL);
                #endregion
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Thêm món ăn thất bại.";
                return Redirect(strURL);
            }
        }

        public ActionResult CapNhatSoLuong(int iMaHoaDon, int iMaMonAn, string strURL, FormCollection f)
        {
            try
            {
                int soLuong = int.Parse(f["txtSoLuongMua"].ToString());

                var monAn = _db.ChiTietHoaDon.SingleOrDefault(n => n.MaHoaDon_id == iMaHoaDon && n.MaMonAn_id == iMaMonAn);
                // Lấy giá món ăn
                MonAn giaMonAn = _db.MonAn.Find(iMaMonAn);
                if (soLuong > 0)
                {
                    //Ghi vào lịch sử gọi món. Có 2 trường hợp. cập nhật tăng => thêm món. Cập nhật giảm => hủy món
                    if (soLuong > monAn.SoLuongMua) // cập nhật thêm số lượng món ăn
                    {
                        LichSuGoiMon ls = new LichSuGoiMon();
                        ls.SoLuongMua = soLuong - monAn.SoLuongMua;
                        //ls.SoLuongTra = 0;
                        ls.ThoiGianGoi = DateTime.Now;
                        //ls.ThoiGianTra = null;
                        ls.MaHoaDon_id = iMaHoaDon;
                        ls.MaMonAn_id = iMaMonAn;
                        // thêm số lượng đã bán trong bảng Món Ăn
                        giaMonAn.SoLuongDaBan = giaMonAn.SoLuongDaBan + (soLuong - monAn.SoLuongMua);
                        _db.LichSuGoiMon.Add(ls);
                        _db.SaveChanges();
                    }
                    else if (soLuong < monAn.SoLuongMua)
                    {
                        LichSuGoiMon ls = new LichSuGoiMon();
                        //ls.SoLuongMua = 0;
                        ls.SoLuongTra = monAn.SoLuongMua - soLuong;
                        //ls.ThoiGianGoi = null;
                        ls.ThoiGianTra = DateTime.Now;
                        ls.MaHoaDon_id = iMaHoaDon;
                        ls.MaMonAn_id = iMaMonAn;
                        _db.LichSuGoiMon.Add(ls);
                        giaMonAn.SoLuongDaBan = giaMonAn.SoLuongDaBan - (monAn.SoLuongMua - soLuong);
                        _db.SaveChanges();
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
                    _db.LichSuGoiMon.Add(ls);
                    // thêm số lượng đã bán trong bảng Món Ăn
                    giaMonAn.SoLuongDaBan = giaMonAn.SoLuongDaBan - soLuong;
                    _db.SaveChanges();

                    _db.ChiTietHoaDon.Remove(monAn);
                }
                // lấy hóa đơn
                var hoaDon = _db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
                ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, 0, 0, 0);

                _db.SaveChanges();

                TempData["ToastMessage"] = "success|Cập nhật số lượng món ăn thành công.";
                return Redirect(strURL);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Cập nhật số lượng món ăn thất bại.";
                return Redirect(strURL);
            }
        }

        public ActionResult XoaMonAn(int iMaHoaDon, int iMaMonAn, string strURL)
        {
            try
            {
                var monAn = _db.ChiTietHoaDon.SingleOrDefault(n => n.MaHoaDon_id == iMaHoaDon && n.MaMonAn_id == iMaMonAn);

                //Ghi vào lịch sử gọi món xóa
                LichSuGoiMon ls = new LichSuGoiMon();
                //ls.SoLuongMua = 0;
                ls.SoLuongTra = monAn.SoLuongMua;
                //ls.ThoiGianGoi = DateTime.Now;
                ls.ThoiGianTra = DateTime.Now;
                ls.MaHoaDon_id = iMaHoaDon;
                ls.MaMonAn_id = iMaMonAn;
                _db.LichSuGoiMon.Add(ls);
                _db.ChiTietHoaDon.Remove(monAn);
                _db.SaveChanges();

                TempData["ToastMessage"] = "success|Xóa món ăn thành công.";
                return Redirect(strURL);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Xóa món ăn thất bại.";
                return Redirect(strURL);
            }
        }

        public ActionResult ThanhToan(int iMaHoaDon, FormCollection f)
        {
            try
            {
                float giamGiaVND = string.IsNullOrEmpty(f["txtGiamGiaVND"]) ? 0 : float.Parse(f["txtGiamGiaVND"]);
                float giamGiaPhanTram = string.IsNullOrEmpty(f["txtGiamGiaPhanTram"]) ? 0 : float.Parse(f["txtGiamGiaPhanTram"]);
                float phiShip = string.IsNullOrEmpty(f["txtPhiShip"]) ? 0 : float.Parse(f["txtPhiShip"]);

                HoaDon hoaDon = _db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
                Ban ban = _db.Ban.SingleOrDefault(n => n.MaBan == hoaDon.MaBan_id);
                if (hoaDon != null)
                {
                    hoaDon.TrangThai = Const.DaThanhToan; // trạng thái trong đơn hàng = 0 là đã thanh toán

                    ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, phiShip, giamGiaVND, giamGiaPhanTram);
                    hoaDon.TongTien = ViewBag.TongTienMonAn;
                    hoaDon.NgayThanhToan = DateTime.Now;
                    _db.SaveChanges();
                }
                if (ban != null)
                {
                    ban.TinhTrang = 0;
                    _db.SaveChanges();
                }
                TempData["ToastMessage"] = "success|Thanh toán thành công.";
                return RedirectToAction("DanhSachBan", "Ban");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Thanh toán hóa đơn thất bại.";
                return RedirectToAction("DanhSachBan", "Ban");
            }

        }
        public ActionResult HuyHoaDon(int iMaHoaDon)
        {
            try
            {
                HoaDon hoaDon = _db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
                Ban ban = _db.Ban.SingleOrDefault(n => n.MaBan == hoaDon.MaBan_id);
                if (ban != null)
                {
                    ban.TinhTrang = 0;
                    _db.SaveChanges();
                }
                // muốn xóa hóa đơn phải xóa trong lịch sử gọi món
                var history = _db.LichSuGoiMon.Where(n => n.MaHoaDon_id == iMaHoaDon).ToList();
                foreach (var item in history)
                {
                    _db.LichSuGoiMon.Remove(item);
                }
                _db.HoaDon.Remove(hoaDon);
                _db.SaveChanges();

                TempData["ToastMessage"] = "success|Hủy thành công.";
                return RedirectToAction("DanhSachBan", "Ban");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Hủy hóa đơn thất bại.";
                return RedirectToAction("DanhSachBan", "Ban");
            }
        }

        private int SoLuongOrder(int iMaHoaDon)
        {
            // Thực hiện truy vấn bất đồng bộ
            var slMonAn = _db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == iMaHoaDon).Sum(n => (int?)n.SoLuongMua) ?? 0;
            return slMonAn;
        }

        private double TongTienOrder(int iMaHoaDon, float phiShip, float fMaGiamGiaVND, float fGiamGiaPhanTram)
        {
            // Thực hiện truy vấn bất đồng bộ cho các bảng cần thiết
            var ttMonAn = _db.ChiTietHoaDon
                                            .Where(n => n.MaHoaDon_id == iMaHoaDon)
                                            .ToList();
            var hoaDon = _db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
            var ban = _db.Ban.SingleOrDefault(n => n.MaBan == hoaDon.MaBan_id);

            double TOTAL = 0;

            if (hoaDon != null && ban != null)
            {
                double thanhTien = ttMonAn.Sum(n => n.ThanhTien);

                TOTAL = thanhTien
                        + phiShip
                        - fMaGiamGiaVND
                        - (thanhTien * fGiamGiaPhanTram / 100);
            }

            return TOTAL;
        }



        #region Sửa Xóa Hóa Đơn
        // danh sách hóa đơn
        public ActionResult DanhSachHoaDon()
        {
            var listHoaDon = _db.HoaDon.OrderByDescending(n => n.MaHoaDon).ToList();
            return View(listHoaDon);
        }
        // xem chi tiết
        public ActionResult ChiTietHoaDon(int iMaHoaDon)
        {
            var chiTiet = _db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
            // Lấy danh sách món ăn đã gọi trong bản chi tiết
            var listOrder = _db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == iMaHoaDon).ToList();
            ViewBag.ListOrder = listOrder;
            // Lấy thời gian gọi món sắp xếp giảm gần theo thời gian đặt / mã hóa đơn
            var listHistory = _db.LichSuGoiMon.Where(n => n.MaHoaDon_id == iMaHoaDon).ToList().OrderByDescending(n => n.MaLichSu);
            ViewBag.ListHistory = listHistory;
            return View(chiTiet);
        }
        public ActionResult XoaHoaDons(int iMaHoaDon)
        {
            try
            {
                var chiTiet = _db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
                if (chiTiet == null)
                {
                    return RedirectToAction("XoaHoaDon", "Error");
                }
                HoaDon hoaDon = _db.HoaDon.Find(iMaHoaDon);
                if (hoaDon == null)
                {
                    return HttpNotFound();
                }
                return View(hoaDon);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Xóa hóa đơn thất bại.";
                return RedirectToAction("DanhSachBan", "Ban");
            }
        }
        // xóa hóa đơn
        public ActionResult XoaHoaDon(int iMaHoaDon)
        {
            var hoaDon = _db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
            // Kiểm tra hóa đơn đã thanh toán mới được phép xóa
            if (hoaDon.TrangThai == 0)
            {
                // xóa danh sách món ăn trong hóa đơn trước
                var monAn = _db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == iMaHoaDon).ToList();
                foreach (var item in monAn)
                {
                    _db.ChiTietHoaDon.Remove(item);
                    _db.SaveChanges();
                }
                // xóa trong danh sách lịch sử gọi món
                var lichSu = _db.LichSuGoiMon.Where(n => n.MaHoaDon_id == iMaHoaDon).ToList();
                foreach (var item in lichSu)
                {
                    _db.LichSuGoiMon.Remove(item);
                    _db.SaveChanges();
                }
                _db.HoaDon.Remove(hoaDon);
                _db.SaveChanges();
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
            var hoaDonCu = _db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
            var ban = _db.Ban.SingleOrDefault(n => n.MaBan == hoaDonCu.MaBan_id);
            ban.TinhTrang = 0;
            //_db.SaveChanges();

            //Kiểm tra bàn mới này đã có khách nào đang ngồi hay k
            var checkBan = _db.Ban.SingleOrDefault(n => n.MaBan == banMoi);
            if (checkBan.TinhTrang == 0) // chưa có khách => cập nhật lại bàn mới
            {
                hoaDonCu.MaBan_id = banMoi;
                checkBan.TinhTrang = 1;
                _db.SaveChanges();

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
                var hoaDonBanMoi = _db.HoaDon.Where(n => n.MaBan_id == banMoi).OrderByDescending(n => n.MaHoaDon).FirstOrDefault();
                var chiTietMonAnMoi = _db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == hoaDonBanMoi.MaHoaDon).ToList();
                // lấy ra danh sách món add vào sanh sách món mới()
                var chiTietMonAnCu = _db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == hoaDonCu.MaHoaDon);
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
                                _db.ChiTietHoaDon.Add(cthd);
                                //_db.SaveChanges();
                            }
                            else // trùng món => + số lượng
                            {
                                var monAnMoi = _db.ChiTietHoaDon.SingleOrDefault(n => n.MaMonAn_id == items.MaMonAn_id & n.MaHoaDon_id == hoaDonBanMoi.MaHoaDon);
                                monAnMoi.SoLuongMua += item.SoLuongMua;
                            }
                        }
                    }

                    //đưa lịch sử hóa đơn cũ vào lịch sử hóa đơn mới. sau khi thực hiện xóa lịch sử củ đi
                    //lấy lịch sử món ăn bàn cũ và mới
                    var lichSuBanCu = _db.LichSuGoiMon.Where(n => n.MaHoaDon_id == hoaDonCu.MaHoaDon).ToList();
                    var lichSuBanMoi = _db.LichSuGoiMon.Where(n => n.MaHoaDon_id == hoaDonBanMoi.MaHoaDon).ToList();
                    foreach (var item in lichSuBanCu)
                    {
                        LichSuGoiMon history = new LichSuGoiMon();
                        history.SoLuongMua = item.SoLuongMua;
                        history.SoLuongTra = item.SoLuongTra;
                        history.ThoiGianGoi = item.ThoiGianGoi;
                        history.ThoiGianTra = item.ThoiGianTra;
                        history.MaHoaDon_id = hoaDonBanMoi.MaHoaDon;
                        history.MaMonAn_id = item.MaMonAn_id;
                        _db.LichSuGoiMon.Add(history);
                        _db.LichSuGoiMon.Remove(item);
                    }
                    _db.HoaDon.Remove(hoaDonCu);
                    _db.SaveChanges();
                    #region Tính lại tổng tiền
                    if (checkBan.Vip == 1)// Nếu bàn vip
                    {
                        //ViewBag.TongTienMonAn = TongTienOrder(hoaDonBanMoi.MaHoaDon, 10, 0, 0, 0);
                    }
                    else
                    {
                        //ViewBag.TongTienMonAn = TongTienOrder(hoaDonBanMoi.MaHoaDon, 0, 0, 0, 0);
                    }
                    #endregion
                }
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Index", "Home");
        }
        //Cập nhật lại tên khách hàng
        public ActionResult CapNhatTenKhachHang(int iMaHoaDon, string strURL, FormCollection f)
        {
            try
            {
                string tenkhachhang = string.IsNullOrEmpty(f["txtTenKhachhang"]) ? "" : f["txtTenKhachhang"].ToString();
                string dienThoai = string.IsNullOrEmpty(f["txtSoDienThoai"]) ? "" : f["txtSoDienThoai"].ToString();
                string diaChi = string.IsNullOrEmpty(f["txtDiaChiKhachHang"]) ? "" : f["txtDiaChiKhachHang"].ToString();


                var hoaDon = _db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
                hoaDon.TenKhachHang = tenkhachhang;
                hoaDon.SDTKhachHang = dienThoai;
                hoaDon.DiaChiKhachHang = diaChi;
                _db.SaveChanges();

                TempData["ToastMessage"] = "success|Cập nhật khách hàng thành công.";
                return Redirect(strURL);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Cập nhật khách hàng thất bại.";
                return Redirect(strURL);
            }
        }
        public ActionResult KhachHang()
        {
            var listKhachHang = _db.HoaDon.OrderByDescending(n => n.NgayTao).ToList();
            return View(listKhachHang);
        }
    }
}
