using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Api.Utils
{
    public class JsonDbConverter : ValueConverter<object?, string>
    {
        public JsonDbConverter() : base(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<object>(v))
        {
        }
    }
}