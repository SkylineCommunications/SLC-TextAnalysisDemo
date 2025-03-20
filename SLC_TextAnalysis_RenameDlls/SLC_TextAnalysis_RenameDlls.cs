using System;
using System.IO;
using Skyline.DataMiner.Automation;

namespace DeleteDlls
{
	public class Script
	{
		public void Run(IEngine engine)
		{
			string dllPath = @"C:\Skyline DataMiner\Files\Microsoft.Extensions.DependencyInjection.dll";
			string dllPath_new = @"C:\Skyline DataMiner\Files\Microsoft.Extensions.DependencyInjection_2.dll";
			string dllPath2 = @"C:\Skyline DataMiner\Files\Microsoft.Extensions.DependencyInjection.Abstractions.dll";
			string dllPath2_new = @"C:\Skyline DataMiner\Files\Microsoft.Extensions.DependencyInjection.Abstractions_2.dll";

			try
			{
				if (File.Exists(dllPath))
				{
					//File.Delete(dllPath);
					File.Move(dllPath, dllPath_new);
					engine.Log("Microsoft.Extensions.DependencyInjection.dll deleted successfully.");
				}
				else
				{
					engine.Log("Microsoft.Extensions.DependencyInjection.dll not found.");
				}
			}
			catch (Exception ex)
			{
				engine.Log($"Error renaming Microsoft.Extensions.DependencyInjection.dll: {ex.Message}");
			}

			try
			{
				if (File.Exists(dllPath2))
				{
					//File.Delete(dllPath2);
					File.Move(dllPath2, dllPath2_new);
					engine.Log("Microsoft.Extensions.DependencyInjection.Abstractions.dll deleted successfully.");
				}
				else
				{
					engine.Log("Microsoft.Extensions.DependencyInjection.Abstractions.dll not found.");
				}
			}
			catch (Exception ex)
			{
				engine.Log($"Error renaming Microsoft.Extensions.DependencyInjection.Abstractions.dll: {ex.Message}");
			}
		}
	}
}