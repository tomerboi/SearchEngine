
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace ProjectPart1
{
    public class ReadFile
    {
        public List<Document> documents = new List<Document>();
        Parse parser = new Parse();
        HashSet<string> stopWordsHashSet;
        bool reset = false;
        int file = 0;
        int numOfDoc = 0;
        public Stopwatch stopwatch;


        public List<Document> readFilesFromFolder(string path, bool WithStemming)
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
            parser.withStemming = WithStemming;
            var stopwords = File.ReadAllText(path + @"\stop_words.txt");
            string[] lines = stopwords.Split('\n');
            int i = 0;
            for (i = 0; i < lines.Length; i += 1)
                lines[i] = lines[i].Trim();
            stopWordsHashSet = new HashSet<string>(lines);
            parser.stopWordsHashSet = stopWordsHashSet;

            for (file = 396001; file <= 396240; file++)
            {


                if (file == 396083 || file == 396155 || file == 396186 || file == 396207)
                    continue;
                using (StreamReader sr = new StreamReader(path + '\\' + "FB" + file))
                {
                    makeDocuments((sr.ReadToEnd()));

                }


            }
            for (file = 496001; file <= 496262; file++)
               {

                   if (file == 496008 || file == 496088 || file == 496149 || file == 496215 || file == 496236 || file == 496252)
                       continue;

                   using (StreamReader sr = new StreamReader(path + '\\' + "FB" + file))
                   {
                       makeDocuments((sr.ReadToEnd()));
                   }

               }
            
            stopwatch.Stop();

            return documents;

        }



        /**
         * devides the given string to documents.
         * */
        internal void makeDocuments(string contents)
        {

            int currentIndex = 0;
            int previousIndex = 0;
            while (currentIndex + 10 < contents.Length && previousIndex + 10 < contents.Length && !reset)
            {

                currentIndex = contents.IndexOf("<DOC>", previousIndex);
                int docStartIndex = currentIndex;


                Document tmpdoc = new Document();
                numOfDoc++;




                previousIndex = contents.IndexOf("<DOCNO>", docStartIndex);
                currentIndex = contents.IndexOf("</DOCNO>", previousIndex);
                
                tmpdoc.ID = contents.Substring(previousIndex + 8, currentIndex - (previousIndex + 9));
                previousIndex = contents.IndexOf("<DATE1>", docStartIndex);
                currentIndex = contents.IndexOf("</DATE1>", previousIndex);

                tmpdoc.date = contents.Substring(previousIndex + 6, currentIndex - (previousIndex + 6));

                previousIndex = contents.IndexOf("<TI>", docStartIndex);
                currentIndex = contents.IndexOf("</TI>", previousIndex);

                tmpdoc.title = contents.Substring(previousIndex + 4, currentIndex - (previousIndex + 4));

                previousIndex = contents.IndexOf("<TEXT>", docStartIndex);
                currentIndex = contents.IndexOf("</TEXT>", previousIndex);
                if (contents.Substring(previousIndex+6,18).Contains("Language: <"))
                {
                     previousIndex = contents.IndexOf("Article", previousIndex)+15;

                }
                
                tmpdoc.text = contents.Substring(previousIndex + 6, currentIndex - (previousIndex + 6));

                previousIndex = contents.IndexOf("</DOC>", currentIndex);



                
                    parser.Parse1(tmpdoc);
                


                tmpdoc.text = null;
                documents.Add(tmpdoc);

            }
        }

        public string CorpusPath { get; set; }

        public string PostingsPath { get; set; }

        internal void resetReadFile()
        {
            reset = true;
        }
    }
}

