﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x_BIMU_Logger
{
    /// <summary>
    /// Packet counter. Tracks number of packets received and packet rate.
    /// </summary>
    class PacketCounter
    {
        /// <summary>
        /// Timer to calculate packet rate.
        /// </summary>
        private System.Windows.Forms.Timer timer;

        /// <summary>
        /// Number of packets received.
        /// </summary>
        public int PacketsReceived { get; private set; }

        /// <summary>
        /// Packet receive rate as packets per second.
        /// </summary>
        public int PacketRate { get; private set; }

        /// <summary>
        /// Used to calculate packet rate.
        /// </summary>
        private DateTime prevTime;

        /// <summary>
        /// Used to calculate packet rate.
        /// </summary>
        private int prevPacketsReceived;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PacketCounter()
        {
            // Initialise variables
            prevPacketsReceived = 0;
            PacketsReceived = 0;

            // Setup timer
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        /// <summary>
        /// Increments packet counter.
        /// </summary>
        public void Increment()
        {
            PacketsReceived++;
        }

        // Zeros packet counter.
        public void Reset()
        {
            prevPacketsReceived = 0;
            PacketsReceived = 0;
            PacketRate = 0;
        }

        /// <summary>
        /// timer Tick event to calculate packet rate.
        /// </summary>
        void timer_Tick(object sender, EventArgs e)
        {
            DateTime nowTime = DateTime.Now;
            TimeSpan t = nowTime - prevTime;
            prevTime = nowTime;
            PacketRate = (int)((float)(PacketsReceived - prevPacketsReceived) / ((float)t.Seconds + (float)t.Milliseconds * 0.001f));
            prevPacketsReceived = PacketsReceived;
        }
    }
}