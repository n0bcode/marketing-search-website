using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Constants
{
    /// <summary>
    /// Lớp chứa các hằng số cho các loại dịch vụ trong hệ thống.
    /// </summary>
    public static class TypeServices
    {
        public const string GoogleSearch = "GoogleSearch";
        public const string TwitterSearch = "TwitterSearch";
        public const string GeminiAI = "GeminiAI";
        public const string GoogleSender = "GoogleSender";
        public const string TwilioSender = "TwilioSender";
        public static List<string> ActiveServices = new List<string>
        {
            GoogleSearch,
            GeminiAI
        };
    }
}