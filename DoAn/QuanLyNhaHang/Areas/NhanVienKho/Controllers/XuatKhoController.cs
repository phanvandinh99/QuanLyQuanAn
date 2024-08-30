using QuanLyNhaHang.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVienKho.Controllers
{
    public class XuatKhoController : Controller
    {
        DatabaseQuanLyNhaHang db = new DatabaseQuanLyNhaHang();
        // Danh sách phiếu xuất
        public ActionResult DanhSachPhieuXuatKho()
        {
            var listPhieuXuat = db.XuatKho.ToList().OrderByDescending(n => n.MaXuatKho);
            return View(listPhieuXuat);
        }
        // Xem chi tiết phiếu xuất
        public ActionResult ChiTiet(int iMaXuatKho)
        {
            ViewBag.XuatKho = db.XuatKho.SingleOrDefault(n => n.MaXuatKho == iMaXuatKho);
            var chiTiet = db.NguyenLieuXuat.Where(n => n.MaXuatKho_id == iMaXuatKho).ToList();
            return View(chiTiet);
        }
        // Xóa phiếu xuất
        public ActionResult XoaPhieuXuat(int iMaXuatKho)
        {
            var listNguyenLieuXuat = db.NguyenLieuXuat.Where(n => n.MaXuatKho_id == iMaXuatKho);
            if (listNguyenLieuXuat != null)
            {
                foreach (var item in listNguyenLieuXuat)
                {
                    db.NguyenLieuXuat.Remove(item);
                }
            }
            var phieuXuat = db.XuatKho.SingleOrDefault(n => n.MaXuatKho == iMaXuatKho);
            db.XuatKho.Remove(phieuXuat);
            db.SaveChanges();
            return RedirectToAction("DanhSachPhieuXuatKho", "XuatKho");
        }

        #region Thêm mới xuất kho
        [HttpGet]
        public ActionResult PhieuXuatKho()
        {
            ViewBag.NguyenLieu = db.NguyenLieu.ToList();
            return View();
        }
        [HttpPost]
        public ActionResult PhieuXuatKho(XuatKho Model, IEnumerable<NguyenLieuXuat> lstModel)
        {
            #region Lưu xuất kho
            XuatKho xk = new XuatKho();
            if (Model.NgayXuat == null)
            {
                xk.NgayXuat = DateTime.Now;
            }
            else
            {
                xk.NgayXuat = Model.NgayXuat;
            }
            db.XuatKho.Add(xk);
            #endregion
            // lấy mã xuất kho
            //var maXuatKho = db.XuatKho.OrderByDescending(n => n.MaXuatKho).FirstOrDefault();

            foreach (var item in lstModel) // chi tiết phiếu xuất
            {

                // tìm mã nguyên liệu
                var nguyenLieu = db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
                if (nguyenLieu.MaLNL_id == 4) // mã nguyên thức uống == 4
                {
                    int SL;
                    if (int.TryParse(item.SoLuongXuat.ToString(), out SL))
                    {
                        NguyenLieuXuat nlx = new NguyenLieuXuat();
                        nlx.MaXuatKho_id = Model.MaXuatKho;
                        nlx.MaNguyenLieu_id = item.MaNguyenLieu_id;
                        nlx.SoLuongXuat = item.SoLuongXuat;
                        db.NguyenLieuXuat.Add(nlx);

                        #region Trừ bớt số lượng trong kho
                        var slNguyenLieu = db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
                        if (slNguyenLieu.SoLuongHienCon >= item.SoLuongXuat)
                        {
                            slNguyenLieu.SoLuongHienCon = slNguyenLieu.SoLuongHienCon - item.SoLuongXuat;
                        }
                        else
                        {
                            // Thoát và k lưu
                            //db.XuatKho.Remove(maXuatKho);
                            return RedirectToAction("KhoKhongDapUng", "Error");
                        }
                        #endregion
                    }
                    else // số lượng thức uống lẽ ví dụ: 1,5 chai, 2,5 chai
                    {
                        // Không in cái đó ra
                    }
                }
                else
                {
                    NguyenLieuXuat nlx = new NguyenLieuXuat();
                    nlx.MaXuatKho_id = Model.MaXuatKho;
                    nlx.MaNguyenLieu_id = item.MaNguyenLieu_id;
                    nlx.SoLuongXuat = item.SoLuongXuat;
                    db.NguyenLieuXuat.Add(nlx);

                    #region Trừ bớt số lượng trong kho
                    var slNguyenLieu = db.NguyenLieu.SingleOrDefault(n => n.MaNguyenLieu == item.MaNguyenLieu_id);
                    if (slNguyenLieu.SoLuongHienCon >= item.SoLuongXuat)
                    {
                        slNguyenLieu.SoLuongHienCon = slNguyenLieu.SoLuongHienCon - item.SoLuongXuat;
                    }
                    else
                    {
                        // Thoát và k lưu
                        //db.XuatKho.Remove(maXuatKho);
                        return RedirectToAction("KhoKhongDapUng", "Error");
                    }

                }
                #endregion
            }
            db.SaveChanges();
            return RedirectToAction("DanhSachPhieuXuatKho", "XuatKho");
        }
        #endregion
    }
}