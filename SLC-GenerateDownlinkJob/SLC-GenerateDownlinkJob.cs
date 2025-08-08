using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Skyline.DataMiner.Automation;
using DomHelpers;
using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
using DomHelpers.SlcWorkflow;
using DomHelpers.Satellitefeeds;
using Skyline.DataMiner.Utils.MediaOps.Helpers.Scheduling;
using Skyline.DataMiner.Utils.MediaOps.Helpers.Workflows;
using Newtonsoft.Json;
using System.Linq;
using SLC_Popups.IAS.Extensions;

namespace SLCGenerateDownlinkJob
{
	

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		// helper class for MediaOps
		private SchedulingHelper _schedulingHelper;

		// DomHelpers
		private DomHelper _domHelperWorkflows;
		private DomHelper _domHelperFeeds;

		private IEngine _engine;

		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		/// engine.ShowUI();
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

			Guid feedId = JsonConvert.DeserializeObject<List<Guid>>(_engine.GetScriptParam("Satellite Feed ID").Value)[0];

			var filter = DomInstanceExposers.Id.Equal(new DomInstanceId(feedId));

			DomInstance domFeedInstance;

			try
			{
				domFeedInstance = _domHelperFeeds.DomInstances.Read(filter).Single<DomInstance>();
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to get Satellite Feed instance with ID : {feedId.ToString()}");
			}

			var instance = new MappedFeedInstance(domFeedInstance);

			var eventName = instance.MappedFeedEventInfo.EventName;
			var startDate = (DateTime)instance.MappedFeedEventInfo.StartDate;
			var endDate = (DateTime)instance.MappedFeedEventInfo.EndDate;

			if (DateTime.Now > startDate && DateTime.Now > endDate)
			{

				startDate = DateTime.Today.AddDays(1) + startDate.TimeOfDay;

				if (startDate > endDate)
				{
					// if the start is later than the end, just create a job that lasts for 1 hours
					endDate = startDate.AddHours(1);
				} else
				{
					endDate = DateTime.Today.AddDays(1) + endDate.TimeOfDay;
				}

				var confirmed = engine.ShowConfirmDialog($@"The detected event time is in the past.
The job will be scheduled for the same time as in the document, but on tomorrow's date.

Start time:		{startDate.ToString()}
End time:		{endDate.ToString()}

Do you want to proceed?");

				if (!confirmed)
				{
					return;
				}

			}

			JobConfiguration jobConfig = new JobConfiguration();
			jobConfig.Name = eventName;
			jobConfig.Start = startDate;
			jobConfig.End = endDate;

			var createdDomJobId = _schedulingHelper.CreateJob(jobConfig);

			//Job job = _schedulingHelper.GetJob(createdDomJobId);
			//job.ExecuteJobAction(JobAction.SaveAsTentative);

			return;
		}


		private void Init()
		{
			_schedulingHelper = new SchedulingHelper(_engine);

			_domHelperWorkflows = new DomHelper(_engine.SendSLNetMessages, SlcWorkflowIds.ModuleId);

			_domHelperFeeds = new DomHelper(_engine.SendSLNetMessages, SatellitefeedsIds.ModuleId);
		}
	}
}
