using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVien.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult KhoKhongDapUng()
        {
            return View();
        }
        public ActionResult XoaHoaDon()
        {
            return View();
        }
        // Lỗi k thêm đc món ăn
        public ActionResult MonAn()
        {
            return View();
        }
        // Lỗi k xóa được món ăn
        public ActionResult XoaMonAn()
        {
            return View();
        }
        // Thông báo xóa được món ăn
        public ActionResult XoaMonAnThanhCong()
        {
            return View();
        } 
        // Thông báo xóa được bàn
        public ActionResult XoaBan()
        {
            return View();
        }
        // Thông báo thanh toán thành công
        public ActionResult ThanhCong()
        {
            return View();
        }
    }
}