
namespace SLCTextAnalysisUpload
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using Skyline.DataMiner.Automation;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			// DO NOT REMOVE THIS COMMENTED-OUT CODE OR THE SCRIPT WON'T RUN!
			// DataMiner evaluates if the script needs to launch in interactive mode.
			// This is determined by a simple string search looking for "engine.ShowUI" in the source code.
			// However, because of the toolkit NuGet package, this string cannot be found here.
			// So this comment is here as a workaround.
			//// engine.ShowUI();
			try
			{
				RunSafe(engine);
			}
			catch (ApplicationException)
			{
				// Prevent failure on exit
			}
			catch (Exception e)
			{
				engine.ExitFail("Run|Something went wrong: " + e);
			}
		}

		private void RunSafe(IEngine engine)
		{
			var controller = new UploadController(engine);
			controller.OnOK += (sender, args) => UploadFile(engine, controller);
			controller.OnCancel += (sender, args) => Exit(engine);
			controller.BuildDialog();
			controller.Run();
		}

		private void Exit(IEngine engine)
		{
			throw new ApplicationException("Exit");
		}

		private void UploadFile(IEngine engine, UploadController controller)
		{
			controller.ToggleOkButton();
			//controller.Update();

			ScriptLogic model = new ScriptLogic();
			bool uploadSucceeded = model.UploadFile(controller);

			if (uploadSucceeded)
			{
				// Use file to create Feed
				var subscriptInfo = engine.PrepareSubScript("SLC_TextAnalysis_Script");

				// engine.GenerateInformation($"File path: {model.PathSavedFile}");

				subscriptInfo.SelectScriptParam("inputFile", model.PathSavedFile);
				subscriptInfo.Synchronous = true;

				try
				{
					subscriptInfo.StartScript();
				}
				catch (Exception)
				{

				}				

				// engine.AddScriptOutput()

				throw new ApplicationException("Exit");
			}
			else
			{
				// Toggle OK button on again such that people can upload another file
				// engine.GenerateInformation("Upload did not succeed");
				controller.ToggleOkButton();
				//controller.Update();
			}
		}
	}
}
