using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FishTrapTimer.Core
{
    public class Listener : Singleton<Listener>
    {
        public TcpListener server;
        public TcpClient connectedClient;
        private bool isStarted;
        public delegate void AppendText(string mat);
        public event AppendText Append;


        public void StartServer(int port)
        {
            if (isStarted == false)
            {
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                isStarted = true;
                Task.Run(() => StartAcceptClientAsync()).ContinueWith((arg) => Console.WriteLine("서버가 종료되었습니다."));
            }
        }

        public void StopServer()
        {
            isStarted = false;
            connectedClient?.Close();
            server.Stop();
        }

        private async Task StartAcceptClientAsync()//항상 연결대기한다.
        {
            while (true)
            {
                if (isStarted == false)
                {
                    break;
                }
                connectedClient = await server.AcceptTcpClientAsync();
                Append("클라이언트와 연결되었습니다.\n");
                BeginRead();
            }
        }
        public void BeginRead()
        {
            if (isStarted == true && connectedClient?.Connected ==true)
            {
                try
                {
                    var buffer = new byte[4096];
                    var ns = connectedClient.GetStream();
                    ns.BeginRead(buffer, 0, buffer.Length, EndRead, buffer);
                }
                catch
                {

                }
            }
        }

        public void EndRead(IAsyncResult result)
        {
            try
            {
                var buffer = (byte[])result.AsyncState;
                var ns = connectedClient.GetStream();
                var bytesAvailable = ns.EndRead(result);
                var msg = Encoding.Unicode.GetString(buffer, 0, bytesAvailable);

                if (bytesAvailable > 0)
                {
                    Append(msg+'\n');
                    BeginRead();
                }
                else
                {
                    throw new Exception();
                }
            }
            catch//연결 강제로 끊김.
            {
                Append("클라이언트가 종료되었습니다.\n");
                connectedClient.Close();
                connectedClient.Dispose();
            }
        }

        public void BeginSend(string xml)
        {
            if (connectedClient?.Connected == true)
            {
                var bytes = Encoding.Unicode.GetBytes(xml);
                var ns = connectedClient.GetStream();
                ns.BeginWrite(bytes, 0, bytes.Length, EndSend, bytes);
            }
        }

        public void EndSend(IAsyncResult result)
        {
            var bytes = (byte[])result.AsyncState;
            Console.WriteLine("Sent  {0} bytes to server.", bytes.Length);
            Console.WriteLine("Sent: {0}", Encoding.Unicode.GetString(bytes));
        }

    }
}
