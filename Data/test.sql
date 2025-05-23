USE master
go
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'QuanLyNhaHang')
BEGIN
    ALTER DATABASE QuanLyNhaHang SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QuanLyNhaHang; 
END
GO
CREATE DATABASE QuanLyNhaHang
go
use QuanLyNhaHang
go
CREATE TABLE DoanhNghiep
(
	MaDoanhNghiep nvarchar(50) Primary key,
	TenDoanhNghiep nvarchar(200) NOT NULL,
	SoDienThoai varchar(10) NOT NULL,
	Email varchar(100) NULL,
	DiaChi nvarchar(255) NOT NULL,
	ThoiGianBatDau datetime NOT NULL,
	ThoiGianKetThuc datetime NOT NULL,
	TongTien float NULL,
)
go
CREATE TABLE Tang(
	MaTang int IDENTITY(1,1) Primary key,
	TenTang nvarchar(50) NOT NULL,
	 
	MaDoanhNghiep_id nvarchar(50) NOT NULL
)
go
CREATE TABLE Ban
(
	MaBan int IDENTITY(1,1) Primary key,
	TenBan nvarchar(50) NOT NULL,
	SoGhe int NOT NULL DEFAULT ((4)),
	Vip int NULL DEFAULT ((0)),
	TinhTrang int NULL DEFAULT ((0)),
	MangDi bit default(0),

	MaTang_id int NULL,
	MaDoanhNghiep_id nvarchar(50) NOT NULL,

	Constraint fk_Ban_Tang foreign key(MaTang_id) references Tang(MaTang)
)
go
CREATE TABLE HoaDon
(
	MaHoaDon int IDENTITY(1,1) Primary key,
	TenKhachHang nvarchar(100) NULL DEFAULT (N'Tại Bàn'),
	SDTKhachHang varchar(10) NULL,
	DiaChiKhachHang nvarchar(255) NULL,
	PhiShip float null,
	GiamGiaTienMat float null,
	GiamGiaPhanTram float null,
	NgayTao datetime NULL DEFAULT (getdate()),
	NgayThanhToan datetime NULL,
	GhiChu nvarchar(200) NULL,
	TongTien float NOT NULL,
	TrangThai int NULL DEFAULT ((0)),

	MaBan_id int NULL,
	MaDoanhNghiep_id nvarchar(50) NOT NULL,
	
	Constraint fk_HoaDon_Ban foreign key(MaBan_id) references Ban(MaBan)
)
go
CREATE TABLE LoaiMonAn
(
	MaLMA int IDENTITY(1,1) Primary key,
	TenLMA nvarchar(100) NOT NULL,
	TongSoLuong int NULL,

	MaDoanhNghiep_id nvarchar(50) NOT NULL,
)
go
CREATE TABLE MonAn
(
	MaMonAn int IDENTITY(1,1)  Primary key,
	TenMonAn nvarchar(255) NOT NULL,
	HinhAnh nvarchar(100) NULL,
	DonGia float NULL,
	NgayCapNhat date NULL,
	ThongTin nvarchar(max) NULL,
	MoTa nvarchar(max) NULL,
	SoLuongDaBan int NULL,

	MaLMA_id int NULL,
	MaDoanhNghiep_id nvarchar(50) NOT NULL,

	Constraint fk_MonAn_LoaiMonAn foreign key(MaLMA_id) references LoaiMonAn(MaLMA)
)
go
CREATE TABLE ChiTietHoaDon
(
	MaHoaDon_id int NOT NULL,
	MaMonAn_id int NOT NULL,
	primary key(MaHoaDon_id, MaMonAn_id),
	SoLuongMua int NOT NULL,
	ThanhTien float NOT NULL,
	NgayGoi datetime NULL,

	Constraint fk_ChiTietHoaDon_HoaDon foreign key(MaHoaDon_id) references HoaDon(MaHoaDon),
	Constraint fk_ChiTietHoaDon_MonAn foreign key(MaMonAn_id) references MonAn(MaMonAn)
)
go
CREATE TABLE LichSuGoiMon
(
	MaLichSu int IDENTITY(1,1) primary key,
	SoLuongMua int NOT NULL,
	SoLuongTra int NULL,
	ThoiGianGoi datetime NULL,
	ThoiGianTra datetime NULL,
	MaHoaDon_id int NULL,
	MaMonAn_id int NULL,

	Constraint fk_LichSuGoiMon_HoaDon foreign key(MaHoaDon_id) references HoaDon(MaHoaDon),
	Constraint fk_LichSuGoiMon_MonAn foreign key(MaMonAn_id) references MonAn(MaMonAn)
)
go
CREATE TABLE LoaiNguyenLieu
(
	MaLNL int IDENTITY(1,1) Primary key,
	TenLNL nvarchar(100) NOT NULL,
	TongSoLuong int NULL,
)
go
CREATE TABLE NguyenLieu
(
	MaNguyenLieu int IDENTITY(1,1) Primary key,
	TenNguyenLieu nvarchar(200) NOT NULL,
	SoLuongHienCon float NULL,
	GhiChu nvarchar(255) NULL,
	GiaNhapCuoi float NOT NULL,

	MaLNL_id int NULL,
	MaDoanhNghiep_id nvarchar(50) NOT NULL,

	Constraint fk_NguyenLieu_LoaiNguyenLieu foreign key(MaLNL_id) references LoaiNguyenLieu(MaLNL)
)
go
CREATE TABLE XuatKho
(
	MaXuatKho int IDENTITY(1,1) Primary key,
	NgayXuatKho datetime NOT NULL,

	MaDoanhNghiep_id nvarchar(50) NOT NULL,
)
go
CREATE TABLE NguyenLieuXuat
(
	MaXuatKho_id int NOT NULL,
	MaNguyenLieu_id int NOT NULL,
	primary key(MaXuatKho_id, MaNguyenLieu_id),
	SoLuongXuat float,
	DonGia float NULL,
	ThanhTien float NULL,

	Constraint fk_NguyenLieuXuat_XuatKho foreign key(MaXuatKho_id) references XuatKho(MaXuatKho),
	Constraint fk_NguyenLieuXuat_NguyenLieu foreign key(MaNguyenLieu_id) references NguyenLieu(MaNguyenLieu)
)
go
CREATE TABLE HoanTra
(
	MaHoanTra int IDENTITY(1,1) Primary key,
	NgayHoanTra datetime NOT NULL,

	MaDoanhNghiep_id nvarchar(50) NOT NULL,
)
go
CREATE TABLE NguyenLieuTra
(
	MaHoanTra_id int NOT NULL,
	MaNguyenLieu_id int NOT NULL,
	primary key(MaHoanTra_id, MaNguyenLieu_id),
	SoLuongTra float,
	DonGia float NULL,
	ThanhTien float NULL,

	MaDoanhNghiep_id nvarchar(50) NOT NULL,
	Constraint fk_NguyenLieuTra_HoanTra foreign key(MaHoanTra_id) references HoanTra(MaHoanTra),
	Constraint fk_NguyenLieuTra_NguyenLieu foreign key(MaNguyenLieu_id) references NguyenLieu(MaNguyenLieu)
)
go
CREATE TABLE PhieuNhap
(
	MaPhieuNhap int IDENTITY(1,1) Primary key,
	NgayNhap date NULL,
	TongTien float NULL,
	MaNhanVien_id varchar(50) NULL,

	MaDoanhNghiep_id nvarchar(50) NOT NULL,
)
go
CREATE TABLE ChiTietPhieuNhap
(
	MaNguyenLieu_id int NOT NULL,
	MaPhieuNhap_id int NOT NULL,
	Primary key(MaNguyenLieu_id, MaPhieuNhap_id),
	SoLuongNhap float NULL,
	GiaNhap float NOT NULL,
	ThanhTien float NULL,

	MaDoanhNghiep_id nvarchar(50) NOT NULL,

	Constraint fk_ChiTietPhieuNhap_NguyenLieu foreign key(MaNguyenLieu_id) references NguyenLieu(MaNguyenLieu),
	Constraint fk_ChiTietPhieuNhap_PhieuNhap foreign key(MaPhieuNhap_id) references PhieuNhap(MaPhieuNhap)
)
go
CREATE TABLE Quyen
(
	MaQuyen int IDENTITY(1,1) Primary key,
	TenQuyen nvarchar(50) NOT NULL,
	GhiChu nvarchar(100) NULL,
)
go
CREATE TABLE NhanVien
(
	MaNhanVien int IDENTITY(1,1) Primary key,
	TaiKhoanNV varchar(50) NOT NULL,
	MatKhauNV varchar(50) NOT NULL,
	DoiMatKhau bit NULL,
	TenNhanVien nvarchar(100) NOT NULL,
	NgaySinh date NULL,
	SoDienThoai varchar(12) NULL,
	Email varchar(200) NOT NULL,
	MaQuyen_id int NULL,

	MaDoanhNghiep_id nvarchar(50) NOT NULL,

	Constraint fk_NhanVien_Quyen foreign key(MaQuyen_id) references Quyen(MaQuyen)
)
go
CREATE TABLE DanhMuc
(
	MaDanhMuc int IDENTITY(1,1) Primary key,
	Icon varchar(50) not null,
	TenChucNang nvarchar(50) NOT NULL,
	Url varchar(50) NOT NULL,
	MoTa nvarchar(150) NOT NULL,
)
go
CREATE TABLE PhanQuyen
(
    MaNhanVien_id int NOT NULL,
    MaDanhMuc_id int NOT NULL,
    PRIMARY KEY(MaNhanVien_id, MaDanhMuc_id),

    CONSTRAINT fk_PhanQuyen_NhanVien FOREIGN KEY(MaNhanVien_id) REFERENCES NhanVien(MaNhanVien) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_PhanQuyen_DanhMuc FOREIGN KEY(MaDanhMuc_id) REFERENCES DanhMuc(MaDanhMuc) ON DELETE CASCADE ON UPDATE CASCADE
);

SET IDENTITY_INSERT DanhMuc ON 

INSERT DanhMuc (MaDanhMuc, Icon, TenChucNang, Url, MoTa) 
VALUES (1, 'screen', N'Bàn Hóa Đơn', '#products', N'Chức năng tạo hóa đơn');

-- Loại món ăn
INSERT DanhMuc (MaDanhMuc, Icon, TenChucNang, Url, MoTa) 
VALUES (2, 'tag', N'Loại Món Ăn', '/NhanVien/LoaiMonAn/Index', N'Quản lý loại món ăn');
INSERT DanhMuc (MaDanhMuc, Icon, TenChucNang, Url, MoTa) 
VALUES (3, 'cupcake', N'Món Ăn', '/NhanVien/MonAn/DanhSachMonAn', N'Chức năng quản lý món ăn');
INSERT DanhMuc (MaDanhMuc, Icon, TenChucNang, Url, MoTa) 
VALUES (4, 'help', N'Nguyên Liệu', '/NhanVien/NguyenLieu/Index', N'Quản lý nguyên liệu');

-- Khu vực
INSERT DanhMuc (MaDanhMuc, Icon, TenChucNang, Url, MoTa) 
VALUES (5, 'screen', N'Tầng', '/NhanVien/Tang/DanhSachTang', N'Quản Lý Tầng');
INSERT DanhMuc (MaDanhMuc, Icon, TenChucNang, Url, MoTa) 
VALUES (6, 'screen', N'Bàn', '/NhanVien/Ban/DanhSachBan', N'Quản lý bàn ăn');

-- Báo cáo
INSERT DanhMuc (MaDanhMuc, Icon, TenChucNang, Url, MoTa) 
VALUES (7, 'file-text', N'Hóa Dơn', '/NhanVien/HoaDon/DanhSachHoaDon', N'Chức năng quản lý hóa đơn');
INSERT DanhMuc (MaDanhMuc, Icon, TenChucNang, Url, MoTa) 
VALUES (8, 'invoice', N'Thống Kê', '/NhanVien/ThongKe/Index', N'Thống kê doanh thu');

-- Cài đặt
INSERT DanhMuc (MaDanhMuc, Icon, TenChucNang, Url, MoTa) 
VALUES (9, 'shop', N'QL Nhà Hàng', '/NhanVien/DoanhNghiep/Index', N'Chức năng quản lý doanh nghiệp');
INSERT DanhMuc (MaDanhMuc, Icon, TenChucNang, Url, MoTa) 
VALUES (10, 'user', N'QL Nhân Viên', '/NhanVien/NhanVien/Index', N'Chức năng quản lý nhân viên');

go
SET IDENTITY_INSERT DanhMuc OFF
go
SET IDENTITY_INSERT Tang ON 
INSERT Tang (MaTang, TenTang, MaDoanhNghiep_id) 
VALUES (1, N'Tầng 1', 'AnhTran')
SET IDENTITY_INSERT Tang OFF
GO
SET IDENTITY_INSERT Ban ON 
INSERT Ban (MaBan, TenBan, SoGhe, Vip, TinhTrang, MangDi, MaTang_id, MaDoanhNghiep_id) 
VALUES (1, N'Mang Đi', 4, 0, 0, 1, 1, 'AnhTran')
SET IDENTITY_INSERT Ban OFF
GO
INSERT DoanhNghiep (MaDoanhNghiep, TenDoanhNghiep, SoDienThoai, Email, DiaChi, ThoiGianBatDau, ThoiGianKetThuc, TongTien) 
VALUES ('AnhTran', N'Anh Trần', '0982345678', N'AnhTran@gmail.com', N'Đà Nẵng', '05/09/2024', '05/09/2050', 0)
go
SET IDENTITY_INSERT Quyen ON 
INSERT Quyen (MaQuyen, TenQuyen, GhiChu) VALUES (1, N'SupperAdmin', N'Quyền Supper Admin')
INSERT Quyen (MaQuyen, TenQuyen, GhiChu) VALUES (2, N'Admin', N'Quyền Admin')
INSERT Quyen (MaQuyen, TenQuyen, GhiChu) VALUES (3, N'Client', N'Nhân Viên')
SET IDENTITY_INSERT Quyen OFF
go
INSERT NhanVien (TaiKhoanNV, MatKhauNV, TenNhanVien, NgaySinh, SoDienThoai, Email, MaQuyen_id, MaDoanhNghiep_id) 
VALUES ('AnhTran123', N'Abc123', N'Cao Anh Trần', CAST(N'1997-02-02' AS Date), N'0987654321', 'Vonhanh271@gmail.com', 1, 'AnhTran')