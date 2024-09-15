using QuanLyNhaHang.Common.Const;
using QuanLyNhaHang.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVien.Controllers
{
    public class ThongKeController : Controller
    {
        private readonly DatabaseQuanLyNhaHang _db;

        public ThongKeController(DatabaseQuanLyNhaHang db)
        {
            _db = db;
        }

        private string GetMaDoanhNghiepFromCookie()
        {
            var cookie = Request.Cookies["UserLogin"];
            return cookie?["MaDoanhNghiep"];
        }

        // GET: NhanVien/ThongKe
        public async Task<ActionResult> Index()
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            // Lấy ngày đầu tiên và cuối cùng của tháng hiện tại
            DateTime startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            DateTime endOfMonthPlusOne = endOfMonth.AddDays(1);

            // Lấy danh sách hóa đơn
            var hoaDon = await _db.HoaDon.Where(n => n.TrangThai == Const.DaThanhToan &&
                                                    n.NgayThanhToan.HasValue &&
                                                    n.NgayThanhToan.Value >= startOfMonth &&
                                                    n.NgayThanhToan.Value < endOfMonthPlusOne &&
                                                    n.MaDoanhNghiep_id == sMaDoanhNghiep)
                                         .ToListAsync();


            // Lấy danh sách các MaHoaDon từ hoaDon
            var maHoaDonList = hoaDon.Select(hd => hd.MaHoaDon).ToList();

            // Lấy danh sách chi tiết hóa đơn liên quan đến các hóa đơn đã lấy
            var chiTietHoaDons = await _db.ChiTietHoaDon
                .Where(ct => maHoaDonList.Contains(ct.MaHoaDon_id))
                .ToListAsync();

            // Tính tổng số hóa đơn
            ViewBag.HoaDonCount = hoaDon.Count;

            // Tính tổng số lượng sản phẩm đã mua
            ViewBag.HoaDonSum = chiTietHoaDons.Sum(ct => ct.SoLuongMua);

            // Tính tổng thành tiền
            ViewBag.ThanhTien = chiTietHoaDons.Sum(ct => ct.ThanhTien);

            return View(hoaDon);
        }


        [HttpPost]
        public async Task<ActionResult> Index(DateTime? startOfMonth, DateTime? endOfMonth)
        {
            try
            {
                string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

                DateTime now = DateTime.Now;
                DateTime currentYearStart = new DateTime(now.Year, 1, 1);
                DateTime firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

                DateTime actualStartOfMonth = startOfMonth ?? firstDayOfMonth;
                DateTime actualEndOfMonth = endOfMonth ?? now;

                DateTime endOfMonthPlusOne = actualEndOfMonth.AddDays(1);

                // Lấy danh sách hóa đơn
                var hoaDon = await _db.HoaDon.Where(n => n.TrangThai == Const.DaThanhToan &&
                                                        n.NgayThanhToan.HasValue &&
                                                        n.NgayThanhToan.Value >= actualStartOfMonth &&
                                                        n.NgayThanhToan.Value < endOfMonthPlusOne &&
                                                        n.MaDoanhNghiep_id == sMaDoanhNghiep)
                                             .ToListAsync();

                // Lấy danh sách các MaHoaDon từ hoaDon
                var maHoaDonList = hoaDon.Select(hd => hd.MaHoaDon).ToList();

                // Lấy danh sách chi tiết hóa đơn liên quan đến các hóa đơn đã lấy
                var chiTietHoaDons = await _db.ChiTietHoaDon
                    .Where(ct => maHoaDonList.Contains(ct.MaHoaDon_id))
                    .ToListAsync();

                // Tính tổng số hóa đơn
                ViewBag.HoaDonCount = hoaDon.Count;

                // Tính tổng số lượng sản phẩm đã mua
                ViewBag.HoaDonSum = chiTietHoaDons.Sum(ct => ct.SoLuongMua);

                // Tính tổng thành tiền
                ViewBag.ThanhTien = chiTietHoaDons.Sum(ct => ct.ThanhTien);

                TempData["ToastMessage"] = "success|Thực hiện thống kê thành công.";
                return View(hoaDon);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Thống kê thất bại.";
                return View();
            }
        }

    }
}