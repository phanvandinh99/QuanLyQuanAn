using QuanLyNhaHang.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVien.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseQuanLyNhaHang _db;

        public HomeController(DatabaseQuanLyNhaHang db)
        {
            _db = db;
        }

        private int GetMaDoanhNghiepFromCookie()
        {
            var cookie = Request.Cookies["UserLogin"];
            if (cookie != null && int.TryParse(cookie["MaDoangNghiep"], out int iMaDoangNghiep))
            {
                return iMaDoangNghiep;
            }
            return 0;
        }

        //Tranh chủ nhân viên bán hàng
        public async Task<ActionResult> Index()
        {
            int iMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            ViewBag.DoanhThu = await DoanhThuDonHang(iMaDoanhNghiep);
            ViewBag.SumHoaDon = await _db.HoaDon
                                .Where(n => n.MaDoangNghiep_id == iMaDoanhNghiep)
                                .CountAsync();
            ViewBag.SumMonAn = await _db.MonAn
                               .Where(n => n.MaDoangNghiep_id == iMaDoanhNghiep)
                               .CountAsync();
            ViewBag.SumNhanVien = await _db.NhanVien
                                  .Where(n => n.MaDoangNghiep_id == iMaDoanhNghiep)
                                  .CountAsync();
            ViewBag.SumBan = await _db.Ban.Where(n => n.MaDoangNghiep_id == iMaDoanhNghiep)
                             .CountAsync();
            // Món Ăn Bán Chạy
            ViewBag.BanChay = await _db.MonAn
                              .Where(n => n.MaDoangNghiep_id == iMaDoanhNghiep)
                              .OrderByDescending(n => n.SoLuongDaBan)
                              .ToListAsync();
            // Hóa đơn
            ViewBag.HoaDon = await _db.HoaDon
                             .Where(n => n.MaDoangNghiep_id == iMaDoanhNghiep
                                    && n.NgayThanhToan.HasValue
                                    && n.NgayThanhToan.Value.Year == DateTime.Now.Year)
                             .OrderByDescending(n => n.NgayThanhToan)
                             .ToListAsync();

            return View();
        }

        public async Task<double> DoanhThuDonHang(int iMaDoanhNghiep)
        {
            double TongDoanhThu = await _db.HoaDon
                                  .Where(n => n.MaDoangNghiep_id == iMaDoanhNghiep
                                         && n.NgayThanhToan.HasValue
                                         && n.NgayThanhToan.Value.Year == DateTime.Now.Year)
                                  .SumAsync(n => (double?)n.TongTien) ?? 0;

            return TongDoanhThu;
        }

        #region Hiển thị danh sách các bàn theo tầng khác nhau
        public async Task<ActionResult> Ban(int iMaTang)
        {
            int iMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            var tenTang = await _db.Tang.SingleOrDefaultAsync(n => n.MaTang == iMaTang & n.MaDoangNghiep_id == iMaDoanhNghiep);
            ViewBag.Tang = tenTang.TenTang;

            var listBan = await _db.Ban
                          .Where(n => n.MaTang_id == iMaTang & n.MaDoangNghiep_id == iMaDoanhNghiep)
                          .OrderBy(n => n.MaBan)
                          .ToListAsync();

            return View(listBan);
        }

        public ActionResult Par_Tang()
        {
            int iMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            var listTang = _db.Tang.Where(n => n.MaDoangNghiep_id == iMaDoanhNghiep)
                           .OrderBy(n => n.MaTang)
                           .ToList();

            return PartialView(listTang);
        }

        #endregion
    }
}