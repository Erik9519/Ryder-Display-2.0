using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RyderDisplay.Components.Network
{
    public class RyderClient
    {
        public interface Callback
        {
            void OnReceive(string cmd, object json);
        }

        /* Connectionn details */
        private string ip, pswd;
        private int port;
        private int timeout = 5000;
        /* Connection status */
        private bool serverThreadRunning, connecting, abort;
        private TcpClient ryderEngine;
        private NetworkStream dataStream;
        private Stopwatch stopwatch;
        private Semaphore m;
        /* Enpoints */
        Dictionary<string, List<Callback>> endpoints = new Dictionary<string, List<Callback>>();

        public RyderClient() {
            this.connecting = this.serverThreadRunning = false;
            this.abort = true;
            this.stopwatch = new Stopwatch();
            this.m = new Semaphore(1, 1);
    }

        public void setup(string ip, int port, string pswd)
        {
            this.ip = ip; this.port = port; this.pswd = pswd;
        }

        public void connect()
        {
            // Ensure only a single connect attempt is running at any given time
            this.m.WaitOne();
            // Initiate connection to Ryder Engine
            this.connecting = true; this.abort = false;
            this.ryderEngine = new TcpClient();
            this.ryderEngine.BeginConnect(IPAddress.Parse(this.ip), this.port, this.onConnected, this.ryderEngine);
            this.stopwatch.Restart();
        }

        public void disconnect() {
            // Signal Ryder Engine thread to stop
            this.m.WaitOne();
            this.abort = true;
            while (this.serverThreadRunning);
            this.m.Release();
        }

        public void sendMsg(string cmd, string content)
        {
            // Ensure multi-thread compatibility
            this.m.WaitOne();
            this.sendMsgWithoutLock(cmd, content);
            this.m.Release();
        }

        private void sendMsgWithoutLock(string cmd, string content)
        {
            string msg = "[\"" + cmd + "\"," + content + "]";
            try {
                // Write Message Length
                string msgLen = String.Format("{0,8:D8}", (msg.Length + 1));
                dataStream.Write(Encoding.ASCII.GetBytes(msgLen + "\n"), 0, msgLen.Length + 1);
                // Write Message
                dataStream.Write(Encoding.ASCII.GetBytes(msg + "\n"), 0, msg.Length + 1);
            } catch (Exception e) { /*This Exception occurs when data is sent without a connection*/ }
        }

        public void addEndpoint(string cmd, Callback callback)
        {
            // Add command if it does not exist
            if (!endpoints.ContainsKey(cmd))
                endpoints.Add(cmd, new List<Callback>());
            // Prohibit duplicate entries
            if (!endpoints[cmd].Contains(callback))
                endpoints[cmd].Add(callback);
        }

        public void removeEndpoint(string cmd, Callback callback)
        {
            if (endpoints.ContainsKey(cmd))
                endpoints[cmd].Remove(callback);
        }

        public void clearEndpoints()
        {
            endpoints.Clear();
        }

        private void onConnected(IAsyncResult result)
        {
            try
            {
                this.ryderEngine.EndConnect(result);
                if (!this.serverThreadRunning)
                {
                    this.serverThreadRunning = true;
                    new Thread(new ThreadStart(this.runServer)).Start();
                }
                this.dataStream = this.ryderEngine.GetStream();
                this.sendMsgWithoutLock("authCode", "\"" + this.pswd + "\"");
            }
            catch (SocketException) { /* This Exception occurs when a connection could not be established */ }
            this.connecting = false;
            this.m.Release();
        }

        private void runServer()
        {
            int receivedLen = 0, msgLen = 9;
            bool msgReadStep = false;
            byte[] msgBuff = new byte[msgLen];
            // Run until we close the connection
            while (!this.abort)
            {
                if (!this.connecting)
                {
                    if (this.stopwatch.ElapsedMilliseconds < this.timeout && this.ryderEngine.Connected)
                    {
                        // Keep reading for as long as there is data available
                        while (this.dataStream.DataAvailable)
                        {
                            receivedLen += this.dataStream.Read(msgBuff, receivedLen, msgLen - receivedLen);
                            // Check received data only upon complete receival
                            if (receivedLen == msgLen)
                            {
                                string msg = Encoding.ASCII.GetString(msgBuff);
                                if (!msgReadStep)
                                {
                                    // Read upcoming message length
                                    msgLen = int.Parse(msg);
                                }
                                else
                                {
                                    // Parse message to JSON
                                    Dictionary<string, object> values = (Dictionary<string, object>)JsonConvert.DeserializeObject<IDictionary<string, object>>(
                                        msg, new JsonConverter[] { new JsonDeserializerToDictionaries() }
                                    );
                                    // Call endpoints
                                    if (this.endpoints.ContainsKey((string)values["0"]))
                                    {
                                        for (int i = 0; i < this.endpoints[(string)values["0"]].Count; i++)
                                            this.endpoints[(string)values["0"]][i].OnReceive((string)values["0"], values["1"]);
                                    }
                                    msgLen = 9;
                                }
                                // Reset
                                msgBuff = new byte[msgLen]; receivedLen = 0; msgReadStep = !msgReadStep;
                            }
                            this.stopwatch.Restart();
                        }
                    }
                    else if (this.stopwatch.ElapsedMilliseconds > this.timeout)
                    {
                        this.ryderEngine.Close(); this.connect();
                    }
                }
                Thread.Sleep(5);
            }
            this.ryderEngine.Close(); this.abort = this.serverThreadRunning = false;
        }
    }
}
