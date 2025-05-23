namespace QuanLyNhaHang.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("HoanTra")]
    public partial class HoanTra
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public HoanTra()
        {
            NguyenLieuTra = new HashSet<NguyenLieuTra>();
        }

        [Key]
        public int MaHoanTra { get; set; }

        public DateTime NgayHoanTra { get; set; }

        [Required]
        [StringLength(50)]
        public string MaDoanhNghiep_id { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NguyenLieuTra> NguyenLieuTra { get; set; }
    }
}
