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
            return cookie?["MaDoangNghiep"];
        }

        [HttpGet]
        public async Task<ActionResult> ThongTinHoaDon(int iMaBan)
        {
            try
            {
                string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

                // Thực hiện các truy vấn không phụ thuộc song song
                var loaiMonAnTask = _db.LoaiMonAn.Where(n => n.MaDoangNghiep_id == sMaDoanhNghiep).ToListAsync();
                var danhSachMonAnTask = _db.MonAn.Where(n => n.MaDoangNghiep_id == sMaDoanhNghiep).ToListAsync();
                var monAnCountTask = _db.MonAn.Where(n => n.MaDoangNghiep_id == sMaDoanhNghiep).CountAsync();
                var banTask = _db.Ban.Where(n => n.MaDoangNghiep_id == sMaDoanhNghiep).ToListAsync();

                await Task.WhenAll(loaiMonAnTask, danhSachMonAnTask, monAnCountTask, banTask);

                // Gán kết quả vào ViewBag sau khi tất cả hoàn thành
                ViewBag.LoaiMonAn = loaiMonAnTask.Result;
                ViewBag.DanhSachMonAn = danhSachMonAnTask.Result;
                ViewBag.MonAn = monAnCountTask.Result;
                ViewBag.Ban = banTask.Result;

                if (iMaBan < Const.MangDi)
                {
                    // Tạo hóa đơn mới
                    var hoaDon = new HoaDon
                    {
                        TenKhachHang = "Mang Đi",
                        SDTKhachHang = "0123456789",
                        NgayTao = DateTime.Now,
                        TongTien = 0,
                        TrangThai = Const.ChuaThanhToan,
                        MaBan_id = Const.MangDi,
                        MaDoangNghiep_id = sMaDoanhNghiep
                    };

                    _db.HoaDon.Add(hoaDon);
                    _db.SaveChanges();

                    // Đặt các giá trị ViewBag mặc định cho hóa đơn mới
                    ViewBag.MonAnKhachChon = 0;
                    ViewBag.TongTienMonAn = 0;
                    ViewBag.SoLuongMonAn = 0;

                    return View(hoaDon);
                }
                else if (iMaBan >= Const.TaiBan)
                {
                    // Kiểm tra bàn có hợp lệ không
                    var checkBan = await _db.Ban.SingleOrDefaultAsync(n => n.MaBan == iMaBan && n.MaDoangNghiep_id == sMaDoanhNghiep);
                    if (checkBan == null)
                    {
                        TempData["ToastMessage"] = "error|Bàn không hợp lệ";
                        return RedirectToAction("Index", "Home");
                    }

                    // Kiểm tra tình trạng của bàn
                    if (checkBan.TinhTrang == Const.KhongCoNguoi)
                    {
                        checkBan.TinhTrang = Const.CoNguoi;
                        _db.SaveChanges();

                        // Tạo hóa đơn mới
                        var hoaDonNew = new HoaDon
                        {
                            TenKhachHang = "Tại Bàn",
                            SDTKhachHang = "1234567890",
                            NgayTao = DateTime.Now,
                            TongTien = 0,
                            TrangThai = Const.ChuaThanhToan,
                            MaBan_id = iMaBan,
                            MaDoangNghiep_id = sMaDoanhNghiep
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
                        var hoaDon = await _db.HoaDon.SingleOrDefaultAsync(n => n.MaBan_id == iMaBan &&
                                                                               n.TrangThai == Const.ChuaThanhToan &&
                                                                               n.MaDoangNghiep_id == sMaDoanhNghiep);
                        if (hoaDon == null)
                        {
                            checkBan.TinhTrang = Const.KhongCoNguoi;
                            _db.SaveChanges();

                            TempData["ToastMessage"] = "error|Hóa đơn không tồn tại";
                            return RedirectToAction("DanhSachBan", "Ban");
                        }

                        // Tính toán tổng tiền và số lượng món ăn
                        ViewBag.MonAnKhachChon = hoaDon;
                        ViewBag.TongTienMonAn = await TongTienOrderAsync(hoaDon.MaHoaDon, 0, 0, 0, 0);
                        ViewBag.SoLuongMonAn = await SoLuongOrderAsync(hoaDon.MaHoaDon);

                        return View(hoaDon);
                    }
                }

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
                //TempData["ToastMessage"] = "error|Tạo hóa đơn thất bại.";
                //return RedirectToAction("DanhSachBan", "Ban");
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
            int soLuongOrder = int.Parse(f["txtSoLuongThem"].ToString());
            int kiemTra = 0;
            #region Kiểm tra trong kho có đáp ứng đủ k
            // lấy danh sách chi tiết sản phẩm
            var listCTSP = _db.ChiTietSanPham.Where(n => n.MaMonAn_id == iMaMonAn && n.Tru == 1);
            if (listCTSP != null)
            {
                foreach (var item in listCTSP)
                {
                    // lấy số lượng trong kho
                    var nguyenLieu = _db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
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

                    #region Trừ nguyên liệu trong kho và lưu vào phiếu xuất kho
                    //B1: tìm món ăn
                    // B2: Lọc ra chi tiết sản phẩm có giá trị = 1
                    var listChitietSanPham = _db.ChiTietSanPham.Where(n => n.MaMonAn_id == giaMonAn.MaMonAn && n.Tru == 1);
                    foreach (var item in listChitietSanPham)
                    {
                        // lấy ra nguyên liệu tương ứng
                        var nguyenLieu = _db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
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
                    _db.SaveChanges();

                    // tạo phiếu xuất
                    double? tongTien = 0;
                    XuatKho xk = new XuatKho();
                    xk.NgayXuat = DateTime.Now;
                    _db.XuatKho.Add(xk);
                    _db.SaveChanges();
                    // lưu nguyên liệu vào nguyên liệu xuất
                    var listNguyenLieu = _db.ChiTietSanPham.Where(n => n.MaMonAn_id == giaMonAn.MaMonAn);
                    // lấy mã xuất kho
                    var xuatKho = _db.XuatKho.OrderByDescending(n => n.MaXuatKho).FirstOrDefault();
                    foreach (var item in listNguyenLieu)
                    {
                        // lấy ra nguyên liệu tương ứng => tìm đc đơn giá
                        var nguyenLieu = _db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
                        NguyenLieuXuat nlx = new NguyenLieuXuat();
                        nlx.MaXuatKho_id = xuatKho.MaXuatKho;
                        nlx.MaNguyenLieu_id = item.MaNguyenLieu_id;
                        nlx.SoLuongXuat = item.SoLuongDung;
                        _db.NguyenLieuXuat.Add(nlx);
                    }
                    _db.SaveChanges();
                    _db.SaveChanges();
                    #endregion

                    return Redirect(strURL);
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

                    #region Trừ nguyên liệu trong kho và lưu vào phiếu xuất kho
                    //B1: tìm món ăn
                    // B2: Lọc ra chi tiết sản phẩm có giá trị = 1
                    var listChitietSanPham = _db.ChiTietSanPham.Where(n => n.MaMonAn_id == giaMonAn.MaMonAn && n.Tru == 1);
                    foreach (var item in listChitietSanPham)
                    {
                        // lấy ra nguyên liệu tương ứng
                        var nguyenLieu = _db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
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
                    _db.SaveChanges();

                    // tạo phiếu xuất
                    double? tongTien = 0;
                    XuatKho xk = new XuatKho();
                    xk.NgayXuat = DateTime.Now;
                    _db.XuatKho.Add(xk);
                    _db.SaveChanges();
                    // lưu nguyên liệu vào nguyên liệu xuất
                    var listNguyenLieu = _db.ChiTietSanPham.Where(n => n.MaMonAn_id == giaMonAn.MaMonAn);
                    // lấy mã xuất kho
                    var xuatKho = _db.XuatKho.OrderByDescending(n => n.MaXuatKho).FirstOrDefault();
                    foreach (var item in listNguyenLieu)
                    {
                        // lấy ra nguyên liệu tương ứng => tìm đc đơn giá
                        var nguyenLieu = _db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
                        NguyenLieuXuat nlx = new NguyenLieuXuat();
                        nlx.MaXuatKho_id = xuatKho.MaXuatKho;
                        nlx.MaNguyenLieu_id = item.MaNguyenLieu_id;
                        nlx.SoLuongXuat = item.SoLuongDung;
                        _db.NguyenLieuXuat.Add(nlx);
                    }
                    _db.SaveChanges();
                    _db.SaveChanges();
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
            var monAn = _db.ChiTietHoaDon.SingleOrDefault(n => n.MaHoaDon_id == iMaHoaDon && n.MaMonAn_id == iMaMonAn);
            // Lấy giá món ăn
            MonAn giaMonAn = _db.MonAn.SingleOrDefault(n => n.MaMonAn == iMaMonAn);
            if (soLuong > 0)
            {
                //Ghi vào lịch sử gọi món. Có 2 trường hợp. cập nhật tăng => thêm món. Cập nhật giảm => hủy món
                if (soLuong > monAn.SoLuongMua) // cập nhật thêm số lượng món ăn
                {
                    int kiemTra = 0;
                    // lấy danh sách chi tiết sản phẩm
                    var listCTSP = _db.ChiTietSanPham.Where(n => n.MaMonAn_id == iMaMonAn && n.Tru == 1);
                    if (listCTSP != null)
                    {
                        foreach (var item in listCTSP)
                        {
                            // lấy số lượng trong kho
                            var nguyenLieu = _db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
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
                        _db.LichSuGoiMon.Add(ls);
                        _db.SaveChanges();

                        #region Trừ nguyên liệu trong kho và lưu vào phiếu xuất kho
                        //B1: tìm món ăn
                        // B2: Lọc ra chi tiết sản phẩm có giá trị = 1
                        var listChitietSanPham = _db.ChiTietSanPham.Where(n => n.MaMonAn_id == giaMonAn.MaMonAn && n.Tru == 1);
                        foreach (var item in listChitietSanPham)
                        {
                            // lấy ra nguyên liệu tương ứng
                            var nguyenLieu = _db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
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
                        _db.SaveChanges();

                        // tạo phiếu xuất
                        XuatKho xk = new XuatKho();
                        xk.NgayXuat = DateTime.Now;
                        _db.XuatKho.Add(xk);
                        _db.SaveChanges();
                        // lưu nguyên liệu vào nguyên liệu xuất
                        var listNguyenLieu = _db.ChiTietSanPham.Where(n => n.MaMonAn_id == giaMonAn.MaMonAn);
                        // lấy mã xuất kho
                        var xuatKho = _db.XuatKho.OrderByDescending(n => n.MaXuatKho).FirstOrDefault();
                        foreach (var item in listNguyenLieu)
                        {
                            // lấy ra nguyên liệu tương ứng => tìm đc đơn giá
                            var nguyenLieu = _db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
                            NguyenLieuXuat nlx = new NguyenLieuXuat();
                            nlx.MaXuatKho_id = xuatKho.MaXuatKho;
                            nlx.MaNguyenLieu_id = item.MaNguyenLieu_id;
                            nlx.SoLuongXuat = item.SoLuongDung;
                            _db.NguyenLieuXuat.Add(nlx);
                        }
                        _db.SaveChanges();
                        _db.SaveChanges();
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
                    _db.LichSuGoiMon.Add(ls);
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
                _db.SaveChanges();

                _db.ChiTietHoaDon.Remove(monAn);
            }
            // lấy hóa đơn
            var hoaDon = _db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
            var ban = _db.Ban.SingleOrDefault(n => n.MaBan == hoaDon.MaBan_id);
            if (ban.Vip == 1)// Nếu bàn vip
            {
                //ViewBag.TongTienMonAn = await TongTienOrderAsync(hoaDon.MaHoaDon, 10, 0, 0, 0);
            }
            else
            {
                //ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, 0, 0, 0, 0);
            }

            _db.SaveChanges();

            return Redirect(strURL);
        }
        public ActionResult XoaMonAn(int iMaHoaDon, int iMaMonAn, string strURL)
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

            HoaDon hoaDon = _db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
            Ban ban = _db.Ban.SingleOrDefault(n => n.MaBan == hoaDon.MaBan_id);
            if (hoaDon != null)
            {

                hoaDon.TrangThai = 0; // trạng thái trong đơn hàng = 0 là đã thanh toán
                if (ban.Vip == 1)// Nếu bàn vip
                {
                    //ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, 10, giamGiaVND, giamGiaPhanTram, giamGiaKhachHang);
                }
                else
                {
                    //ViewBag.TongTienMonAn = TongTienOrder(hoaDon.MaHoaDon, 0, giamGiaVND, giamGiaPhanTram, giamGiaKhachHang);
                }
                hoaDon.TongTien = ViewBag.TongTienMonAn;
                hoaDon.NgayThanhToan = DateTime.Now;
                _db.SaveChanges();
            }
            if (ban != null)
            {
                ban.TinhTrang = 0;
                _db.SaveChanges();
            }
            //return RedirectToAction("Index", "Home");
            return RedirectToAction("ThanhCong", "Error");

        }
        public ActionResult HuyHoaDon(int iMaHoaDon)
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

            return RedirectToAction("Index", "Home");
        }

        private async Task<int> SoLuongOrderAsync(int iMaHoaDon)
        {
            int COUNT = 0;

            // Thực hiện truy vấn bất đồng bộ
            var slMonAnTask = Task.Run(() => _db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == iMaHoaDon));

            // Chờ kết quả từ truy vấn
            var slMonAn = await slMonAnTask;

            if (slMonAn.Count() != 0)
            {
                COUNT = slMonAn.Sum(n => n.SoLuongMua);
            }

            return COUNT;
        }


        // Tổng tiền gồm có: phí phòng vip, giảm giá vnd, giảm giá phần trăm, 
        private async Task<double> TongTienOrderAsync(int iMaHoaDon, float fPhibanVip, float fMaGiamGiaVND, float fGiamGiaPhanTram, float fGiamGiaKhachHang)
        {
            double TOTAL = 0; // Tổng tiền bằng 0 

            // Tìm món ăn thuộc hóa đơn (xử lý bất đồng bộ)
            var ttMonAnTask = Task.Run(() => _db.ChiTietHoaDon.Where(n => n.MaHoaDon_id == iMaHoaDon).ToList());

            // Lấy hóa đơn và bàn (cũng bất đồng bộ)
            var hoaDonTask = Task.Run(() => _db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon));
            var hoaDon = await hoaDonTask;
            var banTask = Task.Run(() => _db.Ban.SingleOrDefault(n => n.MaBan == hoaDon.MaBan_id));

            // Đợi các tác vụ hoàn thành
            var ttMonAn = await ttMonAnTask;
            var ban = await banTask;

            if (ban.Vip == 1) // Bàn VIP
            {
                if (ttMonAn.Count != 0)
                {
                    TOTAL = ttMonAn.Sum(n => n.ThanhTien)
                            + (ttMonAn.Sum(n => n.ThanhTien) * 10 / 100)
                            - fMaGiamGiaVND
                            - fGiamGiaKhachHang
                            - (ttMonAn.Sum(n => n.ThanhTien) * fGiamGiaPhanTram / 100);

                    if (TOTAL < 0)
                    {
                        TOTAL = 0;
                    }
                }
            }
            else // Không phải bàn VIP
            {
                if (ttMonAn.Count != 0)
                {
                    TOTAL = ttMonAn.Sum(n => n.ThanhTien)
                            - fMaGiamGiaVND
                            - fGiamGiaKhachHang
                            - (ttMonAn.Sum(n => n.ThanhTien) * fGiamGiaPhanTram / 100);
                }
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
            string tenkhachhang = f["txtTenKhachhang"].ToString();
            string dienThoai = f["txtSoDienThoai"].ToString();

            var hoaDon = _db.HoaDon.SingleOrDefault(n => n.MaHoaDon == iMaHoaDon);
            hoaDon.TenKhachHang = tenkhachhang;
            hoaDon.SDTKhachHang = dienThoai;

            #region Kiểm tra số điện thoại khách hàng đã tồn tại chưa
            var demSoHoaDon = _db.HoaDon.Where(n => n.SDTKhachHang == dienThoai).OrderByDescending(n => n.MaHoaDon).FirstOrDefault();
            if (demSoHoaDon != null)
            {
                hoaDon.TenKhachHang = demSoHoaDon.TenKhachHang;
            }

            #endregion
            _db.SaveChanges();
            return Redirect(strURL);
        }
        public ActionResult KhachHang()
        {
            var listKhachHang = _db.HoaDon.OrderByDescending(n => n.NgayTao).ToList();
            return View(listKhachHang);
        }
    }
}
