using System;
using Newtonsoft.Json;

namespace HuePod.Nasa
{
	public class Apod
	{
		public string Copyright { get; set; }
		public DateTime Date { get; set; }
		public string Explanation { get; set; }
		public string HdUrl { get; set; }
		[JsonProperty("media_type")]
		public string MediaType { get; set; }
		[JsonProperty("service_version")]
		public string  ServiceVersion{ get; set; }
		public string Title { get; set; }
		public string Url { get; set; }
	}
}

