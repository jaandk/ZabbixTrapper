using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Predes.ZabbixSender.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ZabbixTrapper
{
	class Program
	{
		private static Setting setting;

		static void Main(string[] args)
		{
			LoadSettingsFile();

			while (!CheckForInternetConnection())
			{
				Console.WriteLine($"[{DateTime.Now}] No internet, sleeping");
				Thread.Sleep(3000);
			}

			Console.WriteLine($"[{DateTime.Now}] There is now internet");

			// create client instance 
			MqttClient client = new MqttClient(setting.MqttserverIp);
			// register to message received 
			client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

			string clientId = Guid.NewGuid().ToString();
			client.Connect(clientId);

			// subscribe to the topic "/home/temperature" with QoS 2 
			client.Subscribe(setting.MqttProperties.Select(s => s.MqttTopic).ToArray(), new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

			Console.WriteLine($"[{DateTime.Now}] Is now subscribed to mqtt");
			while (true)
			{
			}
		}

		private static void LoadSettingsFile()
		{
			string settingsFile = "/config/setting.json";
			if (File.Exists(settingsFile))
			{
				setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(settingsFile));
			}
			else
			{
				var tmpSetting = new Setting();
				tmpSetting.MqttserverIp = "1.2.3.4";
				tmpSetting.ZabbixIp = "5.6.7.8";
				tmpSetting.MqttProperties = new List<MqttProp>();
				tmpSetting.MqttProperties.Add(new MqttProp() { MqttTopic = "topic1/topic2/topic3", MqttProperties = new List<string>() { "prop1", "prop2" });
				Directory.CreateDirectory("/config");
				File.WriteAllText(settingsFile, JsonConvert.SerializeObject(tmpSetting));
			}
		}

		private static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
		{
			var resultString = System.Text.Encoding.Default.GetString(e.Message);
			var sensorData = JObject.Parse(resultString);

			var dataList = new List<SenderData>();

			SenderService service = new SenderService(setting.ZabbixIp, 10051);

			var tt = setting.MqttProperties.FirstOrDefault(s => s.MqttTopic == e.Topic);
			foreach (var item in tt.MqttProperties)
			{
				dataList.Add(new SenderData { Host = TopicFormat(tt.MqttTopic), Key = item, Value = (string)sensorData[item] });
			}

			//Trace.WriteLine($"Sendeing data temp {sensorData.temperature} and humid {sensorData.humidity}");
			var response = service.Send(dataList.ToArray());
			Console.WriteLine($"[{DateTime.Now}] {response.Response}");
		}

		private static string TopicFormat(string str)
		{
			return str.Replace("/", ".");
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
	}
}
