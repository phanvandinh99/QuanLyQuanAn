namespace QuanLyNhaHang.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Ban")]
    public partial class Ban
    {
        [Key]
        public int MaBan { get; set; }

        [Required]
        [StringLength(50)]
        public string TenBan { get; set; }

        public int SoGhe { get; set; }

        public int? Vip { get; set; }

        public int? TinhTrang { get; set; }

        public int? MaTang_id { get; set; }

        public string MaDoangNghiep_id { get; set; }

        public virtual Tang Tang { get; set; }
    }
}
