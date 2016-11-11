using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace ProjectPart1
{
    public class Indexer
    {
        //            sortedTermDictionary = new SortedDictionary<string, SortedList< List<string>,string>>>();
        Parse parser = new Parse();
        public SortedDictionary<string, Dictionary<string, List<string>>> sortedTermDictionary;
        public Dictionary<string, string> docDictionary;
        bool reset = false;
        public Stopwatch stopwatch1 = new Stopwatch();
        public Stopwatch stopwatch2 = new Stopwatch();
        public TimeSpan appendtime;
        public TimeSpan writetime;
        TimeSpan filltermTime = new TimeSpan();
        TimeSpan CreateDocDictionary= new TimeSpan();
        TimeSpan CreateTermDictionary = new TimeSpan();



        public List<string> terms = new List<string>();
        KeyValuePair<string, Dictionary<string, List<string>>>[] maxTenTerms = new KeyValuePair<string, Dictionary<string, List<string>>>[10];
        KeyValuePair<string, Dictionary<string, List<string>>> maxTerm;


        public void CreateTermsPostings(string path,bool stem)//creates posting file for the terms
        {
            string stemming = "";
            if (stem)
                stemming = "Stemming";
            stopwatch2.Start();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int n = 0;
            int fileNum = 1;
            foreach (KeyValuePair<string, Dictionary<string, List<string>>> term in sortedTermDictionary)
            {
                n++;

                terms.Add(term.Key);
                TimeSpan time = stopwatch2.Elapsed;
                string addToText = fillTerm(term.Value);
                filltermTime += (stopwatch2.Elapsed - time);

                string number = term.Value.Count.ToString();

                sb.Append(term.Key + ":").Append(number + ';').Append(addToText);
                /*if (n % (sortedTermDictionary.Count / 3) == 0 && fileNum != 3)
                {
                    File.WriteAllText(@path + @"\TermPostings" + fileNum + ".txt", sb.ToString());

                    fileNum++;
                    sb = new System.Text.StringBuilder();
                }
                */


            }
            File.WriteAllText(@path + @"\TermPostings"+stemming+".txt", sb.ToString());

            stopwatch2.Stop();
        }


        //runs on all the doc in which the term is found and concatanetes the indices into a string
        private string fillTerm(Dictionary<string, List<string>> dictionary)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (KeyValuePair<string, List<string>> term in dictionary)
            {
                if (reset == true)
                    return "";

                int n = 0;
                //sb.Append('\t');
                sb.Append(term.Key + ":");
                int docOccurences = term.Value.Count;
                List<string> Indices = term.Value;
                foreach (string index in Indices)
                {

                    if (n == docOccurences - 1)
                        sb.Append(index + ".");
                    else
                        sb.Append(index + ",");
                    n++;
                }

            }
            sb.Append('#');
            return (sb.ToString());
        }

        private string dictToString(Dictionary<string,List<string>> dict)
        {
            TimeSpan t =  stopwatch1.Elapsed;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[S]");
            foreach(KeyValuePair<string,List<string>> termToIndices in dict )
            {
                sb.Append(termToIndices.Key + "@" + termToIndices.Value.Count + ";");

            }
            sb.Append("[E]");

            return sb.ToString();
        }
        //creates the sorted term dictionary and the documents dictionary
        public void createTermAndDictionaries(List<Document> documents)
        {

            stopwatch1.Start();
            sortedTermDictionary = new SortedDictionary<string, Dictionary<string, List<string>>>();
            docDictionary = new Dictionary<string, string>();
            foreach (Document doc in documents)
            {
                TimeSpan time1 = stopwatch1.Elapsed;
                string docValue = "[A]" + doc.numberOfWords + "[/A]"+"[B]"+doc.numOfChars+"[/B]" + "[C]" + doc.title + "[/C]" + "[D]" + Regex.Replace(doc.date, "[0-9]", "") + "[/D]";
                docDictionary.Add("[ID]" + doc.ID + "[/ID]", docValue + dictToString(doc.Dict));
                CreateDocDictionary += stopwatch1.Elapsed - time1;
                time1 = stopwatch1.Elapsed;
                   foreach (KeyValuePair<string, List<String>> temp in doc.Dict)
                   {
                       if (reset)
                           return;
                       if (!sortedTermDictionary.ContainsKey(temp.Key))
                       {
                           Dictionary<string, List<string>> newDict = new Dictionary<string, List<string>>();
                           List<string> indices = new List<string>();
                           indices.AddRange(temp.Value);
                           newDict.Add(doc.ID, indices);
                           sortedTermDictionary.Add(temp.Key, newDict);
                       }
                       else
                       {
                           List<string> indices = new List<string>();
                           //  indices.Add(temp.Value.Count.ToString() + " Times:");
                           indices.AddRange(temp.Value);
                           sortedTermDictionary[temp.Key].Add(doc.ID, indices);

                       }
                   }
                 
                doc.Dict.Clear();
                CreateTermDictionary += stopwatch1.Elapsed - time1;
            }
            stopwatch1.Stop();

        }
        internal void resetIndexer()
        {
            reset = true;
        }

        public string Date(string DATE)//check all the cases of date like it was written in the work 
        {
            string monthString = @"(January|Febuary|March|April|June|July|August|September|October|November|December|Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)";
            string dateRegExpression = @"((\d{1,2}((-)*\d{0,2}){0,1}(th |st |nd |rd | )(of |)" + monthString + @"( \d{4}| \d{2}|)\d{0,2})|\d{4}|" + monthString + @"( \d{4}| \d{2}(,( )*\d{4}|)))";
            Regex Date = new Regex(dateRegExpression);
            MatchCollection matches = Date.Matches(DATE);
            string date = "";
            string date2 = "";
            for (int y = 0; y < matches.Count; y++)
            {

                int i = 0;
                int j = 0;

                if (PreliminaryCheck(matches[y].Value[i]))
                {
                    if (matches[y].Value.Length <= 4)
                    {
                        if (((matches[y].Value[0] == '1' && matches[y].Value[1] == '9') || (matches[y].Value[0] == '2' && matches[y].Value[1] == '0')))
                            date = matches[y].Value;
                        else
                            continue;
                        if (matches[y].Value.Length == 2)
                        {

                            date = "19" + matches[y].Value;
                        }
                        i = matches[y].Value.Length;
                    }
                    if (i < matches[y].Value.Length)
                    {
                        while (i < matches[y].Value.Length && int.TryParse(matches[y].Value[i].ToString(), out j))
                        {
                            date += matches[y].Value[i];
                            i++;
                        }
                        if (matches[y].Value[i] == '-')
                        {
                            i++;
                            while (i < matches[y].Value.Length && (int.TryParse(matches[y].Value[i].ToString(), out j)))
                            {
                                date2 += matches[y].Value[i];
                                i++;
                            }
                        }
                        if (date.Length < 2)
                            date = "0" + date;
                        while (matches[y].Value[i] != ' ')
                            i++;
                        while (matches[y].Value[i] == ' ')
                            i++;
                        date += ' ';
                        if (date2.Length != 0)
                            date2 += ' ';

                        while (i < matches[y].Value.Length && matches[y].Value[i] != ' ')
                        {
                            date += matches[y].Value[i];
                            if (date2.Length != 0)
                                date2 += matches[y].Value[i];
                            i++;
                        }
                        if (i < matches[y].Value.Length)
                        {
                            while (matches[y].Value[i] == ' ')
                                i++;
                            string year = "";
                            while (i < matches[y].Value.Length)
                            {
                                year += matches[y].Value[i];
                                i++;
                            }
                            if (year.Length < 4)
                                year = "19" + year;
                            date += ' ' + year;
                        }
                    }
                }
                else if (i < matches[y].Value.Length && !(PreliminaryCheck(matches[y].Value[i])))
                {
                    string month = "";
                    while (matches[y].Value[i] != ' ')
                    {
                        month += matches[y].Value[i];
                        i++;
                    }
                    while (matches[y].Value[i] == ' ')
                        i++;

                    while (i < matches[y].Value.Length && (int.TryParse(matches[y].Value[i].ToString(), out j)))
                    {
                        date += matches[y].Value[i];
                        i++;
                    }
                    if (date.Length < 4)
                    {
                        date += ' ' + month;
                        if (i < matches[y].Value.Length)
                        {
                            while (i < matches[y].Value.Length && !(int.TryParse(matches[y].Value[i].ToString(), out j)))
                                i++;
                            date += ' ';
                            while (i < matches[y].Value.Length && (int.TryParse(matches[y].Value[i].ToString(), out j)))
                            {
                                date += matches[y].Value[i];
                                i++;
                            }
                        }
                    }
                    else
                    {
                        date = month + ' ' + date;
                    }

                }





            }
            date = Regex.Replace(date, "[0-9]", "");
            return date;

        }
        private static bool PreliminaryCheck(char x)// check if char is a number
        {

            if ((x < '0' || x > '9') &&
            ((x != '-' && x != '+')))
                return false;

            return true;
        }
    }

}