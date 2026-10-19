using FindIt.Utilities;

using System;

namespace FindIt.Domain
{
	// Search checks every prefab against the same text, so the forms it compares are built once.
	public class SearchTerm
	{
		public string Text { get; }
		public string Lowercase { get; }
		public string WithoutSpaces { get; }
		public string Abbreviation { get; }
		public string[] Words { get; }

		// Mono matches ß with ss only in lowercase text, and only when ignoring case.
		public StringComparison Comparison { get; }

		public SearchTerm(string text)
		{
			Text = text;
			Lowercase = text.ToLowerInvariant();
			Words = Lowercase.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			Comparison = Mod.Settings.LinguisticSearch
				? StringComparison.InvariantCultureIgnoreCase
				: StringComparison.Ordinal;

			var abbreviationText = Lowercase.Replace("'s ", " ");

			WithoutSpaces = abbreviationText.Where(x => x != ' ');
			Abbreviation = abbreviationText.GetAbbreviation();
		}
	}
}
