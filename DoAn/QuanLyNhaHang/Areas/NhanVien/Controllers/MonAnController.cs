using QuanLyNhaHang.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public ActionResult DanhSachMonAn()
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            ViewBag.TatCa = _db.MonAn.Where(n=>n.MaDoanhNghiep_id == sMaDoanhNghiep).Count();
            ViewBag.LoaiMonAn = _db.LoaiMonAn.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).ToList();
            var listMonAn = _db.MonAn.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).OrderBy(n => n.MaMonAn).ToList();
            return View(listMonAn);
        }
        public ActionResult DanhSachMonAnList()
        {
            ViewBag.TatCa = _db.MonAn.Count();
            ViewBag.LoaiMonAn = _db.LoaiMonAn.ToList();
            var listMonAn = _db.MonAn.OrderBy(n => n.MaMonAn).ToList();
            return View(listMonAn);
        }
        public ActionResult DanhSachMonAnBanChay()
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            ViewBag.TatCa = _db.MonAn.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).Count();
            ViewBag.LoaiMonAn = _db.LoaiMonAn.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).ToList();
            var listMonAn = _db.MonAn.Where(n => n.MaDoanhNghiep_id == sMaDoanhNghiep).OrderBy(n => n.MaMonAn).ToList().OrderBy(n => n.SoLuongDaBan).ToList().Take(10);
            return View(listMonAn);
        }

        public ActionResult XoaMonAn(int iMaMonAn)
        {
            try
            {
                var chiTietMonAn = _db.ChiTietSanPham.Where(n => n.MaMonAn_id == iMaMonAn).ToList();
                // xóa chi tiết món ăn
                foreach (var item in chiTietMonAn)
                {
                    _db.ChiTietSanPham.Remove(item);
                }
                // xóa Món Ăn
                var monAn = _db.MonAn.Find(iMaMonAn);
                _db.MonAn.Remove(monAn);
                _db.SaveChanges();
                return RedirectToAction("XoaMonAnThanhCong", "Error");
            }
            catch
            {
                return RedirectToAction("XoaMonAn", "Error");
            }
        }

        public ActionResult XemChiTiet(int iMaMonAn)
        {
            var monAn = _db.MonAn.Find(iMaMonAn);
            // lấy m ón ăn cùng loại
            var monAnCungLoai = _db.MonAn.Where(n => n.MaLMA_id == monAn.MaLMA_id).ToList().Take(5);
            ViewBag.MonAnCungLoai = monAnCungLoai;
            // lấy chi tiết món ăn
            var chiTietMonAn = _db.ChiTietSanPham.Where(n => n.MaMonAn_id == iMaMonAn).ToList().OrderByDescending(n => n.Tru);
            ViewBag.ChiTietMonAn = chiTietMonAn;
            return View(monAn);
        }
        public ActionResult ThemMonAn()
        {
            ViewBag.LoaiMonAn = _db.LoaiMonAn.ToList();
            var loaiMonAn = _db.LoaiMonAn.ToList().Count();
            if (loaiMonAn == 0)
            {
                TempData["ToastMessage"] = "error|Bạn phải thêm loại món ăn để tạo món ăn.";
                return RedirectToAction("ThemMoi", "LoaiMonAn");

            }

            var nguyenLieu = _db.NguyenLieu.ToList();
            if (nguyenLieu.Count() == 0)
            {
                TempData["ToastMessage"] = "error|Bạn phải thêm nguyên liệu để tạo món ăn.";
                return RedirectToAction("DanhSachMonAn", "MonAn");
            }

            ViewBag.NguyenLieu = nguyenLieu;
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ThemMonAn(HttpPostedFileBase HinhAnh, MonAn Model, IEnumerable<ChiTietSanPham> listCTSP)
        {
            try
            {
                string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

                ViewBag.LoaiMonAn = _db.LoaiMonAn.ToList();
                ViewBag.NguyenLieu = _db.NguyenLieu.ToList();
                #region Lưu hình ảnh vào thư mục
                if (HinhAnh.ContentLength > 0)
                {
                    var tenAnh = Path.GetFileName(HinhAnh.FileName);
                    var duongDan = Path.Combine(Server.MapPath("~/Assets/img/AnhMonAn"), tenAnh);
                    if (System.IO.File.Exists(duongDan))
                    {
                        ViewBag.upload = "Hình ảnh đã tồn tại";
                    }
                    else
                    {
                        HinhAnh.SaveAs(duongDan);
                    }
                }
                #endregion

                #region Thêm Món Ăn Vào DATA
                MonAn monAn = new MonAn();
                monAn.TenMonAn = Model.TenMonAn;
                monAn.HinhAnh = HinhAnh.FileName;
                monAn.DonGia = Model.DonGia;
                monAn.NgayCapNhat = DateTime.Now;
                monAn.ThongTin = Model.ThongTin;
                monAn.MoTa = Model.MoTa;
                monAn.SoLuongDaBan = 0;
                monAn.MaLMA_id = Model.MaLMA_id;
                monAn.MaDoanhNghiep_id = sMaDoanhNghiep;
                _db.MonAn.Add(monAn);
                _db.SaveChanges();
                #endregion
                //Lấy id mã món ăn
                var maMonAn = _db.MonAn.OrderByDescending(n => n.MaMonAn).FirstOrDefault();
                #region Thêm món ăn vào chi tiết sản phẩm
                if (listCTSP != null)
                {
                    foreach (var item in listCTSP) // chi tiết phiếu nhật
                    {
                        ChiTietSanPham ctsp = new ChiTietSanPham();
                        //kiểm tra chi tiết sản phẩm đã tồn tại chưa
                        var chiTietSanPham = _db.ChiTietSanPham.SingleOrDefault(n => n.MaMonAn_id == maMonAn.MaMonAn && n.MaNguyenLieu_id == item.MaNguyenLieu_id);
                        if (chiTietSanPham != null)
                        {
                            chiTietSanPham.SoLuongDung += item.SoLuongDung;
                            chiTietSanPham.Tru = 0;
                            _db.SaveChanges();
                        }
                        else
                        {
                            ctsp.MaMonAn_id = maMonAn.MaMonAn;
                            ctsp.MaNguyenLieu_id = item.MaNguyenLieu_id;
                            ctsp.SoLuongDung = item.SoLuongDung;
                            ctsp.Tru = 0;
                            ctsp.MaDoanhNghiep_id = sMaDoanhNghiep;
                            _db.ChiTietSanPham.Add(ctsp);
                            _db.SaveChanges();
                        }
                    }
                }
                #endregion
                return RedirectToAction("DanhSachMonAn", "MonAn");
            }
            catch
            {
                return RedirectToAction("MonAn", "Error");
            }
        }
    }
}