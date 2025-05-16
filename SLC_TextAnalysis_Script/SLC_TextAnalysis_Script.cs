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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SLC_Popups;
using SLC_Popups.IAS.Extensions;
using System.Web;

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

			private static readonly string extract_parameters_prompt = @"c:\Skyline DataMiner\Documents\dma_common_documents\Text Analysis PoC\extract_parameters_prompt.txt";
			private static readonly string map_parameters_prompt = @"c:\Skyline DataMiner\Documents\dma_common_documents\Text Analysis PoC\map_parameters_prompt.txt";

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

				string markdown = null;

				try
				{
					markdown = ExtractFileContentAsMarkdown(filename, secrets);
				}
				catch (Exception ex)
				{
					engine.ShowErrorDialog($"Failed to extract markdown text from document. Exception: {ex}");
				}

				engine.Log("Markdown content: " + markdown);

				string json = null;
				try
				{
					json = ExtractJson(markdown);
					engine.Log("JSON content: " + json);
					engine.GenerateInformation("Extracted JSON content: " + json);
				}
				catch (Exception ex)
				{
					engine.ShowErrorDialog($"Failed to extract parameters from markdown text. Exception: {ex}");
				}

				// Create DOM instance from the extracted parameters
				Guid id = Guid.Empty;
				try
				{
					id = CreateExtractedFeed(json, filename);
				}
				catch (Exception ex)
				{
					engine.ShowErrorDialog("Failed to create DOM instance with raw parameter values from LLM response.");
				}


				string mappedJson = null;
				try
				{
					mappedJson = MapValues(json);
					engine.Log("Mapped JSON content: " + mappedJson);
					engine.GenerateInformation("Mapped JSON content: " + mappedJson);
				}
				catch (Exception ex)
				{
					engine.ShowErrorDialog("Failed to map parameters found in document to predefined values.");
				}

				try
				{
					CreateMappedFeed(id, mappedJson, filename);
				}
				catch (Exception ex)
				{
					engine.ShowErrorDialog("Failed to create Satelltie Feed DOM instance from LLM response containing mapped parameters.");
				}
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

			// this will convert the uploaded file (pdf or image) to text formatted in markdown
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

			// this method will extract the parameters from the markdown formated text
			// the prompt used to do is this is consturctued using a text file stored under de documents folder in DataMiner
			private static string ExtractJson(string markdown)
			{
				var promptString = File.ReadAllText(extract_parameters_prompt);

				ChatHistory history = new ChatHistory();

				//_engine.GenerateInformation($"Extract paramters prompt: {promptString}");

				history.AddSystemMessage(promptString);

				IChatCompletionService chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
				history.AddUserMessage("Here is the markdown text: " + markdown);
				var result = chatCompletionService.GetChatMessageContentAsync(history, kernel: _kernel).GetAwaiter().GetResult();
				return result.Content;
			}

			// this method will intelligently map values extracted from the document to possible values in DataMiner (based on the enum options in DOM)
			// the prompt used to do is this is consturctued using a text file stored under de documents folder in DataMiner
			// the code will fill in placeholders dynamically
			private static string MapValues(string json)
				{
					var satStringList = DOMExtensions.GetEnumDisplayValues<SatellitefeedsIds.Enums.SatelliteEnum>(SatellitefeedsIds.Enums.Satellite.ToValue).Select(s => "\"" + s + "\"");
					var polarizationStringList = DOMExtensions.GetEnumDisplayValues<SatellitefeedsIds.Enums.DownlinkpolarizationEnum>(SatellitefeedsIds.Enums.Downlinkpolarization.ToValue).Select(s => "\"" + s + "\"");
					var modStandardStringList = DOMExtensions.GetEnumDisplayValues<SatellitefeedsIds.Enums.ModulationstandardEnum>(SatellitefeedsIds.Enums.Modulationstandard.ToValue).Select(s => "\"" + s + "\"");
					var rolloffStringList = DOMExtensions.GetEnumDisplayValues<SatellitefeedsIds.Enums.RolloffEnum>(SatellitefeedsIds.Enums.Rolloff.ToValue).Select(s => "\"" + s + "\"");
					var fecStringList = DOMExtensions.GetEnumDisplayValues<SatellitefeedsIds.Enums.FECEnum>(SatellitefeedsIds.Enums.FEC	.ToValue).Select(s => "\"" + s + "\"");

					var history = new ChatHistory();

					// read prompt message from file
					var promptString = File.ReadAllText(map_parameters_prompt);
					// values that will replace placehodlers in the prompt (e.g. allowed satellite names based on what is defined in the system as possible enum values in DOM in this case)
					var replacements = new Dictionary<string, string>
					{
						{ "satStringList", $"[{string.Join(", ", satStringList)}]" },
						{ "polarizationStringList", $"[{string.Join(", ", polarizationStringList)}]" },
						{ "modStandardStringList", $"[{string.Join(", ", modStandardStringList)}]" },
						{ "rolloffStringList", $"[{string.Join(", ", rolloffStringList)}]" },
						{ "fecStringList", $"[{string.Join(", ", fecStringList)}]" },
					};

					// Replace placeholders
					string systemMessage = Regex.Replace(promptString, @"\{\{(\w+)\}\}", match =>
					{
						string key = match.Groups[1].Value;
						return replacements.TryGetValue(key, out string value) ? value : match.Value;
					});

						history.AddSystemMessage( systemMessage);
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
				using (StreamReader r = new StreamReader("C:\\Skyline DataMiner\\AI-sample\\secrets\\secrets.json"))
				{
					string json = r.ReadToEnd();
					return JsonConvert.DeserializeObject<AzureSecrets>(json);
				}
			}
		}
	}

