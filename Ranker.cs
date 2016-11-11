using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectPart1
{
    public class Ranker
    {
        Dictionary<string, double> cosSimDict;
        Dictionary<string, double> titleDict;
        Dictionary<string, double> IndicesRankingDictionary;
        Dictionary<string, double> locationDictionary;
        Dictionary<string, double> rareWords;
        Dictionary<string, Term> sortedTermDictionary;
        public double Normalization;
        public Ranker(Dictionary<string, Term> SortedTermDictionary)
        {
            cosSimDict = new Dictionary<string, double>();
            titleDict = new Dictionary<string, double>();
            IndicesRankingDictionary = new Dictionary<string, double>();
            locationDictionary = new Dictionary<string, double>();
            sortedTermDictionary = SortedTermDictionary;
            Normalization = 0;
        }
        public List<ResultLine> rankAlldoc(Dictionary<string, DocumentPosting> DocumentPostRelated, List<KeyValuePair<Term, double>> TermsList, string queryNum,bool stem)//returns sorted list order by rank
        {

            rareWords = RareWord(TermsList);//list of the string in the query and their rare grade: 1 most unrare
            SortedList<double, string> bestDocs = new SortedList<double, string>();
            foreach (KeyValuePair<string, DocumentPosting> doc in DocumentPostRelated)
            {
                double rank = cosSim(TermsList, doc);
                cosSimDict.Add(doc.Key, rank);
                Title(doc, rareWords);
                rankByIndices(doc, TermsList,stem);
                rankByLocation(doc, TermsList);
            }
            List<KeyValuePair<double, string>> ans = new List<KeyValuePair<double, string>>();
            Normalize(cosSimDict, 1.2, 1.2);
            Normalize(titleDict, 1, 1);
            if (!stem)
            Normalize(IndicesRankingDictionary, 0.9,0.9);
            else
            Normalize(IndicesRankingDictionary, 0.8,0.8);

            Normalize(locationDictionary, 3, 3);

            ans = fiftyBestDocs();
            
            return createResultsLineList(ans,queryNum);
        }

        private List<ResultLine> createResultsLineList(List<KeyValuePair<double, string>> docsList,string queryNum)
        {
            List<ResultLine> ans = new List<ResultLine>(); 
            foreach (KeyValuePair<double,string> doc in docsList)
            {
                ResultLine line = new ResultLine(doc.Key, doc.Value);
                line.queryNum = queryNum;
                ans.Add(line);
            }
            return ans;
        }
        private List<KeyValuePair<double, string>> fiftyBestDocs()
        {
            double indexRank = 0;
            double titleRank = 0;
            double locationRank = 0;
            SortedList<double, string> lastDocs = new SortedList<double, string>(new DuplicateKeyComparer<double>());
            foreach (KeyValuePair<string, double> toAdd in cosSimDict)
            {
                if (IndicesRankingDictionary.ContainsKey(toAdd.Key))
                {
                    indexRank = IndicesRankingDictionary[toAdd.Key];
                }
                if (titleDict.ContainsKey(toAdd.Key))
                {
                    titleRank = titleDict[toAdd.Key];
                }
                if (locationDictionary.ContainsKey(toAdd.Key))
                {
                    locationRank = locationDictionary[toAdd.Key];
                }
                lastDocs.Add(toAdd.Value + titleRank + indexRank + locationRank, toAdd.Key);//add more to the double
                indexRank = 0;
                titleRank = 0;
            }
            var top50list = lastDocs.Take(50).ToList();
            return top50list;
        }
        private Dictionary<string, double> RareWord(List<KeyValuePair<Term, double>> sortedQuery)
        {
            double min_df = 10000000;
            Dictionary<string, double> rareWords = new Dictionary<string, double>();
            SortedList<double, string> IdfGrades = new SortedList<double, string>(new DuplicateKeyComparer<double>());
            foreach (KeyValuePair<Term, double> terms in sortedQuery)
            {
                /*  if (terms.Key.df < min_df)
                      min_df = terms.Key.df;*/
                double Idf = Math.Log(130471.0 / (double)terms.Key.df, 2);
                IdfGrades.Add(Idf, terms.Key.termString);
            }
            double interval = (double)(1 / IdfGrades.Count);
            int o = 0;
            int r = 0;
            double interval2 = ((double)1 / IdfGrades.Count);

            KeyValuePair<string, double> toAdd;
            foreach (KeyValuePair<double, string> terms in IdfGrades)
            {
                o++;
                // double normal = min_df / (double)terms.Key.df;
                if (o == 1)
                {
                    toAdd = new KeyValuePair<string, double>(terms.Value, 1);

                }
                else
                {
                    toAdd = new KeyValuePair<string, double>(terms.Value, 1 - interval2 * r);

                }
                r++;
                rareWords.Add(toAdd.Key,toAdd.Value);
            }
            return rareWords;

        }
        private void Normalize(Dictionary<string, double> toNormal, double max_Normal, double weight)
        {
            double max_garde = 0;
            double min_grade = 10000000;
            foreach (KeyValuePair<string, double> normal in toNormal)
            {
                if (normal.Value > max_garde)
                {
                    max_garde = normal.Value;
                }
                if (normal.Value < min_grade)
                {
                    min_grade = normal.Value;
                }
            }
            //double Gap = max_garde / max_Normal;
            foreach (KeyValuePair<string, double> normal1 in toNormal.ToList())
            {
                if (normal1.Value == max_garde)
               {
                   toNormal[normal1.Key] = max_Normal;
               }
               else
                {
                    toNormal[normal1.Key] = ((normal1.Value - min_grade) / (max_garde - min_grade)) * weight;
                }
            }

        }
        private double cosSim(List<KeyValuePair<Term, double>> Wiq, KeyValuePair<string, DocumentPosting> doc)//returns doc with his cosSim to the query
        {
            double cosSim = 0;
            double counter = 0;
            double firstSigma = 0;
            double secondSigma = 0;
            double demo = Convert.ToDouble(doc.Value.numOfWords);
            foreach (KeyValuePair<Term, double> partialQuery in Wiq)
            {
                if (partialQuery.Key.DocumentToIndicesDictionary.ContainsKey(doc.Key))
                {
                    double Tfij = (double)partialQuery.Key.DocumentToIndicesDictionary[doc.Key].Count / demo;
                    double Idf = Math.Log(130471.0 / (double)partialQuery.Key.df, 2);
                    double Wij = Tfij * Idf;
                    counter += (Wij * partialQuery.Value);
                }
                firstSigma += (partialQuery.Value * partialQuery.Value);
            }
            for (int j = 0; j < doc.Value.termsList.Count; j++)
            {
                // Term term = doc.Value[j];
                double Tfij = (double)doc.Value.termsList[j].Value / demo;
                double Idf = Math.Log(130471.0 / (double)sortedTermDictionary[doc.Value.termsList[j].Key].df, 2);
                double Wij = Tfij * Idf;
                secondSigma += (Wij * Wij);
            }
            cosSim = counter / Math.Sqrt(firstSigma * secondSigma);

            return cosSim;

        }
        private void Title(KeyValuePair<string, DocumentPosting> doc, Dictionary<string, double> rareWords)
        {
            double title = 0;
            string DocTitle = doc.Value.title.ToLower();
            foreach (KeyValuePair<string, double> partialQuery in rareWords)
            {
                if (DocTitle.Contains(partialQuery.Key))
                {
                    title += partialQuery.Value;
                }
            }
            if (title > 0)
            {
                titleDict.Add(doc.Key, title);
            }
        }
        private IEnumerable<T> Take<T>(IEnumerable<T> source, int limit)
        {
            foreach (T item in source)
            {
                if (limit-- <= 0)
                    yield break;

                yield return item;
            }
        }

        private void rankByLocation(KeyValuePair<string, DocumentPosting> doc, List<KeyValuePair<Term, double>> sortedQuery)
        {
            double rank = 0;
            int t;
            foreach (KeyValuePair<Term, double> term in sortedQuery)
            {
                if (term.Key.DocumentToIndicesDictionary.ContainsKey(doc.Key))
                {
                    int min_Index = Int32.Parse(term.Key.DocumentToIndicesDictionary[doc.Key][0]);

                    string firstIndexValue = String.Format("{0:0.0}", 1 - ((double)min_Index / (double)doc.Value.numOfChars));
                    double value = (Convert.ToDouble(firstIndexValue));
                    if (value == 0)
                        continue;

                    double partOfAns = (value  * term.Value );//change length to length cy char,normalize that the min value will get the best score

                    rank += partOfAns;
                }
            }
            if (rank > 0)
                locationDictionary.Add(doc.Key, rank);
        }
        private void rankByIndices(KeyValuePair<string, DocumentPosting> doc, List<KeyValuePair<Term, double>> sortedQuery,bool stem)
        {

            double rank = 0;
            List<Term> CommonDocTerms = new List<Term>();
            foreach (KeyValuePair<Term, double> term in sortedQuery)
            {
                if (term.Key.DocumentToIndicesDictionary.ContainsKey(doc.Key))
                    CommonDocTerms.Add(term.Key);
            }

            for (int termNumber = 0; termNumber < CommonDocTerms.Count; termNumber++)
            {

                List<string> indicesI = CommonDocTerms[termNumber].DocumentToIndicesDictionary[doc.Key];

                for (int termCompared = termNumber; termCompared < CommonDocTerms.Count; termCompared++)
                {
                    if (termCompared == termNumber)
                        continue;
                    List<string> indicesJ = CommonDocTerms[termCompared].DocumentToIndicesDictionary[doc.Key];
                    foreach (string indexI in indicesI)
                    {
                        foreach (string indexJ in indicesJ)
                        {
                            if (indexI == indexJ)
                                continue;
                            double distance = Math.Abs(Convert.ToInt32(indexJ) - Convert.ToInt32(indexI));
                            if (distance <= 90)
                            {
                                distance = distance / 10;
                                if (!stem)
                                    distance = distance * distance;
                                rank = rank + ((1 / distance));
                            }

                        }
                    }
                }



            }
            if (rank > 0)
                IndicesRankingDictionary.Add(doc.Key, rank);
        }
    }
}