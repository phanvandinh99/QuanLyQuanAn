using QuanLyNhaHang.Models;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVien.Controllers
{
    public class MonAnController : Controller
    {
        private readonly DatabaseQuanLyNhaHang _db;

        public MonAnController(DatabaseQuanLyNhaHang db)
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

        // hiển thị danh sách các món ăn
        public async Task<ActionResult> DanhSachMonAn()
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            ViewBag.TatCa = await _db.MonAn.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).CountAsync();
            ViewBag.LoaiMonAn = await _db.LoaiMonAn.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).ToListAsync();
            var listMonAn = await _db.MonAn.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).OrderBy(n => n.MaMonAn).ToListAsync();
            return View(listMonAn);
        }
        public async Task<ActionResult> DanhSachMonAnList()
        {
            ViewBag.TatCa = await _db.MonAn.CountAsync();
            ViewBag.LoaiMonAn = await _db.LoaiMonAn.ToListAsync();
            var listMonAn = await _db.MonAn.OrderBy(n => n.MaMonAn).ToListAsync();
            return View(listMonAn);
        }
        public async Task<ActionResult> DanhSachMonAnBanChay()
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            ViewBag.TatCa = await _db.MonAn.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).CountAsync();
            ViewBag.LoaiMonAn = await _db.LoaiMonAn.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).ToListAsync();
            var listMonAn = await _db.MonAn.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep)
                                           .OrderByDescending(n => n.SoLuongDaBan)
                                           .Take(10)
                                           .ToListAsync();

            return View(listMonAn);
        }

        public async Task<ActionResult> XoaMonAn(int iMaMonAn)
        {
            try
            {
                var monAn = await _db.MonAn.FindAsync(iMaMonAn);
                _db.MonAn.Remove(monAn);
                await _db.SaveChangesAsync();
                TempData["ToastMessage"] = "success|Xóa món ăn thành công";

                return RedirectToAction("DanhSachMonAn", "MonAn");
            }
            catch
            {
                TempData["ToastMessage"] = "error|Lỗi khi xóa món ăn do ràng buộc khóa";
                return RedirectToAction("DanhSachMonAn", "MonAn");
            }
        }

        public async Task<ActionResult> XemChiTiet(int iMaMonAn)
        {
            try
            {
                var monAn = await _db.MonAn.FindAsync(iMaMonAn);
                // lấy món ăn cùng loại
                var monAnCungLoai = await _db.MonAn.Where(n => n.MaLMA_id == monAn.MaLMA_id).Take(5).ToListAsync();
                ViewBag.MonAnCungLoai = monAnCungLoai;
                return View(monAn);
            }
            catch (Exception ex)
            {
                return RedirectToAction("DanhSachMonAn", "MonAn");
            }

        }

        public async Task<ActionResult> ThemMonAn()
        {
            ViewBag.LoaiMonAn = await _db.LoaiMonAn.ToListAsync();
            var loaiMonAn = await _db.LoaiMonAn.CountAsync();
            if (loaiMonAn == 0)
            {
                TempData["ToastMessage"] = "error|Bạn phải thêm loại món ăn để tạo món ăn.";
                return RedirectToAction("ThemMoi", "LoaiMonAn");

            }

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> ThemMonAn(HttpPostedFileBase HinhAnh, MonAn Model)
        {
            try
            {
                string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

                // Set default image
                string tenAnh = "AnhMacDinh.jpg";

                // Handle image upload if present
                if (HinhAnh?.ContentLength > 0)
                {
                    tenAnh = Path.GetFileName(HinhAnh.FileName);
                    var duongDan = Path.Combine(Server.MapPath("~/Assets/img/AnhMonAn"), tenAnh);

                    // Save the image only if it doesn't exist
                    if (!System.IO.File.Exists(duongDan))
                    {
                        HinhAnh.SaveAs(duongDan);
                    }
                    else
                    {
                        ViewBag.upload = "Hình ảnh đã tồn tại";
                    }
                }

                // Create new MonAn object and populate properties
                MonAn monAn = new MonAn
                {
                    TenMonAn = Model.TenMonAn,
                    HinhAnh = tenAnh, // Use the image name (uploaded or default)
                    DonGia = Model.DonGia,
                    NgayCapNhat = DateTime.Now,
                    ThongTin = Model.ThongTin,
                    MoTa = Model.MoTa,
                    SoLuongDaBan = 0,
                    MaLMA_id = Model.MaLMA_id,
                    MaDoanhNghiep_id = sMaDoanhNghiep
                };

                // Thực hiện số lượng loại món ăn
                var loaiMonAn = await _db.LoaiMonAn.FindAsync(monAn.MaLMA_id);
                loaiMonAn.TongSoLuong++;

                // Add to database
                _db.MonAn.Add(monAn);
                await _db.SaveChangesAsync();

                ViewBag.LoaiMonAn = await _db.LoaiMonAn.ToListAsync();

                TempData["ToastMessage"] = "success|Thêm món ăn thành công.";
                return RedirectToAction("DanhSachMonAn", "MonAn");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Thêm món ăn thất bại.";

                return RedirectToAction("MonAn", "Error");
            }
        }


    }
}