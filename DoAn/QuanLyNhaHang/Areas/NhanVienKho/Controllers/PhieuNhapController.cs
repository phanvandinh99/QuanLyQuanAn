using QuanLyNhaHang.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVienKho.Controllers
{
    public class PhieuNhapController : Controller
    {
        DatabaseQuanLyNhaHang db = new DatabaseQuanLyNhaHang();
        // danh sách phiếu nhập
        public ActionResult DanhSachPhieuNhap()
        {
            var listDanhSachPhieuNhap = db.PhieuNhap.OrderByDescending(n => n.MaPhieuNhap).ToList();
            return View(listDanhSachPhieuNhap);
        }
        // hiển thị danh sách chi tiết sản phẩm
        public ActionResult ChiTietNhap(int iMaPhieuNhap)
        {
            var PhieuNhap = db.PhieuNhap.SingleOrDefault(n => n.MaPhieuNhap == iMaPhieuNhap);
            ViewBag.ChiTiet = db.ChiTietPhieuNhap.Where(n => n.MaPhieuNhap_id == iMaPhieuNhap).ToList();
            return View(PhieuNhap);
        }
        public ActionResult XoaPhieuNhap(int iMaPhieuNhap)
        {
            var phieuNhap = db.PhieuNhap.SingleOrDefault(n => n.MaPhieuNhap == iMaPhieuNhap); // Lấy được mã phiếu nhập
            var listNguyenLieuNhap = db.ChiTietPhieuNhap.Where(n => n.MaPhieuNhap_id == iMaPhieuNhap).ToList();
            foreach (var item in listNguyenLieuNhap)
            {
                db.ChiTietPhieuNhap.Remove(item);
            }
            db.PhieuNhap.Remove(phieuNhap);
            db.SaveChanges();
            return RedirectToAction("DanhSachPhieuNhap", "PhieuNhap");
        }
        // Cập nhật chi tiết phiếu nhập
        [HttpGet]
        public ActionResult CapNhat(int iMaPhieuNhap)
        {
            var PhieuNhap = db.PhieuNhap.SingleOrDefault(n => n.MaPhieuNhap == iMaPhieuNhap);
            ViewBag.ChiTiet = db.ChiTietPhieuNhap.Where(n => n.MaPhieuNhap_id == iMaPhieuNhap).ToList();
            return View(PhieuNhap);
        }
        [HttpPost]
        public ActionResult CapNhat(int iMaPhieuNhap, int iMaNguyenLieu, string strURL, FormCollection f)
        {
            float soLuongNhap = float.Parse(f["txtSoLuongNhap"].ToString());
            float GiaNhap = float.Parse(f["txtGiaNhap"].ToString());

            // lấy nguyên liệu nhập tương ứng
            var chiTietNhap = db.ChiTietPhieuNhap.SingleOrDefault(n => n.MaPhieuNhap_id == iMaPhieuNhap && n.MaNguyenLieu_id == iMaNguyenLieu);
            // truy xuất nguyên liệu
            var nguyenLieu = db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == iMaNguyenLieu);
            if (soLuongNhap < chiTietNhap.SoLuongNhap) // giảm số lượng nhập xuống => trừ trong số lượng hiện còn
            {
                if (nguyenLieu.MaLNL_id == 4 || nguyenLieu.MaNguyenLieu == 1) // thức uống hoặc khăn lạnh
                {
                    if (soLuongNhap == (int)soLuongNhap) // kiểm tra có nguyên hay 
                    {
                        #region giảm bớt số lượng hiện còn nguyên liệu
                        nguyenLieu.SoLuongHienCon = nguyenLieu.SoLuongHienCon - (chiTietNhap.SoLuongNhap - soLuongNhap);
                        nguyenLieu.GiaNhapCuoi = GiaNhap;
                        #endregion
                        chiTietNhap.SoLuongNhap = soLuongNhap;
                        chiTietNhap.GiaNhap = GiaNhap;
                        db.SaveChanges();
                    }
                    else // thức uống cập nhật lẽ 1,5 2.5
                    {
                        // không làm gì cả
                    }
                }
                else // còn lại có thể nhập lẽ
                {
                    #region giảm bớt số lượng hiện còn nguyên liệu
                    nguyenLieu.SoLuongHienCon = nguyenLieu.SoLuongHienCon - (chiTietNhap.SoLuongNhap - soLuongNhap);
                    nguyenLieu.GiaNhapCuoi = GiaNhap;
                    #endregion
                    chiTietNhap.SoLuongNhap = soLuongNhap;
                    chiTietNhap.GiaNhap = GiaNhap;
                    db.SaveChanges();
                }
            }
            else if (soLuongNhap > chiTietNhap.SoLuongNhap) //cập nhật số lượng nhập tăng lên so với ban đầu
            {
                if (nguyenLieu.MaLNL_id == 4 || nguyenLieu.MaNguyenLieu == 1) // thức uống hoặc khăn lạnh
                {
                    if (soLuongNhap == (int)soLuongNhap) // kiểm tra có nguyên hay 
                    {
                        #region giảm bớt số lượng hiện còn nguyên liệu
                        nguyenLieu.SoLuongHienCon = nguyenLieu.SoLuongHienCon + (soLuongNhap - chiTietNhap.SoLuongNhap);
                        nguyenLieu.GiaNhapCuoi = GiaNhap;
                        #endregion
                        chiTietNhap.SoLuongNhap = soLuongNhap;
                        chiTietNhap.GiaNhap = GiaNhap;
                        db.SaveChanges();
                    }
                    else // thức uống cập nhật lẽ 1,5 2.5
                    {
                        // không làm gì cả
                    }
                }
                else // còn lại có thể nhập lẽ
                {
                    #region giảm bớt số lượng hiện còn nguyên liệu
                    nguyenLieu.SoLuongHienCon = nguyenLieu.SoLuongHienCon + (soLuongNhap - chiTietNhap.SoLuongNhap);
                    nguyenLieu.GiaNhapCuoi = GiaNhap;
                    #endregion
                    chiTietNhap.SoLuongNhap = soLuongNhap;
                    chiTietNhap.GiaNhap = GiaNhap;
                    db.SaveChanges();
                }
            }
            else // giữ nguyên số lượng nhập thay đổi giá tiền
            {
                #region giảm bớt số lượng hiện còn nguyên liệu
                nguyenLieu.GiaNhapCuoi = GiaNhap;
                #endregion
                chiTietNhap.GiaNhap = GiaNhap;
                db.SaveChanges();
            }
            return Redirect(strURL);
        }
        public ActionResult XoaNguyenLieuNhap(int iMaNguyenLieuNhap, int iMaPhieuNhap)
        {
            var nguyenLieuNhap = db.ChiTietPhieuNhap.SingleOrDefault(n => n.MaNguyenLieu_id == iMaNguyenLieuNhap & n.MaPhieuNhap_id == iMaNguyenLieuNhap);
            #region Xóa số lượng hiện còn

            #endregion
            db.ChiTietPhieuNhap.Remove(nguyenLieuNhap);
            var PhieuNhap = db.PhieuNhap.SingleOrDefault(n => n.MaPhieuNhap == iMaPhieuNhap);
            ViewBag.ChiTiet = db.ChiTietPhieuNhap.Where(n => n.MaPhieuNhap_id == iMaPhieuNhap).ToList();
            return View(PhieuNhap);
        }
    }
}