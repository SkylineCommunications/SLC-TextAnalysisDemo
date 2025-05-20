namespace SLC_Popups.IAS.Objects
{
	using SLC_Popups.IAS.Components;

	public static class AutomationData
	{
		public static readonly string InitialDropdownValue = "- Select -";

		public static Choice<T> CreateDefaultDropDownOption<T>()
		{
			return Choice.Create<T>(default, InitialDropdownValue);
		}
	}
}
