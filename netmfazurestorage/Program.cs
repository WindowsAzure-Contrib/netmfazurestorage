﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Net.NetworkInformation;
using NetMf.CommonExtensions;
using netmfazurestorage.Account;
using netmfazurestorage.Blob;
using netmfazurestorage.NTP;
using netmfazurestorage.Table;
using netmfazurestorage.Tests;

namespace netmfazurestorage
{
    public class Program
    {
        private const string AccountName = "netmftest";  // please upload pictures of cats to this storage account.
        private const string AccountKey = "gYU/Nf/ib97kHQrVkxhdD3y0lKz6ZOVaR1FjeDpESecGuqZOEq1TE+5+SXfZ/DBzKsXH3m0NDsLxTbTqQxL9yA==";
        private static BlobClient _blobClient;
        private static TableClient _tableClient;
        private static InterruptPort _onBoardButton;
        private static string _macAddress;
        private static object Padlock = new object();
        private static AnalogInput _analogInput;

        public static void RunTests()
        {
            var queueTests = new QueueTests(AccountName, AccountKey);
            queueTests.Run();

            var tableTests = new TableTests(AccountName, AccountKey);
            tableTests.Run();
        }


        public static void Main()
        {
        

            RunTests();
            return;

            //tidy up
            File.Delete("\\SD\\Data.csv");

            try
            {
                //retrive and set device time via NTP
                var networkTime = NtpClient.GetNetworkTime();
                Utility.SetLocalTime(networkTime);

                _macAddress = GetMAC();
                _blobClient = new BlobClient(new CloudStorageAccount(AccountName, AccountKey));
                _tableClient = new TableClient(AccountName, AccountKey);
                _tableClient.CreateTable("netmfdata");

                _onBoardButton = new InterruptPort(Cpu.Pin.GPIO_NONE, true,
                                                                Port.ResistorMode.Disabled,
                                                                Port.InterruptMode.InterruptEdgeHigh);
                _onBoardButton.OnInterrupt += onBoardButton_OnInterrupt;

                _analogInput = new AnalogInput(Cpu.AnalogChannel.ANALOG_NONE);

            }
            catch(Exception ex)
            {
                Debug.Print("Error setting up Device: " + ex.ToString());
            }

            int counter = 0;
            while (true)
            {
                counter++;
                var data = _analogInput.Read() * 40D;
                _tableClient.AddTableEntityForTemperature("netmfdata", _macAddress, counter.ToString(), DateTime.Now, data, "UK");

                lock (Padlock)
                {
                    using (FileStream fs = File.Open("\\SD\\Data.csv", FileMode.Append, FileAccess.Write))
                    {
                        Debug.Print(data.ToString());
                        var dataBytes = Encoding.UTF8.GetBytes(
                            StringUtility.Format("{0}, {1}, {2}\r\n",
                                                 _macAddress, DateTime.Now.ToString(),
                                                 data)
                            );

                        fs.Write(dataBytes, 0, dataBytes.Length);
                        fs.Flush();
                    }
                }

                Thread.Sleep(1000);
                Debug.Print("Working");
            }
        }

        private static string GetMAC()
        {
            NetworkInterface[] netIF = NetworkInterface.GetAllNetworkInterfaces();
            
            string macAddress = "";

            // Create a character array for hexidecimal conversion.
            const string hexChars = "0123456789ABCDEF";

            // Loop through the bytes.
            for (int b = 0; b < 6; b++)
            {
                // Grab the top 4 bits and append the hex equivalent to the return string.
                macAddress += hexChars[netIF[0].PhysicalAddress[b] >> 4];

                // Mask off the upper 4 bits to get the rest of it.
                macAddress += hexChars[netIF[0].PhysicalAddress[b] & 0x0F];

                // Add the dash only if the MAC address is not finished.
                if (b < 5) macAddress += "-";
            }

            return macAddress;
        }

        static void onBoardButton_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            try
            {
                lock (Padlock)
                {
                    var filename = StringUtility.Format("{0}.csv", time.ToString("yyyyMMddhhmmss"));
                    Debug.Print(filename);
                    var success = _blobClient.PutBlockBlob("demo",
                                                     filename,
                                                      "\\SD\\Data.csv");

                    if (success)
                    {
                        Debug.Print("Files uploaded to netmf.blob.core.windows.net");
                    }
                    else
                    {
                        Debug.Print("There was an error, check debug output");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print("Critical error: " + ex);
            }
        }

    }
}
