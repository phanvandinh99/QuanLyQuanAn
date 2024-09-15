namespace QuanLyNhaHang.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("NhanVien")]
    public partial class NhanVien
    {
        [Key]
        public int MaNhanVien { get; set; }

        [Required]
        [StringLength(50)]
        public string TaiKhoanNV { get; set; }

        [Required]
        [StringLength(50)]
        public string MatKhauNV { get; set; }

        [Required]
        [StringLength(100)]
        public string TenNhanVien { get; set; }

        [Column(TypeName = "date")]
        public DateTime? NgaySinh { get; set; }

        [StringLength(10)]
        public string SoDienThoai { get; set; }

        public int? MaQuyen_id { get; set; }

        [Required]
        [StringLength(50)]
        public string MaDoanhNghiep_id { get; set; }

        public virtual Quyen Quyen { get; set; }
    }
}
