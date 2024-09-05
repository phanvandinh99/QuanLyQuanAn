namespace QuanLyNhaHang.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("LoaiMonAn")]
    public partial class LoaiMonAn
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public LoaiMonAn()
        {
            MonAn = new HashSet<MonAn>();
        }

        [Key]
        public int MaLMA { get; set; }

        [Required]
        [StringLength(100)]
        public string TenLMA { get; set; }

        public int? TongSoLuong { get; set; }

        public string MaDoangNghiep_id { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MonAn> MonAn { get; set; }
    }
}
