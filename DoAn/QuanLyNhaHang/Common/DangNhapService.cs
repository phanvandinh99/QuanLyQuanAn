using QuanLyNhaHang.Models;
using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web;

namespace QuanLyNhaHang.Common
{
    public class DangNhapService
    {
        private readonly DatabaseQuanLyNhaHang _db;

        public DangNhapService(DatabaseQuanLyNhaHang db)
        {
            _db = db;
        }

        public async Task<bool> DangNhapAsync(string sTaiKhoan, string sMatKhau)
        {
            var nhanVien = await _db.NhanVien
                .SingleOrDefaultAsync(n => n.TaiKhoanNV == sTaiKhoan && n.MatKhauNV == sMatKhau);

            if (nhanVien == null) return false;

            var cookie = new HttpCookie("UserLogin")
            {
                Values = {
                    ["TaiKhoanNV"] = nhanVien.TaiKhoanNV,
                    ["MaQuyen_id"] = nhanVien.MaQuyen_id.ToString()
                },
                Expires = DateTime.Now.AddDays(7)
            };

            HttpContext.Current.Response.Cookies.Add(cookie);
            return true;
        }

        public void DangXuat()
        {
            var cookie = HttpContext.Current.Request.Cookies["UserLogin"];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }
    }
}
