using GodSharp.SerialPort;
using Ninject;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;

namespace TeamsRemote
{
    public class ArduinoCommunicator : IDisposable
    {

        private static TraceSource Trace = new TraceSource("Trace");
        private readonly object sendLock = new object();
        private readonly Settings settings;
        private GodSerialPort myPort;
        readonly Timer keepAlive;
        public ArduinoCommunicator(Settings settings)
        {
            this.settings = settings;
            this.Initialize();

            keepAlive = new Timer(100000) { AutoReset = true };
            keepAlive.Elapsed += (s, e) => SendMessage("keepAlive");
            keepAlive.Start();
        }

        private void Initialize()
        {
            var portName = settings.COMPort;

            try
            {
                portName = GodSerialPort.GetPortNames().Single<string>();
            }
            catch
            {
            }

            myPort = new GodSerialPort(portName, baudRate: 9600, parity: 0);
            myPort.UseDataReceived(true, OnDataReceived);
            myPort.OnPinChange(action: (port, e) => { Trace.TraceEvent(TraceEventType.Error, 3, "pinchanged:" + e.ToString()); });
            myPort.OnError(action: (port, e) => { Trace.TraceEvent(TraceEventType.Error, 1,"error:" + e.ToString()); });

            myPort.Open();

        }

        private void OnDataReceived(GodSerialPort arg1, byte[] arg2)
        {
            var received = Encoding.ASCII.GetString(arg2, 0, arg2.Length).Trim();
            var remote = Service.NinjectRegistry.Get<TeamsRemote>();

            switch (received)
            {
                case "mic":
                    Trace.TraceInformation("Toggle mute status...");
                    remote.ToggleMute();
                    break;
                case "hand":
                    Trace.TraceInformation("Toggle raise hand...");
                    remote.ToggleRaiseHand();
                    break;
                case "cam":
                    Trace.TraceInformation("Toggle camera...");
                    remote.ToggleCamera();
                    break;
                default:
                    Trace.TraceInformation("Received: "+received);
                    break;
            }

        }

        public bool SendMessage(string message)
        {
            lock (sendLock)
            {
                try
                {
                    Trace.TraceInformation($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}");
                    myPort.WriteLine(message);
                }
                catch (Exception e)
                {
                    Trace.TraceEvent(TraceEventType.Error,2, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: Anschluss geschlossen/Gerät nicht angeschlossen, versuche neu zu verbinden...");
                    this.Initialize();
                    TeamsRemoteController.ResetController();
                }
            }
            return true;
        }

        public void Dispose()
        {
            myPort.Close();
        }
    }
}
