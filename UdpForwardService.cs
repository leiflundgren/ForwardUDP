using ForwardUDP.Components;
using ForwardUDP.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ForwardUDP
{
    public partial class UdpForwardService : ServiceBase
    {
        private FwdUDP forwarder;


        public UdpForwardService()
        {
            InitializeComponent();
        }

        public void ConsoleStart(string[] args)
        {
            OnStart(args);
        }
        public void ConsoleStop()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)        
        {
            IPEndPoint ParseIP(params string[] strings)
            {
                foreach (string s in strings)
                {
                    if (string.IsNullOrEmpty(s)) continue;
                    if (IPEndPointExtensions.TryParse(s, out IPEndPoint ep))
                        return ep;
                }
                return null;
            }


            base.OnStart(args);

            IPEndPoint listenTo = ParseIP( args.GetCommandLineArg("local"), MySettings.Default.Local);
            IPEndPoint target = ParseIP( args.GetCommandLineArg("target"), MySettings.Default.Target);

            forwarder = new FwdUDP(listenTo, target);
            
        }

        protected override void OnStop()
        {
            FwdUDP forwarder = this.forwarder;
            this.forwarder = null;
            if (forwarder != null)
            {
                forwarder.Dispose();
            }
        }

        internal void Run()
        {
            throw new NotImplementedException();
        }

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.ServiceName = "FwdUDP";
        }
    }
}
