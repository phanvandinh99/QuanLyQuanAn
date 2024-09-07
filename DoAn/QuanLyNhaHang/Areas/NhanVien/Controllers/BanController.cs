using QuanLyNhaHang.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVien.Controllers
{
    public class BanController : Controller
    {
        private readonly DatabaseQuanLyNhaHang _db;

        public BanController(DatabaseQuanLyNhaHang db)
        {
            _db = db;
        }

        private string GetMaDoanhNghiepFromCookie()
        {
            var cookie = Request.Cookies["UserLogin"];
            return cookie?["MaDoangNghiep"];
        }

        public async Task<ActionResult> DanhSachBan()
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            ViewBag.Ban = await _db.Ban.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).CountAsync();
            var listBan = await _db.Ban.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).ToListAsync();
            return View(listBan);
        }

        public async Task<ActionResult> XemChiTiet(int iMaBan)
        {
            try
            {
                var ban = await _db.Ban.FindAsync(iMaBan);
                return View(ban);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Xem chi tiết bàn thất bại.";
                return RedirectToAction("DanhSachBan", "Ban");
            }
        }

        //Thêm bàn
        public async Task<ActionResult> ThemBan()
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            ViewBag.MaTang = await _db.Tang.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).ToListAsync();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ThemBan(Ban Model)
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            try
            {
                if (null == (Model.MaTang_id))
                {
                    TempData["ToastMessage"] = "error|Bạn phải thêm tầng/ khu";
                    return RedirectToAction("ThemTang", "Tang");
                }
                Model.TinhTrang = 0;
                Model.MaDoanhNghiep_id = sMaDoanhNghiep;
                _db.Ban.Add(Model);
                await _db.SaveChangesAsync();

                TempData["ToastMessage"] = "success|Thêm bàn thành công.";

                return RedirectToAction("DanhSachBan", "Ban");
            }
            catch (System.Exception)
            {
                TempData["ToastMessage"] = "error|Thêm bàn thất bại.";
                return RedirectToAction("DanhSachBan", "Ban");
            }
        }

        //Cập Nhật Bàn
        public async Task<ActionResult> CapNhatBan(int iMaBan)
        {
            try
            {
                var ban = await _db.Ban.SingleOrDefaultAsync(n => n.MaBan == iMaBan);
                if (ban == null)
                {
                    TempData["ToastMessage"] = "error|Không tìm thấy bàn.";
                    return RedirectToAction("DanhSachBan", "Ban");
                }

                ViewBag.MaTang_id = new SelectList(await _db.Tang.ToListAsync(), "MaTang", "TenTang", ban.MaTang_id);
                ViewBag.MaTang = await _db.Tang.ToListAsync();
                return View(ban);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Lỗi khi cập nhật bàn.";
                return RedirectToAction("DanhSachBan", "Ban");
            }

        }
        [HttpPost]
        public async Task<ActionResult> CapNhatBan(Ban Model)
        {
            try
            {
                var ban = await _db.Ban.SingleOrDefaultAsync(n => n.MaBan == Model.MaBan);

                if (ban == null)
                {
                    TempData["ToastMessage"] = "error|Không tìm thấy bàn.";
                    return RedirectToAction("DanhSachBan", "Ban");
                }

                ban.TenBan = Model.TenBan;
                ban.SoGhe = Model.SoGhe;
                ban.Vip = Model.Vip;
                ban.TinhTrang = 0;
                ban.MaTang_id = Model.MaTang_id;
                await _db.SaveChangesAsync();

                TempData["ToastMessage"] = "success|Cập nhật bàn thành công.";
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Có lỗi xãy ra khi cập nhật bàn.";
            }

            return RedirectToAction("DanhSachBan", "Ban");
        }

        public async Task<ActionResult> XoaBan(int iMaBan)
        {
            try
            {
                _db.Ban.Remove(await _db.Ban.FindAsync(iMaBan));
                await _db.SaveChangesAsync();

                TempData["ToastMessage"] = "success|Xóa bàn thành công.";

                return RedirectToAction("DanhSachBan", "Ban");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Xóa bàn thất bại.";
                return RedirectToAction("XoaBan", "Error");
            }
        }
    }
}