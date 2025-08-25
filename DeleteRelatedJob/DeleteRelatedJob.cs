using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
using Skyline.DataMiner.Net.Profiles;
using Skyline.DataMiner.Utils.MediaOps.Helpers.Relationships;
using Skyline.DataMiner.Utils.MediaOps.Helpers.Scheduling;

namespace DeleteRelatedJob
{
	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		// helper class for MediaOps
		private SchedulingHelper _schedulingHelper;
		private RelationshipsHelper _relationshipsHelper;


		// DomHelpers
		//private DomHelper _domHelperWorkflows;
		//private DomHelper _domHelperFeeds;


		private IEngine _engine;

		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			try
			{
				_engine = engine;	

				Init();

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

			Guid linkId = JsonConvert.DeserializeObject<List<Guid>>(_engine.GetScriptParam("Link ID").Value)[0];

			var link = _relationshipsHelper.GetLink(linkId);

			var jobId = link.ParentObjectId;

			_schedulingHelper.DeleteJob(Guid.Parse(jobId));

			link.Delete();
		}

		private void Init()
		{
			_schedulingHelper = new SchedulingHelper(_engine);

			_relationshipsHelper = new RelationshipsHelper(_engine);
		}
	}
}
