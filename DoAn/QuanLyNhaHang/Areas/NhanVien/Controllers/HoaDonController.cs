using QuanLyNhaHang.Common.Const;
using QuanLyNhaHang.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<ActionResult> ThongTinHoaDon(int? iMaBan)
        {
            try
            {
                // Kiểm tra bàn có null không
                if (iMaBan == null)
                {
                    TempData["ToastMessage"] = "error|Bàn không hợp lệ";
                    return RedirectToAction("Index", "Home");
                }


                string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

                // Kiểm tra bàn có hợp lệ không
                var checkBan = await _db.Ban.SingleOrDefaultAsync(n => n.MaBan == iMaBan && n.MaDoanhNghiep_id == sMaDoanhNghiep);
                if (checkBan == null)
                {
                    TempData["ToastMessage"] = "error|Bàn không hợp lệ";
                    return RedirectToAction("Index", "Home");
                }

                var loaiMonAn = await _db.LoaiMonAn.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).ToListAsync();
                var monAn = await _db.MonAn.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).ToListAsync();
                var banTask = await _db.Ban.Where(n=>n.MaBan != iMaBan &&
                                                  n.MangDi == Const.boolTaiBan &&
                                                  n.MaDoanhNghiep_id == sMaDoanhNghiep).ToListAsync();

                ViewBag.LoaiMonAn = loaiMonAn;
                ViewBag.DanhSachMonAn = monAn;
                ViewBag.Ban = banTask;

                if (checkBan.MangDi == Const.boolMangDi) // Khách mang đi
                {
                    ViewBag.MangDi = Const.MangDi;

                    // Kiểm tra hóa đơn mang đi có trạng thái là chưa thanh toán
                    HoaDon hoaDon = await _db.HoaDon.FirstOrDefaultAsync(n => n.MaBan_id == Const.MangDi && n.TrangThai == Const.ChuaThanhToan);

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
                        await _db.SaveChangesAsync();

                        ViewBag.MonAnKhachChon = 0;
                        ViewBag.TongTienMonAn = 0;
                        ViewBag.SoLuongMonAn = 0;

                        return View(HoaDon);
                    }

                    // Tính toán tổng tiền và số lượng món ăn
                    ViewBag.MonAnKhachChon = await _db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == hoaDon.MaHoaDon).ToListAsync();
                    ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, 0, 0, 0);
                    ViewBag.SoLuongMonAn = SoLuongOrder(hoaDon.MaHoaDon);

                    return View(hoaDon);
                }
                else // Khách ngồi tại bàn
                {
                    ViewBag.MangDi = Const.TaiBan;
                    ViewBag.TenBan = checkBan.TenBan;

                    // Kiểm tra tình trạng của bàn
                    if (checkBan.TinhTrang == Const.KhongCoNguoi)
                    {
                        checkBan.TinhTrang = Const.CoNguoi;
                        await _db.SaveChangesAsync();

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
                        await _db.SaveChangesAsync();

                        ViewBag.MonAnKhachChon = 0;
                        ViewBag.TongTienMonAn = 0;
                        ViewBag.SoLuongMonAn = 0;

                        return View(hoaDonNew);
                    }
                    else
                    {
                        // Lấy hóa đơn của bàn đã có người
                        var hoaDon = await _db.HoaDon.SingleOrDefaultAsync(n => n.MaBan_id == iMaBan &&
                                                                           n.TrangThai == Const.ChuaThanhToan &&
                                                                           n.MaDoanhNghiep_id == sMaDoanhNghiep);
                        if (hoaDon == null)
                        {
                            checkBan.TinhTrang = Const.KhongCoNguoi;
                            await _db.SaveChangesAsync();

                            TempData["ToastMessage"] = "error|Hóa đơn không tồn tại";
                            return RedirectToAction("DanhSachBan", "Ban");
                        }

                        // Tính toán tổng tiền và số lượng món ăn
                        ViewBag.MonAnKhachChon = await _db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == hoaDon.MaHoaDon).ToListAsync();
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

        [HttpGet]
        public async Task<ActionResult> ThongTinHoaDonMonAn(int iMaBan, int iMaHoaDon, int iMaLMA)
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

        public async Task<ActionResult> Order(int iMaHoaDon, int iMaMonAn, string strURL, FormCollection f) // Order = thêm món ăn vào hóa đơn
        {
            try
            {
                string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

                int soLuongOrder = int.Parse(f["txtSoLuongThem"].ToString());

                #region Tính Toán
                // Lấy giá món ăn
                MonAn giaMonAn = await _db.MonAn.FindAsync(iMaMonAn);
                var monAn = await _db.ChiTietHoaDon.SingleOrDefaultAsync(n => n.MaHoaDon_id == iMaHoaDon && n.MaMonAn_id == iMaMonAn);
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
                    await _db.SaveChangesAsync();

                    //Ghi vào lịch sử gọi món
                    LichSuGoiMon ls = new LichSuGoiMon();
                    ls.SoLuongMua = soLuongOrder;
                    ls.SoLuongTra = 0;
                    ls.ThoiGianGoi = DateTime.Now;
                    ls.ThoiGianTra = null;
                    ls.MaHoaDon_id = iMaHoaDon;
                    ls.MaMonAn_id = iMaMonAn;
                    _db.LichSuGoiMon.Add(ls);
                    await _db.SaveChangesAsync();
                }
                else // Món ăn đã tồn tại (ví dụ: Trước đó khách gọi món lẩu, giờ cũng gọi thêm 1 suất nữa)
                {
                    monAn.SoLuongMua = monAn.SoLuongMua + soLuongOrder;
                    monAn.ThanhTien = (double)((monAn.SoLuongMua) * giaMonAn.DonGia);
                    await _db.SaveChangesAsync();

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
                    await _db.SaveChangesAsync();
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

        public async Task<ActionResult> CapNhatSoLuong(int iMaHoaDon, int iMaMonAn, string strURL, FormCollection f)
        {
            try
            {
                int soLuong = int.Parse(f["txtSoLuongMua"].ToString());

                var monAn = await _db.ChiTietHoaDon.SingleOrDefaultAsync(n => n.MaHoaDon_id == iMaHoaDon && n.MaMonAn_id == iMaMonAn);
                // Lấy giá món ăn
                MonAn giaMonAn = await _db.MonAn.FindAsync(iMaMonAn);
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
                        await _db.SaveChangesAsync();

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
                        await _db.SaveChangesAsync();
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
                    await _db.SaveChangesAsync();


                    _db.ChiTietHoaDon.Remove(monAn);
                }
                // lấy hóa đơn
                var hoaDon = _db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
                ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, 0, 0, 0);

                await _db.SaveChangesAsync();


                TempData["ToastMessage"] = "success|Cập nhật số lượng món ăn thành công.";
                return Redirect(strURL);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Cập nhật số lượng món ăn thất bại.";
                return Redirect(strURL);
            }
        }

        public async Task<ActionResult> XoaMonAn(int iMaHoaDon, int iMaMonAn, string strURL)
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

        public async Task<ActionResult> ThanhToan(int iMaHoaDon, FormCollection f)
        {
            try
            {
                float giamGiaVND = string.IsNullOrEmpty(f["txtGiamGiaVND"]) ? 0 : float.Parse(f["txtGiamGiaVND"]);
                float giamGiaPhanTram = string.IsNullOrEmpty(f["txtGiamGiaPhanTram"]) ? 0 : float.Parse(f["txtGiamGiaPhanTram"]);
                float phiShip = string.IsNullOrEmpty(f["txtPhiShip"]) ? 0 : float.Parse(f["txtPhiShip"]);

                HoaDon hoaDon = await _db.HoaDon.FindAsync(iMaHoaDon);
                Ban ban = await _db.Ban.FindAsync(hoaDon.MaBan_id);
                if (hoaDon != null)
                {
                    hoaDon.TrangThai = Const.DaThanhToan; // trạng thái trong đơn hàng = 0 là đã thanh toán

                    ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, phiShip, giamGiaVND, giamGiaPhanTram);
                    hoaDon.PhiShip = phiShip;
                    hoaDon.GiamGiaTienMat = giamGiaVND;
                    hoaDon.GiamGiaPhanTram = giamGiaPhanTram;
                    hoaDon.TongTien = ViewBag.TongTienMonAn;
                    hoaDon.NgayThanhToan = DateTime.Now;
                    await _db.SaveChangesAsync();

                }
                if (ban != null)
                {
                    ban.TinhTrang = 0;
                    await _db.SaveChangesAsync();
                }
                TempData["ToastMessage"] = "success|Thanh toán thành công.";
                return RedirectToAction("Ban", "Home", new { iMaTang = ban.MaTang_id });
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Thanh toán hóa đơn thất bại.";
                return RedirectToAction("DanhSachHoaDon", "HoaDon");
            }

        }
        public async Task<ActionResult> HuyHoaDon(int iMaHoaDon)
        {
            try
            {
                HoaDon hoaDon = await _db.HoaDon.FindAsync(iMaHoaDon);
                Ban ban = await _db.Ban.FindAsync(hoaDon.MaBan_id);
                if (ban != null)
                {
                    ban.TinhTrang = 0;
                    await _db.SaveChangesAsync();
                }
                // muốn xóa hóa đơn phải xóa trong lịch sử gọi món
                var history = _db.LichSuGoiMon.Where(n => n.MaHoaDon_id == iMaHoaDon).ToList();
                foreach (var item in history)
                {
                    _db.LichSuGoiMon.Remove(item);
                }
                _db.HoaDon.Remove(hoaDon);
                await _db.SaveChangesAsync();

                TempData["ToastMessage"] = "success|Hủy thành công.";
                return RedirectToAction("Ban", "Home", new { iMaTang = ban.MaTang_id });
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Hủy hóa đơn thất bại.";
                return RedirectToAction("Index", "Home");
            }
        }

        #region Sửa Xóa Hóa Đơn
        // danh sách hóa đơn
        public async Task<ActionResult> DanhSachHoaDon()
        {
            var listHoaDon = await _db.HoaDon.OrderByDescending(n => n.MaHoaDon).ToListAsync();
            return View(listHoaDon);
        }
        // xem chi tiết
        public async Task<ActionResult> ChiTietHoaDon(int iMaHoaDon)
        {
            var chiTiet = await _db.HoaDon.FindAsync(iMaHoaDon);
            // Lấy danh sách món ăn đã gọi trong bản chi tiết
            var listOrder = await _db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == iMaHoaDon).ToListAsync();
            ViewBag.ListOrder = listOrder;
            // Lấy thời gian gọi món sắp xếp giảm gần theo thời gian đặt / mã hóa đơn
            var listHistory = await _db.LichSuGoiMon.Where(n => n.MaHoaDon_id == iMaHoaDon).OrderByDescending(n => n.MaLichSu).ToListAsync();
            ViewBag.ListHistory = listHistory;
            return View(chiTiet);
        }
        public async Task<ActionResult> XoaHoaDons(int iMaHoaDon)
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
        public async Task<ActionResult> XoaHoaDon(int iMaHoaDon)
        {
            var hoaDon = await _db.HoaDon.FindAsync(iMaHoaDon);
            // Kiểm tra hóa đơn đã thanh toán mới được phép xóa
            if (hoaDon.TrangThai == 0)
            {
                // xóa danh sách món ăn trong hóa đơn trước
                var monAn = await _db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == iMaHoaDon).ToListAsync();
                foreach (var item in monAn)
                {
                    _db.ChiTietHoaDon.Remove(item);
                    await _db.SaveChangesAsync();
                }
                // xóa trong danh sách lịch sử gọi món
                var lichSu = _db.LichSuGoiMon.Where(n => n.MaHoaDon_id == iMaHoaDon).ToList();
                foreach (var item in lichSu)
                {
                    _db.LichSuGoiMon.Remove(item);
                    await _db.SaveChangesAsync();
                }
                _db.HoaDon.Remove(hoaDon);
                await _db.SaveChangesAsync();

                TempData["ToastMessage"] = "success|Xóa hóa đơn thành công.";

                return RedirectToAction("DanhSachHoaDon", "HoaDon");
            }
            else
            {
                TempData["ToastMessage"] = "error|Xóa hóa đơn thất bại.";

                return RedirectToAction("XoaHoaDon", "Error");
            }
        }
        #endregion


        public async Task<ActionResult> ChuyenBan(int iMaHoaDon, FormCollection f)
        {
            int banMoi = int.Parse(f["Ban"]);

            // Retrieve HoaDon and Ban entities
            var hoaDonCu = await _db.HoaDon.Include(hd => hd.Ban).FirstOrDefaultAsync(hd => hd.MaHoaDon == iMaHoaDon);
            var banMoiEntity = await _db.Ban.FindAsync(banMoi);

            if (hoaDonCu == null || banMoiEntity == null)
            {
                return RedirectToAction("Index", "Home"); // Redirect if entities are not found
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    // Reset old table status
                    if (hoaDonCu.Ban != null)
                    {
                        hoaDonCu.Ban.TinhTrang = 0;
                    }

                    // Update new table status
                    if (banMoiEntity.TinhTrang == 0) // Table is free
                    {
                        hoaDonCu.MaBan_id = banMoi;
                        banMoiEntity.TinhTrang = 1;
                        await _db.SaveChangesAsync();
                    }
                    else // Table is occupied
                    {
                        // Retrieve the latest bill on the new table
                        var hoaDonBanMoi = await _db.HoaDon.Where(n => n.MaBan_id == banMoi).OrderByDescending(n => n.MaHoaDon).FirstOrDefaultAsync();
                        if (hoaDonBanMoi == null)
                        {
                            return RedirectToAction("Index", "Home"); // Redirect if no bill found
                        }

                        var chiTietMonAnCu = await _db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == hoaDonCu.MaHoaDon).ToListAsync();
                        var chiTietMonAnMoi = await _db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == hoaDonBanMoi.MaHoaDon).ToListAsync();

                        var chiTietMonAnMoiDict = chiTietMonAnMoi.ToDictionary(c => c.MaMonAn_id);

                        // Process items from old bill
                        foreach (var item in chiTietMonAnCu)
                        {
                            if (chiTietMonAnMoiDict.TryGetValue(item.MaMonAn_id, out var existingItem))
                            {
                                // Update quantity if the item exists in the new bill
                                existingItem.SoLuongMua += item.SoLuongMua;
                                _db.Entry(existingItem).State = EntityState.Modified;
                            }
                            else
                            {
                                // Add new item if it does not exist in the new bill
                                var newChiTiet = new ChiTietHoaDon
                                {
                                    MaHoaDon_id = hoaDonBanMoi.MaHoaDon,
                                    MaMonAn_id = item.MaMonAn_id,
                                    SoLuongMua = item.SoLuongMua,
                                    ThanhTien = item.ThanhTien
                                };
                                _db.ChiTietHoaDon.Add(newChiTiet);
                            }
                        }

                        // Transfer history
                        var lichSuBanCu = await _db.LichSuGoiMon.Where(n => n.MaHoaDon_id == hoaDonCu.MaHoaDon).ToListAsync();
                        foreach (var item in lichSuBanCu)
                        {
                            var history = new LichSuGoiMon
                            {
                                SoLuongMua = item.SoLuongMua,
                                SoLuongTra = item.SoLuongTra,
                                ThoiGianGoi = item.ThoiGianGoi,
                                ThoiGianTra = item.ThoiGianTra,
                                MaHoaDon_id = hoaDonBanMoi.MaHoaDon,
                                MaMonAn_id = item.MaMonAn_id
                            };
                            _db.LichSuGoiMon.Add(history);
                        }

                        _db.LichSuGoiMon.RemoveRange(lichSuBanCu);
                        _db.HoaDon.Remove(hoaDonCu);

                        await _db.SaveChangesAsync();

                        ViewBag.TongTienMonAn = TongTienOrder(hoaDonBanMoi.MaHoaDon, 0, 0, 0);
                    }

                    transaction.Commit();

                    TempData["ToastMessage"] = "success|Chuyển bàn thành công.";

                    return RedirectToAction("ThongTinHoaDon", "HoaDon", new { iMaBan = banMoi });
                }
                catch
                {
                    TempData["ToastMessage"] = "error|Chuyển bàn thất bại.";
                    transaction.Rollback();
                }
            }

            TempData["ToastMessage"] = "error|Thất bại.";

            return RedirectToAction("Index", "Home");
        }



        //Cập nhật lại tên khách hàng
        public async Task<ActionResult> CapNhatTenKhachHang(int iMaHoaDon, string strURL, FormCollection f)
        {
            try
            {
                string tenkhachhang = string.IsNullOrEmpty(f["txtTenKhachhang"]) ? "" : f["txtTenKhachhang"].ToString();
                string dienThoai = string.IsNullOrEmpty(f["txtSoDienThoai"]) ? "" : f["txtSoDienThoai"].ToString();
                string diaChi = string.IsNullOrEmpty(f["txtDiaChiKhachHang"]) ? "" : f["txtDiaChiKhachHang"].ToString();


                var hoaDon =    await _db.HoaDon.FindAsync(iMaHoaDon);
                hoaDon.TenKhachHang = tenkhachhang;
                hoaDon.SDTKhachHang = dienThoai;
                hoaDon.DiaChiKhachHang = diaChi;
                await _db.SaveChangesAsync();

                TempData["ToastMessage"] = "success|Cập nhật khách hàng thành công.";
                return Redirect(strURL);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Cập nhật khách hàng thất bại.";
                return Redirect(strURL);
            }
        }
        public async Task<ActionResult> KhachHang()
        {
            var listKhachHang = await _db.HoaDon.OrderByDescending(n => n.NgayTao).ToListAsync();
            return View(listKhachHang);
        }
    }
}
