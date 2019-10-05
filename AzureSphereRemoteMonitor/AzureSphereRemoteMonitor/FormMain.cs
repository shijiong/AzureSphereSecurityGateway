﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.IO;
using Newtonsoft.Json;

namespace AzureSphereRemoteMonitor
{
    public partial class FormMain : Form
    {
        static ServiceClient serviceClient;
        static string connectionString = "Insert your connection string here";
        static string iotHubD2cEndpoint = "messages/events";
        static EventHubClient eventHubClient;
        string DeviceID = "AvnetStarterKit";
        SensorData SensorDataObj;
        int TempThres = 27;
        bool cooleron = false;

        public FormMain()
        {
            InitializeComponent();
            SensorDataObj = new SensorData();
            this.Shown += Form_Shown;
        }

               private async void Form_Shown(object sender, EventArgs e)
        {
            try
            {
                serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
                eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);
                var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;

                foreach (string partition in d2cPartitions)
                {
                    ReceiveMessagesFromDeviceAsync(partition);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

        }

        public class SensorData
        {
            public int deviceID;
            public int temperature;
            public int humidity;
            public int light;
            public int gas;
            public int pir;
            //public string Currentdatetime;


            public SensorData() { } //required for JSON serializers
            public SensorData(int deviceid, int temp, int humi, int ligh, int Gas, int Pir)
            {
                deviceID = deviceid;
                temperature = temp;
                humidity = humi;
                light = ligh;
                gas = Gas;
                pir = Pir;
                //Currentdatetime = Cdatetime;
               
            }

        }

        public T Deserialize<T>(string json)
        {
            var _Bytes = Encoding.Unicode.GetBytes(json);
            using (MemoryStream _Stream = new MemoryStream(_Bytes))
            {
                var _Serializer = new DataContractJsonSerializer(typeof(T));
                return (T)_Serializer.ReadObject(_Stream);
            }
        }
        private async Task ReceiveMessagesFromDeviceAsync(string partition)
        {
            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.Now);

            while (true)
            {
                try
                {
                    EventData eventData = await eventHubReceiver.ReceiveAsync();
                    if (eventData == null) continue;
                    byte[] data = eventData.GetBytes();
                    string connectionDeviceId = eventData.SystemProperties["iothub-connection-device-id"].ToString();
                    Debug.WriteLine("Message from " + connectionDeviceId);
                    if (connectionDeviceId != DeviceID)
                        continue;
                    
                        string json = System.Text.Encoding.UTF8.GetString(data);
                        Debug.WriteLine(json);
                        //SensorDataObj = Deserialize<SensorData>(json);
                        SensorDataObj = JsonConvert.DeserializeObject<SensorData>(json);
                        device.Invoke(new Action(() =>
                        {
                            device.Text = SensorDataObj.deviceID.ToString();
                        }));
                        gas.Invoke(new Action(() =>
                        {
                            gas.Text = SensorDataObj.gas.ToString();
                        }));
                        pir.Invoke(new Action(() =>
                        {
                            pir.Text = SensorDataObj.pir.ToString();
                         }));
                        light.Invoke(new Action(() =>
                        {
                            light.Text = SensorDataObj.light.ToString();
                        }));
                        temperature.Invoke(new Action(() =>
                        {
                            temperature.Text = SensorDataObj.temperature.ToString();
                        }));
                        humi.Invoke(new Action(() =>
                        {
                            humi.Text = SensorDataObj.humidity.ToString();
                        }));
                        msgTime.Invoke(new Action(() =>
                        {
                            msgTime.Text = System.DateTime.Now.ToString();
                        }));
                    if (SensorDataObj.temperature >= TempThres)
                    {
                        if (!cooleron)
                        {
                            SendCloudToDeviceMessageAsync("On");    //Power on Cooler
                            sysstatus.Invoke(new Action(() =>
                            {
                                sysstatus.Text = "Relay ON";
                            }));
                            cooleron = true;
                        }
                    }
                    else
                    {
                        if (cooleron)
                        {
                            SendCloudToDeviceMessageAsync("Off");    //Power on Cooler
                            sysstatus.Invoke(new Action(() =>
                            {
                                sysstatus.Text = "Relay OFF";
                            }));
                            cooleron = false;
                        }
                    }


                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }

                //  string data = Encoding.UTF8.GetString(eventData.GetBytes());
                //  Debug.WriteLine(string.Format("Message received. Partition: {0} Data: '{1}'", partition, data));
            }
        }

        private void SendCloudToDeviceMessageAsync(string Data)
        {
            try
            {
                var commandMessage = new Microsoft.Azure.Devices.Message(Encoding.ASCII.GetBytes(Data));
                serviceClient.SendAsync(DeviceID, commandMessage);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }


        private void SetTemp_Click(object sender, EventArgs e)
        {
            TempThres = Convert.ToInt16(TempThreshold.Text);
        }
    }
}
