using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DomHelpers;
using DomHelpers.Satellitefeeds;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
using static DomHelpers.Satellitefeeds.SatellitefeedsIds.Enums;
using ExtensionsNamespace;

namespace Feeds
{
	public class MappedFeed
	{
		// string which contains the mapped values 
		string json;

		IEngine _engine;

		DomHelper _domHelper;

		// DOM Guid referencing the extracted feed information (not mapped to acutal values yet)
		public Guid ExtractedFeedGuid {  get; set; }

		public string FileName{ get; set; }

		public MappedFeed(IEngine engine, Guid extractedFeedGuid, string jsonstring, string filename)
		{
			_engine = engine;

			_domHelper = new DomHelper(engine.SendSLNetMessages, SatellitefeedsIds.ModuleId);

			json = jsonstring;

			ExtractedFeedGuid = extractedFeedGuid;

			FileName = filename;
		}

		public void CreateMappedFeed()
		{
			MappedFeedInstance instance = new MappedFeedInstance();

			// get list of Display values for the different enums
			var satelliteStringList = DOMExtensions.GetEnumDisplayValues<SatelliteEnum>(SatellitefeedsIds.Enums.Satellite.ToValue);
			var downlinkStringList = DOMExtensions.GetEnumDisplayValues<DownlinkpolarizationEnum>(SatellitefeedsIds.Enums.Downlinkpolarization.ToValue);
			var uplinkStringList = DOMExtensions.GetEnumDisplayValues<UplinkpolarizationEnum>(SatellitefeedsIds.Enums.Uplinkpolarization.ToValue);
			var modulationStringList = DOMExtensions.GetEnumDisplayValues<ModulationstandardEnum>(SatellitefeedsIds.Enums.Modulationstandard.ToValue);
			var rollofFStringList = DOMExtensions.GetEnumDisplayValues<RolloffEnum>(SatellitefeedsIds.Enums.Rolloff.ToValue);
			var fecStringList = DOMExtensions.GetEnumDisplayValues<FECEnum>(SatellitefeedsIds.Enums.FEC.ToValue);

			// read JSON
			FeedJsonDeserialized feedJsonDeserialized = JsonSerializer.Deserialize<FeedJsonDeserialized>(json);

			instance.MappedFeedEventInfo.EventName = feedJsonDeserialized.Event.EventName;
			try
			{
				instance.MappedFeedEventInfo.StartDate = DateTime.ParseExact(feedJsonDeserialized.Event.StartTime, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
				instance.MappedFeedEventInfo.EndDate = DateTime.ParseExact(feedJsonDeserialized.Event.EndTime, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
			}
			catch (FormatException ex)
			{
				_engine.GenerateInformation($"Format Exception when reading Start Date: {feedJsonDeserialized.Event.StartTime} and End Date: {feedJsonDeserialized.Event.EndTime}");
			}

			if (feedJsonDeserialized.Satellites.Count > 0)
			{
				Satellite satFeed = feedJsonDeserialized.Satellites.Single();

				instance.MappedFeedParameters.Satellite = SatellitefeedsIds.Enums.Satellite.ToEnum(satelliteStringList.FirstOrDefault(s => s == satFeed.SatelliteName) ?? "Not found");

				if (double.TryParse(satFeed.Uplink.Frequency.Replace(" MHz", ""), out double uplinkFreq))
				{
					instance.MappedFeedParameters.UplinkFrequency = uplinkFreq;
				}
				if (double.TryParse(satFeed.Downlink.Frequency.Replace(" MHz", ""), out double downlinkFreq))
				{
					instance.MappedFeedParameters.DownlinkFrequency = downlinkFreq;
				}

				instance.MappedFeedParameters.UplinkPolarization = SatellitefeedsIds.Enums.Uplinkpolarization.ToEnum(uplinkStringList.FirstOrDefault(s => s == satFeed.Uplink.Polarization) ?? "Not found");
				instance.MappedFeedParameters.DownlinkPolarization = SatellitefeedsIds.Enums.Downlinkpolarization.ToEnum(downlinkStringList.FirstOrDefault(s => s == satFeed.Downlink.Polarization) ?? "Not found");

				Parameters satParams = satFeed.Parameters;
				instance.MappedFeedParameters.ModulationStandard = SatellitefeedsIds.Enums.Modulationstandard.ToEnum(modulationStringList.FirstOrDefault(s => s == satParams.ModulationStandard) ?? "Not found");
				if (double.TryParse(satFeed.Parameters.SymbolRate.Replace(" MSym/s", ""), out double symRate))
				{
					instance.MappedFeedParameters.SymbolRate = symRate;
				}
				instance.MappedFeedParameters.RollOff = SatellitefeedsIds.Enums.Rolloff.ToEnum(rollofFStringList.FirstOrDefault(s => s == satParams.RollOff) ?? "Not found");
				instance.MappedFeedParameters.FEC = SatellitefeedsIds.Enums.FEC.ToEnum(fecStringList.FirstOrDefault(s => s == satParams.FEC) ?? "Not found");
			}
			

			instance.MappingInfo.ExtractedFeedLink = ExtractedFeedGuid;
			instance.MappingInfo.MappedFeedJSON = json;
			instance.MappingInfo.FileName = FileName;

			instance.Save(_domHelper);
		}


		//private List<string> GetEnumDisplayValues<T>(Func<T, string> toValueFunc) where T : Enum
		//{
		//	List<T> enumList = Enum.GetValues(typeof(T)).Cast<T>().ToList();
		//	var stringList = enumList.Select(toValueFunc).ToList();
		//	//_engine.GenerateInformation($"Possible values: {string.Join(", ", stringList)}");

		//	return stringList;
		//}

	}	
}