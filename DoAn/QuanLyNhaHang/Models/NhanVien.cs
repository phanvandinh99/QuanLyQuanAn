namespace QuanLyNhaHang.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("NhanVien")]
    public partial class NhanVien
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NhanVien()
        {
            PhanQuyen = new HashSet<PhanQuyen>();
        }

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PhanQuyen> PhanQuyen { get; set; }
    }
}
