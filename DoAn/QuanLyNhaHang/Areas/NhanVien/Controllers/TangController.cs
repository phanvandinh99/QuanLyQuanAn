using QuanLyNhaHang.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVien.Controllers
{
    public class TangController : Controller
    {
        private readonly DatabaseQuanLyNhaHang _db;

        public TangController(DatabaseQuanLyNhaHang db)
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

        public async Task<ActionResult> DanhSachTang()
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            ViewBag.Tang = await _db.Tang.Where(n=>n.MaDoanhNghiep_id==sMaDoanhNghiep).CountAsync();
            var list = await _db.Tang.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).ToListAsync();
            return View(list);
        }

        public async Task<ActionResult> XemChiTiet(int iMaTang)
        {
            try
            {
                var tang = await _db.Tang.SingleOrDefaultAsync(n => n.MaTang == iMaTang);
                // Tổng số bàn
                ViewBag.Tang = await _db.Ban.Where(n => n.MaTang_id == iMaTang).CountAsync();
                return View(tang);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Xem chi tiết tầng thất bại.";
                return RedirectToAction("DanhSachTang", "Ban");
            }
        }

        //Thêm bàn
        public ActionResult ThemTang()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ThemTang(Tang Model)
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            try
            {
                Model.MaDoanhNghiep_id = sMaDoanhNghiep;
                _db.Tang.Add(Model);
                await _db.SaveChangesAsync();

                TempData["ToastMessage"] = "success|Thêm tầng thành công.";

                return RedirectToAction("DanhSachTang", "Tang");
            }
            catch (System.Exception)
            {
                TempData["ToastMessage"] = "error|Thêm tầng thất bại.";
                return RedirectToAction("DanhSachTang", "Tang");
            }
        }

        //Cập Nhật Bàn
        public async Task<ActionResult> CapNhatTang(int iMaTang)
        {
            try
            {
                var tang = await _db.Tang.SingleOrDefaultAsync(n => n.MaTang == iMaTang);
                if (tang == null)
                {
                    TempData["ToastMessage"] = "error|Không tìm thấy tầng.";
                    return RedirectToAction("DanhSachTang", "Tang");
                }
                return View(tang);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Lỗi khi cập nhật tầng.";
                return RedirectToAction("DanhSachTang", "Tang");
            }

        }
        [HttpPost]
        public async Task<ActionResult> CapNhatTang(Tang Model)
        {
            try
            {
                var tang = await _db.Tang.SingleOrDefaultAsync(n => n.MaTang == Model.MaTang);
                if (tang == null)
                {
                    TempData["ToastMessage"] = "error|Không tìm thấy tầng.";
                    return RedirectToAction("DanhSachTang", "Tang");
                }

                tang.TenTang = Model.TenTang;
                await _db.SaveChangesAsync();

                TempData["ToastMessage"] = "success|Cập nhật tầng thành công.";
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Lỗi khi cập nhật tầng.";
            }

            return RedirectToAction("DanhSachTang", "Tang");
        }

        public async Task<ActionResult> XoaTang(int iMaTang)
        {
            try
            {
                _db.Tang.Remove(await _db.Tang.FindAsync(iMaTang));
                await _db.SaveChangesAsync();

                TempData["ToastMessage"] = "success|Xóa tầng thành công.";

                return RedirectToAction("DanhSachTang", "Tang");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Xóa tầng thất bại vì ràng buộc khóa";
                return RedirectToAction("DanhSachTang", "Tang");
            }
        }
    }
}