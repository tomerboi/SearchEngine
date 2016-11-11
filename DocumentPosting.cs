using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectPart1
{
   public class DocumentPosting
    {
       public int numOfWords;
       public int numOfChars;
        public string title;
        public string date;
        public List<KeyValuePair<string, int>> termsList;

        public DocumentPosting(int numOfWords, int numOfChars, string title, string date, List<KeyValuePair<string, int>> termsList)
        {
            this.numOfWords = numOfWords;
            this.numOfChars = numOfChars;
            this.title = title;
            this.date = date;
            this.termsList = termsList;
        }
    }
}
