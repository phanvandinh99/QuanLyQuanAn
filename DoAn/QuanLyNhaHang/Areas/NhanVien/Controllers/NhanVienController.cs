using QuanLyNhaHang.Common.Const;
using QuanLyNhaHang.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVien.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly DatabaseQuanLyNhaHang _db;

        public NhanVienController(DatabaseQuanLyNhaHang db)
        {
            _db = db;
        }

        private string GetMaDoanhNghiepFromCookie()
        {
            var cookie = Request.Cookies["UserLogin"];
            return cookie?["MaDoanhNghiep"];
        }

        public async Task<ActionResult> Index()
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            if (sMaDoanhNghiep == Const.MaDoanhNghiep)
            {
                var list = await _db.NhanVien.Where(n => n.MaQuyen_id != Const.SupperAdmin &&
                                      n.MaQuyen_id != Const.Admin &&
                                      n.MaDoanhNghiep_id == sMaDoanhNghiep
                                      )
                              .OrderBy(n => n.MaQuyen_id)
                              .ToListAsync();
                return View(list);
            }
            else
            {
                var list = await _db.NhanVien.Where(n => n.MaQuyen_id != Const.SupperAdmin &&
                                             n.MaQuyen_id != Const.Admin &&
                                             n.MaDoanhNghiep_id == sMaDoanhNghiep
                                             )
                                     .OrderBy(n => n.MaQuyen_id)
                                     .ToListAsync();
                return View(list);
            }
        }

        public async Task<ActionResult> XemChiTiet(int iMaNhanVien)
        {
            try
            {
                var nhanVien = await _db.NhanVien.FindAsync(iMaNhanVien);
                return View(nhanVien);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Xem chi tiết nhân viên thất bại.";
                return RedirectToAction("Index", "NhanVien");
            }
        }

        // Thêm Nhan Vien
        public async Task<ActionResult> ThemMoi()
        {
            var danhMuc = await _db.DanhMuc.Where(n=>n.MaDanhMuc != Const.QuanLyNhaHang).ToListAsync();
            ViewBag.DanhMuc = danhMuc;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ThemMoi(QuanLyNhaHang.Models.NhanVien model, int[] danhMucIds)
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    // Kiểm tra mã doanh nghiệp và email đã tồn tại
                    bool emailExists = await _db.NhanVien.AnyAsync(n => n.Email == model.Email && n.MaDoanhNghiep_id == sMaDoanhNghiep);

                    if (emailExists)
                    {
                        var danhMuc = await _db.DanhMuc.Where(n => n.MaDanhMuc != Const.QuanLyNhaHang).ToListAsync();
                        ViewBag.DanhMuc = danhMuc;

                        TempData["ToastMessage"] = "error|Email đã tồn tại.";
                        return View();
                    }

                    if (string.IsNullOrEmpty(model.MatKhauNV))
                    {
                        model.MatKhauNV = Common.GeneratePassword.NewPassword();
                    }

                    model.DoiMatKhau = Const.DoiMatKhau;
                    model.MaQuyen_id = Const.Employee;
                    model.MaDoanhNghiep_id = sMaDoanhNghiep;

                    _db.NhanVien.Add(model);
                    await _db.SaveChangesAsync();

                    bool emailSent = await Common.SendMail.SendEmailAsync(
                        "Tài khoản nhân viên tại cửa hàng",
                        $"<p>Chào mừng nhân viên: <strong>{model.TenNhanVien}</strong></p>" +
                        "<p>Tài khoản đăng nhập của bạn <a href=\"https://QLQuanAn.com.vn\">https://QLQuanAn.com.vn</a> là</p>" +
                        $"<p><strong>Mã doanh nghiệp:</strong> {sMaDoanhNghiep}</p>" +
                        $"<p><strong>Tài khoản:</strong> {model.TaiKhoanNV}</p>" +
                        $"<p><strong>Mật khẩu:</strong> {model.MatKhauNV}</p>",
                        model.Email
                    );

                    if (danhMucIds != null && danhMucIds.Any())
                    {
                        var phanQuyens = danhMucIds.Select(id => new PhanQuyen
                        {
                            MaNhanVien_id = model.MaNhanVien,
                            MaDanhMuc_id = id
                        }).ToList();

                        _db.PhanQuyen.AddRange(phanQuyens);
                        await _db.SaveChangesAsync();
                    }

                    // Commit transaction if everything is successful
                    transaction.Commit();
                    TempData["ToastMessage"] = "success|Thêm nhân viên thành công.";
                    return RedirectToAction("Index", "NhanVien");
                }
                catch (Exception ex)
                {
                    // Rollback transaction if an error occurs
                    transaction.Rollback();
                    TempData["ToastMessage"] = "error|Thêm nhân viên thất bại.";
                    return RedirectToAction("Index", "NhanVien");
                }
            }
        }


        // Cập nhật Nhan Vien
        public async Task<ActionResult> CapNhat(int iMaNhanVien)
        {
            var nhanVien = _db.NhanVien.Find(iMaNhanVien);
            if (nhanVien == null)
            {
                TempData["ToastMessage"] = "error|Không tìm thấy nhân viên.";
                return RedirectToAction("Index", "NhanVien");
            }

            var danhMuc = await _db.DanhMuc.Where(n => n.MaDanhMuc != Const.QuanLyNhaHang).ToListAsync();
            ViewBag.DanhMuc = danhMuc;

            return View(nhanVien);
        }

        [HttpPost]
        public async Task<ActionResult> CapNhat(QuanLyNhaHang.Models.NhanVien model, int[] danhMucIds)
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var nhanVien = await _db.NhanVien.FindAsync(model.MaNhanVien);
                    if (nhanVien == null)
                    {
                        TempData["ToastMessage"] = "error|Không tìm thấy nhân viên.";
                        return RedirectToAction("Index", "NhanVien");
                    }

                    // Kiểm tra mã doanh nghiệp và email đã tồn tại
                    bool emailExists = await _db.NhanVien.AnyAsync(n => n.Email == model.Email &&
                                                                        n.MaDoanhNghiep_id == sMaDoanhNghiep &&
                                                                        n.MaNhanVien != model.MaNhanVien);

                    if (emailExists)
                    {
                        var danhMuc = await _db.DanhMuc.Where(n => n.MaDanhMuc != Const.QuanLyNhaHang).ToListAsync();
                        ViewBag.DanhMuc = danhMuc;

                        TempData["ToastMessage"] = "error|Email đã tồn tại.";
                        return View();
                    }

                    if (string.IsNullOrEmpty(model.MatKhauNV))
                    {
                        model.MatKhauNV = Common.GeneratePassword.NewPassword();
                    }

                    // Cập nhật thông tin cơ bản của nhân viên
                    nhanVien.TaiKhoanNV = model.TaiKhoanNV;
                    nhanVien.MatKhauNV = model.MatKhauNV;
                    nhanVien.TenNhanVien = model.TenNhanVien;
                    nhanVien.DoiMatKhau = Const.DoiMatKhau;
                    nhanVien.SoDienThoai = model.SoDienThoai;
                    await _db.SaveChangesAsync();

                    bool emailSent = await Common.SendMail.SendEmailAsync(
                        "Cập nhật tài khoản",
                        "<p>Tài khoản đăng nhập của bạn <a href=\"https://QLQuanAn.com.vn\">https://QLQuanAn.com.vn</a> đã được cập nhật</p>" +
                        $"<p><strong>Mã doanh nghiệp:</strong> {sMaDoanhNghiep}</p>" +
                        $"<p><strong>Tài khoản:</strong> {model.TaiKhoanNV}</p>" +
                        $"<p><strong>Mật khẩu:</strong> {model.MatKhauNV}</p>",
                        model.Email
                    );

                    // Xóa tất cả các quyền cũ của nhân viên
                    var quyenHienTai = _db.PhanQuyen.Where(pq => pq.MaNhanVien_id == model.MaNhanVien);
                    _db.PhanQuyen.RemoveRange(quyenHienTai);
                    await _db.SaveChangesAsync();

                    // Kiểm tra xem danhMucIds có null hoặc không có phần tử nào không
                    if (danhMucIds != null && danhMucIds.Length > 0)
                    {
                        // Thêm các quyền mới từ danh sách danhMucIds
                        var phanQuyens = danhMucIds.Select(id => new PhanQuyen
                        {
                            MaNhanVien_id = model.MaNhanVien,
                            MaDanhMuc_id = id
                        }).ToList();

                        if (phanQuyens.Any())
                        {
                            _db.PhanQuyen.AddRange(phanQuyens);
                            await _db.SaveChangesAsync();
                        }
                    }

                    // Commit transaction if everything is successful
                    transaction.Commit();
                    TempData["ToastMessage"] = "success|Cập nhật nhân viên thành công.";
                    return RedirectToAction("Index", "NhanVien");
                }
                catch (Exception ex)
                {
                    // Rollback transaction if an error occurs
                    transaction.Rollback();
                    TempData["ToastMessage"] = "error|Cập nhật nhân viên thất bại.";
                    return RedirectToAction("Index", "NhanVien");
                }
            }
        }



        public async Task<ActionResult> Xoa(int iMaNhanVien)
        {
            try
            {
                _db.NhanVien.Remove(await _db.NhanVien.FindAsync(iMaNhanVien));
                await _db.SaveChangesAsync();

                TempData["ToastMessage"] = "success|Xóa nhân viên thành thành công.";

                return RedirectToAction("Index", "NhanVien");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Xóa nhân viên thất bại.";
                return RedirectToAction("Index", "NhanVien");
            }
        }
    }
}