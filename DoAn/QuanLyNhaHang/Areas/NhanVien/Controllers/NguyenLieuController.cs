using QuanLyNhaHang.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVien.Controllers
{
    public class NguyenLieuController : Controller
    {
        private readonly DatabaseQuanLyNhaHang _db;

        public NguyenLieuController(DatabaseQuanLyNhaHang db)
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
        // Hiển thị danh sách nguyên liệu
        public ActionResult Index()
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            var listNguyenLieu = _db.NguyenLieu.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep)
                                               .ToList()
                                               .OrderBy(n => n.TenNguyenLieu);
            return View(listNguyenLieu);
        }
        //Xem chi tiết
        public ActionResult XemChiTiet(int? iMaNguyenLieu)
        {
            NguyenLieu nguyenLieu = _db.NguyenLieu.Find(iMaNguyenLieu);
            if (nguyenLieu == null)
            {
                return HttpNotFound();
            }
            return View(nguyenLieu);
        }
        // Cập nhật
        public ActionResult CapNhat(int? iMaNguyenLieu)
        {
            NguyenLieu nguyenLieu = _db.NguyenLieu.Find(iMaNguyenLieu);
            return View(nguyenLieu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhat([Bind(Include = "MaNguyenLieu,TenNguyenLieu,SoLuongHienCon,GhiChu,MaLNL_id")] NguyenLieu nguyenLieu)
        {
            if (ModelState.IsValid)
            {
                string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

                nguyenLieu.MaDoanhNghiep_id = sMaDoanhNghiep;
                _db.Entry(nguyenLieu).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Index", "NguyenLieu");
            }
            return View(nguyenLieu);
        }

        // Xóa
        public ActionResult Xoa(int iMaNguyenLieu)
        {
            try
            {
                NguyenLieu nguyenLieu = _db.NguyenLieu.Find(iMaNguyenLieu);
                _db.NguyenLieu.Remove(nguyenLieu);
                _db.SaveChanges();
                return RedirectToAction("Index", "NguyenLieu");
            }
            catch
            {
                return RedirectToAction("KhoaNgoai", "Error");
            }

        }

        // Thêm mới
        public ActionResult ThemMoi()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemMoi([Bind(Include = "MaNguyenLieu,TenNguyenLieu,SoLuongHienCon,GiaNhapCuoi,GhiChu")] NguyenLieu nguyenLieu)
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            if (ModelState.IsValid)
            {
                NguyenLieu nl = new NguyenLieu();
                nl.TenNguyenLieu = nguyenLieu.TenNguyenLieu;
                nl.SoLuongHienCon = nguyenLieu.SoLuongHienCon;
                nl.GiaNhapCuoi = nguyenLieu.GiaNhapCuoi;
                nl.MaDoanhNghiep_id = sMaDoanhNghiep;
                nl.GhiChu = nguyenLieu.GhiChu;
                _db.NguyenLieu.Add(nl);
                _db.SaveChanges();
                return RedirectToAction("Index", "NguyenLieu");
            }

            return View(nguyenLieu);
        }

    }
}