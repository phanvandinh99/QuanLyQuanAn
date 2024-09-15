using QuanLyNhaHang.Models;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using QuanLyNhaHang.Common.Const;

namespace QuanLyNhaHang.Areas.NhanVien.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly DatabaseQuanLyNhaHang _db;

        public NhanVienController(DatabaseQuanLyNhaHang db)
        {
            _db = db;
        }

        private string GetMaDoanhNghiepFromCookie()
        {
            var cookie = Request.Cookies["UserLogin"];
            return cookie?["MaDoanhNghiep"];
        }

        public async Task<ActionResult> Index()
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            var list = await _db.NhanVien.Where(n => n.MaQuyen_id != Const.SupperAdmin &&
                                                n.MaQuyen_id != Const.Admin &&
                                                n.MaDoanhNghiep_id == sMaDoanhNghiep
                                                )
                                        .OrderBy(n => n.MaQuyen_id)
                                        .ToListAsync();
            return View(list);
        }

        // Thêm bàn
        public async Task<ActionResult> ThemMoi()
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            var tangs = await _db.Tang.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).ToListAsync();
            if (tangs == null || !tangs.Any())
            {
                TempData["ToastMessage"] = "error|Bạn phải thêm tầng/ khu";
                return RedirectToAction("ThemTang", "Tang");
            }

            ViewBag.MaTang = tangs;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ThemMoi(Ban Model)
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            try
            {
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





        public ActionResult DSNhanVienKho()
        {
            var list = _db.NhanVien.Where(n => n.MaQuyen_id == 2).OrderBy(n => n.MaQuyen_id).ToList();
            return View(list);
        }
        public ActionResult ThemNhanVien()
        {
            ViewBag.Quyen = _db.Quyen.ToList();
            return View();
        }
        [HttpPost]
        public ActionResult ThemNhanVien(QuanLyNhaHang.Models.NhanVien Model)
        {
            _db.NhanVien.Add(Model);
            _db.SaveChanges();
            return RedirectToAction("DSNhanVien", "NhanVien");
        }
        public ActionResult Xoa(string sTaiKhoanid)
        {
            var nhanVien = _db.NhanVien.Find(sTaiKhoanid);
            _db.NhanVien.Remove(nhanVien);
            _db.SaveChanges();
            return RedirectToAction("DSNhanVien", "NhanVien");
        }
        public ActionResult XemChiTiet(string sTaiKhoanid)
        {
            var nhanVien = _db.NhanVien.Find(sTaiKhoanid);
            return View(nhanVien);
        }
        public ActionResult CapNhat(string sTaiKhoanid)
        {
            QuanLyNhaHang.Models.NhanVien nhanVien = _db.NhanVien.Find(sTaiKhoanid);
            ViewBag.MaQuyen_id = new SelectList(_db.Quyen, "MaQuyen", "TenQuyen", nhanVien.MaQuyen_id);
            return View(nhanVien);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhat([Bind(Include = "TaiKhoanNV,MatKhauNV,TenNhanVien,NgaySinh,SoDienThoai,MaQuyen_id")] QuanLyNhaHang.Models.NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(nhanVien).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("DSNhanVien", "NhanVien");
            }
            ViewBag.MaQuyen_id = new SelectList(_db.Quyen, "MaQuyen", "TenQuyen", nhanVien.MaQuyen_id);
            return View();
        }
    }
}