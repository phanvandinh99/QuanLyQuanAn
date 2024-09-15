namespace QuanLyNhaHang.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("ChiTietPhieuNhap")]
    public partial class ChiTietPhieuNhap
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaNguyenLieu_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaPhieuNhap_id { get; set; }

        public double? SoLuongNhap { get; set; }

        public double GiaNhap { get; set; }

        public double? ThanhTien { get; set; }

        [Required]
        [StringLength(50)]
        public string MaDoanhNghiep_id { get; set; }

        public virtual NguyenLieu NguyenLieu { get; set; }

        public virtual PhieuNhap PhieuNhap { get; set; }
    }
}
