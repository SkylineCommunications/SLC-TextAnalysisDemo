using System;
using System.Collections.Generic;
using System.Linq;
using Skyline.AppInstaller;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Net.AppPackages;
using Skyline.DataMiner.Net.Helper;
using Skyline.DataMiner.Net.Messages;

/// <summary>
/// DataMiner Script Class.
/// </summary>
internal class Script
{
	private static string _setupContentPath;
	private IEngine _engine;

	/// <summary>
	/// The script entry point.
	/// </summary>
	/// <param name="engine">Provides access to the Automation engine.</param>
	/// <param name="context">Provides access to the installation context.</param>
	[AutomationEntryPoint(AutomationEntryPointType.Types.InstallAppPackage)]
	public void Install(IEngine engine, AppInstallContext context)
    {
        try
        {
            engine.Timeout = new TimeSpan(0, 10, 0);
            engine.GenerateInformation("Starting installation");
            var installer = new AppInstaller(Engine.SLNetRaw, context);
            installer.InstallDefaultContent();

			// string setupContentPath = installer.GetSetupContentDirectory();
            _setupContentPath = installer.GetSetupContentDirectory();

			// Custom installation logic can be added here for each individual install package.
            var exceptions = new List<Exception>();
            installer.Log("Importing DOM...");
            exceptions.AddRange(ImportDom(engine));

            if (exceptions.Any())
			{
				throw new AggregateException(exceptions);
			}

            _engine = engine;
            ModifyScriptXmlFile("SLC_TextAnalysis_Prompt");
            ModifyScriptXmlFile("SLC_TextAnalysis_Script");
		}
        catch (Exception e)
        {
            engine.ExitFail($"Exception encountered during installation: {e}");
        }
    }

	private static List<Exception> ImportDom(IEngine engine)
	{
		var exceptions = new List<Exception>();

		try
		{
			// Will import all dom modules that are found in this folder
			// ImportDom(engine, @"c:\Skyline DataMiner\DOM\EventManager");
			string path = _setupContentPath + @"\DOMImportExport";
			engine.GenerateInformation($"setupContentPath for DOM: {path}");

			ImportDom(engine, path);
		}
		catch (Exception e)
		{
			exceptions.Add(e);
		}

		return exceptions;
	}

	private static void ImportDom(IEngine engine, string path)
	{
		var subScript = engine.PrepareSubScript("DOM ImportExport");
		subScript.SelectScriptParam("Action", "Import");
		subScript.SelectScriptParam("Path", path);
		subScript.SelectScriptParam("ModuleNames", "-1");
		subScript.Synchronous = true;
		subScript.StartScript();
	}

	/// <summary>
	/// Modifies the Script_SLC_TextAnalysis_Prompt.xml file to update DLL paths.
	/// </summary>
	/// <param name="scriptName">The name of the automation script to modify.</param>
	private void ModifyScriptXmlFile(string scriptName)
	{
		try
		{
			Logger.Log($"Updating automation script: {scriptName}");
			var script = GetScript(scriptName);

			var exe = script.Exes.FirstOrDefault();
			var dllRefs = exe.CSharpDllRefs;

			Logger.Log($"Current DLL references: {dllRefs}");

			// Replace the DLL paths
			dllRefs = dllRefs.Replace(
						@"ProtocolScripts\DllImport\microsoft.extensions.dependencyinjection.abstractions\8.0.2\lib\net462\Microsoft.Extensions.DependencyInjection.Abstractions.dll",
						@"Files\Microsoft.Extensions.DependencyInjection.Abstractions.dll");

			dllRefs = dllRefs.Replace(
						@"ProtocolScripts\DllImport\microsoft.extensions.dependencyinjection\8.0.1\lib\net462\Microsoft.Extensions.DependencyInjection.dll",
						@"Files\Microsoft.Extensions.DependencyInjection.dll");

			exe.CSharpDllRefs = dllRefs;
			Logger.Log($"Updated DLL references: {dllRefs}");

			// Create the script with the updated DLL references
			CreateScript(scriptName, exe, script.Folder);
		}
		catch (Exception ex)
		{
			Logger.Log($"Error updating automation script: {ex.Message}");
		}
	}

	private IEnumerable<string> GetAllScripts()
	{
		return (_engine.SendSLNetSingleResponseMessage(new GetInfoMessage(InfoType.Scripts)) as GetScriptsResponseMessage)?.Scripts;
	}

	private bool ScriptExists(string scriptName)
	{
		if (string.IsNullOrWhiteSpace(scriptName))
		{
			return false;
		}

		var allScripts = GetAllScripts();

		if (allScripts.IsNullOrEmpty())
		{
			return false;
		}

		return allScripts.Any(s => string.Equals(s, scriptName, StringComparison.OrdinalIgnoreCase));
	}

	private GetScriptInfoResponseMessage GetScript(string scriptName)
	{
		if (!ScriptExists(scriptName))
		{
			Logger.Log($"Script {scriptName} not found");
			return null;
		}

		return Engine.SLNet.SendSingleResponseMessage(new GetScriptInfoMessage(scriptName)) as GetScriptInfoResponseMessage;
	}

	private void CreateScript(string scriptName, AutomationExeInfo automationExeInfo, string folder)
	{
		SaveAutomationScriptMessage request = new SaveAutomationScriptMessage()
		{
			IsUpdate = true,
			Definition = new GetScriptInfoResponseMessage()
			{
				Name = scriptName,
				Folder = folder,
				Options = ScriptOptions.AllowUndef | ScriptOptions.SavedFromCube,
				Type = AutomationScriptType.Automation,
				Exes = new AutomationExeInfo[] { automationExeInfo },
			},
		};

		_engine.SendSLNetSingleResponseMessage(request);
	}
}