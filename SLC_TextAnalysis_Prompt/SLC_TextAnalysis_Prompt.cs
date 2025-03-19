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
using Skyline.DataMiner.Utils.InteractiveAutomationScript;
using System;
using System.IO;
using System.Text.Json;
using TextAnalysisPrompt;

namespace TextAnalysis
{
	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private static Kernel _kernel;
		private InteractiveController app;

		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			// DO NOT REMOVE THE COMMENTED OUT CODE BELOW OR THE SCRIPT WONT RUN!
			// Interactive scripts need to be launched differently.
			// This is determined by a simple string search looking for "engine.ShowUI" in the source code.
			// However, due to the NuGet package, this string can no longer be detected.
			// This comment is here as a temporary workaround until it has been fixed.
			//// engine.ShowUI();

			try
			{
				app = new InteractiveController(engine);

				var dialog = new TextAnalysisDialog(engine);
				dialog.Accepted += Dialog_Accepted;
				dialog.Cancelled += Dialog_Cancelled;

				app.ShowDialog(dialog);
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
		private void Dialog_Cancelled(object sender, EventArgs e)
		{
			app.Engine.ExitSuccess("Running prompt cancelled.");
		}

		private void Dialog_Accepted(object sender, EventArgs e)
		{
			var dialog = sender as TextAnalysisDialog;
			if (dialog == null)
				throw new ArgumentException("Invalid sender type");

			try
			{
				var secrets = AzureSecrets.GetUserSecrets();

				string fileName = "";
				if (!string.IsNullOrWhiteSpace(dialog.FilePath))
				{
					var markdown = ExtractFileContentAsMarkdown(dialog.FilePath, secrets);
					dialog.Input = "Here is the markdown text: " + markdown;

					fileName = Path.GetFileName(dialog.FilePath);
					app.Engine.AddScriptOutput("FileName", fileName);
				}
				app.Engine.Log("File name: " + fileName);

				InitializeKernel(secrets);
				var output = ChatCompletion(dialog.Input, dialog.Prompt);
				app.Engine.Log("TextAnalysisPrompt output: " + output);
				app.Engine.AddScriptOutput("Output", output);
			}
			catch (Exception ex)
			{
				app.Engine.Log("Failed to run prompt: " + ex.Message);
				ShowExceptionDialog(app, "Failed to run prompt", ex);
				return;
			}

			app.Engine.ExitSuccess("Successfully run prompt");
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

		private static string ChatCompletion(string input, string prompt)
		{
			ChatHistory history = new ChatHistory();
			history.AddSystemMessage(prompt);
			IChatCompletionService chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
			if (!string.IsNullOrWhiteSpace(input))
				history.AddUserMessage(input);
			var result = chatCompletionService.GetChatMessageContentAsync(history, kernel: _kernel).GetAwaiter().GetResult();
			return result.Content;
		}
		public static void ShowExceptionDialog(InteractiveController app, string title, Exception ex)
		{
			var exceptionDialog = new ExceptionDialog(app.Engine, ex);
			exceptionDialog.Title = title;
			exceptionDialog.OkButton.Pressed += (s, args) => app.Engine.ExitSuccess(title);

			app.ShowDialog(exceptionDialog);
		}
	}
}
