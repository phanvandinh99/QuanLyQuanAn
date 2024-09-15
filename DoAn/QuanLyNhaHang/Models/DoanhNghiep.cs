namespace QuanLyNhaHang.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DoanhNghiep")]
    public partial class DoanhNghiep
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(50)]
        public string MaDoanhNghiep { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(200)]
        public string TenDoanhNghiep { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(10)]
        public string SoDienThoai { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(255)]
        public string DiaChi { get; set; }

        [Key]
        [Column(Order = 4)]
        public DateTime ThoiGianBatDau { get; set; }

        public DateTime? ThoiGianKetThuc { get; set; }

        public double? TongTien { get; set; }
    }
}
