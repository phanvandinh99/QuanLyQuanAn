using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using QuanLyNhaHang.Common;
using QuanLyNhaHang.Models;
using System.Threading.Tasks;
using QuanLyNhaHang.Common.Const;

namespace QuanLyNhaHang.Areas.NhanVien.Controllers
{
    [Auth]
    public class HomeController : Controller
    {
        private readonly DatabaseQuanLyNhaHang _db;

        public HomeController(DatabaseQuanLyNhaHang db)
        {
            _db = db;
        }

        private string GetMaDoanhNghiepFromCookie()
        {
            var cookie = Request.Cookies["UserLogin"];
            if (cookie != null && cookie["MaDoanhNghiep"] != null)
            {
                return cookie["MaDoanhNghiep"];
            }
            return null;
        }

        //Tranh chủ nhân viên bán hàng
        public async Task<ActionResult> Index()
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            ViewBag.DoanhThu = await DoanhThuDonHang(sMaDoanhNghiep);
            ViewBag.SumHoaDon = await _db.HoaDon
                                .Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep &&
                                       n.TrangThai == Const.DaThanhToan)
                                .CountAsync();
            ViewBag.SumMonAn = await _db.MonAn
                               .Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep)
                               .CountAsync();
            ViewBag.SumNhanVien = await _db.NhanVien
                                  .Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep)
                                  .CountAsync();
            ViewBag.SumBan = await _db.Ban.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep)
                             .CountAsync();
            // Món Ăn Bán Chạy
            ViewBag.BanChay = await _db.MonAn
                              .Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep)
                              .OrderByDescending(n => n.SoLuongDaBan)
                              .ToListAsync();
            // Hóa đơn
            ViewBag.HoaDon = await _db.HoaDon
                             .Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep
                                    && n.NgayThanhToan.HasValue
                                    && n.NgayThanhToan.Value.Year == DateTime.Now.Year)
                             .OrderByDescending(n => n.NgayThanhToan)
                             .ToListAsync();

            return View();
        }

        public async Task<double> DoanhThuDonHang(string sMaDoanhNghiep)
        {
            double TongDoanhThu = await _db.HoaDon
                                  .Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep
                                         && n.NgayThanhToan.HasValue
                                         && n.NgayThanhToan.Value.Year == DateTime.Now.Year)
                                  .SumAsync(n => (double?)n.TongTien) ?? 0;

            return TongDoanhThu;
        }

        #region Hiển thị danh sách các bàn theo tầng khác nhau
        public async Task<ActionResult> Ban(int iMaTang)
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            var tenTang = await _db.Tang.SingleOrDefaultAsync(n => n.MaTang == iMaTang &
                                                              n.MaDoanhNghiep_id == sMaDoanhNghiep);
            ViewBag.Tang = tenTang.TenTang;
            
            var listBan = await _db.Ban
                          .Where(n => n.MaTang_id == iMaTang &&
                                 n.MaDoanhNghiep_id == sMaDoanhNghiep)
                          .OrderBy(n => n.MaBan)
                          .ToListAsync();

            return View(listBan);
        }


        public ActionResult Par_Tang()
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            var listTang = _db.Tang.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep)
                           .OrderBy(n => n.MaTang)
                           .ToList();

            return PartialView(listTang);
        }

        #endregion
    }
}