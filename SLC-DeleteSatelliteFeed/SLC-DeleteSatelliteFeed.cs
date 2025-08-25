using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
using Skyline.DataMiner.Net.Profiles;
using Skyline.DataMiner.Utils.MediaOps.Helpers.Scheduling;
using Skyline.DataMiner.Utils.MediaOps.Helpers.Workflows;
using Skyline.DataMiner.Utils.MediaOps.Helpers.Relationships;
using Newtonsoft.Json;
using DomHelpers.SlcWorkflow;
using DomHelpers.Satellitefeeds;
using DomHelpers.SlcRelationships;
using System.Linq;
using Skyline.DataMiner.Net.Upload;
using Skyline.DataMiner.Net.Messages.SLDataGateway;
using DomIds;

namespace SLCDeleteSatelliteFeed
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
		private DomHelper _domHelperRelationships;

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



			// cleanup the raw extracted feed values (stored in sepaarte DOM object) 
			var extractedFeedFilter = DomInstanceExposers.Id.Equal(new DomInstanceId(instance.MappingInfo.ExtractedFeedLink.Value));

			DomInstance domExtactedFeedInstance;
			try
			{
				domExtactedFeedInstance = _domHelperFeeds.DomInstances.Read(extractedFeedFilter).Single<DomInstance>();
				var extractedFeedInstance = new ExtractedFeedInstance(domExtactedFeedInstance);
				_engine.GenerateInformation("delete extracted feed");
				extractedFeedInstance.Delete(_domHelperFeeds);
			}
			catch (Exception ex)
			{
				// nothing
				// extracted feed must have already been deleted
			}

			// delete realtionships and the underlying jobs
			// get relationships for mapped feed
			var feedObjectType = _relationshipsHelper.GetObjectType("Satellite Feed");

			if (feedObjectType == null)
			{
				_engine.GenerateInformation("Satellite Feed Object Type not found");
				// nothing
			}
			else
			{
				// filter in DOM because helper class not working
				var domRelationshipFilter = DomInstanceExposers.FieldValues.DomInstanceField(DomIds.SlcRelationships.Sections.LinkInfo.ChildObjectType).Equal(feedObjectType.Id).AND(DomInstanceExposers.FieldValues.DomInstanceField(DomIds.SlcRelationships.Sections.LinkInfo.ChildObjectID).Equal(feedId.ToString()));
				var domRelationshipsInstances = _domHelperRelationships.DomInstances.Read(domRelationshipFilter);

				// not working --> fix from MO team required
				// var links = _relationshipsHelper.GetLinks(new LinkFilter() { DomObjectTypeId = feedObjectType.Id, ObjectId = instance.ID.Id.ToString() });

				//_engine.GenerateInformation($"delete links: no of links: {domRelationshipsInstances.Count()}");

				foreach (var link in domRelationshipsInstances)
				{
					var relationshipInstance = new LinksInstance(link);
					var relatedJob = relationshipInstance.LinkInfo.ParentObjectID;
					_schedulingHelper.DeleteJob(Guid.Parse(relatedJob));

					relationshipInstance.Delete(_domHelperRelationships);
				}
			}

			// delete the mapped satellite feed instance
			_engine.GenerateInformation("delete mapped feed");
			instance.Delete(_domHelperFeeds);
		}

		private void Init()
		{
			_schedulingHelper = new SchedulingHelper(_engine);

			_relationshipsHelper = new RelationshipsHelper(_engine);

			_domHelperWorkflows = new DomHelper(_engine.SendSLNetMessages, SlcWorkflowIds.ModuleId);

			_domHelperFeeds = new DomHelper(_engine.SendSLNetMessages, SatellitefeedsIds.ModuleId);

			_domHelperRelationships = new DomHelper(_engine.SendSLNetMessages, SlcRelationships.ModuleId);

			_profileHelper = new ProfileHelper(_engine.SendSLNetMessages);
		}
	}
}
