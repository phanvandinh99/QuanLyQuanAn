namespace QuanLyNhaHang.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("NguyenLieuXuat")]
    public partial class NguyenLieuXuat
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaXuatKho_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaNguyenLieu_id { get; set; }

        public double? SoLuongXuat { get; set; }

        public double? DonGia { get; set; }

        public double? ThanhTien { get; set; }

        public virtual NguyenLieu NguyenLieu { get; set; }

        public virtual XuatKho XuatKho { get; set; }
    }
}
