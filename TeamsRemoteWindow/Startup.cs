using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Ninject;
using Owin;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace TeamsRemote
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.JsonFormatter.SerializerSettings =
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            app.UseNinject(CreateKernel);
            app.UseWebApi(config);
        }

        public static IKernel CreateKernel()
        {
            return Service.NinjectRegistry;
        }
    }

}