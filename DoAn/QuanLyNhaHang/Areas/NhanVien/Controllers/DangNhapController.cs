using System;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using QuanLyNhaHang.Models;
using System.Threading.Tasks;

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
        public async Task<ActionResult> DangNhap(string sMaDoanhNghiep, string sTaiKhoan, string sMatKhau)
        {
            if (string.IsNullOrEmpty(sTaiKhoan) || string.IsNullOrEmpty(sMatKhau) || string.IsNullOrEmpty(sMaDoanhNghiep))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin !");
            }
            else
            {
                var nhanVien = await _db.NhanVien
                               .SingleOrDefaultAsync(n => n.TaiKhoanNV == sTaiKhoan &&
                                                     n.MatKhauNV == sMatKhau &&
                                                     n.MaDoanhNghiep_id == sMaDoanhNghiep);

                if (nhanVien == null)
                {
                    TempData["ToastMessage"] = "error|Tài khoản không hợp lệ.";
                    return View();
                }
                else
                {
                    // Tạo cookie lưu thông tin đăng nhập
                    var cookie = new HttpCookie("UserLogin")
                    {
                        Values = {
                            ["MaNhanVien"] = nhanVien.MaNhanVien.ToString(),
                            ["TaiKhoanNV"] = nhanVien.TaiKhoanNV,
                            ["TenNhanVien"] = nhanVien.TenNhanVien,
                            ["MaQuyen_id"] = nhanVien.MaQuyen_id.ToString(),
                            ["MaDoanhNghiep"] = nhanVien.MaDoanhNghiep_id.ToString()
                        },
                        Expires = DateTime.Now.AddDays(1)
                    };
                    Response.Cookies.Add(cookie);

                    // Chuyển hướng tới URL ban đầu hoặc trang chính
                    string returnUrl = Session["ReturnUrl"] as string;
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        Session["ReturnUrl"] = null;
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
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }
            return RedirectToAction("DangNhap", "DangNhap");
        }
    }
}
