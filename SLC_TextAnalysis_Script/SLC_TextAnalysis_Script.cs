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
****************************************************************************
*/

	using Azure;
	using Azure.AI.DocumentIntelligence;
	using Azure.AI.OpenAI;
	using Azure.Core;
	using Microsoft.SemanticKernel;
	using Microsoft.SemanticKernel.ChatCompletion;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Parser.ProtocolParsers;
	using System;
	using System.IO;
	using System.Text.Json;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Net.SLSearch.Misc;
	using static System.Net.Mime.MediaTypeNames;
	using Feeds;
	using ExtensionsNamespace;
using DomHelpers.Satellitefeeds;
using System.Linq;

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
			private static Kernel _kernel;

		private static IEngine _engine;

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
				catch (ScriptAbortException)
				{
					// Catch normal abort exceptions (engine.ExitFail or engine.ExitSuccess)
					throw; // Comment if it should be treated as a normal exit of the script.
				}
				catch (ScriptForceAbortException)
				{
					// Catch forced abort exceptions, caused via external maintenance messages.
					throw;
				}
				catch (ScriptTimeoutException)
				{
					// Catch timeout exceptions for when a script has been running for too long.
					throw;
				}
				catch (InteractiveUserDetachedException)
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
				// TODO: Define code here
				var secrets = AzureSecrets.GetUserSecrets();
				InitializeKernel(secrets);
				string filename = engine.GetScriptParam("inputFile").Value;
				var filenameOnly = Path.GetFileName(filename);

				var markdown = ExtractFileContentAsMarkdown(filename, secrets);
				engine.Log("Markdown content: " + markdown);

				var json = ExtractJson(markdown);
				engine.Log("JSON content: " + json);
				engine.GenerateInformation("Extracted JSON content: " + json);
				// Create DOM instance of the extracted parameters
				var id = CreateExtractedFeed(json, filename);

				var mappedJson = MapValues(json);
				engine.Log("Mapped JSON content: " + mappedJson);
				engine.GenerateInformation("Mapped JSON content: " + mappedJson);
				CreateMappedFeed(id, mappedJson, filename);
		}

			private Guid CreateExtractedFeed(string json, string filename)
			{
			ExtractedFeed extractedFeed = new ExtractedFeed(_engine, json, filename);

				extractedFeed.CreateExtractedFeed();

				return extractedFeed.instance.ID.Id;
			}

			private void CreateMappedFeed(Guid extractedFeedGuid, string json, string filename)
			{
				MappedFeed mappedFeed = new MappedFeed(_engine, extractedFeedGuid, json, filename);

				mappedFeed.CreateMappedFeed();
			}

		private static void InitializeKernel(AzureSecrets secrets)
			{
				if (_kernel == null)
				{
					AzureOpenAIClient openAIClient = new AzureOpenAIClient(new Uri(secrets.AzureOpenAIEndpoint), new AzureKeyCredential(secrets.AzureOpenAIKey));
					IKernelBuilder builder = Kernel.CreateBuilder();
					builder.Services.AddAzureOpenAIChatCompletion(secrets.ModelDeploymentName, openAIClient);
					_kernel = builder.Build();
				}
			}

			private static string ExtractFileContentAsMarkdown(string filePath, AzureSecrets secrets)
			{
				using (var stream = new FileStream(filePath, FileMode.Open))
				{
					using (var memoryStream = new MemoryStream())
					{
						stream.CopyTo(memoryStream);
						string base64Content = Convert.ToBase64String(memoryStream.ToArray());

						var client = new DocumentIntelligenceClient(new Uri(secrets.DocumentIntelligenceEndpoint), new AzureKeyCredential(secrets.DocumentIntelligenceKey));
						using (RequestContent content = RequestContent.Create(new { base64Source = base64Content }))
						{
							var operation = client.AnalyzeDocument(WaitUntil.Completed, "prebuilt-layout", content, outputContentFormat: "markdown");
							BinaryData responseData = operation.Value;

							JsonElement result = JsonDocument.Parse(responseData.ToStream()).RootElement;
							var analyzeResult = result.GetProperty("analyzeResult");
							return analyzeResult.GetProperty("content").ToString();
						}
					}
				}
			}

			private static string ExtractJson(string markdown)
			{
				ChatHistory history = new ChatHistory();
				history.AddSystemMessage(
					@"You are an intelligent text analysis tool. You will receive a document converted into Markdown format. 
                Your task is to extract specific information from the Markdown text and return a JSON object in the format 
                as the following example:
                    {
	                    ""provider"": ""Eutelsat"",

                        ""event"": {
		                    ""event_name"": ""Skyline Communications Empower event"",
		                    ""start_time"": ""2025-03-19T10:00:00Z"",
		                    ""end_time"": ""2025-03-21T17:30:00Z""
	                    },
	                    ""satellites"": [
		                    {
			                    ""satellite_name"": ""EUT3B-3E"",
			                    ""uplink"": {
				                    ""frequency"": ""13062.5 MHz"",
				                    ""polarization"": ""Horizontal""
			                    },
			                    ""downlink"": {
				                    ""frequency"": ""11262.5 MHz"",
				                    ""polarization"": ""Vertical""
			                    },
			                    ""parameters"": {
				                    ""modulation_standard"": ""NS4"",
				                    ""symbol_rate"": ""35.294118 MSym/s"",
				                    ""fec"": ""7/8"",
				                    ""roll_off"": ""2%"",
				                    ""video_format"": ""1080i50"",
				                    ""encryption_type"": ""BISS-1"",
				                    ""dolby_audio"": ""No""
			                    },
			                    ""audio_channel_pattern"": [
				                    { ""channel"": 1, ""description"": ""PGM ENGLISH STEREO LEFT"" },
				                    { ""channel"": 2, ""description"": ""PGM ENGLISH STEREO RIGHT"" },
				                    { ""channel"": 3, ""description"": ""INTERNATIONAL SOUND STEREO LEFT"" },
				                    { ""channel"": 4, ""description"": ""INTERNATIONAL SOUND STEREO RIGHT"" },
				                    { ""channel"": 8, ""description"": ""OTHER"" }
			                    ]
		                    }
	                    ]
                    }
                Ensure the JSON object is formatted correctly, contains all the required information and don't add extra information. 
                Fill in ""Not found"" if no value could be found in the Markdown text. Return only the JSON object and nothing else.
                You are strictly forbidden from answering in a markdown json block! Please provide a deterministic response with as 
                minimal randomness in your response as possible.
				Additional info:
					When things that are represented in a list such as satellites and auio_channel_pattern are not found, just return an empty json array []
				"
				);
				IChatCompletionService chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
				history.AddUserMessage("Here is the markdown text: " + markdown);
				var result = chatCompletionService.GetChatMessageContentAsync(history, kernel: _kernel).GetAwaiter().GetResult();
				return result.Content;
			}

			private static string MapValues(string json)
			{
				var satStringList = DOMExtensions.GetEnumDisplayValues<SatellitefeedsIds.Enums.SatelliteEnum>(SatellitefeedsIds.Enums.Satellite.ToValue).Select(s => "\"" + s + "\"");
				var polarizationStringList = DOMExtensions.GetEnumDisplayValues<SatellitefeedsIds.Enums.DownlinkpolarizationEnum>(SatellitefeedsIds.Enums.Downlinkpolarization.ToValue).Select(s => "\"" + s + "\"");
				var modStandardStringList = DOMExtensions.GetEnumDisplayValues<SatellitefeedsIds.Enums.ModulationstandardEnum>(SatellitefeedsIds.Enums.Modulationstandard.ToValue).Select(s => "\"" + s + "\"");
				var rolloffStringList = DOMExtensions.GetEnumDisplayValues<SatellitefeedsIds.Enums.RolloffEnum>(SatellitefeedsIds.Enums.Rolloff.ToValue).Select(s => "\"" + s + "\"");
				var fecStringList = DOMExtensions.GetEnumDisplayValues<SatellitefeedsIds.Enums.FECEnum>(SatellitefeedsIds.Enums.FEC	.ToValue).Select(s => "\"" + s + "\"");

			var history = new ChatHistory();

			var systemMessage = @"You are an intelligent language model. Your task is to replace values in a given JSON object with values 
                from a specified list of possible values and to change specific values into specified formats. Ensure that 
                the replacements are contextually appropriate and maintain the integrity of the data structure. Fill in 
                'NO MATCH' if no reasonable replacement could be found in the possible values. You are strictly forbidden 
                from answering in a markdown json block! Here is the list of possible values: 
                {
				"
				+ @"""provider"": [""Globecast"", ""Eutelsat"", ""Overon"", ""Associated Press"", ""Eurovision Services (EBU)"", ""SES"", ""Not found""]," + $"[{string.Join(", ", satStringList)}]" +
				  @"""satellite_name"":" + $"[{string.Join(", ", satStringList)}], " +
				  @"""polarization"":" + $"[{string.Join(", ", polarizationStringList)}], " +
				  @"""modulation"":" + $"[{string.Join(", ", modStandardStringList)}], " +
				  @"""roll_off"":" + $"[{string.Join(", ", rolloffStringList)}], " +
				  @"""fecStringList"":" + $"[{string.Join(", ", fecStringList)}], " +
					@"""encryption_type"": [""BISS-1"", ""BISS-CA"", ""BISS-E"", ""None"", ""Not found""],
	                ""dolby_audio"": [""Yes"", ""No"", ""Not found""],
	                ""description"": [""PGM English Stereo Left"", ""PGM English Stereo Right"", ""PGM English Mono"", ""Commentary Clean Language 1 Mono"", ""Commentary Clean Language 2 Mono"", ""International Sound Stereo Left"", ""International Sound Stereo Right"", ""International Sound MONO"", ""PGM Language 3 Mono"", ""PGM Language 4 Mono"", ""Dolby E Channel A"", ""Dolby E Channel B"", ""Not found""],
	                ""video_format"": [""1080i50"", ""1080i60"", ""1080p50"", ""1080p60"", ""Not found""]
                }
                The required formats: 
                    Date values should be formatted as dd/mm/yyyy hh:mm:ss. 
                    Frequency values should be formatted in MHz with 6 digits and 3 decimals places (e.g., 123.567 MHz). 
                    Symbol rate should be formatted in MSym/s with 8 decimals.
				Additional information that might help with mapping:
					for Satellites, you will see that the names are often an abbreviation of a Satellite provider (e.g. EUT) followed by numbers and letters (e.g. 7A), we're not interested in the letters so you can map ""EUT 7A-7E"" for example to ""EUT 7""
					polarization is often indicated as X (equivalent to Horizontal) or Y polarization (equivalent to Vertical) 
				";

			history.AddSystemMessage( systemMessage
				);
				IChatCompletionService chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
				history.AddUserMessage("Here is the JSON object: " + json);
				var result = chatCompletionService.GetChatMessageContentAsync(history, kernel: _kernel).GetAwaiter().GetResult();
				return result.Content;
			}
		}
	}

	//---------------------------------
	// AzureSecrets.cs
	//---------------------------------

	namespace TextAnalysis
	{
		public class AzureSecrets
		{
			public string DocumentIntelligenceEndpoint { get; set; }

			public string DocumentIntelligenceKey { get; set; }

			public string AzureOpenAIEndpoint { get; set; }

			public string AzureOpenAIKey { get; set; }

			public string ModelDeploymentName { get; set; }

			public static AzureSecrets GetUserSecrets()
			{
				using (StreamReader r = new StreamReader("C:\\Skyline DataMiner\\Documents\\DMA_COMMON_DOCUMENTS\\Text Analysis PoC\\secrets.json"))
				{
					string json = r.ReadToEnd();
					return JsonConvert.DeserializeObject<AzureSecrets>(json);
				}
			}
		}
	}

