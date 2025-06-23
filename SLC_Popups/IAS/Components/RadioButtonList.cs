namespace SLC_Popups.IAS.Components
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class RadioButtonList<T> : RadioButtonList
	{
		private readonly Dictionary<string, Choice<T>> _mapping = new Dictionary<string, Choice<T>>();

		public new IEnumerable<Choice<T>> Options
		{
			get
			{
				return _mapping.Values;
			}

			set
			{
				var list = value.ToList();

				_mapping.Clear();
				foreach (var option in list)
					_mapping[option.DisplayValue ?? string.Empty] = option;

				SetOptions(list.Select(x => x.DisplayValue ?? string.Empty));
			}
		}

		public Choice<T> SelectedOption
		{
			get
			{
				if (Selected == null)
				{
					return default;
				}

				_mapping.TryGetValue(Selected, out var selectedOption);

				return selectedOption;
			}
		}

		public T SelectedValue => SelectedOption.Value;

		public void Clear()
		{
			Options = Enumerable.Empty<Choice<T>>();
		}
	}
}