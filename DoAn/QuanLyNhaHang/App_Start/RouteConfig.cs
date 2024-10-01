using System.Web.Mvc;
using System.Web.Routing;

namespace QuanLyNhaHang
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Nhân viên Admin
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { Controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "QuanLyNhaHang.Areas.NhanVien.Controllers" }
            ).DataTokens.Add("area", "NhanVien");
        }
    }
}

