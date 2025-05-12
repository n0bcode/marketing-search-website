using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Api.Utils
{
    public static class RegexHelper
    {
        private const string RegexNumber = @"[0-9]";
        private const string RegexStringVerEng = @"[a-zA-Z]";
        private const string RegexStringVerVN = @"[a-zA-Z]";
        public static string NormalizeKeyword(string keyword)
        {
            // Remove any special characters and normalize spaces
            string normalized = Regex.Replace(keyword, @"[^\w\s]", string.Empty);
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

            // Convert to lowercase for uniformity
            return normalized.ToLower();
        }

        public static bool IsKeywordVariant(string original, string variant)
        {
            // Normalize both the original and the variant
            string normalizedOriginal = NormalizeKeyword(original);
            string normalizedVariant = NormalizeKeyword(variant);

            // Check if the normalized variant contains the normalized original
            return normalizedVariant.Contains(normalizedOriginal);
        }
        public static bool AreStringsSimilar(string str1, string str2)
        {
            // Normalize both strings by removing special characters and converting to lowercase
            string normalizedStr1 = NormalizeString(str1);
            string normalizedStr2 = NormalizeString(str2);

            // Split the normalized strings into words
            var wordsStr1 = normalizedStr1.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var wordsStr2 = normalizedStr2.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Check if any word in str1 exists in str2
            foreach (var word in wordsStr1)
            {
                if (wordsStr2.Contains(word))
                {
                    return true; // Found a similar word
                }
            }

            // If no similar words found, return false
            return false;
        }
        public static string? FindClosestMatch(string target, List<string> candidates)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return null; // Không có ứng viên nào
            }

            string? closestMatch = null;
            double closestSimilarity = 0.0; // Tỷ lệ tương đồng

            foreach (var candidate in candidates)
            {
                int distance = CalculateLevenshteinDistance(target, candidate);
                int maxLength = Math.Max(target.Length, candidate.Length);

                // Tính tỷ lệ tương đồng
                double similarity = 1.0 - (double)distance / maxLength;

                if (similarity > 0.5) // Kiểm tra nếu tỷ lệ tương đồng lớn hơn 50%
                {
                    if (similarity > closestSimilarity)
                    {
                        closestSimilarity = similarity;
                        closestMatch = candidate;
                    }
                }
            }

            return closestMatch;
        }

        #region [PRIVATE METHOD SUPPORT] 
        private static string NormalizeString(string input)
        {
            // Remove special characters and normalize spaces
            string normalized = Regex.Replace(input, @"[^\w\s]", string.Empty); // Remove punctuation
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim(); // Normalize spaces

            // Convert to lowercase for uniformity
            return normalized.ToLower();
        }

        private static int CalculateLevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            var d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; i++)
            {
                d[i, 0] = i; // Chi phí để chuyển đổi từ s đến chuỗi rỗng
            }

            for (int j = 0; j <= m; j++)
            {
                d[0, j] = j; // Chi phí để chuyển đổi từ chuỗi rỗng đến t
            }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1; // Nếu ký tự khác nhau thì chi phí là 1
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost); // Tính toán chi phí
                }
            }

            return d[n, m];
        }
        #endregion
    }

}