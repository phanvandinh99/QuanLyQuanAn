using QuanLyNhaHang.Common;
using QuanLyNhaHang.Models;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Unity;
using Unity.Mvc5;

namespace QuanLyNhaHang
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Cấu hình Unity
            var container = BuildUnityContainer();
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));

            // Đăng ký kiểm tra login toàn cục
            GlobalFilters.Filters.Add(new AuthAttribute());
        }

        private IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();

            container.RegisterType<DatabaseQuanLyNhaHang, DatabaseQuanLyNhaHang>();

            return container;
        }
    }
}
