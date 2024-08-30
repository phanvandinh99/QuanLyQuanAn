using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVienKho.Controllers
{
    public class ErrorController : Controller
    {
        // GET: NhanVienKho/Error
        public ActionResult ThongBaoLoi()
        {
            return View();
        }

        public ActionResult TrungSanPhamKhiNhap() // lỗi trùng sản phẩm khi nhập
        {
            return View();
        }

        public ActionResult KhoaNgoai() // lỗi xóa trường có liên quan đến khóa ngoại bảng khác
        {
            return View();
        }
        public ActionResult KhoKhongDapUng()
        {

            return View();
        }
    }
}