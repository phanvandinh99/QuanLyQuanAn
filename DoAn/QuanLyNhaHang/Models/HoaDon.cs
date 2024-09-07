namespace QuanLyNhaHang.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("HoaDon")]
    public partial class HoaDon
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public HoaDon()
        {
            ChiTietHoaDon = new HashSet<ChiTietHoaDon>();
            LichSuGoiMon = new HashSet<LichSuGoiMon>();
        }

        [Key]
        public int MaHoaDon { get; set; }

        [StringLength(100)]
        public string TenKhachHang { get; set; }

        [StringLength(10)]
        public string SDTKhachHang { get; set; }

        [StringLength(255)]
        public string DiaChiKhachHang { get; set; }

        public DateTime? NgayTao { get; set; }

        public DateTime? NgayThanhToan { get; set; }

        [StringLength(200)]
        public string GhiChu { get; set; }

        public double TongTien { get; set; }

        public int? TrangThai { get; set; }

        public int MaBan_id { get; set; }

        public string MaDoanhNghiep_id { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDon { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LichSuGoiMon> LichSuGoiMon { get; set; }
    }
}
