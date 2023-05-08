using Newtonsoft.Json;

namespace AWS.Communication.Models
{
    public class TareWeight
    {
        [JsonProperty("TOKENNO_GPNO")]
        public string TokenNoGpNo { get; set; }
        [JsonProperty("COMPPLANT")]
        public string CompPlant { get; set; }
        [JsonProperty("SRNO")]
        public string SrNo { get; set; }
        [JsonProperty("IN_OUT")]
        public string InOut { get; set; }
        [JsonProperty("VBELN_EBELN")]
        public string VbelnEbeln { get; set; }
        [JsonProperty("MATNR")]
        public string MatNr { get; set; }
        [JsonProperty("POSNR")]
        public string PosNr { get; set; }
        [JsonProperty("TAREWT")]
        public string TareWt { get; set; }
        [JsonProperty("TAREUOM")]
        public string TareUom { get; set; }
        [JsonProperty("TAREDT")]
        public string TareDt { get; set; }
        [JsonProperty("TARETIME")]
        public string TareTime { get; set; }
        [JsonProperty("UNLODEEND")]
        public string UnlodeEnd { get; set; }
    }
}