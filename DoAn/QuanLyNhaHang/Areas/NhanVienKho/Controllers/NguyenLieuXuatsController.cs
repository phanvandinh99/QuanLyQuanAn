using QuanLyNhaHang.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace QuanLyNhaHang.Areas.NhanVienKho.Controllers
{
    public class NguyenLieuXuatsController : Controller
    {
        private DatabaseQuanLyNhaHang db = new DatabaseQuanLyNhaHang();

        // GET: NhanVienKho/NguyenLieuXuat
        public ActionResult Index()
        {
            var nguyenLieuXuats = db.NguyenLieuXuat.Include(n => n.NguyenLieu).Include(n => n.XuatKho);
            return View(nguyenLieuXuats.ToList());
        }

        // GET: NhanVienKho/NguyenLieuXuat/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NguyenLieuXuat nguyenLieuXuat = db.NguyenLieuXuat.Find(id);
            if (nguyenLieuXuat == null)
            {
                return HttpNotFound();
            }
            return View(nguyenLieuXuat);
        }

        // GET: NhanVienKho/NguyenLieuXuat/Create
        public ActionResult Create()
        {
            ViewBag.MaNguyenLieu_id = new SelectList(db.NguyenLieu, "MaNguyenLieu", "TenNguyenLieu");
            ViewBag.MaXuatKho_id = new SelectList(db.XuatKho, "MaXuatKho", "MaXuatKho");
            return View();
        }

        // POST: NhanVienKho/NguyenLieuXuat/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaXuatKho_id,MaNguyenLieu_id,SoLuongXuat")] NguyenLieuXuat nguyenLieuXuat)
        {
            if (ModelState.IsValid)
            {
                db.NguyenLieuXuat.Add(nguyenLieuXuat);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MaNguyenLieu_id = new SelectList(db.NguyenLieu, "MaNguyenLieu", "TenNguyenLieu", nguyenLieuXuat.MaNguyenLieu_id);
            ViewBag.MaXuatKho_id = new SelectList(db.XuatKho, "MaXuatKho", "MaXuatKho", nguyenLieuXuat.MaXuatKho_id);
            return View(nguyenLieuXuat);
        }

        // GET: NhanVienKho/NguyenLieuXuat/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NguyenLieuXuat nguyenLieuXuat = db.NguyenLieuXuat.Find(id);
            if (nguyenLieuXuat == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaNguyenLieu_id = new SelectList(db.NguyenLieu, "MaNguyenLieu", "TenNguyenLieu", nguyenLieuXuat.MaNguyenLieu_id);
            ViewBag.MaXuatKho_id = new SelectList(db.XuatKho, "MaXuatKho", "MaXuatKho", nguyenLieuXuat.MaXuatKho_id);
            return View(nguyenLieuXuat);
        }

        // POST: NhanVienKho/NguyenLieuXuat/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaXuatKho_id,MaNguyenLieu_id,SoLuongXuat")] NguyenLieuXuat nguyenLieuXuat)
        {
            if (ModelState.IsValid)
            {
                db.Entry(nguyenLieuXuat).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaNguyenLieu_id = new SelectList(db.NguyenLieu, "MaNguyenLieu", "TenNguyenLieu", nguyenLieuXuat.MaNguyenLieu_id);
            ViewBag.MaXuatKho_id = new SelectList(db.XuatKho, "MaXuatKho", "MaXuatKho", nguyenLieuXuat.MaXuatKho_id);
            return View(nguyenLieuXuat);
        }

        // GET: NhanVienKho/NguyenLieuXuat/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NguyenLieuXuat nguyenLieuXuat = db.NguyenLieuXuat.Find(id);
            if (nguyenLieuXuat == null)
            {
                return HttpNotFound();
            }
            return View(nguyenLieuXuat);
        }

        // POST: NhanVienKho/NguyenLieuXuat/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            NguyenLieuXuat nguyenLieuXuat = db.NguyenLieuXuat.Find(id);
            db.NguyenLieuXuat.Remove(nguyenLieuXuat);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
