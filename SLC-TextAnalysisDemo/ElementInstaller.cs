namespace Elements
{
    using System;
    using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Automation;
	using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.Messages.Advanced;

    internal class ElementInstaller
    {
        private readonly IEngine engine;

        public ElementInstaller(IEngine engine)
        {
            this.engine = engine;
        }

        public void InstallDefaultContent()
        {
            int viewID = CreateViews(new string[] { "DataMiner Catalog", "Empower 2025", "Using AI to automate your operations" });
            CreateElement("Empower 2025 - Restart", "Empower - Restart SLAutomation", "0.0.0.1", viewID);
        }

        private void AssignVisioToView(int viewID, string visioFileName)
        {
            var request = new AssignVisualToViewRequestMessage(viewID, new Skyline.DataMiner.Net.VisualID(visioFileName));

            engine.SendSLNetMessage(request);
        }

        private int? GetView(string viewName)
        {
            var views = engine.SendSLNetMessage(new GetInfoMessage(InfoType.ViewInfo));
            foreach (var m in views)
            {
                var viewInfo = m as ViewInfoEventMessage;
                if (viewInfo == null)
                    continue;

                if (viewInfo.Name == viewName)
                    return viewInfo.ID;
            }

            return null;
        }

        private int CreateNewView(string viewName, string parentViewName)
        {
            var request = new SetDataMinerInfoMessage
            {
                bInfo1 = int.MaxValue,
                bInfo2 = int.MaxValue,
                DataMinerID = -1,
                HostingDataMinerID = -1,
                IInfo1 = int.MaxValue,
                IInfo2 = int.MaxValue,
                Sa1 = new SA(new string[] { viewName, parentViewName }),
                What = (int)NotifyType.NT_ADD_VIEW_PARENT_AS_NAME,
            };

            var response = engine.SendSLNetSingleResponseMessage(request);
            if (!(response is SetDataMinerInfoResponseMessage infoResponse))
                throw new ArgumentException("Unexpected message returned by DataMiner");

            return infoResponse.iRet;
        }

        private int CreateViews(string[] viewNames)
        {
            int? firstNonExistingViewLevel = null;
            int? lastExistingViewID = null;
            string lastExistingViewName = null;

            for (int i = viewNames.Length - 1; i >= 0; --i)
            {
                int? viewID = GetView(viewNames[i]);
                if (viewID.HasValue)
                {
                    lastExistingViewID = viewID;
                    lastExistingViewName = viewNames[i];
                    firstNonExistingViewLevel = i + 1;
                    break;
                }
            }

            if (firstNonExistingViewLevel.HasValue && firstNonExistingViewLevel == viewNames.Length)
                return lastExistingViewID.Value;

            if (!firstNonExistingViewLevel.HasValue)
            {
                // No views in the tree already exist, so create all views starting from the root view
                lastExistingViewID = -1;
                lastExistingViewName = engine.GetDms().GetView(-1).Name;
                firstNonExistingViewLevel = 0;
            }

            for (int i = firstNonExistingViewLevel.Value; i < viewNames.Length; ++i)
            {
                lastExistingViewID = CreateNewView(viewNames[i], lastExistingViewName);
                lastExistingViewName = viewNames[i];
            }

            return lastExistingViewID.Value;
        }

        private void CreateElement(string elementName, string protocolName, string protocolVersion, int viewID,
            string trendTemplate = "Default", string alarmTemplate = "")
        {
            var request = new AddElementMessage
            {
                ElementName = elementName,
                ProtocolName = protocolName,
                ProtocolVersion = protocolVersion,
                TrendTemplate = trendTemplate,
                AlarmTemplate = alarmTemplate,
                ViewIDs = new int[] { viewID },
            };

            var dms = engine.GetDms();
            if (dms.ElementExists(elementName))
            {
                var elementRequest = new GetElementByNameMessage(elementName);
                var elementResponse = engine.SendSLNetSingleResponseMessage(elementRequest);
                if (!(elementResponse is ElementInfoEventMessage elementInfo))
                    throw new ArgumentException("Unexpected message returned by DataMiner");

                // Still send a message to make sure the correct protocol, template and view are assigned, but fill in the current ids of the element
                request.DataMinerID = elementInfo.DataMinerID;
                request.ElementID = elementInfo.ElementID;
            }

            engine.SendSLNetSingleResponseMessage(request);
        }
    }
}
