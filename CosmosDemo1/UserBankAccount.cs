using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CosmosDemo1
{
    public class UserBankAccount
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("accountNumber")]
        public string accountNumber;

        [JsonProperty("balance")]
        public string Balance;

        [JsonConverter(typeof(IsoDateTimeConverter))]
        [JsonProperty("updateTime")]
        public DateTime UpdateTime;
    }
}
