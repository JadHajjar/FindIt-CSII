using Colossal.Entities;

using FindIt.Domain;

using Game.Prefabs;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Unity.Entities;

namespace FindIt.Utilities
{
    internal static class SearchUtil
    {
        public static bool HasMoreThanOne<T, T2>(this List<T> enumerable, Func<T, T2> predicate)
        {
            if (enumerable.Count < 2)
            {
                return false;
            }

            var first = predicate(enumerable[0]);

            for (var i = 1; i < enumerable.Count; i++)
            {
                if (!predicate(enumerable[i]).Equals(first))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsDecal(this EntityManager entityManager, Entity entity)
        {
            if (!entityManager.TryGetBuffer<SubMesh>(entity, true, out var subMesh) || subMesh.Length == 0)
            {
                return false;
            }

            if (!entityManager.TryGetComponent<MeshData>(subMesh[0].m_SubMesh, out var component))
            {
                return false;
            }

            return component.m_State == MeshFlags.Decal;
        }

        public static bool IsBrandEntity(this EntityManager entityManager, Entity entity)
        {
            if (entityManager.HasComponent<BrandObjectData>(entity))
            {
                return true;
            }

            if (!entityManager.TryGetBuffer<ObjectRequirementElement>(entity, true, out var requirementBuffer))
            {
                return false;
            }

            for (var i = 0; i < requirementBuffer.Length; i++)
            {
                if (entityManager.HasComponent<BrandData>(requirementBuffer[i].m_Requirement))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool SearchCheck(
            this SearchTerm searchTerm,
            string termToBeSearched,
            string lowercase = null,
            string abbreviation = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm.Text) && string.IsNullOrWhiteSpace(termToBeSearched))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(searchTerm.Text) || string.IsNullOrWhiteSpace(termToBeSearched))
            {
                return false;
            }

            // Search calls this for every prefab, and comparing lowercase strings ordinally is much
            // faster than a culture-aware comparison.
            lowercase ??= termToBeSearched.ToLowerInvariant();

            if (lowercase.IndexOf(searchTerm.Lowercase, StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            if (searchTerm.Lowercase.SpellCheck(lowercase, searchTerm.Text.Length + 1) <= (int)Math.Ceiling((searchTerm.Text.Length - 3) / 5M))
            {
                return true;
            }

            if (searchTerm.AbbreviationCheck(lowercase, abbreviation))
            {
                return true;
            }

            if (searchTerm.Text.IndexOf(' ') < 0)
            {
                return false;
            }

            // LINQ's All would allocate a delegate and an enumerator on every call, so this checks
            // the words in a loop.
            foreach (var word in searchTerm.Words)
            {
                if (lowercase.IndexOf(word, StringComparison.Ordinal) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        // Search calls this for every prefab, so it reads the name without copying it and keeps
        // one row of distances instead of the whole table.
        // The search text is already trimmed and single-spaced.
        public static int SpellCheck(this string text, string name, int length)
        {
            var start = 0;
            var end = Math.Min(name.Length, length);

            while (start < end && char.IsWhiteSpace(name[start]))
            {
                start++;
            }

            while (end > start && char.IsWhiteSpace(name[end - 1]))
            {
                end--;
            }

            // Levenshtein Algorithm
            var distances = new int[text.Length + 1];

            for (var i = 0; i <= text.Length; distances[i] = i++)
            { }

            for (var j = start; j < end; j++)
            {
                if (name[j] is ' ' && name[j - 1] is ' ')
                {
                    continue;
                }

                var diagonal = distances[0];

                distances[0] = diagonal + 1;

                for (var i = 1; i <= text.Length; i++)
                {
                    var above = distances[i];

                    distances[i] = Math.Min(Math.Min(above + 1, distances[i - 1] + 1),
                        diagonal + (text[i - 1] == name[j] ? 0 : 1));
                    diagonal = above;
                }
            }

            return distances[text.Length];
        }

        public static bool AbbreviationCheck(this SearchTerm searchTerm, string lowercase, string abbreviation = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm.Text))
            {
                return false;
            }

            abbreviation ??= lowercase.GetSearchAbbreviation();

            if (abbreviation.Length > 2 && abbreviation.StartsWith(searchTerm.WithoutSpaces, StringComparison.Ordinal))
            {
                return true;
            }

            return searchTerm.Abbreviation.Length > 2
                && searchTerm.Abbreviation.StartsWith(
                    lowercase.Replace("'s ", " ").Where(x => x != ' '),
                    StringComparison.Ordinal);
        }

        public static string GetSearchAbbreviation(this string text)
        {
            return text.ToLowerInvariant().Replace("'s ", " ").GetAbbreviation();
        }

        public static string GetAbbreviation(this string S)
        {
            // Indexing calls this for every prefab name, and search for every tag, so it reads the
            // characters one by one rather than matching a regex.
            var SB = new StringBuilder();
            var i = 0;

            while (i < S.Length)
            {
                if (!IsWordChar(S[i]))
                {
                    i++;
                    continue;
                }

                var start = i;
                var isNumber = true;

                while (i < S.Length && IsWordChar(S[i]))
                {
                    isNumber &= char.IsDigit(S[i]);
                    i++;
                }

                if (isNumber)
                {
                    SB.Append(S, start, i - start);
                }
                else
                {
                    SB.Append(S[start]);
                }

                // A word keeps its apostrophe ending: "Morgan's" adds one letter.
                if (i + 1 < S.Length && S[i] is '\'' && IsWordChar(S[i + 1]))
                {
                    i++;

                    while (i < S.Length && IsWordChar(S[i]))
                    {
                        i++;
                    }
                }
            }

            // Letters followed by a number, as in "ab12", become "ab 12".
            var letters = 0;

            while (letters < SB.Length && SB[letters] is >= 'A' and <= 'z')
            {
                letters++;
            }

            var digits = letters;

            while (digits < SB.Length && SB[digits] is >= '0' and <= '9')
            {
                digits++;
            }

            if (letters > 0 && digits > letters && digits == SB.Length)
            {
                SB.Insert(letters, ' ');
            }

            return SB.ToString();
        }

        public static string Where(this string text, Func<char, bool> Test)
        {
            var builder = new StringBuilder(text.Length);

            foreach (var c in text)
            {
                if (Test(c))
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        public static string FormatWords(this string str, bool forceUpper = false)
        {
            var chars = new char[str.Length * 2];
            var length = 0;

            for (var i = 0; i < str.Length; i++)
            {
                if (i > 0 && str[i - 1] is >= 'a' and <= 'z' && str[i] is >= 'A' and <= 'Z')
                {
                    chars[length++] = ' ';
                }

                chars[length++] = str[i];
            }

            for (var i = 0; i < length; i++)
            {
                if (chars[i] is >= 'a' and <= 'z'
                    && (i is 0 || (!IsWordChar(chars[i - 1]) && chars[i - 1] is not '\'')))
                {
                    chars[i] = char.ToUpper(chars[i]);
                }
            }

            str = new string(chars, 0, length);

            if (forceUpper)
            {
                str = Regex.Replace(str, @"(^[a-z])|(\ [a-z])", x => x.Value.ToUpper(), RegexOptions.IgnoreCase);
            }

            return str;
        }

        private static bool IsWordChar(char c)
        {
            // The characters a regex \b treats as part of a word.
            return char.IsLetterOrDigit(c)
                || c is '\u200D' or '\u200C'
                || char.GetUnicodeCategory(c)
                    is UnicodeCategory.NonSpacingMark or UnicodeCategory.ConnectorPunctuation;
        }
    }
}
