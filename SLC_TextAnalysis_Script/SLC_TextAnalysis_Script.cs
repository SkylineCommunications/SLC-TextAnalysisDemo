/// <summary>
/// Represents a DataMiner Automation script.
/// </summary>

/*
****************************************************************************
*  Copyright (c) 2025,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

Skyline Communications NV
Ambachtenstraat 33
B-8870 Izegem
Belgium
Tel.	: +32 51 31 35 69
Fax.	: +32 51 31 01 29
E-mail	: info@skyline.be
Web		: www.skyline.be
Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

13/03/2025	1.0.0.1		Willem Mélange, Skyline	Initial version
07/11/2025	2.0.0.0		Willem Mélange, Use DataMiner Assistant DxM
****************************************************************************
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DomHelpers.Satellitefeeds;
using ExtensionsNamespace;
using Feeds;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Net.Apps.DocumentIntelligence;
using Skyline.DataMiner.Net.Apps.DocumentIntelligence.Objects;
using SLC_Popups.IAS.Extensions;

//---------------------------------
// TextAnalysis.cs
//---------------------------------

namespace TextAnalysis
{
	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private static IEngine _engine;

		private static readonly string _context = @"c:\Skyline DataMiner\Documents\dma_common_documents\AI-sample\event_context_example.txt";

		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			try
			{
				_engine = engine;
				RunSafe(engine);
			}
			catch (ScriptAbortException e)
			{
				// Catch normal abort exceptions (engine.ExitFail or engine.ExitSuccess)
				throw; // Comment if it should be treated as a normal exit of the script.
			}
			catch (ScriptForceAbortException e)
			{
				// Catch forced abort exceptions, caused via external maintenance messages.
				throw;
			}
			catch (ScriptTimeoutException e)
			{
				// Catch timeout exceptions for when a script has been running for too long.
				throw;
			}
			catch (InteractiveUserDetachedException e)
			{
				// Catch a user detaching from the interactive script by closing the window.
				// Only applicable for interactive scripts, can be removed for non-interactive scripts.
				throw;
			}
			catch (Exception e)
			{
				engine.ExitFail("Run|Something went wrong: " + e);
			}
		}

		private void RunSafe(IEngine engine)
		{
			string filepath = engine.GetScriptParam("inputFile").Value;
			var filename = Path.GetFileName(filepath);

			// Create DOM instance from the extracted parameters
			Guid id = Guid.Empty;
			string json = null;
			try
			{
				json = ExtractValues(filepath);
				engine.Log("JSON content: " + json);
			}
			catch (Exception ex)
			{
				engine.ShowErrorDialog("Failed to get parameters found in document. Is DataMiner Assistant running correctly?");
			}

			try
			{
				CreateMappedFeed(id, json, filename);
			}
			catch (Exception ex)
			{
				engine.ShowErrorDialog("Failed to create Satellite Feed DOM instance from LLM response containing mapped parameters.");
			}
		}		

		private void CreateMappedFeed(Guid extractedFeedGuid, string json, string filename)
		{
			MappedFeed mappedFeed = new MappedFeed(_engine, extractedFeedGuid, json, filename);

			mappedFeed.CreateMappedFeed();
		}

		private string ExtractValues(string filepath)
		{
			var fileBytes = File.ReadAllBytes(filepath);
			var fileName = Path.GetFileName(filepath);
			var docIntelHelper = new DocumentIntelligenceHelper(_engine.SendSLNetMessages);
			var output = docIntelHelper.AnalyzeDocuments(GetContext(), new List<Document>() { new Document() { Name = fileName, Content = fileBytes } });
			// var output = "output from LLM or Document Intelligence API";
			return output;
		}

		private string GetContext()
		{
			var context = File.ReadAllText(_context);
			var satStringList = DOMExtensions.GetEnumDisplayValues<SatellitefeedsIds.Enums.SatelliteEnum>(SatellitefeedsIds.Enums.Satellite.ToValue).Select(s => "\"" + s + "\"");
			var polarizationStringList = DOMExtensions.GetEnumDisplayValues<SatellitefeedsIds.Enums.DownlinkpolarizationEnum>(SatellitefeedsIds.Enums.Downlinkpolarization.ToValue).Select(s => "\"" + s + "\"");
			var modStandardStringList = DOMExtensions.GetEnumDisplayValues<SatellitefeedsIds.Enums.ModulationstandardEnum>(SatellitefeedsIds.Enums.Modulationstandard.ToValue).Select(s => "\"" + s + "\"");
			var rolloffStringList = DOMExtensions.GetEnumDisplayValues<SatellitefeedsIds.Enums.RolloffEnum>(SatellitefeedsIds.Enums.Rolloff.ToValue).Select(s => "\"" + s + "\"");
			var fecStringList = DOMExtensions.GetEnumDisplayValues<SatellitefeedsIds.Enums.FECEnum>(SatellitefeedsIds.Enums.FEC.ToValue).Select(s => "\"" + s + "\"");
			var replacements = new Dictionary<string, string>
			{
				{ "satStringList", $"[{string.Join(", ", satStringList)}]" },
				{ "polarizationStringList", $"[{string.Join(", ", polarizationStringList)}]" },
				{ "modStandardStringList", $"[{string.Join(", ", modStandardStringList)}]" },
				{ "rolloffStringList", $"[{string.Join(", ", rolloffStringList)}]" },
				{ "fecStringList", $"[{string.Join(", ", fecStringList)}]" },
			};
			string filledContext = Regex.Replace(context, @"\{\{(\w+)\}\}", match =>
			{
				string key = match.Groups[1].Value;
				return replacements.TryGetValue(key, out string value) ? value : match.Value;
			});
			return filledContext;
		}
	}
}