namespace QuanLyNhaHang.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("DoanhNghiep")]
    public partial class DoanhNghiep
    {
        [Key]
        [StringLength(50)]
        public string MaDoanhNghiep { get; set; }

        [StringLength(200)]
        public string TenDoanhNghiep { get; set; }

        [StringLength(10)]
        public string SoDienThoai { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(255)]
        public string DiaChi { get; set; }

        public DateTime ThoiGianBatDau { get; set; }

        public DateTime ThoiGianKetThuc { get; set; }

        public double? TongTien { get; set; }
    }
}
