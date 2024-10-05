using System;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using QuanLyNhaHang.Models;
using System.Threading.Tasks;
using System.Linq;
using QuanLyNhaHang.Common.Const;
using QuanLyNhaHang.Common;

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
            try
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
                        // Lấy mã doanh nghiệp
                        var maDoanhNghiep = _db.DoanhNghiep.SingleOrDefault(n => n.MaDoanhNghiep == nhanVien.MaDoanhNghiep_id);
                        if (maDoanhNghiep.ThoiGianKetThuc <= DateTime.Now)
                        {
                            TempData["ToastMessage"] = "error|Doanh nghiệp hết hợp đồng.";
                            return View();
                        }

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

                        // Kiểm tra có cần thay đổi mật khẩu hay không
                        if (nhanVien.DoiMatKhau == Const.DoiMatKhau)
                        {
                            return RedirectToAction("DoiMatKhau", "DangNhap", new { @sTaiKhoan = sTaiKhoan });
                        }

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
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Đã xảy ra lỗi trong quá trình dăng nhập.";
                return View();
            }
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

        public ActionResult DoiMatKhau(string sTaiKhoan)
        {
            ViewBag.TaiKhoan = sTaiKhoan;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DoiMatKhau(string sTaiKhoan, string sMatKhau, string sMatKhauMoi)
        {
            try
            {
                // Tìm nhân viên
                var nhanVien = await _db.NhanVien.SingleOrDefaultAsync(n => n.TaiKhoanNV == sTaiKhoan && n.MatKhauNV == sMatKhau);

                if (nhanVien == null)
                {
                    TempData["ToastMessage"] = "error|Mật khẩu không đúng.";
                }
                else
                {
                    // Thay đổi mật khẩu
                    nhanVien.MatKhauNV = sMatKhauMoi;
                    nhanVien.DoiMatKhau = Const.KhongDoiMatKhau;
                    await _db.SaveChangesAsync();

                    TempData["ToastMessage"] = "success|Cập nhật mật khẩu thành công.";
                    return Redirect("/NhanVien/Home/Index");
                }

                // Gửi lại dữ liệu cho view nếu có lỗi
                ViewBag.TaiKhoan = sTaiKhoan;
                ViewBag.MatKhauCu = sMatKhau;
                ViewBag.MatKhauMoi = sMatKhauMoi;
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Đã xảy ra lỗi trong quá trình thay đổi mật khẩu.";
            }

            return RedirectToAction("DangNhap", "DoiMatKhau", new { @sTaiKhoan = sTaiKhoan });
        }

        public ActionResult QuenMatKhau()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> QuenMatKhau(string sEmail)
        {
            try
            {
                var nhanVien = await _db.NhanVien.SingleOrDefaultAsync(n => n.Email == sEmail);

                if (nhanVien == null)
                {
                    TempData["ToastMessage"] = "error|Email không tồn tại trong hệ thống.";
                    return View();
                }
                else
                {
                    // Thay đổi mật khẩu

                    string password = Common.GeneratePassword.NewPassword();

                    bool result = await Common.SendMail.SendEmailAsync(
                        "Hệ thống quản lý quán ăn",
                        "Bạn vừa xác nhận yêu cầu lấy lại mật khẩu:\n" +
                        "Mật khẩu của bạn là: " + password,
                        sEmail);

                    if (result == false)
                    {
                        TempData["ToastMessage"] = "error|Lỗi khi gửi mail.";
                        return View();
                    };
                    nhanVien.MatKhauNV = password;
                    nhanVien.DoiMatKhau = Const.DoiMatKhau;
                    await _db.SaveChangesAsync();

                    TempData["ToastMessage"] = "infor|Kiểm tra email để lấy mật khẩu.";
                    return RedirectToAction("DangNhap", "DangNhap");
                }
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Đã xảy ra lỗi trong quá trình cấp mật khẩu.";
                return RedirectToAction("DangNhap", "DangNhap");
            }

        }

    }
}
