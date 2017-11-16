using Predes.ZabbixSender.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ZabbixTrapper
{
	class Program
	{
		static void Main(string[] args)
		{

			while (!CheckForInternetConnection())
			{
				Console.WriteLine($"[{DateTime.Now}] No internet, sleeping");
				Thread.Sleep(3000);
			}
			 
			Console.WriteLine($"[{DateTime.Now}] There is now internet");

			// create client instance 
			MqttClient client = new MqttClient("10.0.3.7");
			// register to message received 
			client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

			string clientId = Guid.NewGuid().ToString();
			client.Connect(clientId);

			// subscribe to the topic "/home/temperature" with QoS 2 
			client.Subscribe(new string[] { "bruh/sensornode1" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
			Console.WriteLine($"[{DateTime.Now}] Is now subscribed to mqtt");
			while (true)
			{
				Thread.Sleep(3000);
			}
		}

		public static bool CheckForInternetConnection()
		{
			try
			{
				using (var client = new WebClient())
				{
					using (client.OpenRead("http://clients3.google.com/generate_204"))
					{
						return true;
					}
				}
			}
			catch
			{
				return false;
			}
		}

		private static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
		{
			var resultString = System.Text.Encoding.Default.GetString(e.Message);
			var sensorData = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(resultString, new { temperature = "", humidity = "" });

			var dataList = new List<SenderData>();

			SenderService service = new SenderService("10.0.3.6", 10051);
			dataList.Add(new SenderData { Host = "testhost1", Key = "temperature", Value = sensorData.temperature });
			dataList.Add(new SenderData { Host = "testhost1", Key = "humidity", Value = sensorData.humidity });
			Trace.WriteLine($"Sendeing data temp {sensorData.temperature} and humid {sensorData.humidity}");
			var response = service.Send(dataList.ToArray());
			Console.WriteLine($"[{DateTime.Now}] {response.Response}");
		}
	}
}
