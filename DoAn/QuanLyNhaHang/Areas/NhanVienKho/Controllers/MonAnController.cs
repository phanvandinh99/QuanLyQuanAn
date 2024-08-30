using QuanLyNhaHang.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVienKho.Controllers
{
    public class MonAnController : Controller
    {
        DatabaseQuanLyNhaHang db = new DatabaseQuanLyNhaHang();
        // Danh sách món ăn
        public ActionResult DanhSachMonAn()
        {
            var listMonAn = db.MonAn.OrderBy(n => n.MaMonAn).ToList();
            return View(listMonAn);
        }
        public ActionResult XemChiTiet(int iMaMonAn)
        {
            var monAn = db.MonAn.Find(iMaMonAn);
            // lấy m ón ăn cùng loại
            var monAnCungLoai = db.MonAn.Where(n => n.MaLMA_id == monAn.MaLMA_id).ToList().Take(5);
            ViewBag.MonAnCungLoai = monAnCungLoai;
            // lấy chi tiết món ăn
            ViewBag.ChiTietMonAn = db.ChiTietSanPham.Where(n => n.MaMonAn_id == iMaMonAn).ToList();
            return View(monAn);
        }
        [HttpGet]
        public ActionResult ThemMonAn()
        {
            ViewBag.NhomMonAn = db.NhomMonAn.ToList();
            ViewBag.LoaiMonAn = db.LoaiMonAn.ToList();
            ViewBag.NguyenLieu = db.NguyenLieu.ToList();
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ThemMonAn(HttpPostedFileBase HinhAnh,MonAn Model, IEnumerable<ChiTietSanPham> listCTSP)
        {
            //try
            //{
            //ViewBag.NhomMonAn = db.NhomMonAn.ToList();
            //ViewBag.LoaiMonAn = db.NhomMonAn.ToList();
            //ViewBag.NguyenLieu = db.NguyenLieu.ToList();
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
            monAn.MaNMA_id = Model.MaNMA_id;
            monAn.MaLMA_id = Model.MaLMA_id;
            db.MonAn.Add(monAn);
            db.SaveChanges();
            #endregion
            //Lấy id mã món ăn
            var maMonAn = db.MonAn.OrderByDescending(n => n.MaMonAn).FirstOrDefault();
            #region Thêm món ăn vào chi tiết sản phẩm
            if (listCTSP != null)
            {
                foreach (var item in listCTSP) // chi tiết phiếu nhật
                {
                    ChiTietSanPham ctsp = new ChiTietSanPham();
                    //kiểm tra chi tiết sản phẩm đã tồn tại chưa
                    var chiTietSanPham = db.ChiTietSanPham.SingleOrDefault(n => n.MaMonAn_id == maMonAn.MaMonAn && n.MaNguyenLieu_id == item.MaNguyenLieu_id);
                    if (chiTietSanPham != null)
                    {
                        chiTietSanPham.SoLuongDung += item.SoLuongDung;
                        chiTietSanPham.Tru = item.Tru;
                        db.SaveChanges();
                    }
                    else
                    {

                        ctsp.MaMonAn_id = maMonAn.MaMonAn;
                        ctsp.MaNguyenLieu_id = item.MaNguyenLieu_id;
                        ctsp.SoLuongDung = item.SoLuongDung;
                        ctsp.Tru = item.Tru;
                        db.ChiTietSanPham.Add(ctsp);
                        db.SaveChanges();
                    }
                }
            }
            #endregion
            return RedirectToAction("HaiSan","NguyenLieu");
        }
        //catch
        //{
        //    return RedirectToAction("TrungSanPhamKhiNhap", "Error");
        //}

    }
}
