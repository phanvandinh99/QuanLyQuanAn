using QuanLyNhaHang.Models;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using QuanLyNhaHang.Common.Const;
using System.Web.UI;
using System;

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
        public async Task<ActionResult> ThemMoi(QuanLyNhaHang.Models.NhanVien Model, int[] danhMucIds)
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            try
            {
                Model.MaQuyen_id = Const.Employee;
                Model.MaDoanhNghiep_id = sMaDoanhNghiep;

                _db.NhanVien.Add(Model);
                await _db.SaveChangesAsync();

                var phanQuyens = danhMucIds.Select(id => new PhanQuyen
                {
                    MaNhanVien_id = Model.MaNhanVien,
                    MaDanhMuc_id = id
                }).ToList();

                if (phanQuyens.Any())
                {
                    _db.PhanQuyen.AddRange(phanQuyens);
                    await _db.SaveChangesAsync();
                }

                TempData["ToastMessage"] = "success|Thêm nhân viên thành công.";
                return RedirectToAction("Index", "NhanVien");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Thêm nhân viên thất bại.";
                return RedirectToAction("Index", "NhanVien");
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
        public async Task<ActionResult> CapNhat(QuanLyNhaHang.Models.NhanVien Model, int[] danhMucIds)
        {
            string sMaDoanhNghiep = GetMaDoanhNghiepFromCookie();

            try
            {
                var nhanVien = await _db.NhanVien.FindAsync(Model.MaNhanVien);
                if (nhanVien == null)
                {
                    TempData["ToastMessage"] = "error|Không tìm thấy nhân viên.";
                    return RedirectToAction("Index", "NhanVien");
                }

                // Cập nhật thông tin cơ bản của nhân viên
                nhanVien.TaiKhoanNV = Model.TaiKhoanNV;
                nhanVien.MatKhauNV = Model.MatKhauNV;
                nhanVien.TenNhanVien = Model.TenNhanVien;
                nhanVien.SoDienThoai = Model.SoDienThoai;

                await _db.SaveChangesAsync();

                // Xóa tất cả các quyền cũ của nhân viên
                var quyenHienTai = _db.PhanQuyen.Where(pq => pq.MaNhanVien_id == Model.MaNhanVien);
                _db.PhanQuyen.RemoveRange(quyenHienTai);
                await _db.SaveChangesAsync();

                // Kiểm tra xem danhMucIds có null hoặc không có phần tử nào không
                if (danhMucIds != null && danhMucIds.Length > 0)
                {
                    // Thêm các quyền mới từ danh sách danhMucIds
                    var phanQuyens = danhMucIds.Select(id => new PhanQuyen
                    {
                        MaNhanVien_id = Model.MaNhanVien,
                        MaDanhMuc_id = id
                    }).ToList();

                    if (phanQuyens.Any())
                    {
                        _db.PhanQuyen.AddRange(phanQuyens);
                        await _db.SaveChangesAsync();
                    }
                }

                TempData["ToastMessage"] = "success|Cập nhật nhân viên thành công.";
                return RedirectToAction("Index", "NhanVien");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "error|Cập nhật nhân viên thất bại.";
                return RedirectToAction("Index", "NhanVien");
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