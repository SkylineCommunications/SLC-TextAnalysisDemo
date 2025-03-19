
namespace ExtensionsNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;



	public class DOMExtensions
		{
			public static List<string> GetEnumDisplayValues<T>(Func<T, string> toValueFunc) where T : Enum
			{
				List<T> enumList = Enum.GetValues(typeof(T)).Cast<T>().ToList();
				var stringList = enumList.Select(toValueFunc).ToList();
				//_engine.GenerateInformation($"Possible values: {string.Join(", ", stringList)}");

				return stringList;
			}
		}
}
