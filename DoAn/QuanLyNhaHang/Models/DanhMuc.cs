namespace QuanLyNhaHang.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("DanhMuc")]
    public partial class DanhMuc
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DanhMuc()
        {
            PhanQuyen = new HashSet<PhanQuyen>();
        }

        [Key]
        public int MaDanhMuc { get; set; }

        [Required]
        [StringLength(50)]
        public string Icon { get; set; }

        [Required]
        [StringLength(50)]
        public string TenChucNang { get; set; }

        [Required]
        [StringLength(50)]
        public string Url { get; set; }

        [Required]
        [StringLength(150)]
        public string MoTa { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PhanQuyen> PhanQuyen { get; set; }
    }
}
