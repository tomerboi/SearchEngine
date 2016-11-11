using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ProjectPart1
{
    public class Parse
    {
        public SortedDictionary<string, List<Document>> sortedDictionary = new SortedDictionary<string, List<Document>>();
        private string Term = "";
        public string word2;
        public HashSet<string> stopWordsHashSet;
        public bool withStemming;
        Document parsedDoc;
        string[] docParagraphs;
        int currentParagraph = 0;
        bool upperCaseParagraph = false;
        SortedDictionary<string, List<Document>> lastDict = new SortedDictionary<string, List<Document>>();
        Stemmer stemmer = new Stemmer();
        List<string> shitParagraphs;
        int currentIndex = 0;
        double check;
        Dictionary<string, int> queryAns;
        string query;
        public Parse()
        {
            stopWordsHashSet = new HashSet<string>();
        }
        public Dictionary<string, int> Parse1(string query1,bool stem)
        {
            withStemming = stem;
            query = query1;
            queryAns = new Dictionary<string, int>();

            /////////////////////DATE
            Date1();



            ////////////////FROM TO
            FromTo1();




            ///////////////////PERCENT
            Percent1();


            //////////////////PRICE
            Price1();
            Organizations1();
            Fraction1();
            ///////////////MULTIPLE NUM
            multipleNum1();
            allTheRest1();




            return queryAns;
        }

        private void multipleNum1()
        {
            Regex multipleNum = new Regex("[0-9]+[,.]*[0-9]*[.,]*[0-9]* (hundreds|millions|billion|trillion|thousends)");//checks for 76.9 miilion/billiob/trillion/hundreds/thousends
            MatchCollection matches = multipleNum.Matches(query);
            for (int y = 0; y < matches.Count; y++)
            {

                int i = 0;
                int j = 0;
                string mult = "";//million/hundreds/trillion...
                string number = "";
                while (int.TryParse(matches[y].Value[i].ToString(), out j))
                {
                    number += matches[y].Value[i];
                    i++;
                }
                if (matches[y].Value[i] == '.')
                {
                    number += matches[y].Value[i];
                    i++;
                    while (int.TryParse(matches[y].Value[i].ToString(), out j))
                    {
                        number += matches[y].Value[i];
                        i++;
                    }
                }
                while (matches[y].Value[i] == ' ')
                    i++;

                while (i < matches[y].Value.Length)
                {
                    mult += matches[y].Value[i];
                    i++;
                }
                double num = Convert.ToDouble(number);
                if (mult == "hundreds")
                    num = num * 100;
                if (mult == "thousends")
                    num = num * 1000;
                if (mult == "millions")
                    num = num * 1000000;
                if (mult == "billion")
                    num = num * 1000000000;
                if (mult == "trillion")
                    num = num * 1000000000000;
                string toMult = num.ToString();
                addToDict(toMult, queryAns);

                Replace1(matches[y]);
            }
        }

        private void allTheRest1()
        {
            int i = 0;
            int j = 0;
            int numOfSpaces = 0;
            string word = "";
            string anotherWord = "";
            string anotherWord2 = "";

            while (i < query.Length)
            {
                while (i < query.Length && query[i] != ' ' && word != ".")
                {
                    word += query[i];
                    i++;
                }
                if (i < query.Length && query[i] == ' ' && word.Length == 0)
                {
                    i++;
                    continue;
                }

                if (Char.IsPunctuation(word[0]) || word[0] == '-' || word[0] == ' ')
                {
                    word = word.Substring(1, word.Length - 1);
                }
                if (word.Length == 0 || word == ".")
                    continue;
                if (word[word.Length - 1] == '.' && word[word.Length - 2] == '.')
                {

                    word = word.Substring(0, word.IndexOf('.'));
                    if (word.Length == 0)
                        break;

                }
                if (word.Length == 0)
                    continue;
                if (word.Length != 0 && PreliminaryCheck(word[0]))//check if number
                {
                    if (Char.IsPunctuation(word[word.Length - 1]) || word[word.Length - 1] == '-')
                        word = word.Substring(0, word.Length - 1);
                    if (word.Contains(","))
                        word = word.Replace(",", string.Empty);
                    if (Double.TryParse(word, out check))
                    {
                        if (word[word.Length - 1] == '.')
                            word = word.Substring(0, word.Length - 1);
                        if (word.Contains("."))
                        {
                            if (word[word.IndexOf(".") + 1] == '0')
                            {
                                word2 = word.Substring(word.IndexOf(".") + 1);
                                if (Convert.ToInt32(word2) == 0)
                                {
                                    word = word.Substring(0, word.IndexOf("."));
                                }
                                word2 = "";
                            }

                        }


                    }

                    addToDict(word, queryAns);
                    word = "";
                    continue;
                }
                if (Char.IsPunctuation(word[word.Length - 1]) || word[word.Length - 1] == '-')
                {
                    word = word.Substring(0, word.Length - 1);
                    if (stopWordsHashSet.Contains(word))
                    {
                        word = "";
                        continue;
                    }
                    else
                    {
                        addToDict(word.ToLower(), queryAns);
                        word = "";
                        continue;
                    }
                }
                if (Char.IsPunctuation(word[0]) || word[0] == '-')
                {
                    word = word.Substring(1, word.Length - 1);
                    if (stopWordsHashSet.Contains(word.ToLower()))
                    {
                        word = "";
                        continue;
                    }
                    else
                    {

                        addToDict(word.ToLower(), queryAns);
                        word = "";
                        continue;
                    }
                }

                if (stopWordsHashSet.Contains(word.ToLower()))
                {
                    word = "";
                    continue;
                }
                if (Char.IsUpper(word[0]))// check if first char is uppercase
                {
                    while (i < query.Length && query[i] == ' ')
                    {
                        i++;
                        numOfSpaces++;
                    }
                    if (numOfSpaces > 2)
                    {
                        addToDict(word.ToLower(), queryAns);
                        word = "";
                        anotherWord = "";
                        numOfSpaces = 0;
                        continue;
                    }
                    //numOfSpaces = 0;
                    if (i < query.Length)
                    {
                        anotherWord += query[i];
                        if (Char.IsUpper(anotherWord[0]))
                        {
                            i++;
                            while (i < query.Length && query[i] != ' ')
                            {
                                anotherWord += query[i];
                                i++;
                            }

                            if (Char.IsPunctuation(anotherWord[anotherWord.Length - 1]))
                            {
                                anotherWord = anotherWord.Substring(0, anotherWord.Length - 1);
                            }
                            if (!stopWordsHashSet.Contains(anotherWord.ToLower()))
                            {
                                addToDict(anotherWord.ToLower(), queryAns);
                                addToDict(word.ToLower(), queryAns);
                                word += ' ' + anotherWord;
                            }


                        }
                    }
                }



                addToDict(word.ToLower(), queryAns);
                word = "";
                anotherWord = "";
                anotherWord2 = "";
                numOfSpaces = 0;




            }
        }

        private void Fraction1()
        {
            Regex fraction = new Regex("(([0-9]+[.,]*[0-9]*)( )){0,1}[0-9]+[.,]*[0-9]*/[0-9]+[.,]*[0-9]*");//checks for fraction 35 3/5
            MatchCollection matches = fraction.Matches(query);
            for (int y = 0; y < matches.Count; y++)
            {

                int i = 0;
                int j = 0;
                string frac = "";
                string s = "";
                string noSpace = matches[y].Value;

                while (int.TryParse(noSpace[i].ToString(), out j))
                {
                    s += noSpace[i];
                    i++;
                }
                if (noSpace[i] == ' ')
                {
                    while (noSpace[i] == ' ')
                        i++;
                    string counter = "";
                    while (int.TryParse(noSpace[i].ToString(), out j) || noSpace[i] == ',')
                    {
                        counter += noSpace[i];
                        i++;
                    }
                    i++;
                    string denominator = "";
                    while (i < noSpace.Length)
                    {
                        denominator += noSpace[i];
                        i++;
                    }
                    denominator = denominator.Replace(",", string.Empty);
                    counter = counter.Replace(",", string.Empty);
                    if (Convert.ToDouble(counter) <= 10 && Convert.ToDouble(denominator) <= 10)
                    {
                        double final = Convert.ToDouble(s) + (Convert.ToDouble(counter) / Convert.ToDouble(denominator));
                        frac = final.ToString();
                    }

                }
                else if (noSpace[i] == '/')
                {
                    i++;
                    string denominator = "";
                    while (i < noSpace.Length)
                    {
                        denominator += noSpace[i];
                        i++;
                    }
                    if (Convert.ToDouble(s) <= 10 && Convert.ToDouble(denominator) <= 10)
                    {
                        double final = (Convert.ToDouble(s) / Convert.ToDouble(denominator));
                        frac = final.ToString();
                    }
                }
                if (frac.Length != 0)
                    addToDict(frac, queryAns);
                else
                    addToDict(noSpace.Replace(",", string.Empty), queryAns);
                Replace1(matches[y]);

            }
        }

        private void Organizations1()
        {
            Regex Names = new Regex(@"(((\b[A-Z][a-z]*\b)( ){1,2}){1,2}(of|and|the|for|in|Of|And|The|For|In)( ){1,2}(of|and|the|for|in|Of|And|The|For|In)*( ){0,2}){1,5}(\b[A-Z][a-z]*\b)( ){0,2}(\b[A-Z][a-z]*\b){0,1}");
            MatchCollection matches = Names.Matches(query);
            int i = matches.Count;
            for (int j = 0; j < i; j++)
            {

                string LowerCasedMatch = (string)matches[j].Value.ToLower();
                addToDict(LowerCasedMatch, queryAns);

            }
        }

        private void Price1()
        {
            Regex Price = new Regex(@"Dollars( ){0,1}[0-9]*[.,]*[0-9]*( ){0,1}[0-9]+[.,]*[0-9]*/[0-9]+[.,]*[0-9]*|Dollars [0-9]+[.,]*[0-9]*m*b*n*|\$[0-9]+[,]*[0-9]*[.,]*[0-9]*(( )+(million|billion)+)*");//checks for all the numbers with percent
            // Regex Price = new Regex("Dollars [0-9]* [0-9]+/[0-9]+");m*i*l*i*o*n*b*i*l*i*o*n*
            MatchCollection matches = Price.Matches(query);
            for (int y = 0; y < matches.Count; y++)
            {

                int i = 0;
                int j = 0;
                string number = "";

                if (matches[y].Value[i] == 'D')
                {
                    while (i < matches[y].Value.Length && matches[y].Value[i] != ' ')
                        i++;
                    while (i < matches[y].Value.Length && matches[y].Value[i] == ' ')
                        i++;
                    while (i < matches[y].Value.Length && int.TryParse(matches[y].Value[i].ToString(), out j))
                    {
                        number += matches[y].Value[i];
                        i++;
                    }
                    if (i < matches[y].Value.Length)
                    {
                        if (matches[y].Value[i] == '.' || matches[y].Value[i] == ',')//for 24,665.54
                        {
                            number += matches[y].Value[i];
                            i++;
                            while (i < matches[y].Value.Length && int.TryParse(matches[y].Value[i].ToString(), out j))
                            {
                                number += matches[y].Value[i];
                                i++;
                            }
                        }
                    }

                    if (i < matches[y].Value.Length && matches[y].Value[i] == '.')
                    {
                        number += matches[y].Value[i];
                        i++;
                        while (i < matches[y].Value.Length && int.TryParse(matches[y].Value[i].ToString(), out j))
                        {
                            number += matches[y].Value[i];
                            i++;
                        }
                    }
                    if (matches[y].Value[matches[y].Value.Length - 1] == '.')
                    {
                        while (i < matches[y].Value.Length)
                        {
                            number += matches[y].Value[i];
                            i++;
                        }
                        number += '0';
                    }
                    if (i < matches[y].Value.Length && matches[y].Value[i] == ' ')
                    {
                        while (i < matches[y].Value.Length && matches[y].Value[i] == ' ')
                            i++;
                        string counter = "";
                        while (i < matches[y].Value.Length && int.TryParse(matches[y].Value[i].ToString(), out j))
                        {
                            counter += matches[y].Value[i];
                            i++;
                        }
                        i++;
                        string denominator = "";
                        while (i < matches[y].Value.Length)
                        {
                            denominator += matches[y].Value[i];
                            i++;
                        }
                        double final = Convert.ToDouble(number) + (Convert.ToDouble(counter) / Convert.ToDouble(denominator));
                        number = '$' + final.ToString();

                    }
                    else if (i < matches[y].Value.Length && matches[y].Value[i] == '/')
                    {
                        i++;
                        string denominator = "";
                        while (i < matches[y].Value.Length)
                        {
                            denominator += matches[y].Value[i];
                            i++;
                        }
                        double final = Convert.ToDouble(number) / Convert.ToDouble(denominator);
                        number = '$' + final.ToString();
                    }
                    if (i < matches[y].Value.Length)
                    {
                        if (matches[y].Value[i] == 'm')
                        {
                            double final = Convert.ToDouble(number) * 1000000;
                            number = '$' + final.ToString();
                        }
                        if (matches[y].Value[i] == 'b' && matches[y].Value[i + 1] == 'n')
                        {
                            double final = Convert.ToDouble(number) * 1000000000;
                            number = '$' + final.ToString();
                        }
                    }

                    if (number[0] != '$')
                    {
                        number = '$' + number;
                    }
                }
                if (matches[y].Value[0] == '$')
                {
                    i++;
                    if (PreliminaryCheck(matches[y].Value[matches[y].Value.Length - 1]))
                    {
                        while (i < matches[y].Value.Length)
                        {
                            number += matches[y].Value[i];
                            i++;
                        }
                    }
                    if (matches[y].Value[matches[y].Value.Length - 1] == '.' || matches[y].Value[matches[y].Value.Length - 1] == ',')
                    {
                        while (i < matches[y].Value.Length)
                        {
                            number += matches[y].Value[i];
                            i++;
                        }
                        number = number.Substring(0, number.Length - 1);
                    }
                    while (i < matches[y].Value.Length && matches[y].Value[i] != ' ')
                    {
                        number += matches[y].Value[i];
                        i++;
                    }
                    if (i < matches[y].Value.Length)
                    {
                        while (i < matches[y].Value.Length && matches[y].Value[i] == ' ')
                            i++;
                        string toDouble = "";
                        while (i < matches[y].Value.Length)
                        {
                            toDouble += matches[y].Value[i];
                            i++;
                        }
                        if (toDouble == "millions" || toDouble == "million")
                        {
                            double final = Convert.ToDouble(number) * 1000000;
                            number = '$' + final.ToString();
                        }
                        if (toDouble == "billions" || toDouble == "billion")
                        {
                            double final = Convert.ToDouble(number) * 1000000000;
                            number = '$' + final.ToString();
                        }

                    }
                    if (number[0] != '$')
                    {
                        number = '$' + number;
                    }
                }
                number = number.Replace(",", string.Empty);
                addToDict(number, queryAns);

                Replace1(matches[y]);
            }
        }

        private void Percent1()
        {
            Regex Percent = new Regex("[0-9]+[.,]*[0-9]*( )*(-){0,1}(%|percent|percents|percentage)+");//checks for all the numbers with percent
            MatchCollection matches = Percent.Matches(query);
            for (int y = 0; y < matches.Count; y++)
            {

                int i = 0;
                int j = 0;
                string number = "";
                while (int.TryParse(matches[y].Value[i].ToString(), out j))
                {
                    number += matches[y].Value[i];
                    i++;
                }
                number += '%';
                addToDict(number, queryAns);

                Replace1(matches[y]);

            }
        }

        private void FromTo1()
        {
            Regex regex = new Regex("([0-9]+([.,]*[0-9]+){0,1}-( ){0,1}[0-9]+([.,]*[0-9]+){0,1})|Between( )*[0-9]+[.,]*[0-9]* and( )*[0-9]+[.,]*[0-9]*|between( )*[0-9]+[.,]*[0-9]* and( )*[0-9]+[.,]*[0-9]*");//for all num-num
            MatchCollection matches = regex.Matches(query);
            int u = matches.Count;
            int j = 0;
            for (j = 0; j < u; j++)
            {

                int k = 0;
                int p = 0;
                string num1 = "";
                string num2 = "";
                string between = matches[j].Value;
                int i = 0;
                if (matches[j].Value[k] == 'B' || matches[j].Value[k] == 'b')
                {
                    while (!(int.TryParse(matches[j].Value[k].ToString(), out p)))
                        k++;
                    while ((int.TryParse(matches[j].Value[k].ToString(), out p)))
                    {
                        num1 += matches[j].Value[k];
                        k++;
                    }
                    while (!(int.TryParse(matches[j].Value[k].ToString(), out p)))
                        k++;
                    while (k < matches[j].Value.Length && (int.TryParse(matches[j].Value[k].ToString(), out p)))
                    {
                        num2 += matches[j].Value[k];
                        k++;
                    }
                    num1 = num1.Replace(",", string.Empty);
                    num2 = num2.Replace(",", string.Empty);
                    between = num1 + '-' + num2;
                }

                else
                {
                    while (matches[j].Value[i] != '-')
                    {
                        num1 += matches[j].Value[i];
                        i++;
                    }
                    int l = i + 1;
                    while (l < matches[j].Value.Length)
                    {
                        num2 += matches[j].Value[l];
                        l++;
                    }
                    num1 = num1.Replace(",", string.Empty);
                    num2 = num2.Replace(",", string.Empty);
                    between = num1 + '-' + num2;
                }

                addToDict(between, queryAns);
                if (num2[0] == ' ')
                {
                    num2 = num2.Substring(1);
                }
                addToDict(num1, queryAns);
                addToDict(num2, queryAns);

                Replace1(matches[j]);

            }
        }
        public void Replace1(Match match)// replaces terms with * with the same length length
        {
            string toReplace = "";
            for (int t = 0; t < match.Length; t++)
                toReplace += ' ';
            int k = match.Length;
            query = query.Remove(match.Index, match.Value.Length).Insert(match.Index, toReplace);//maybe match.Value.Length+1
            // parsedDoc.text = parsedDoc.text(match.Index, match.Value.Length + 1).Insert(match.Index, toReplace);

        }
        private void addToDict(string value, Dictionary<string, int> ans)//Add term to query's dictionary
        {
            if (value.Length != 0 && !stopWordsHashSet.Contains(value))
            {
                if (withStemming)
                    value = stemmer.stemTerm(value.ToLower());
                Term = Term.ToLower();
                if (!ans.ContainsKey(value))
                {
                    ans.Add(value, 1);
                }
                else
                {
                    ans[value]++;
                }

            }
        }

        private void Date1()
        {
            string monthString = @"(January|Febuary|March|April|June|July|August|September|October|November|December|Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)";
            string dateRegExpression = @"((\d{1,2}((-)*\d{0,2}){0,1}(th |st |nd |rd | )(of |)" + monthString + @"( \d{4}| \d{2}|)\d{0,2})|\d{4}|" + monthString + @"( \d{4}| \d{2}(,( )*\d{4}|)))";
            Regex Date = new Regex(dateRegExpression);
            MatchCollection matches = Date.Matches(query);

            for (int y = 0; y < matches.Count; y++)
            {

                int i = 0;
                int j = 0;
                string date = "";
                string date2 = "";
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


                if (date.Length >= 1)
                {
                    addToDict(date.ToLower(), queryAns);
                    if (date2.Length != 0)
                        addToDict(date2.ToLower(), queryAns);
                    Replace1(matches[y]);
                }


            }
        }
        public void Parse1(Document doc)
        {


            shitParagraphs = new List<string>();
            parsedDoc = doc;

            string[] delimiter = { "\n\n" };

            docParagraphs = doc.text.Split(delimiter, StringSplitOptions.None);

            for (currentParagraph = 0; currentParagraph < docParagraphs.Length; currentParagraph++)
            {
                docParagraphs[currentParagraph].Replace("\n", "  ");


                // check if paragraph is full of uppercased words;
                upperCaseParagraph = checkIfShitParagraph(docParagraphs[currentParagraph]);



                /////////////////////DATE
                Date();



                ////////////////FROM TO
                FromTo();




                ///////////////////PERCENT
                Percent();


                //////////////////PRICE
                Price();



                if (!upperCaseParagraph)
                {
                    //////////////// ORGANISATIONS
                    Organizations();



                }

                Fraction();


                ///////////////MULTIPLE NUM
                multipleNum();




                // remove all punctuation except 'dash'
                docParagraphs[currentParagraph] = Regex.Replace(docParagraphs[currentParagraph], @"[^A-Za-z0-9.,]", " ");


                ////////////////add ALL THE REST of the words to the dictionary including numbers and names starts with uppercase
                allTheRest();



                // add to the index the length of the paragraph
                currentIndex = currentIndex + docParagraphs[currentParagraph].Length;// maybe +4 because we remove the split by "\n\n"
            }
            currentIndex = 0;

        }


        private static readonly char[] TrimStartArray = { '0' };
        private static bool PreliminaryCheck(char x)// check if char is a number
        {

            if ((x < '0' || x > '9') &&
            ((x != '-' && x != '+')))
                return false;

            return true;
        }

        private void allTheRest()
        {
            int i = 0;
            int j = 0;
            int numOfSpaces = 0;
            string word = "";
            string anotherWord = "";
            string anotherWord2 = "";

            while (i < docParagraphs[currentParagraph].Length)
            {
                while (i < docParagraphs[currentParagraph].Length && docParagraphs[currentParagraph][i] != ' ' && word != ".")
                {
                    if (word.Length > 1 && word[word.Length - 1] == '.' && docParagraphs[currentParagraph][i] == '.')
                    {
                        i++;
                        break;
                    }
                    word += docParagraphs[currentParagraph][i];
                    i++;
                }
                if (docParagraphs[currentParagraph][i] == ' ' && word.Length == 0)
                {
                    i++;
                    continue;
                }
                if (Char.IsPunctuation(word[0]) || word[0] == '-' || word[0] == ' ')
                {
                    word = word.Substring(1, word.Length - 1);
                }
                if (word.Length == 0 || word == ".")
                    continue;
                if (word[word.Length - 1] == '.' && word[word.Length - 2] == '.')
                {

                    word = word.Substring(0, word.IndexOf('.'));
                    if (word.Length == 0)
                        break;

                }
                if (word.Length == 0)
                    continue;
                if (word.Length != 0 && PreliminaryCheck(word[0]))//check if number
                {
                    if (Char.IsPunctuation(word[word.Length - 1]) || word[word.Length - 1] == '-')
                        word = word.Substring(0, word.Length - 1);
                    if (word.Contains(","))
                        word = word.Replace(",", string.Empty);
                    if (Double.TryParse(word, out check))
                    {
                        if (word[word.Length - 1] == '.')
                            word = word.Substring(0, word.Length - 1);
                        if (word.Contains("."))
                        {
                            if (word[word.IndexOf(".") + 1] == '0')
                            {
                                word2 = word.Substring(word.IndexOf(".") + 1);
                                if (Convert.ToInt32(word2) == 0)
                                {
                                    word = word.Substring(0, word.IndexOf("."));
                                }
                                word2 = "";
                            }

                        }


                    }

                    addToDict(word, i);
                    word = "";
                    continue;
                }
                if (Char.IsPunctuation(word[word.Length - 1]) || word[word.Length - 1] == '-')
                {
                    word = word.Substring(0, word.Length - 1);
                    if (stopWordsHashSet.Contains(word))
                    {
                        word = "";
                        continue;
                    }
                    else
                    {
                        addToDict(word.ToLower(), i);
                        word = "";
                        continue;
                    }
                }
                if (Char.IsPunctuation(word[0]) || word[0] == '-')
                {
                    word = word.Substring(1, word.Length - 1);
                    if (stopWordsHashSet.Contains(word.ToLower()))
                    {
                        word = "";
                        continue;
                    }
                    else
                    {

                        addToDict(word.ToLower(), i);
                        word = "";
                        continue;
                    }
                }

                if (stopWordsHashSet.Contains(word.ToLower()))
                {
                    word = "";
                    continue;
                }
                if (Char.IsUpper(word[0]) && !upperCaseParagraph)// check if first char is uppercase
                {
                    while (i < docParagraphs[currentParagraph].Length && docParagraphs[currentParagraph][i] == ' ')
                    {
                        i++;
                        numOfSpaces++;
                    }
                    if (numOfSpaces > 2)
                    {
                        addToDict(word.ToLower(), i);
                        word = "";
                        anotherWord = "";
                        numOfSpaces = 0;
                        continue;
                    }
                    //numOfSpaces = 0;
                    if (i < docParagraphs[currentParagraph].Length)
                    {
                        anotherWord += docParagraphs[currentParagraph][i];
                        if (Char.IsUpper(anotherWord[0]))
                        {
                            i++;
                            while (i < docParagraphs[currentParagraph].Length && docParagraphs[currentParagraph][i] != ' ')
                            {
                                anotherWord += docParagraphs[currentParagraph][i];
                                i++;
                            }
                            string Lastunc = "";
                            int p = 1;
                            while (Char.IsPunctuation(anotherWord[anotherWord.Length - p]))
                            {
                                Lastunc += anotherWord[anotherWord.Length - p];
                                p++;
                            }
                            anotherWord = anotherWord.Substring(0, anotherWord.Length - Lastunc.Length);

                            if (!stopWordsHashSet.Contains(anotherWord.ToLower()))
                            {
                                addToDict(anotherWord.ToLower(), i);
                                addToDict(word.ToLower(), i);
                                word += ' ' + anotherWord;
                            }


                        }
                    }
                }



                addToDict(word.ToLower(), i);
                word = "";
                anotherWord = "";
                anotherWord2 = "";
                numOfSpaces = 0;




            }
        }

        private bool checkIfShitParagraph(string paragraph)//check if more than 70% of the paragraph is uppercase
        {
            bool shitParagraph = false;

            double characterCount = paragraph.Length;
            double wordCount = Regex.Matches(paragraph, @"[A-Za-z]+").Count;

            long UpperCaseWordsCount = Regex.Matches(paragraph, @"[A-Z]+[a-z]*[,.;'()]*( )").Count;
            if (wordCount == 0)
                return false;
            double upperCaseRate = UpperCaseWordsCount / wordCount;
            double wordsInParagraphRate = characterCount / wordCount;
            if (wordCount != 0 && upperCaseRate > 0.72 && wordCount > 7 && wordsInParagraphRate < 10)
            {
                return true;
            }
            shitParagraph = false;

            return shitParagraph;
        }
        public void Organizations()// check for case like Bank of America
        {
            Regex Names = new Regex(@"(((\b[A-Z][a-z]*\b)( ){1,2}){1,2}(of|and|the|for|in|Of|And|The|For|In)( ){1,2}(of|and|the|for|in|Of|And|The|For|In)*( ){0,2}){1,5}(\b[A-Z][a-z]*\b)(( )(\b[A-Z][a-z]*\b)){0,1}");
            MatchCollection matches = Names.Matches(docParagraphs[currentParagraph]);
            int i = matches.Count;
            for (int j = 0; j < i; j++)
            {

                string LowerCasedMatch = (string)matches[j].Value.ToLower();
                if (LowerCasedMatch.StartsWith("report for"))
                {
                    if (LowerCasedMatch[LowerCasedMatch.Length - 1] == ' ')
                        LowerCasedMatch = LowerCasedMatch.TrimEnd();
                }

                if (LowerCasedMatch[0] == ' ')
                    LowerCasedMatch = LowerCasedMatch.TrimStart();
                addToDict(LowerCasedMatch, matches[j].Index);

            }
        }


        private void addToDict(Match match)//Add term to document's dictionary
        {
            string term = match.Value;

            if (term.Length != 0 && !stopWordsHashSet.Contains(term))
            {
                parsedDoc.numberOfWords++;

                if (withStemming)
                    term = stemmer.stemTerm(term.ToLower());
                Term = Term.ToLower();

                if (parsedDoc.Dict.ContainsKey(term))
                {
                    parsedDoc.Dict[term].Add((currentIndex + match.Index).ToString());
                    if (parsedDoc.Dict[term].Count > parsedDoc.max_tf)
                        parsedDoc.max_tf = parsedDoc.Dict[term].Count;
                }
                else
                {
                    List<string> list = new List<string>();
                    list.Add((currentIndex + match.Index).ToString());
                    parsedDoc.Dict.Add(term, list);

                    if (parsedDoc.Dict[term].Count > parsedDoc.max_tf)
                        parsedDoc.max_tf = parsedDoc.Dict[term].Count;
                }
            }
        }

        private void addToDict(string value, int Index)//Add term to document's dictionary
        {

            if (value.Length != 0 && !stopWordsHashSet.Contains(value))
            {
                parsedDoc.numberOfWords++;

                if (withStemming)
                    value = stemmer.stemTerm(value.ToLower());
                Term = Term.ToLower();
                if (parsedDoc.Dict.ContainsKey(value))
                {

                    parsedDoc.Dict[value].Add((currentIndex + Index).ToString());
                    if (parsedDoc.Dict[value].Count > parsedDoc.max_tf)
                        parsedDoc.max_tf = parsedDoc.Dict[value].Count;
                }
                else
                {
                    List<string> list = new List<string>();
                    list.Add((currentIndex + Index).ToString());
                    parsedDoc.Dict.Add(value, list);

                    if (parsedDoc.Dict[value].Count > parsedDoc.max_tf)
                        parsedDoc.max_tf = parsedDoc.Dict[value].Count;
                }
            }
        }



        public void FromTo()//check for cases like 22-25, between 22 and 25
        {

            Regex regex = new Regex("([0-9]+([.,]*[0-9]+){0,1}-( ){0,1}[0-9]+([.,]*[0-9]+){0,1})|Between( )*[0-9]+[.,]*[0-9]* and( )*[0-9]+[.,]*[0-9]*|between( )*[0-9]+[.,]*[0-9]* and( )*[0-9]+[.,]*[0-9]*");//for all num-num
            MatchCollection matches = regex.Matches(docParagraphs[currentParagraph]);
            int u = matches.Count;
            int j = 0;
            for (j = 0; j < u; j++)
            {

                int k = 0;
                int p = 0;
                string num1 = "";
                string num2 = "";
                string between = matches[j].Value;
                int i = 0;
                if (matches[j].Value[k] == 'B' || matches[j].Value[k] == 'b')
                {
                    while (!(int.TryParse(matches[j].Value[k].ToString(), out p)))
                        k++;
                    while ((int.TryParse(matches[j].Value[k].ToString(), out p)))
                    {
                        num1 += matches[j].Value[k];
                        k++;
                    }
                    while (!(int.TryParse(matches[j].Value[k].ToString(), out p)))
                        k++;
                    while (k < matches[j].Value.Length && (int.TryParse(matches[j].Value[k].ToString(), out p)))
                    {
                        num2 += matches[j].Value[k];
                        k++;
                    }
                    num1 = num1.Replace(",", string.Empty);
                    num2 = num2.Replace(",", string.Empty);
                    between = num1 + '-' + num2;
                }

                else
                {
                    while (matches[j].Value[i] != '-')
                    {
                        num1 += matches[j].Value[i];
                        i++;
                    }
                    int l = i + 1;
                    while (l < matches[j].Value.Length)
                    {
                        num2 += matches[j].Value[l];
                        l++;
                    }
                    num1 = num1.Replace(",", string.Empty);
                    num2 = num2.Replace(",", string.Empty);
                    between = num1 + '-' + num2;
                }

                addToDict(between, matches[j].Index);
                if (num2[0] == ' ')
                {
                    num2 = num2.Substring(1);
                }
                addToDict(num1, matches[j].Index);
                addToDict(num2, matches[j].Index);

                Replace(matches[j]);

            }

        }
        public void Fraction()// check for cases like 3 3/5, 23/98
        {
            Regex fraction = new Regex("(([0-9]+[.,]*[0-9]*)( )){0,1}[0-9]+[.,]*[0-9]*/[0-9]+[.,]*[0-9]*");//checks for fraction 35 3/5
            MatchCollection matches = fraction.Matches(docParagraphs[currentParagraph]);
            for (int y = 0; y < matches.Count; y++)
            {

                int i = 0;
                int j = 0;
                string frac = "";
                string s = "";
                string noSpace = matches[y].Value;

                while (int.TryParse(noSpace[i].ToString(), out j))
                {
                    s += noSpace[i];
                    i++;
                }
                if (noSpace[i] == ' ')
                {
                    while (noSpace[i] == ' ')
                        i++;
                    string counter = "";
                    while (int.TryParse(noSpace[i].ToString(), out j) || noSpace[i] == ',')
                    {
                        counter += noSpace[i];
                        i++;
                    }
                    i++;
                    string denominator = "";
                    while (i < noSpace.Length)
                    {
                        denominator += noSpace[i];
                        i++;
                    }
                    denominator = denominator.Replace(",", string.Empty);
                    counter = counter.Replace(",", string.Empty);
                    if (Convert.ToDouble(counter) <= 10 && Convert.ToDouble(denominator) <= 10)
                    {
                        double final = Convert.ToDouble(s) + (Convert.ToDouble(counter) / Convert.ToDouble(denominator));
                        frac = final.ToString();
                    }

                }
                else if (noSpace[i] == '/')
                {
                    i++;
                    string denominator = "";
                    while (i < noSpace.Length)
                    {
                        denominator += noSpace[i];
                        i++;
                    }
                    if (Convert.ToDouble(s) <= 10 && Convert.ToDouble(denominator) <= 10)
                    {
                        double final = (Convert.ToDouble(s) / Convert.ToDouble(denominator));
                        frac = final.ToString();
                    }
                }
                if (frac.Length != 0)
                    addToDict(frac, matches[y].Index);
                else
                    addToDict(noSpace.Replace(",", string.Empty), matches[y].Index);
                Replace(matches[y]);

            }
        }
        public void multipleNum()//check for cases like 10m, 10 million
        {
            Regex multipleNum = new Regex("[0-9]+[,.]*[0-9]*[.,]*[0-9]* (hundreds|millions|billion|trillion|thousends)");//checks for 76.9 miilion/billiob/trillion/hundreds/thousends
            MatchCollection matches = multipleNum.Matches(docParagraphs[currentParagraph]);
            for (int y = 0; y < matches.Count; y++)
            {

                int i = 0;
                int j = 0;
                string mult = "";//million/hundreds/trillion...
                string number = "";
                while (int.TryParse(matches[y].Value[i].ToString(), out j))
                {
                    number += matches[y].Value[i];
                    i++;
                }
                if (matches[y].Value[i] == '.')
                {
                    number += matches[y].Value[i];
                    i++;
                    while (int.TryParse(matches[y].Value[i].ToString(), out j))
                    {
                        number += matches[y].Value[i];
                        i++;
                    }
                }
                while (matches[y].Value[i] == ' ')
                    i++;

                while (i < matches[y].Value.Length)
                {
                    mult += matches[y].Value[i];
                    i++;
                }
                double num = Convert.ToDouble(number);
                if (mult == "hundreds")
                    num = num * 100;
                if (mult == "thousends")
                    num = num * 1000;
                if (mult == "millions")
                    num = num * 1000000;
                if (mult == "billion")
                    num = num * 1000000000;
                if (mult == "trillion")
                    num = num * 1000000000000;
                string toMult = num.ToString();
                addToDict(toMult, matches[y].Index);

                Replace(matches[y]);
            }
        }
        public void Percent()// check for cases like 10%, 10 percent
        {
            Regex Percent = new Regex("[0-9]+[.,]*[0-9]*( )*(-){0,1}(%|percent|percents|percentage)+");//checks for all the numbers with percent
            MatchCollection matches = Percent.Matches(docParagraphs[currentParagraph]);
            for (int y = 0; y < matches.Count; y++)
            {

                int i = 0;
                int j = 0;
                string number = "";
                while (int.TryParse(matches[y].Value[i].ToString(), out j))
                {
                    number += matches[y].Value[i];
                    i++;
                }
                number += '%';
                addToDict(number, matches[y].Index);

                Replace(matches[y]);

            }
        }
        public void Price()//check for cases like 10$, 10 Dollars
        {
            Regex Price = new Regex(@"Dollars( ){0,1}[0-9]*[.,]*[0-9]*( ){0,1}[0-9]+[.,]*[0-9]*/[0-9]+[.,]*[0-9]*|Dollars [0-9]+[.,]*[0-9]*m*b*n*|\$[0-9]+[,]*[0-9]*[.,]*[0-9]*(( )+(million|billion)+)*");//checks for all the numbers with percent
            // Regex Price = new Regex("Dollars [0-9]* [0-9]+/[0-9]+");m*i*l*i*o*n*b*i*l*i*o*n*
            MatchCollection matches = Price.Matches(docParagraphs[currentParagraph]);
            for (int y = 0; y < matches.Count; y++)
            {

                int i = 0;
                int j = 0;
                string number = "";

                if (matches[y].Value[i] == 'D')
                {
                    while (i < matches[y].Value.Length && matches[y].Value[i] != ' ')
                        i++;
                    while (i < matches[y].Value.Length && matches[y].Value[i] == ' ')
                        i++;
                    while (i < matches[y].Value.Length && int.TryParse(matches[y].Value[i].ToString(), out j))
                    {
                        number += matches[y].Value[i];
                        i++;
                    }
                    if (i < matches[y].Value.Length)
                    {
                        if (matches[y].Value[i] == '.' || matches[y].Value[i] == ',')//for 24,665.54
                        {
                            number += matches[y].Value[i];
                            i++;
                            while (i < matches[y].Value.Length && int.TryParse(matches[y].Value[i].ToString(), out j))
                            {
                                number += matches[y].Value[i];
                                i++;
                            }
                        }
                    }

                    if (i < matches[y].Value.Length && matches[y].Value[i] == '.')
                    {
                        number += matches[y].Value[i];
                        i++;
                        while (i < matches[y].Value.Length && int.TryParse(matches[y].Value[i].ToString(), out j))
                        {
                            number += matches[y].Value[i];
                            i++;
                        }
                    }
                    if (matches[y].Value[matches[y].Value.Length - 1] == '.')
                    {
                        while (i < matches[y].Value.Length)
                        {
                            number += matches[y].Value[i];
                            i++;
                        }
                        number += '0';
                    }
                    if (i < matches[y].Value.Length && matches[y].Value[i] == ' ')
                    {
                        while (i < matches[y].Value.Length && matches[y].Value[i] == ' ')
                            i++;
                        string counter = "";
                        while (i < matches[y].Value.Length && int.TryParse(matches[y].Value[i].ToString(), out j))
                        {
                            counter += matches[y].Value[i];
                            i++;
                        }
                        i++;
                        string denominator = "";
                        while (i < matches[y].Value.Length)
                        {
                            denominator += matches[y].Value[i];
                            i++;
                        }
                        double final = Convert.ToDouble(number) + (Convert.ToDouble(counter) / Convert.ToDouble(denominator));
                        number = '$' + final.ToString();

                    }
                    else if (i < matches[y].Value.Length && matches[y].Value[i] == '/')
                    {
                        i++;
                        string denominator = "";
                        while (i < matches[y].Value.Length)
                        {
                            denominator += matches[y].Value[i];
                            i++;
                        }
                        double final = Convert.ToDouble(number) / Convert.ToDouble(denominator);
                        number = '$' + final.ToString();
                    }
                    if (i < matches[y].Value.Length)
                    {
                        if (matches[y].Value[i] == 'm')
                        {
                            double final = Convert.ToDouble(number) * 1000000;
                            number = '$' + final.ToString();
                        }
                        if (matches[y].Value[i] == 'b' && matches[y].Value[i + 1] == 'n')
                        {
                            double final = Convert.ToDouble(number) * 1000000000;
                            number = '$' + final.ToString();
                        }
                    }

                    if (number[0] != '$')
                    {
                        number = '$' + number;
                    }
                }
                if (matches[y].Value[0] == '$')
                {
                    i++;
                    if (PreliminaryCheck(matches[y].Value[matches[y].Value.Length - 1]))
                    {
                        while (i < matches[y].Value.Length)
                        {
                            number += matches[y].Value[i];
                            i++;
                        }
                    }
                    if (matches[y].Value[matches[y].Value.Length - 1] == '.' || matches[y].Value[matches[y].Value.Length - 1] == ',')
                    {
                        while (i < matches[y].Value.Length)
                        {
                            number += matches[y].Value[i];
                            i++;
                        }
                        number = number.Substring(0, number.Length - 1);
                    }
                    while (i < matches[y].Value.Length && matches[y].Value[i] != ' ')
                    {
                        number += matches[y].Value[i];
                        i++;
                    }
                    if (i < matches[y].Value.Length)
                    {
                        while (i < matches[y].Value.Length && matches[y].Value[i] == ' ')
                            i++;
                        string toDouble = "";
                        while (i < matches[y].Value.Length)
                        {
                            toDouble += matches[y].Value[i];
                            i++;
                        }
                        if (toDouble == "millions" || toDouble == "million")
                        {
                            double final = Convert.ToDouble(number) * 1000000;
                            number = '$' + final.ToString();
                        }
                        if (toDouble == "billions" || toDouble == "billion")
                        {
                            double final = Convert.ToDouble(number) * 1000000000;
                            number = '$' + final.ToString();
                        }

                    }
                    if (number[0] != '$')
                    {
                        number = '$' + number;
                    }
                }
                number = number.Replace(",", string.Empty);
                addToDict(number, matches[y].Index);

                Replace(matches[y]);
            }
        }


        public void Date()//check all the cases of date like it was written in the work 
        {
            string monthString = @"(January|Febuary|March|April|June|July|August|September|October|November|December|Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)";
            string dateRegExpression = @"((\d{1,2}((-)*\d{0,2}){0,1}(th |st |nd |rd | )(of |)" + monthString + @"( \d{4}| \d{2}|)\d{0,2})|\d{4}|" + monthString + @"( \d{4}| \d{2}(,( )*\d{4}|)))";
            Regex Date = new Regex(dateRegExpression);
            MatchCollection matches = Date.Matches(docParagraphs[currentParagraph]);

            for (int y = 0; y < matches.Count; y++)
            {

                int i = 0;
                int j = 0;
                string date = "";
                string date2 = "";
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


                if (date.Length >= 1)
                {
                    addToDict(date.ToLower(), matches[y].Index);
                    if (date2.Length != 0)
                        addToDict(date2.ToLower(), matches[y].Index);
                    Replace(matches[y]);
                }


            }
        }
        public void Replace(Match match)// replaces terms with * with the same length length
        {
            string toReplace = "";
            for (int t = 0; t < match.Value.Length; t++)
                toReplace += '*';
            int k = docParagraphs[currentParagraph].Length;
            docParagraphs[currentParagraph] = docParagraphs[currentParagraph].Remove(match.Index, match.Value.Length).Insert(match.Index, toReplace);//maybe match.Value.Length+1
            // parsedDoc.text = parsedDoc.text(match.Index, match.Value.Length + 1).Insert(match.Index, toReplace);

        }

    }
}



