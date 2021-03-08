using Ninject;
using System.Web.Http;

namespace TeamsRemote
{
    public class TeamsRemoteController : ApiController
    {
        private static string lastStatus = "";

        public static void ResetController()
        {
            lastStatus = "";
        }

        // GET api/values 
        public IHttpActionResult Post([FromUri] string status)
        {
            if (!lastStatus.Equals(status))
            {
                var arduino = Service.NinjectRegistry.Get<ArduinoCommunicator>();
                arduino.SendMessage(status);
                lastStatus = status;
            }
            
            return Ok("ok");
        }

    }
}
