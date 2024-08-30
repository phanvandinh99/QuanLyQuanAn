using QuanLyNhaHang.Models;
using System.Linq;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVienKho.Controllers
{
    public class FilterController : Controller
    {
        DatabaseQuanLyNhaHang db = new DatabaseQuanLyNhaHang();
        #region Lọc Danh sách món ăn
        public ActionResult LocNguyenLieu(int iMaLNL)
        {
            ViewBag.ListNguyenLieu = db.LoaiNguyenLieu.ToList();
            ViewBag.TatCa = db.NguyenLieu.Count();
            var listNguyenLieu = db.NguyenLieu.Where(n=>n.MaLNL_id== iMaLNL);
            return View(listNguyenLieu);
        }
        public ActionResult LocNhapNguyenLieu(int iMaLNL)
        {
            ViewBag.ListNguyenLieu = db.LoaiNguyenLieu.ToList();
            ViewBag.TatCa = db.LoaiNguyenLieu.Count();
            ViewBag.NguyenLieu = db.NguyenLieu.Where(n => n.MaLNL_id == iMaLNL);
            ViewBag.NhanVien = db.NhanVien.Where(n => n.MaQuyen_id == 2).ToList(); //1 nhân viên, 2 kho
            ViewBag.NhaCungCap = db.NhaCC.ToList();
            return View();
        }
        #endregion
    }
}