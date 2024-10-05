using System;
using System.Web.Mvc;

namespace QuanLyNhaHang.Common
{
    public class AuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var requestUrl = filterContext.HttpContext.Request.Url.AbsolutePath;

            // Bỏ qua các trang không yêu cầu xác thực
            if (requestUrl.Equals("/NhanVien/DangNhap/DangNhap", StringComparison.OrdinalIgnoreCase) ||
                requestUrl.Equals("/NhanVien/DangNhap/QuenMatKhau", StringComparison.OrdinalIgnoreCase))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // Kiểm tra nếu người dùng chưa đăng nhập
            var cookie = filterContext.HttpContext.Request.Cookies["UserLogin"];
            if (cookie == null || string.IsNullOrEmpty(cookie["TaiKhoanNV"]))
            {
                // Lưu URL hiện tại để chuyển hướng về sau khi đăng nhập thành công
                var returnUrl = filterContext.HttpContext.Request.Url.AbsoluteUri;
                filterContext.HttpContext.Session["ReturnUrl"] = returnUrl;

                // Chuyển hướng về trang đăng nhập
                filterContext.Result = new RedirectResult("/NhanVien/DangNhap/DangNhap");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
