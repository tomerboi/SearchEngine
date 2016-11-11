using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectPart1
{
    public class Document
    {
        public string ID;
        public string title;
        public string text;
        public string date;
        public double TotalTime;
        public Dictionary<string, List<string>> Dict;
        private string tf;
        private string title1;
        private string date1;
        public int numberOfWords=0;

        public Document()
        {
            Dict = new Dictionary<string, List<string>>();
        }

        public Document(string tf, string title, string date)
        {
            // TODO: Complete member initialization
            this.tf = tf;
            this.title1 = title;
            this.date = date;
        }
        public int max_tf { get; set; }

        public int numOfChars { get; set; }
    }

}
