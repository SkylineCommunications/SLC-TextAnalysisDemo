﻿namespace SLC_Popups.IAS.Objects
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public abstract class ScriptDialog : Dialog
	{
		protected ScriptDialog(IEngine engine)
			: base(engine)
		{
			Init();
		}

		#region Properties
		protected ScriptLayout Layout { get; set; }
		#endregion

		#region Methods
		public abstract void Build();

		private void Init()
		{
			Layout = new ScriptLayout
			{
				RowPosition = 0,
			};
		}
		#endregion

		#region Classes
		protected class ScriptLayout
		{
			public int RowPosition { get; set; }
		}
		#endregion
	}
}
