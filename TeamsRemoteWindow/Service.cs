using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using System;
using System.IO;
using Ninject;
using System.Diagnostics;

namespace TeamsRemote
{
    class Service
    {
        private static TraceSource Trace = new TraceSource("Trace");
        public static readonly IKernel NinjectRegistry = new StandardKernel();

        string baseAddress = "http://localhost:8080/";

        private IDisposable webApp;
        private ArduinoCommunicator serialCommunicator;
        private Settings settings;
        private TeamsRemote teamsRemote;

        public Service()
        {
            using (StreamReader sr = new StreamReader("settings.json"))
            {
                string line = sr.ReadToEnd();
                settings = JsonConvert.DeserializeObject<Settings>(line);
            }
            
            
            serialCommunicator = new ArduinoCommunicator(settings);
            teamsRemote = new TeamsRemote();
            NinjectRegistry.Bind<TeamsRemote>().ToMethod((c) => { return teamsRemote; });
            NinjectRegistry.Bind<ArduinoCommunicator>().ToMethod((c) => { return serialCommunicator; });
            NinjectRegistry.Bind<Settings>().ToMethod((c) => { return settings; });
        }

        public void Start()
        {
            webApp = WebApp.Start<Startup>(url: baseAddress);
            
            Trace.TraceInformation("Web Server is running: " + baseAddress + "api/teamsremote");
        }

        public void Stop()
        {
            webApp.Dispose();
        }
    }
}
