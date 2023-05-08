using Newtonsoft.Json;

namespace AWS.Communication.Models
{
    public class GrossWeight
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
        [JsonProperty("GROSSWT")]
        public string GrossWt { get; set; }
        [JsonProperty("GROSSUOM")]
        public string GrossUom { get; set; }
        [JsonProperty("GROSSDT")]
        public string GrossDt { get; set; }
        [JsonProperty("GROSSTIME")]
        public string GrossTime { get; set; }
        [JsonProperty("LODEEND")]
        public string LodeEnd { get; set; }
    }
}