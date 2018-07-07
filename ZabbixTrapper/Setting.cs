using System;
using System.Collections.Generic;
using System.Text;

namespace ZabbixTrapper
{
	public class Setting
	{
		public string MqttserverIp { get; set; }
		public string ZabbixIp { get; set; }
		public List<MqttProp> MqttProperties { get; set; }

	}

	public class MqttProp
	{
		public string MqttTopic { get; set; }
		public List<string> MqttProperties { get; set; }

	}
}
