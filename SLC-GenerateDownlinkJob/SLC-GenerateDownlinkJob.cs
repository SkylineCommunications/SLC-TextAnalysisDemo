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
using Skyline.DataMiner.Utils.MediaOps.Helpers.Relationships;
using Skyline.DataMiner.Net.Profiles;
using Newtonsoft.Json;
using System.Linq;
using SLC_Popups.IAS.Extensions;
using Skyline.DataMiner.Net.Serialization;
using Skyline.DataMiner.Net.Messages.SLDataGateway;
using Skyline.DataMiner.Net;
using DomIds;
using Parameter = Skyline.DataMiner.Net.Profiles.Parameter;
using Skyline.DataMiner.Analytics.RCA;
using Skyline.DataMiner.Utils.MediaOps.Common.IOData.Relationships.Scripts.RelationshipHandler;

namespace SLCGenerateDownlinkJob
{
	

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private ProfileHelper _profileHelper;

		// helper class for MediaOps
		private SchedulingHelper _schedulingHelper;
		private RelationshipsHelper _relationshipsHelper;


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

			// general info from the satellite feed document
			var eventName = instance.MappedFeedEventInfo.EventName;
			var startDate = (DateTime)instance.MappedFeedEventInfo.StartDate;
			var endDate = (DateTime)instance.MappedFeedEventInfo.EndDate;
			// satellite parameters
			var frequency = instance.MappedFeedParameters.DownlinkFrequency;
			var modulationStandard = SatellitefeedsIds.Enums.Modulationstandard.ToValue(instance.MappedFeedParameters.ModulationStandard.Value);
			var polarization = SatellitefeedsIds.Enums.Modulationstandard.ToValue(instance.MappedFeedParameters.ModulationStandard.Value);
			var satellite = SatellitefeedsIds.Enums.Modulationstandard.ToValue(instance.MappedFeedParameters.ModulationStandard.Value);
			var symbolrate = instance.MappedFeedParameters.SymbolRate;

			// create job 
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
End time:			{endDate.ToString()}

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
			jobConfig.DomWorkflowId = GetWorkflowId("Satellite Downlink");

			var createdDomJobId = _schedulingHelper.CreateJob(jobConfig);

			var jobInstance = GetJob(createdDomJobId);
			var jobId = jobInstance.JobInfo.JobID;

			var jobParamConfig = GetJobConfig(jobInstance);

			SetConfig(jobParamConfig, instance.MappedFeedParameters);

			// var job = _schedulingHelper.GetJob(createdDomJobId);
			//Job job = _schedulingHelper.GetJob(createdDomJobId);
			//job.ExecuteJobAction(JobAction.SaveAsTentative);

			// creates relationship link between the mapped feed and the created job
			CreateLink(instance, jobInstance);

			return;
		}

		private Guid GetWorkflowId(string workflowName)
		{
			var workflowFitler = DomInstanceExposers.FieldValues.DomInstanceField(DomIds.SlcWorkflow.Sections.WorkflowInfo.WorkflowName).Equal(workflowName);

			DomInstance domWorkflowInstance;

			try
			{
				domWorkflowInstance = _domHelperWorkflows.DomInstances.Read(workflowFitler).Single<DomInstance>();
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to get Workflow instance with name : {workflowName}");
			}

			return domWorkflowInstance.ID.Id;
		}

		private JobsInstance GetJob(Guid domId)
		{

			var filter = DomInstanceExposers.Id.Equal(new DomInstanceId(domId));

			DomInstance domJobdInstance;

			try
			{
				domJobdInstance = _domHelperWorkflows.DomInstances.Read(filter).Single<DomInstance>();
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to get Satellite Feed instance with ID : {domId.ToString()}");
			}

			return new JobsInstance(domJobdInstance);
		}

		ConfigurationInstance GetJobConfig(JobsInstance job)
		{
			var jobConfigId = job.JobExecution.JobConfiguration.Value;

			var filter = DomInstanceExposers.Id.Equal(new DomInstanceId(jobConfigId));

			DomInstance domConfigInstance;

			try
			{
				domConfigInstance = _domHelperWorkflows.DomInstances.Read(filter).Single<DomInstance>();
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to get Job Config with ID: {jobConfigId.ToString()} for Job instance with ID : {job.ID.Id.ToString()}");
			}

			return new ConfigurationInstance(domConfigInstance);
		}

		private void SetConfig(ConfigurationInstance config, MappedFeedParametersSection mappedParamsSection)
		{
			var mappedParams = new List<MappedParameters>() {
				new MappedParameters { Name = "Downlink Frequecy", profileParam = _profileHelper.ProfileParameters.Read(ParameterExposers.Name.Equal("Downlink Frequency")).SingleOrDefault(), doubleValue = mappedParamsSection.DownlinkFrequency.Value},
				new MappedParameters { Name = "Modulation Standard", profileParam = _profileHelper.ProfileParameters.Read(ParameterExposers.Name.Equal("Modulation Standard")).SingleOrDefault(), stringValue = SatellitefeedsIds.Enums.Modulationstandard.ToValue(mappedParamsSection.ModulationStandard.Value)}, 
				new MappedParameters { Name = "Satellite", profileParam = _profileHelper.ProfileParameters.Read(ParameterExposers.Name.Equal("Satellite")).SingleOrDefault(), stringValue = SatellitefeedsIds.Enums.Satellite.ToValue(mappedParamsSection.Satellite.Value)},
				new MappedParameters { Name = "Symbol Rate", profileParam = _profileHelper.ProfileParameters.Read(ParameterExposers.Name.Equal("Symbol Rate")).SingleOrDefault(), doubleValue = mappedParamsSection.SymbolRate.Value},
				new MappedParameters { Name = "Polarization", profileParam = _profileHelper.ProfileParameters.Read(ParameterExposers.Name.Equal("Polarization")).SingleOrDefault(), stringValue = SatellitefeedsIds.Enums.Downlinkpolarization.ToValue(mappedParamsSection.DownlinkPolarization.Value)},
			};

			foreach (var mappedParam in mappedParams)
			{
				var parameter = config.ProfileParameterValues.FirstOrDefault(p => p.ProfileParameterID == mappedParam.profileParam.ID.ToString());

				if (parameter != null)
				{
					if (mappedParam.profileParam.Type == Parameter.ParameterType.Number)
					{
						_engine.GenerateInformation("Set number parameter");
						parameter.DoubleValue = (int)mappedParam.doubleValue;
					}
					else if (mappedParam.profileParam.Type == Parameter.ParameterType.Discrete)
					{
						_engine.GenerateInformation($"Set discrete parameter: value = {mappedParam.stringValue}");
						parameter.StringValue = mappedParam.stringValue;
					}
					else
					{
						_engine.GenerateInformation("Parameter is no number or discrete and will hence not be handled");
						continue;
					}
				}
			}

			// save job configurations to job configuration in (slc)workflow
			config.Save(_domHelperWorkflows);
		}

		private bool CreateLink(MappedFeedInstance instance, JobsInstance jobInstance)
		{
			// add relationship between the job and the feed
			var feedObjectType = _relationshipsHelper.GetObjectType("Satellite Feed");

			if (feedObjectType == null)
			{
				var feedObjectTypeId = _relationshipsHelper.CreateObjectType(new ObjectTypeConfiguration() { Name = "Satellite Feed" });
				feedObjectType = _relationshipsHelper.GetObjectType("Satellite Feed");
			}

			var jobObjectType = _relationshipsHelper.GetObjectType("Job");

			if (jobObjectType == null)
			{
				var jobObjectTypeId = _relationshipsHelper.CreateObjectType(new ObjectTypeConfiguration() { Name = "Job" });
				jobObjectType = _relationshipsHelper.GetObjectType("Job");
			}

			// create URL to link between job and mapped feeds
			var jobLink = $@"/app/ab26de0d-b0d7-456c-9c7b-155008a5cbc9/Job View?object manager instances=(slc)workflow/{jobInstance.ID.Id.ToString()}#{{""actions"":[{{""__type"":""Skyline.DataMiner.Web.Common.v1.DMAApplicationPagePanelAction"",""AsOverlay"":true,""Draggable"":false,""Panel"":""d5d8c129-f72e-495f-b7aa-60f02aa284c3"",""Position"":""Right"",""PostActions"": null,""Type"":6,""Width"":75}}]}}";
			// to do : make it select the actual feed in the app 
			var mappedFeedLink = $@"/app/92da828f-cd3e-4442-bd61-835d90db15ba/Parameter%20Extractor";

			var linkConfig = new LinkConfiguration()
			{
				Child = new LinkDetailsConfiguration() { DomObjectTypeId = feedObjectType.Id, ObjectId = instance.ID.Id.ToString(), ObjectName = instance.MappedFeedEventInfo.EventName, URL = mappedFeedLink },
				Parent = new LinkDetailsConfiguration() { DomObjectTypeId = jobObjectType.Id, ObjectId = jobInstance.ID.Id.ToString(), ObjectName = jobInstance.JobInfo.JobID, URL = jobLink }
			};

			_relationshipsHelper.CreateLink(linkConfig);

			return true;
		}

		private class MappedParameters
		{
			public string Name { get; set; }
			public Parameter profileParam { get; set; }
			public double doubleValue { get; set; }
			public string stringValue { get; set; }
		}

		private void Init()
		{
			_schedulingHelper = new SchedulingHelper(_engine);

			_relationshipsHelper = new RelationshipsHelper(_engine);

			_domHelperWorkflows = new DomHelper(_engine.SendSLNetMessages, SlcWorkflowIds.ModuleId);

			_domHelperFeeds = new DomHelper(_engine.SendSLNetMessages, SatellitefeedsIds.ModuleId);

			_profileHelper = new ProfileHelper(_engine.SendSLNetMessages);
		}
	}
}
