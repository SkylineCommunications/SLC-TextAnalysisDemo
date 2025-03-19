using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DomHelpers;
using DomHelpers.Satellitefeeds;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
using Skyline.DataMiner.Net.Messages;
using static DomHelpers.Satellitefeeds.SatellitefeedsIds.Enums;

namespace Feeds
{
    public class ExtractedFeed
	{
		string _json;
		DomHelper _domHelper;

		public ExtractedFeedInstance instance { get; set; }

		public string FileName { get; set; }

		public ExtractedFeed(IEngine engine, string jsonstring, string fileName)
		{
			_json = jsonstring;

			_domHelper = new DomHelper(engine.SendSLNetMessages, SatellitefeedsIds.ModuleId);

			FileName = fileName;
		}

		public void SetFeedString(string jsonstring)
		{
			_json = jsonstring;
			
		}

		public void CreateExtractedFeed()
		{
			FeedJsonDeserialized extractedFeedDeserialized = JsonSerializer.Deserialize<FeedJsonDeserialized>(_json);

			instance = new ExtractedFeedInstance();

			instance.ExtractedFeedEventInfo.EventName = extractedFeedDeserialized.Event.EventName;
			instance.ExtractedFeedEventInfo.StartDate = extractedFeedDeserialized.Event.StartTime;
			instance.ExtractedFeedEventInfo.EndDate = extractedFeedDeserialized.Event.EndTime;

			Satellite satFeed = extractedFeedDeserialized.Satellites.Single();

			instance.ExtractedFeedParameters.Satellite = satFeed.SatelliteName;
			instance.ExtractedFeedParameters.UplinkFrequency = satFeed.Uplink.Frequency;
			instance.ExtractedFeedParameters.DownlinkFrequency = satFeed.Downlink.Frequency;
			instance.ExtractedFeedParameters.UplinkPolarization = satFeed.Uplink.Polarization;
			instance.ExtractedFeedParameters.DownlinkPolarization = satFeed.Downlink.Polarization;

			Parameters satParams = satFeed.Parameters;
			instance.ExtractedFeedParameters.ModulationStandard = satParams.ModulationStandard;
			instance.ExtractedFeedParameters.SymbolRate = satParams.SymbolRate;
			instance.ExtractedFeedParameters.RollOff = satParams.RollOff;
			instance.ExtractedFeedParameters.FEC = satParams.FEC;

			instance.ExtractedFeedRawData.JSON = _json;
			instance.ExtractedFeedRawData.FileName = FileName;

			instance.Save(_domHelper);
		}

	}

}
