namespace QuanLyNhaHang.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ChiTietHoaDon")]
    public partial class ChiTietHoaDon
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaHoaDon_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaMonAn_id { get; set; }

        public int SoLuongMua { get; set; }

        public double ThanhTien { get; set; }

        public DateTime? NgayGoi { get; set; }

        public virtual HoaDon HoaDon { get; set; }

        public virtual MonAn MonAn { get; set; }
    }
}
