using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Text.RegularExpressions;

namespace x_BIMU_Logger
{
    /// <summary>
    /// x-BMU Interface class to handle serial port, serial stream decoding and wirting to CSV files.
    /// </summary>
    class XBimuInterface
    {
        /// <summary>
        /// SerialPort to communicate with XBee module.
        /// </summary>
        private SerialPort serialPort;

        /// <summary>
        /// Flag to indicate if recption of XStick channel value is active.
        /// </summary>
        private bool xStickRxActive;

        /// <summary>
        /// Buffer for decoding received XStick channel value.
        /// </summary>
        private string xStickRxBuffer;

        /// <summary>
        /// Received XStick channel value.  A value of -1 indicates not connected.
        /// </summary>
        public int XStickChannel { get; private set; }

        /// <summary>
        /// SerialStreamDecoder to decode serial data stream into packets.
        /// </summary>
        private SerialDecoder serialDecoder;

        /// <summary>
        /// Packet counter to calculate performance statics.
        /// </summary>
        public PacketCounter PacketCounter { get; private set; }

        /// <summary>
        /// CSV file writer.
        /// </summary>
        private CsvFileWriter csvFileWriter;

        /// <summary>
        /// Revieed battery voltage.  A value of 9 indicates value has not yet been recieved.
        /// </summary>
        public float BatteryVoltage { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public XBimuInterface()
        {
            // Initialise class members
            xStickRxActive = false;
            xStickRxBuffer = "";
            XStickChannel = -1;
            serialDecoder = new SerialDecoder();
            PacketCounter = new PacketCounter();
            csvFileWriter = null;

            // Anonymous function to handle quaternion packet received event
            serialDecoder.QuaternionReceived += new SerialDecoder.onQuaternionReceived(
                delegate(int[] i)
                {
                    PacketCounter.Increment();
                    if (csvFileWriter != null)
                    {
                        csvFileWriter.WriteQuaternionData(i[0], i[1], i[2], i[3], i[4]);
                    }
                }
            );

            // Anonymous function to handle sensor packet received event
            serialDecoder.SensorsReceived += new SerialDecoder.onSensorsReceived(
                delegate(int[] i)
                {
                    PacketCounter.Increment();
                    if (csvFileWriter != null)
                    {
                        csvFileWriter.WriteSensorData(i[0], i[1], i[2], i[3], i[4], i[5], i[6], i[7], i[8], i[9]);
                    }
                }
            );

            // Anonymous function to handle battery packet received event
            serialDecoder.BatteryReceived += new SerialDecoder.onBatteryReceived(
                delegate(int[] i)
                {
                    PacketCounter.Increment();
                    if (csvFileWriter != null)
                    {
                        csvFileWriter.WriteBatteryData(i[0], i[1]);
                    }
                    BatteryVoltage = (float)i[0] / 1000.0f;
                }
            );
        }

        /// <summary>
        /// Automatically connect to XStick.  Loops through aviable serial ports and connects to first respnding XStick.
        /// </summary>
        public void AutoConnect()
        {
            foreach (string portName in System.IO.Ports.SerialPort.GetPortNames())
            {
                OpenSerialPort(portName);
                if (XStickChannel == -1)
                {
                    CloseSerialPort();
                }
                else
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Disconnect from XStick
        /// </summary>
        public void Disconnect()
        {
            XStickChannel = -1;
            BatteryVoltage = 0.0f;
            CloseSerialPort();
        }

        /// <summary>
        /// Starts logging to CSV files.
        /// </summary>
        /// <param name="fileName">
        /// File path of CSV files.  Will be extedned with XTick channel number and packet type.
        /// </param>
        public void StartLogging(string filePath)
        {
            csvFileWriter = new CsvFileWriter(filePath + "_" + XStickChannel.ToString());
        }

        /// <summary>
        /// Stops logging to CSV files.
        /// </summary>
        public void StopLogging()
        {
            if (csvFileWriter != null)
            {
                csvFileWriter.CloseFiles();
                csvFileWriter = null;
            }
        }

        /// <summary>
        /// Opens serial port and sets XStickChannel.
        /// </summary>
        /// <param name="portName">
        /// Name of port to be opened.
        /// </param> 
        /// <returns>
        /// true if successful.
        /// </returns>
        private bool OpenSerialPort(string portName)
        {
            try
            {
                serialPort = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
                serialPort.Handshake = Handshake.RequestToSend;
                serialPort.WriteTimeout = 100;  // set timeout else writes to port without RTS will freeze application
                serialPort.DtrEnable = true;
                serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
                serialPort.Open();
                xStickRxActive = true;
                Thread.Sleep(110);
                SendSerialPort("+++");  // enter command mode
                Thread.Sleep(110);
                XStickChannel = -1;
                SendSerialPort("ATCH\r");   // read channel
                Thread.Sleep(50);
                SendSerialPort("ATFR\r");   // software reset
                Thread.Sleep(50);
                xStickRxActive = false;
                if (XStickChannel == -1)
                {
                    serialPort.Close();
                    return false;
                }
                PacketCounter.Reset();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Closes serial port.
        /// </summary>
        private void CloseSerialPort()
        {
            try
            {
                serialPort.Close();
            }
            catch { }
        }

        /// <summary>
        /// Sends string to serial port.
        /// </summary>
        /// <param name="s">
        /// String to send to serial port.
        /// </param>
        private void SendSerialPort(string s)
        {
            try
            {
                serialPort.Write(s.ToCharArray(), 0, s.Length);
            }
            catch { }
        }

        /// <summary>
        /// serialPort DataReceived event to process bytes through serialDecoder.
        /// </summary>
        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Get bytes from serial port
            int bytesToRead = serialPort.BytesToRead;
            byte[] readBuffer = new byte[bytesToRead];
            serialPort.Read(readBuffer, 0, bytesToRead);

            // Process bytes one at a time
            foreach (byte b in readBuffer)
            {
                // Decode channel value if XStick communication active
                if (xStickRxActive)
                {
                    if ((char)b == '\r')   // attempt to decode channel value if new line character received
                    {
                        if (Regex.IsMatch(xStickRxBuffer, @"^[A-F0-9]+$") && XStickChannel == -1)
                        {
                            XStickChannel = Convert.ToInt32(xStickRxBuffer, 16);
                        }
                        xStickRxBuffer = "";
                    }
                    else
                    {
                        xStickRxBuffer += (char)b;
                    }
                }
                else
                {
                    serialDecoder.ProcessNewByte(b);    // process every byte through serialDecoder
                }
            }
        }
    }
}
