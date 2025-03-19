using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace Feeds
{
	public class FeedJsonDeserialized
	{
		[JsonPropertyName("provider")]
		public string Provider { get; set; }

		[JsonPropertyName("event")]
		public EventDetails Event { get; set; }

		[JsonPropertyName("satellites")]
		public List<Satellite> Satellites { get; set; }
	}

	public class EventDetails
	{
		[JsonPropertyName("event_name")]
		public string EventName { get; set; }

		[JsonPropertyName("start_time")]
		public string StartTime { get; set; }

		[JsonPropertyName("end_time")]
		public string EndTime { get; set; }
	}

	public class Satellite
	{
		[JsonPropertyName("satellite_name")]
		public string SatelliteName { get; set; }

		[JsonPropertyName("uplink")]
		public Transmission Uplink { get; set; }

		[JsonPropertyName("downlink")]
		public Transmission Downlink { get; set; }

		[JsonPropertyName("parameters")]
		public Parameters Parameters { get; set; }

		[JsonPropertyName("audio_channel_pattern")]
		public List<AudioChannel> AudioChannelPattern { get; set; }
	}

	public class Transmission
	{
		[JsonPropertyName("frequency")]
		public string Frequency { get; set; }

		[JsonPropertyName("polarization")]
		public string Polarization { get; set; }
	}

	public class Parameters
	{
		[JsonPropertyName("modulation_standard")]
		public string ModulationStandard { get; set; }

		[JsonPropertyName("symbol_rate")]
		public string SymbolRate { get; set; }

		[JsonPropertyName("fec")]
		public string FEC { get; set; }

		[JsonPropertyName("roll_off")]
		public string RollOff { get; set; }

		[JsonPropertyName("video_format")]
		public string VideoFormat { get; set; }

		[JsonPropertyName("encryption_type")]
		public string EncryptionType { get; set; }

		[JsonPropertyName("dolby_audio")]
		public string DolbyAudio { get; set; }
	}

	public class AudioChannel
	{
		[JsonPropertyName("channel")]
		public int Channel { get; set; }

		[JsonPropertyName("description")]
		public string Description { get; set; }
	}
}
