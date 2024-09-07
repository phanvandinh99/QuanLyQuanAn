using QuanLyNhaHang.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVien.Controllers
{
    public class LoaiMonAnController : Controller
    {
        private readonly DatabaseQuanLyNhaHang _db;

        public LoaiMonAnController(DatabaseQuanLyNhaHang db)
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

        // Danh sách loại món ăn
        public async Task<ActionResult> Index()
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            var list = await _db.LoaiMonAn.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).ToListAsync();
            return View(list);
        }

        public async Task<ActionResult> XemChiTiet(int iMaLMA)
        {
            try
            {
                var loaiMonAn = await _db.LoaiMonAn.SingleOrDefaultAsync(n => n.MaLMA == iMaLMA);
                return View(loaiMonAn);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Xem chi tiết loại món ăn thất bại.";
                return RedirectToAction("Index", "LoaiMonAn");
            }
        }

        //Thêm loại món ăn
        public ActionResult ThemMoi()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ThemMoi(LoaiMonAn Model)
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            try
            {
                Model.MaDoanhNghiep_id = sMaDoanhNghiep;
                Model.TongSoLuong = 0;
                _db.LoaiMonAn.Add(Model);
                await _db.SaveChangesAsync();

                TempData["ToastMessage"] = "success|Thêm loại món ăn thành công.";

                return RedirectToAction("Index", "LoaiMonAn");
            }
            catch (System.Exception)
            {
                TempData["ToastMessage"] = "error|Thêm loại món ăn thất bại.";
                return RedirectToAction("Index", "LoaiMonAn");

            }
        }

        //Cập Nhật Bàn
        public async Task<ActionResult> CapNhat(int iMaLMA)
        {
            try
            {
                var loaiMonAn = await _db.LoaiMonAn.SingleOrDefaultAsync(n => n.MaLMA == iMaLMA);
                if (loaiMonAn == null)
                {
                    TempData["ToastMessage"] = "error|Không tìm thấy loại món ăn.";
                    return RedirectToAction("Index", "LoaiMonAn");
                }
                return View(loaiMonAn);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Lỗi khi cập nhật loain món ăn.";
                return RedirectToAction("Index", "LoaiMonAn");
            }

        }
        [HttpPost]
        public async Task<ActionResult> CapNhat(LoaiMonAn Model)
        {
            try
            {
                var loaiMonAn = await _db.LoaiMonAn.SingleOrDefaultAsync(n => n.MaLMA == Model.MaLMA);
                if (loaiMonAn == null)
                {
                    TempData["ToastMessage"] = "error|Không tìm thấy loại món ăn.";
                    return RedirectToAction("Index", "LoaiMonAn");
                }

                loaiMonAn.TenLMA = Model.TenLMA;
                await _db.SaveChangesAsync();

                TempData["ToastMessage"] = "success|Cập nhật loại món ăn thành công.";
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Lỗi khi cập nhật loại món ăn.";
            }

            return RedirectToAction("Index", "LoaiMonAn");
        }

        public async Task<ActionResult> Xoa(int iMaLMA)
        {
            try
            {
                _db.LoaiMonAn.Remove(await _db.LoaiMonAn.FindAsync(iMaLMA));
                await _db.SaveChangesAsync();

                TempData["ToastMessage"] = "success|Xóa loại món ăn thành công.";

                return RedirectToAction("Index", "LoaiMonAn");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Xóa loại món ăn thất bại vì ràng buộc khóa";
                return RedirectToAction("Index", "LoaiMonAn");
            }
        }
    }
}