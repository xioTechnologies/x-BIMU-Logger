x-BIMU-Logger
=============

This is a Windows application for logging synchronised data from multiple [x-BIMUs](http://www.x-io.co.uk/products/x-bimu/) using [802.15.4 XBee](http://www.x-io.co.uk/x-bimu-802-15-4-xbee/) modules and [802.15.4 XStick](http://www.x-io.co.uk/x-bimu-802-15-4-xstick/) USB dongles.  Each x-BIMU requires a separate XStick configured to a unique channel.  Each x-BIMU samples sensors and sends packets asynchronously.  The fixed latency and low-jitter of each dedicated wireless channel allows synchronisation to be achieved by time-stamping the time-of-arrival of each packet.  It is not necessary for the x-BIMUs to have the same sample rates or for the sample rate to be known.


Windows application
-------------------

The Windows application is a stand-alone executable written in C#.  It contains 12 *Connect* buttons, text boxes for the target *Directory* and *File Name*, and a *Start Logging* button.  Click any of the *Connect* buttons to automatically connect to an XStick connected to the computer.  When connected, the button colour will change to to that of the channel and the button text will display the channel number and packet-rate.  The bar graph below each button displays the battery level if this data is being sent.  The *Directory* and *File Name* text boxes define the target location and file name of the logged files.  One file will be created per sensor per packet.  The *File Name* will be extended with the channel number and packet type, for example: `LoggedData_12_Sensor.csv`.  The first column of each CSV file is the synchronised time-stamp of each sample.

<div align="center">
<img src="https://raw.github.com/xioTechnologies/x-BIMU-Logger/master/x-BIMU%20Logger%20Screenshot.png"/>
</div>
 

Example data (plotted in MATLAB)
---------------------------------

The example data and MATLAB script to demonstrate the synchronization achieved.  The example data was logged from 9 x-BIMUs, each set up to send sensor data at 128 Hz with binary packet mode enabled.  The 9 XSticks were connected to the computer via 2 USB hubs.  Sending of quaternion packets were disabled.  The logged data represents the 9 x-BIMUs sitting stationary on a desk for 1 minute.  The desk was tapped 5 times at the start and end of the logging session.  The synchronisation is confirmed by plotting the 9 gyroscope x axis measurements and checking if they align in time.

<div align="center">
<img src="https://raw.github.com/xioTechnologies/x-BIMU-Logger/master/Example%20Data%20Plot.png"/>
</div>

<div align="center">
<img src="https://raw.github.com/xioTechnologies/x-BIMU-Logger/master/Example%20Data%20Plot%20%28Single%20Tap%29.png"/>
</div>

FTDI driver latency timer
---------------------------------

The XSticks use the [FTDI USB Serial Port drivers](http://www.ftdichip.com/Drivers/VCP.htm).  The default setting of the driver is for a Latency Timer value of 16 ms.  This value will prevent accurate synchronisation of the received x-BIMU packets.  The Latency Timer value should be changed to 1 ms (the minimum) via *Device Manager > USB Serial Port Properties > Advanced*.

<div align="center">
<img src="https://raw.github.com/xioTechnologies/x-BIMU-Logger/master/FTDI%20Driver%20Latency%20Timer.png"/>
</div>

Version history
---------------------------------

* **v1.0**  Initial release
* **v1.1**  CSV files include headings
