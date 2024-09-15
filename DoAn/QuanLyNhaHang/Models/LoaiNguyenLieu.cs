namespace QuanLyNhaHang.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("LoaiNguyenLieu")]
    public partial class LoaiNguyenLieu
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public LoaiNguyenLieu()
        {
            NguyenLieu = new HashSet<NguyenLieu>();
        }

        [Key]
        public int MaLNL { get; set; }

        [Required]
        [StringLength(100)]
        public string TenLNL { get; set; }

        public int? TongSoLuong { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NguyenLieu> NguyenLieu { get; set; }
    }
}
