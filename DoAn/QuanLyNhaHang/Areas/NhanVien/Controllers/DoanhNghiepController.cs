using QuanLyNhaHang.Common.Const;
using QuanLyNhaHang.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVien.Controllers
{
    public class DoanhNghiepController : Controller
    {
        private readonly DatabaseQuanLyNhaHang _db;

        public DoanhNghiepController(DatabaseQuanLyNhaHang db)
        {
            _db = db;
        }

        private string GetMaDoanhNghiepFromCookie()
        {
            var cookie = Request.Cookies["UserLogin"];
            return cookie?["MaDoanhNghiep"];
        }

        // GET: NhanVien/DoanhNghiep
        public async Task<ActionResult> Index()
        {
            var listDoanhNghiep = await _db.DoanhNghiep.ToListAsync();
            ViewBag.DoanhNghiep = listDoanhNghiep.Count();

            return View(listDoanhNghiep);
        }

        public async Task<ActionResult> XemChiTiet(string sMaDoanhNghiep)
        {
            try
            {
                var doanhNghiep = await _db.DoanhNghiep.FindAsync(sMaDoanhNghiep);
                if (doanhNghiep == null)
                {
                    TempData["ToastMessage"] = "error|Doanh nghiệp không tồn tại.";
                    return RedirectToAction("Index", "DoanhNghiep");
                }

                return View(doanhNghiep);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Xem chi tiết doanh nghiệp thất bại.";
                return RedirectToAction("Index", "DoanhNghiep");
            }
        }

        // Thêm doanh nghiệp
        public async Task<ActionResult> ThemMoi()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ThemMoi(DoanhNghiep model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastMessage"] = "error|Dữ liệu không hợp lệ.";
                return RedirectToAction("Index", "DoanhNghiep");
            }

            try
            {
                _db.DoanhNghiep.Add(model);
                await _db.SaveChangesAsync();

                // Thêm mới nhân viên
                var nhanVien = new QuanLyNhaHang.Models.NhanVien
                {
                    TaiKhoanNV = "Admin",
                    MatKhauNV = "Abc123",
                    TenNhanVien = "Ban Quản Trị",
                    MaQuyen_id = Const.SupperAdmin,
                    MaDoanhNghiep_id = model.MaDoanhNghiep
                };
                _db.NhanVien.Add(nhanVien);

                // Thêm mới tầng
                var tang = new Tang
                {
                    TenTang = "Tầng 1",
                    MaDoanhNghiep_id = model.MaDoanhNghiep
                };
                _db.Tang.Add(tang);

                // Thêm mới bàn
                var ban = new Ban
                {
                    TenBan = "Mang Đi",
                    TinhTrang = Const.MangDi,
                    MangDi = Const.boolMangDi,
                    MaTang_id = tang.MaTang,
                    MaDoanhNghiep_id = model.MaDoanhNghiep
                };
                _db.Ban.Add(ban);

                await _db.SaveChangesAsync();

                TempData["ToastMessage"] = "success|Thêm Doanh nghiệp thành công.";
                return RedirectToAction("Index", "DoanhNghiep");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = $"error|Thêm doanh nghiệp thất bại";
                return RedirectToAction("Index", "DoanhNghiep");
            }
        }



        //Cập Nhật Doanh Nghiệp
        public async Task<ActionResult> CapNhat(string sMaDoanhNghiep)
        {
            try
            {
                var doanhNghiep = await _db.DoanhNghiep.FindAsync(sMaDoanhNghiep);

                if (doanhNghiep == null)
                {
                    TempData["ToastMessage"] = "error|Không tìm thấy doanh nghiệp.";
                    return RedirectToAction("Index", "DoanhNghiep");
                }
                return View(doanhNghiep);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Lỗi khi cập nhật doanh nghiệp.";
                return RedirectToAction("Index", "DoanhNghiep");
            }

        }
        [HttpPost]
        public async Task<ActionResult> CapNhat(DoanhNghiep Model)
        {
            try
            {
                var doanhNghiep = await _db.DoanhNghiep.FindAsync(Model.MaDoanhNghiep);

                if (doanhNghiep == null)
                {
                    TempData["ToastMessage"] = "error|Không tìm thấy doanh nghiệp.";
                    return RedirectToAction("Index", "DoanhNghiep");
                }

                doanhNghiep.TenDoanhNghiep = Model.TenDoanhNghiep;
                doanhNghiep.SoDienThoai = Model.SoDienThoai;
                doanhNghiep.Email = Model.Email;
                doanhNghiep.DiaChi = Model.DiaChi;
                doanhNghiep.ThoiGianBatDau = Model.ThoiGianBatDau;
                doanhNghiep.ThoiGianKetThuc = Model.ThoiGianKetThuc;
                doanhNghiep.TongTien = Model.TongTien;
                await _db.SaveChangesAsync();

                TempData["ToastMessage"] = "success|Cập nhật doanh nghiệp thành công.";
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Có lỗi xãy ra khi cập nhật doanh nghiệp.";
            }

            return RedirectToAction("Index", "DoanhNghiep");
        }

        public async Task<ActionResult> Xoa(string sMaDoanhNghiep)
        {
            try
            {
                _db.DoanhNghiep.Remove(await _db.DoanhNghiep.FindAsync(sMaDoanhNghiep));
                await _db.SaveChangesAsync();

                TempData["ToastMessage"] = "success|Xóa doanh nghiệp thành công.";

                return RedirectToAction("Index", "DoanhNghiep");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Xóa doanh nghiệp thất bại.";
                return RedirectToAction("Index", "DoanhNghiep");
            }
        }

    }
}