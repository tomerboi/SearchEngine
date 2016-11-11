using System.Collections.Generic;

namespace ProjectPart1
{
    public class RankerDicts
    {
        public Dictionary<string, double> cosSimDict;
        public Dictionary<string, double> titleDict;
        public Dictionary<string, double> IndicesRankingDictionary;
        public Dictionary<string, double> locationDictionary;
        public string queryNum;

        public RankerDicts(Dictionary<string, double> cosSimDict, Dictionary<string, double> titleDict, Dictionary<string, double> IndicesRankingDictionary, Dictionary<string, double> locationDictionary,string queryNum)
        {
            this.cosSimDict = cosSimDict;
            this.titleDict = titleDict;
            this.IndicesRankingDictionary = IndicesRankingDictionary;
            this.locationDictionary = locationDictionary;
            this.queryNum = queryNum;
        }


        public List<KeyValuePair<double, string>> top50 { get; set; }
    }
}
