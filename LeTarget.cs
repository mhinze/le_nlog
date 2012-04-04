﻿/*
    Logentries Log4Net Logging agent
    Copyright 2010,2011 Logentries, Jlizard
    Mark Lacomber <marklacomber@gmail.com>
                                            */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Security;
using System.Net.Sockets;
using System.IO;

using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Internal;
using NLog.Internal.NetworkSenders;
using NLog.Layouts;
using NLog.Targets;

namespace Le
{
    [Target("Logentries")]
    public sealed class LeTarget : TargetWithLayout
    {
        private SslStream sslSock = null;
        private TcpClient leSocket = null;
        private System.Text.UTF8Encoding encoding;

        public LeTarget()
        {

        }

        string GetKey()
        {
            return ConfigurationManager.AppSettings["LOGENTRIES_ACCOUNT_KEY"];
        }

        string GetLocation()
        {
            return ConfigurationManager.AppSettings["LOGENTRIES_LOCATION"];
        }

        [RequiredParameter]
        public bool Debug { get; set; }

        public bool KeepConnection { get; set; }

        private void createSocket(String key, String location)
        {
            this.leSocket = new TcpClient("api.logentries.com", 443);
            this.leSocket.NoDelay = true;
            this.sslSock = new SslStream(this.leSocket.GetStream());
            this.encoding = new System.Text.UTF8Encoding();

            this.sslSock.AuthenticateAsClient("logentries.com");

            String output = "PUT /" + key + "/hosts/" + location + "/?realtime=1 HTTP/1.1\r\n";
            this.sslSock.Write(this.encoding.GetBytes(output), 0, output.Length);
            output = "Host: api.logentries.com\r\n";
            this.sslSock.Write(this.encoding.GetBytes(output), 0, output.Length);
            output = "Accept-Encoding: identity\r\n";
            this.sslSock.Write(this.encoding.GetBytes(output), 0, output.Length);
            output = "Transfer_Encoding: chunked\r\n\r\n";
            this.sslSock.Write(this.encoding.GetBytes(output), 0, output.Length);
        }

        private byte[] GetBytesToWrite(LogEventInfo logEvent)
        {
            string text = this.Layout.Render(logEvent) + "\r\n";

            return this.encoding.GetBytes(text);
        }

        protected override void Write(LogEventInfo logEvent)
        {
            if (this.sslSock == null)
            {
                try
                {
                    this.createSocket(this.GetKey(), this.GetLocation());
                }
                catch (Exception e)
                {
                    WriteDebugMessages("Error connecting to Logentries", e);
                }
            }

            byte[] message = this.GetBytesToWrite(logEvent);

            try
            {
                this.sendToLogentries(message);
            }
            catch (Exception)
            {
                try
                {
                    this.createSocket(this.GetKey(), this.GetLocation());
                    this.sendToLogentries(message);
                }
                catch (Exception ex)
                {
                    WriteDebugMessages("Error sending log to Logentries", ex);
                }
            }
        }

        private void sendToLogentries(byte[] message)
        {
            this.sslSock.Write(message, 0, message.Length);
        }

        private void WriteDebugMessages(string message, Exception e)
        {
            if (!this.Debug) return;
            string[] messages = { message, e.ToString() };
            foreach (var msg in messages)
            {
                System.Diagnostics.Debug.WriteLine(msg);
                Console.Error.WriteLine(msg);
            }
        }
    }
}