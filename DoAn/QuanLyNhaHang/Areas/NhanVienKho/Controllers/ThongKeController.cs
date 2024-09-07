using QuanLyNhaHang.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVienKho.Controllers
{
    public class ThongKeController : Controller
    {
        DatabaseQuanLyNhaHang db = new DatabaseQuanLyNhaHang();
        public ActionResult ThongKe()
        {
            ViewBag.DoanhThu = DoanhThuDonHang();
            ViewBag.NguyenLieu = db.NguyenLieu.Count();
            ViewBag.XuatKho = db.XuatKho.Count();
            ViewBag.HoanTra = db.HoanTra.Count();
            ViewBag.PhieuNhapCount = db.PhieuNhap.Count();

            ViewBag.HoaDon = db.HoaDon.OrderByDescending(n => n.MaHoaDon).ToList();
            ViewBag.PhieuNhap = db.PhieuNhap.OrderByDescending(n => n.MaPhieuNhap).ToList();

            ViewBag.HoaDon = db.HoaDon.OrderBy(n => n.MaHoaDon).ToList();
            ViewBag.ThongKe = 0; // kết quả thống kê doanh thu
            ViewBag.ThongKePhieuNhap = 0; // kết quả thống kê phiếu nhập
            ViewBag.listHoaDon = null;
            ViewBag.listPhieuNhap = null;
            ViewBag.SUM = 0;
            ViewBag.Loi = null;
            return View();
        }
        [HttpPost]
        public ActionResult ThongKe(FormCollection f)
        {
            int iNgay = int.Parse(f["Ngay"].ToString());
            int iThang = int.Parse(f["Thang"].ToString());
            int iNam = int.Parse(f["Nam"].ToString());

            ViewBag.DoanhThu = DoanhThuDonHang();
            ViewBag.NguyenLieu = db.NguyenLieu.Count();
            ViewBag.XuatKho = db.XuatKho.Count();
            ViewBag.HoanTra = db.HoanTra.Count();
            ViewBag.PhieuNhap = db.PhieuNhap.Count();
            if (iNgay == 0 && iThang == 0) //Thống kê theo năm
            {
                ViewBag.ThongKe = DoanhThuNam(iNam);
                ViewBag.ThongKePhieuNhap = DoanhThuNam_Nhap(iNam);
                ViewBag.listHoaDon = db.HoaDon.Where(n => n.NgayThanhToan.Value.Year == iNam).ToList();
                ViewBag.listPhieuNhap = db.PhieuNhap.Where(n => n.NgayNhap.Value.Year == iNam).ToList();
                ViewBag.SUM = DoanhThuNam(iNam) - DoanhThuNam_Nhap(iNam);
                if (DoanhThuNam(iNam) - DoanhThuNam_Nhap(iNam) > 0)
                {
                    ViewBag.Loi = "Lãi";
                }
                else ViewBag.Loi = "Lỗ";
            }
            else if (iNgay == 0) // thống kê tháng năm
            {
                ViewBag.ThongKe = DoanhThuThangNam(iThang, iNam);
                ViewBag.ThongKePhieuNhap = DoanhThuThangNam_Nhap(iThang, iNam);
                ViewBag.listHoaDon = db.HoaDon.Where(n => n.NgayThanhToan.Value.Month == iThang & n.NgayThanhToan.Value.Year == iNam).ToList();
                ViewBag.listPhieuNhap = db.PhieuNhap.Where(n => n.NgayNhap.Value.Month == iThang & n.NgayNhap.Value.Year == iNam).ToList();
                ViewBag.SUM = DoanhThuThangNam(iThang, iNam) - DoanhThuThangNam_Nhap(iThang, iNam);
                if (DoanhThuThangNam(iThang, iNam) - DoanhThuThangNam_Nhap(iThang, iNam) > 0)
                {
                    ViewBag.Loi = "Lãi";
                }
                else ViewBag.Loi = "Lỗ";
            }
            else // ngày tháng năm
            {
                ViewBag.ThongKe = DoanhThuNgayThangNam(iNgay, iThang, iNam);
                ViewBag.ThongKePhieuNhap = DoanhThuNgayThangNam_Nhap(iNgay, iThang, iNam);
                ViewBag.listHoaDon = db.HoaDon.Where(n => n.NgayThanhToan.Value.Day == iNgay & n.NgayThanhToan.Value.Month == iThang & n.NgayThanhToan.Value.Year == iNam).ToList();
                ViewBag.listPhieuNhap = db.PhieuNhap.Where(n => n.NgayNhap.Value.Day == iNgay & n.NgayNhap.Value.Month == iThang & n.NgayNhap.Value.Year == iNam).ToList();
                ViewBag.SUM = DoanhThuNgayThangNam(iNgay, iThang, iNam) - DoanhThuNgayThangNam_Nhap(iNgay, iThang, iNam);
                if (DoanhThuNgayThangNam(iNgay, iThang, iNam) - DoanhThuNgayThangNam_Nhap(iNgay, iThang, iNam) > 0)
                {
                    ViewBag.Loi = "Lãi";
                }
                else ViewBag.Loi = "Lỗ";
            }
            return View();
        }


        #region Controller thống kê ngày đến ngày
        [HttpPost]
        public ActionResult ThongKeTuan(FormCollection f)
        {
            int iNgayBatDau = int.Parse(f["NgayBatDau"].ToString());
            int iNgayKetThuc = int.Parse(f["NgayKetThuc"].ToString());
            int iThang = int.Parse(f["Thang"].ToString());

            ViewBag.DoanhThu = DoanhThuDonHang();
            ViewBag.NguyenLieu = db.NguyenLieu.Count();
            ViewBag.XuatKho = db.XuatKho.Count();
            ViewBag.HoanTra = db.HoanTra.Count();
            ViewBag.PhieuNhap = db.PhieuNhap.Count();

            ViewBag.ThongKe = DoanhThuTuanHoaDon(iNgayBatDau, iNgayKetThuc, iThang);
            ViewBag.ThongKePhieuNhap = DoanhThuTuan(iNgayBatDau, iNgayKetThuc, iThang);
            ViewBag.listHoaDon = db.HoaDon.Where(n => n.NgayThanhToan.Value.Day >= iNgayBatDau & n.NgayThanhToan.Value.Day<=iNgayKetThuc&n.NgayThanhToan.Value.Month==iThang).ToList();
            ViewBag.listPhieuNhap = db.PhieuNhap.Where(n => n.NgayNhap.Value.Day >= iNgayBatDau & n.NgayNhap.Value.Day <= iNgayKetThuc & n.NgayNhap.Value.Month == iThang).ToList();


            ViewBag.SUM = DoanhThuTuanHoaDon(iNgayBatDau, iNgayKetThuc, iThang) - DoanhThuTuan(iNgayBatDau, iNgayKetThuc, iThang);
            if (DoanhThuTuanHoaDon(iNgayBatDau, iNgayKetThuc, iThang) - DoanhThuTuan(iNgayBatDau, iNgayKetThuc, iThang) > 0)
            {
                ViewBag.Loi = "Lãi";
            }
            else ViewBag.Loi = "Lỗ";
            return View();
        }
        #endregion
        #region Controller thống kê Quý
        [HttpPost]
        public ActionResult ThongKeQuy(FormCollection f)
        {
            int iThangBatDau = int.Parse(f["ThangBatDau"].ToString());
            int iNamBatDau = int.Parse(f["NamBatDau"].ToString());
            int iThangKetThuc = int.Parse(f["ThangKetThuc"].ToString());
            int iNamKetThuc = int.Parse(f["NamKetThuc"].ToString());

            ViewBag.DoanhThu = DoanhThuDonHang();
            ViewBag.NguyenLieu = db.NguyenLieu.Count();
            ViewBag.XuatKho = db.XuatKho.Count();
            ViewBag.HoanTra = db.HoanTra.Count();
            ViewBag.PhieuNhap = db.PhieuNhap.Count();

            ViewBag.ThongKe = DoanhThuThangDenThang_HoaDon(iThangBatDau, iNamBatDau, iThangKetThuc, iNamKetThuc);
            ViewBag.ThongKePhieuNhap = DoanhThuThangDenThang(iThangBatDau, iNamBatDau, iThangKetThuc, iNamKetThuc);

            ViewBag.listHoaDon = db.HoaDon.Where(n => n.NgayThanhToan.Value.Month >= iThangBatDau & n.NgayThanhToan.Value.Year >= iNamBatDau& n.NgayThanhToan.Value.Month <= iThangKetThuc & n.NgayThanhToan.Value.Year <= iNamKetThuc).ToList();
            ViewBag.listPhieuNhap = db.PhieuNhap.Where(n => n.NgayNhap.Value.Month >= iThangBatDau & n.NgayNhap.Value.Year >= iNamBatDau & n.NgayNhap.Value.Month <= iThangKetThuc & n.NgayNhap.Value.Year <= iNamKetThuc).ToList();


            ViewBag.SUM = DoanhThuThangDenThang_HoaDon(iThangBatDau, iNamBatDau, iThangKetThuc, iNamKetThuc) - DoanhThuThangDenThang(iThangBatDau, iNamBatDau, iThangKetThuc, iNamKetThuc);
            if (DoanhThuThangDenThang_HoaDon(iThangBatDau, iNamBatDau, iThangKetThuc, iNamKetThuc) - DoanhThuThangDenThang(iThangBatDau, iNamBatDau, iThangKetThuc, iNamKetThuc) > 0)
            {
                ViewBag.Loi = "Lãi";
            }
            else ViewBag.Loi = "Lỗ";
            return View();
        }
        #endregion


        public double DoanhThuDonHang()
        {
            // doanh thu tất cả 
            double TongDoanhThu = db.HoaDon.Sum(n => n.TongTien);
            return TongDoanhThu;
        }
        #region Hàm tính tổng doanh thu hóa đơn và doanh thu nhập
        public double DoanhThuThangNam(int Thang, int Nam)
        {

            var listDDH = db.HoaDon.Where(n => n.NgayThanhToan.Value.Month == Thang && n.NgayThanhToan.Value.Year == Nam);
            double TongTien = 0;
            foreach (var item in listDDH)
            {
                TongTien += item.ChiTietHoaDon.Sum(n => n.ThanhTien);
            }
            return TongTien;
        }
        public double DoanhThuNgayThangNam(int Ngay, int Thang, int Nam)
        {

            var listDDH = db.HoaDon.Where(n => n.NgayThanhToan.Value.Day == Ngay && n.NgayThanhToan.Value.Month == Thang && n.NgayThanhToan.Value.Year == Nam);
            double TongTien = 0;
            foreach (var item in listDDH)
            {
                TongTien += item.ChiTietHoaDon.Sum(n => n.ThanhTien);
            }
            return TongTien;
        }
        public double DoanhThuNam(int Nam)
        {
            var listDDH = db.HoaDon.Where(n => n.NgayThanhToan.Value.Year == Nam);
            double TongTien = 0;
            foreach (var item in listDDH)
            {
                TongTien += item.ChiTietHoaDon.Sum(n => n.ThanhTien);
            }
            return TongTien;
        }
        public double? DoanhThuThangNam_Nhap(int Thang, int Nam)
        {

            var listPhieuNhap = db.PhieuNhap.Where(n => n.NgayNhap.Value.Month == Thang && n.NgayNhap.Value.Year == Nam);
            double? TongTien = 0;
            foreach (var item in listPhieuNhap)
            {
                TongTien += item.TongTien;
            }
            return TongTien;
        }
        public double? DoanhThuNgayThangNam_Nhap(int Ngay, int Thang, int Nam)
        {

            var listPhieuNhap = db.PhieuNhap.Where(n => n.NgayNhap.Value.Day == Ngay && n.NgayNhap.Value.Month == Thang && n.NgayNhap.Value.Year == Nam);
            double? TongTien = 0;
            foreach (var item in listPhieuNhap)
            {
                TongTien += item.TongTien;
            }
            return TongTien;
        }
        public double? DoanhThuNam_Nhap(int Nam)
        {
            var listPhieuNhap = db.PhieuNhap.Where(n => n.NgayNhap.Value.Year == Nam);
            double? TongTien = 0;
            foreach (var item in listPhieuNhap)
            {
                TongTien += item.TongTien;
            }
            return TongTien;
        }
        // Tính tổng tiền theo tháng phiếu nhập
        public double? DoanhThuTuan(int NgayBatDau, int NgayKetThuc, int Thang)
        {
            var listPhieuNhap = db.PhieuNhap.Where(n => n.NgayNhap.Value.Day >= NgayBatDau && n.NgayNhap.Value.Day <= NgayKetThuc && n.NgayNhap.Value.Month == Thang);
            double? TongTien = 0;
            foreach (var item in listPhieuNhap)
            {
                TongTien += item.TongTien;
            }
            return TongTien;
        }
        public double DoanhThuTuanHoaDon(int NgayBatDau, int NgayKetThuc, int Thang)
        {

            var listDDH = db.HoaDon.Where(n => n.NgayThanhToan.Value.Day >= NgayBatDau && n.NgayThanhToan.Value.Day <= NgayKetThuc && n.NgayThanhToan.Value.Month == Thang);
            double TongTien = 0;
            foreach (var item in listDDH)
            {
                TongTien += item.ChiTietHoaDon.Sum(n => n.ThanhTien);
            }
            return TongTien;
        }
        // tính tổng tiền theo tháng đến tháng của năm nào đó 
        public double? DoanhThuThangDenThang(int ThangBatDau, int NambatDau, int ThangKetThuc, int NamKetThuc)
        {
            var listPhieuNhap = db.PhieuNhap.Where(n => n.NgayNhap.Value.Month >= ThangBatDau && n.NgayNhap.Value.Year >= NambatDau && n.NgayNhap.Value.Month <= ThangKetThuc && n.NgayNhap.Value.Year <= NamKetThuc);
            double? TongTien = 0;
            foreach (var item in listPhieuNhap)
            {
                TongTien += item.TongTien;
            }
            return TongTien;
        }
        public double DoanhThuThangDenThang_HoaDon(int ThangBatDau, int NambatDau, int ThangKetThuc, int NamKetThuc)
        {

            var listDDH = db.HoaDon.Where(n => n.NgayThanhToan.Value.Month >= ThangBatDau && n.NgayThanhToan.Value.Year >= NambatDau && n.NgayThanhToan.Value.Month <= ThangKetThuc && n.NgayThanhToan.Value.Year <= NamKetThuc);
            double TongTien = 0;
            foreach (var item in listDDH)
            {
                TongTien += item.ChiTietHoaDon.Sum(n => n.ThanhTien);
            }
            return TongTien;
        }
        #endregion
    }
}