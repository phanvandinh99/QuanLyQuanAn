using QuanLyNhaHang.Models;
using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVien.Controllers
{
    public class DangNhapController : Controller
    {
        private readonly DatabaseQuanLyNhaHang _db;

        public DangNhapController(DatabaseQuanLyNhaHang db)
        {
            _db = db;
        }

        public ActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DangNhap(string sTaiKhoan, string sMatKhau)
        {
            if (string.IsNullOrEmpty(sTaiKhoan) || string.IsNullOrEmpty(sMatKhau))
            {
                ModelState.AddModelError("", "Vui lòng nhập tên đăng nhập và mật khẩu của bạn !");
            }
            else
            {
                var nhanVien = await _db.NhanVien
                    .SingleOrDefaultAsync(n => n.TaiKhoanNV == sTaiKhoan && n.MatKhauNV == sMatKhau);

                if (nhanVien == null)
                {
                    TempData["ToastMessage"] = "error|Tài khoản hoặc mật khẩu không đúng.";
                    return View();
                }
                else
                {
                    // Tạo cookie lưu thông tin đăng nhập
                    var cookie = new HttpCookie("UserLogin")
                    {
                        Values = {
                            ["TenNhanVien"] = nhanVien.TenNhanVien,
                            ["MaDoangNghiep"] = nhanVien.MaDoangNghiep_id.ToString(),
                            ["TenNhanVien"] = nhanVien.NgaySinh.ToString(),
                            ["MaQuyen_id"] = nhanVien.MaQuyen_id.ToString()
                        },
                        Expires = DateTime.Now.AddDays(7)
                    };
                    Response.Cookies.Add(cookie);

                    // Chuyển hướng tới URL ban đầu hoặc trang chính
                    string returnUrl = Session["ReturnUrl"] as string;
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        Session["ReturnUrl"] = null; // Xóa returnUrl sau khi sử dụng
                        return Redirect(returnUrl);
                    }
                    return Redirect("/NhanVien/Home/Index");
                }
            }

            return View();
        }

        public ActionResult DangXuat()
        {
            // Xóa cookie đăng nhập
            var cookie = Request.Cookies["UserLogin"];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1); // Đặt thời gian hết hạn trong quá khứ
                Response.Cookies.Add(cookie);
            }
            return RedirectToAction("DangNhap", "DangNhap");
        }
    }
}
