using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;




namespace ProjectPart1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial
        class MainWindow : Window
    {
        int queryCounter = 0;
        ResultsWindow resultsWindow;
        DictWindow dictWindow;
        Term t1;
        Term t2;
        Stopwatch readPostingsTimer;
        TimeSpan ReadDocsTime = new TimeSpan();
        int fromMonth;
        int toMonth;
        Searcher search;
        Ranker rank;
        Dictionary<string, DocumentPosting> DocsDict;
        Dictionary<string, Term> sortedTermDictionary;
        string query = "";
        string queryNum = "0";
        ReadFile readfile;
        bool resetPressed = false;
        Indexer indexer;
        bool dataPathOn = false;
        bool postingsPathOn = false;
        bool stem;
        public string PostingsPath;
        public string DataSetPath;
        public string dictPath;
        Parse parser;
        string QueriesFilePath;
        public MainWindow()
        {
            InitializeComponent();


            DataContext = readfile;


        }
        //path 1 postings files
        private void browse1_Click(object sender, RoutedEventArgs e)
        {

            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result.ToString() == "Cancel")
                return;
            path1.Text = dialog.SelectedPath;

            if (path1.Text != "Postings path")
            {
                dataPathOn = true;
                PostingsPath = dialog.SelectedPath;
                if (File.Exists(@PostingsPath + @"\TermPostings.txt" )|| File.Exists(@PostingsPath + @"\TermPostingsStemming.txt"))
                    ReadTermsPostingsButton.IsEnabled = true;
            }
            if (dataPathOn && postingsPathOn)
            {
                Start.IsEnabled = true;
            }
        }
        //path 2 data set and stop words text file
        private void browse2_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result.ToString() == "Cancel")
                return;
            path2.Text = dialog.SelectedPath;

            if (path2.Text != "Data and stop wards path")
            {
                postingsPathOn = true;
                DataSetPath = dialog.SelectedPath;
                ;
            }

            if (dataPathOn && postingsPathOn)
                Start.IsEnabled = true;
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            stem = checkBox.IsChecked.Value;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

            stem = checkBox.IsChecked.Value;
        }

        // start the run in a separate thread
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            reset.IsEnabled = true;
            readfile = new ReadFile();
            Start.IsEnabled = false;
            stem = checkBox.IsChecked.Value;
            Start.Content = "Running...";
            Thread readFromFolderThread = new Thread(delegate()
            {
                DoEverything();
            });
            readFromFolderThread.Start();

            stem = checkBox.IsChecked.Value;


        }
        // the function that runs readfile, parser, indexer and postings
        private void DoEverything()
        {

            this.Dispatcher.Invoke((Action)(() =>
            {
                Start.Content = "Parsing...";

            }));
            Stopwatch mainwatch = new Stopwatch();
            mainwatch.Start();

            readfile.readFilesFromFolder(DataSetPath, stem);// read the files and parse

            if (resetPressed)
            {
                stop();
                return;
            }
            indexer = new Indexer();
            TimeSpan readfileTime = readfile.stopwatch.Elapsed;
            int docCount = readfile.documents.Count;
            this.Dispatcher.Invoke((Action)(() =>
            {
                Start.Content = "Indexing Terms...";
                StatsTextBlock.Items.Clear();
                StatsTextBlock.Items.Add( "Read file + Parse Time : " + readfileTime);
                StatsTextBlock.Items.Add("Number of documents parsed: " + docCount);
                StatsTextBlock.Items.Add("Total Time so far: " + readfileTime);
            }));



            indexer.createTermAndDictionaries(readfile.documents);//creates the dictionaries (inverted index)


            TimeSpan indexerTime = indexer.stopwatch1.Elapsed;
            readfile = null;
            int termsCount = indexer.sortedTermDictionary.Count;
            if (resetPressed)
            {
                stop();
                return;
            }
            this.Dispatcher.Invoke((Action)(() =>
            {
                Start.Content = "Creating postings...";

                StatsTextBlock.Items.Clear();
                StatsTextBlock.Items.Add("Read file + Parse Time : " + readfileTime);
                StatsTextBlock.Items.Add("Number of documents parsed: " + docCount);
                StatsTextBlock.Items.Add("inverted Index Creation Time : " + indexerTime);

                StatsTextBlock.Items.Add("Number of terms indexed : " + termsCount);

                StatsTextBlock.Items.Add("Total Time so far: " + (readfileTime + indexerTime));

            }));


            Stopwatch lastmainwatch = new Stopwatch();
            lastmainwatch.Start();

            // writes the documents postings file
            string Stemming = "";
            if (stem)
                Stemming="Stemming";
            using (StreamWriter sw = File.AppendText(@PostingsPath + @"\DocPostings"+Stemming+".txt"))
            {
                string json = JsonConvert.SerializeObject(indexer.docDictionary, Formatting.Indented);

                sw.Write(json);

            }


            this.Dispatcher.Invoke((Action)(() =>
            {
                Start.Content = "Creating postings...";


                StatsTextBlock.Items.Clear();
                StatsTextBlock.Items.Add("Read file + Parse Time : " + readfileTime);
                StatsTextBlock.Items.Add("Number of documents parsed: " + docCount);
                StatsTextBlock.Items.Add("inverted Index Creation Time : " + indexerTime);
                StatsTextBlock.Items.Add("Number of terms indexed : " + termsCount);
                StatsTextBlock.Items.Add("Documents postings created.");
                StatsTextBlock.Items.Add("Total Time so far : " + (readfileTime + indexerTime + lastmainwatch.Elapsed));


            }));
            indexer.docDictionary.Clear();

            if (resetPressed)
            {
                stop();
                return;
            }


            // writes the terms postings file
            indexer.CreateTermsPostings(@PostingsPath,stem);


            indexer.sortedTermDictionary.Clear();

            // the text file from which the inverted index is loaded and showed


            indexer = null;
            readfile = null;
            this.Dispatcher.Invoke((Action)(() =>
            {

                StatsTextBlock.Items.Clear();
                StatsTextBlock.Items.Add("Read file + Parse Time : " + readfileTime);
                StatsTextBlock.Items.Add("Number of documents parsed: " + docCount);
                StatsTextBlock.Items.Add("inverted Index Creation Time : " + indexerTime);
                StatsTextBlock.Items.Add("Number of terms indexed : " + termsCount);
                StatsTextBlock.Items.Add("Documents postings created.");
                StatsTextBlock.Items.Add("Terms postings created.");
                StatsTextBlock.Items.Add("Posting Files creation Time: " + lastmainwatch.Elapsed.ToString());
                StatsTextBlock.Items.Add("Total Time  : " + (mainwatch.Elapsed));
                if (File.Exists(@PostingsPath + @"\TermPostings.txt"))
                    ReadTermsPostingsButton.IsEnabled = true;
                ShowInvertedIndex.IsEnabled = true;


            }));

            if (resetPressed)
            {
                stop();
                return;
            }

            this.Dispatcher.Invoke((Action)(() =>
            {
                Start.Content = "START (press RESET)";
            }));
            lastmainwatch.Stop();


        }




        private void stop()
        {

            resetPressed = false;

        }
        private void reset_button(object sender, RoutedEventArgs e)
        {
            resetPressed = true;
            if (readfile != null)
                readfile.resetReadFile();
            if (indexer != null)
                indexer.resetIndexer();
            Start.Content = "START";
            StatsTextBlock.Items.Clear();
            readfile = new ReadFile();
            indexer = null;

            if (File.Exists(PostingsPath + @"\DocPostings.txt"))
            {
                File.Delete(PostingsPath + @"\DocPostings.txt");
            }
            if (File.Exists(PostingsPath + @"\TermPostings.txt"))
            {
                File.Delete(PostingsPath + @"\TermPostings.txt");
            }
            if (File.Exists(PostingsPath + @"\DocPostingsStemming.txt"))
            {
                File.Delete(PostingsPath + @"\DocPostingsStemming.txt");
            }
            if (File.Exists(PostingsPath + @"\TermPostingsStemming.txt"))
            {
                File.Delete(PostingsPath + @"\TermPostingsStemming.txt");
            }

            stop();
            if (postingsPathOn && dataPathOn)
            {
                selectQueryFile.IsEnabled = false;
                QueryEntry.IsEnabled = false;
                Start.IsEnabled = true;
                save_result_button.IsEnabled = false;
            }


        }






        private void click_showDict(object sender, RoutedEventArgs e)
        {
            StatsTextBlock.Items.Clear();

            foreach(string term in sortedTermDictionary.Keys)
            {
                StatsTextBlock.Items.Add(term);

            }


        }






        private void ReadTermsPostingsButton_Click(object sender, RoutedEventArgs e)
        {
            StatsTextBlock.Items.Clear();
            Thread readFromPostingsThread = new Thread(delegate()
            {
                ReadPostings();
            });
            readFromPostingsThread.Start();
        }
        private void ReadPostings()
        {
            int k = 1;
            //PostingsPath = @"D:\yohay14.1";
            readPostingsTimer = new Stopwatch();
            readPostingsTimer.Start();

            readDocPostings();
            ReadTermsPostings();



            readPostingsTimer.Stop();







        }

        private void ReadTermsPostings()
        {
            int n = 0;
            string path = "";
            if (!stem)
                path = PostingsPath + @"\TermPostings.txt";
            else
                path = PostingsPath + @"\TermPostingsStemming.txt";


            sortedTermDictionary = new Dictionary<string, Term>();

            if (File.Exists(path))
            {

                string postingFile = File.ReadAllText(path);
                int firstIndex = 0;
                int secondIndex = 0;

                while (firstIndex < postingFile.Length - 5)
                {
                    n++;
                    if (n % 2000 == 0)
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            ReadPostingsTextBlock.Items.Clear();

                            ReadPostingsTextBlock.Items.Add( "Term number" + n.ToString() + "read.");
                        }));
                    }


                    secondIndex = postingFile.IndexOf(':', firstIndex);
                    string termKey = postingFile.Substring(firstIndex, secondIndex - firstIndex);
                    firstIndex = postingFile.IndexOf(';', secondIndex + 1);
                    Term term = new Term(Convert.ToInt32(postingFile.Substring(secondIndex + 1, firstIndex - (secondIndex + 1))), termKey);
                    secondIndex = postingFile.IndexOf('#', firstIndex + 5);
                    string termValue = postingFile.Substring(firstIndex + 1, (secondIndex - (firstIndex + 1)));
                    firstIndex = secondIndex + 1;
                    term.PostingValue = termValue;
                    sortedTermDictionary.Add(termKey, term);




                }
                postingFile = null;
                this.Dispatcher.Invoke((Action)(() =>
                {
                    ReadPostingsTextBlock.Items.Clear();

                    ReadPostingsTextBlock.Items.Add("Term number" + n.ToString() + "read.");
                    ReadPostingsTextBlock.Items.Add( "read postings Time: " + readPostingsTimer.Elapsed.ToString());
                    ShowInvertedIndex.IsEnabled = true;



                    selectQueryFile.IsEnabled = true;
                    QueryEntry.IsEnabled = true;

                }));
            }
        }

        private void readDocPostings()
        {
            string path = "";
            if (!stem)
                path = PostingsPath + @"\DocPostings.txt";
            else
                path = PostingsPath + @"\DocPostingsStemming.txt";
            if (File.Exists(path))
            {
                string postingFile = File.ReadAllText(path);
                int firstIndex = 0;
                int secondIndex = 0;
                int endIndex = postingFile.IndexOf('}');
                DocsDict = new Dictionary<string, DocumentPosting>();
                int n = 0;
                while (firstIndex < postingFile.Length - 15)
                {
                    n++;
                    firstIndex = postingFile.IndexOf("[ID]", firstIndex);
                    secondIndex = postingFile.IndexOf("[/ID]", firstIndex + 4);
                    string DocID = postingFile.Substring(firstIndex + 4, (secondIndex - (firstIndex + 4)));
                    firstIndex = postingFile.IndexOf("[A]", secondIndex + 4);
                    secondIndex = postingFile.IndexOf("[/A]", firstIndex + 3);
                    int numOfWords = Convert.ToInt32(postingFile.Substring(firstIndex + 3, secondIndex - (firstIndex + 3)));
                    firstIndex = postingFile.IndexOf("[B]", secondIndex + 4);
                    secondIndex = postingFile.IndexOf("[/B]", firstIndex + 4);
                    int numOfChars = Convert.ToInt32(postingFile.Substring(firstIndex + 3, secondIndex - (firstIndex + 3)));
                    firstIndex = postingFile.IndexOf("[C]", secondIndex + 4);
                    secondIndex = postingFile.IndexOf("[/C]", firstIndex + 4);
                    string title = postingFile.Substring(firstIndex + 3, secondIndex - (firstIndex + 3)).ToLower();
                    firstIndex = postingFile.IndexOf("[D]", secondIndex + 4);
                    secondIndex = postingFile.IndexOf("[/D]", firstIndex + 3);
                    string date = postingFile.Substring(firstIndex + 3, secondIndex - (firstIndex + 3));
                    date = Regex.Match(date, "[A-Za-z]+").ToString();
                    firstIndex = postingFile.IndexOf("[S]", secondIndex + 3);
                    secondIndex = postingFile.IndexOf("[E]", firstIndex + 10);
                    string dictionary = postingFile.Substring(firstIndex + 3, secondIndex - (firstIndex + 3));
                    List<KeyValuePair<string, int>> list = stringToList(dictionary);

                    if (n % 1000 == 0)
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            ReadPostingsTextBlock.Items.Clear();

                            ReadPostingsTextBlock.Items.Add( "Doc number" + n.ToString() + "read." + '\n');
                        }));
                    }

                    firstIndex = postingFile.IndexOf("\r\n", secondIndex);

                    DocumentPosting doc = new DocumentPosting(numOfWords, numOfChars, title, date, list);

                    DocsDict.Add(DocID, doc);

                }

                ReadDocsTime = readPostingsTimer.Elapsed;
                this.Dispatcher.Invoke((Action)(() =>
                {
                    ReadPostingsTextBlock.Items.Clear();

                    ReadPostingsTextBlock.Items.Add("Doc number" + n.ToString() + "read.");
                    ReadPostingsTextBlock.Items.Add("read doc postings Time: " + readPostingsTimer.Elapsed.ToString());

                                             ;
                }));
            }
        }


        public List<KeyValuePair<string, int>> stringToList(string docDictionaryString)
        {
            List<KeyValuePair<string, int>> ans = new List<KeyValuePair<string, int>>();
            int firstIndex = 0;
            while (firstIndex < docDictionaryString.Length - 2)
            {
                int secondIndex = docDictionaryString.IndexOf('@', firstIndex);
                string term = docDictionaryString.Substring(firstIndex, secondIndex - (firstIndex));

                firstIndex = docDictionaryString.IndexOf(';', secondIndex + 1);
                int tf = Convert.ToInt16(docDictionaryString.Substring(secondIndex + 1, firstIndex - (secondIndex + 1)));

                firstIndex++;
                ans.Add(new KeyValuePair<string, int>(term, tf));

            }
            return ans;
        }


        private void PressEnterEnterQuery(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {

                if (!checkDates())
                    return;


                query = QueryEntry.Text;
                int i = 0;

                string queryNum = queryCounter.ToString();
                search = new Searcher(sortedTermDictionary, DocsDict);
                if (!search.checkPercent(query))
                search.ParseQuery(query, stem);

                search.getAllRelatedDocuments(fromMonth, toMonth,queryNum,stem);
                if (search.ans.Count == 0)
                {
                    PopUpWindow popup = new PopUpWindow();
                    popup.popUpTxtBox.Text = "No documents found.";
                    popup.ShowDialog();
                }

                initiateShowResultsWindow();


            }
        }

        private bool checkDates()
        {
            bool ans = false;

            this.Dispatcher.Invoke((Action)(() =>
            {

                if (textFromMonth.Text == "" || textToMonth.Text == "")
                {
                    fromMonth = 1;
                    toMonth = 12;
                    ans = true;
                }
                else
                {
                    PopUpWindow popup;

                    int num1 = 0;
                    bool isNumeric1 = int.TryParse(textFromMonth.Text, out num1);
                    int num2 = 0;
                    bool isNumeric2 = int.TryParse(textToMonth.Text, out num2);


                    if (!isNumeric1 || !isNumeric2 || (num1 > 12 || num1 <= 0) || (num2 > 12 || num2 <= 0) || (num1 > num2))
                    {
                        popup = new PopUpWindow();
                        popup.popUpTxtBox.Text = "Please enter a valid month.";
                        popup.ShowDialog();
                        ans = false;
                    }


                    fromMonth = num1;
                    toMonth = num2;
                    ans = true;
                }
            }));

            return ans;

        }


        private void Save_Document_Results_Click(object sender, RoutedEventArgs e)
        {
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = "results1.txt";
            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            List<string> lines = new List<string>();
            if (saveFileDialog1.ShowDialog() == true)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    QueriesResultsPath = saveFileDialog1.FileName;
                    if (QueriesResultsPath == "")
                        return;
                    foreach (ResultLine ansDoc in search.ans)
                    {
                        string str = String.Format("{0:0.00}", ansDoc.score);
                        lines.Add(ansDoc.queryNum + " 0 " + ansDoc.docID + " 1 " + str + " mt \n");
                    }

                    myStream.Close();
                    foreach (string line in lines)
                    {
                        {
                            using (StreamWriter sw2 = File.AppendText(QueriesResultsPath))
                                sw2.WriteLine(line);

                        }


                    }
                }

            }
        }

        private void Select_Query_From_File_Click(object sender, RoutedEventArgs e)
        {
            string sFileName = "";
            OpenFileDialog choofdlog = new OpenFileDialog();
            choofdlog.Filter = "All Files (*.*)|*.*";
            choofdlog.FilterIndex = 1;


            if (choofdlog.ShowDialog() == true)
            {
                sFileName = choofdlog.FileName;
            }
            else return;

            QueriesFilePath = sFileName;
            Thread readQueriesThread = new Thread(delegate()
            {
                ReadQueries();
            });
            readQueriesThread.Start();
            QueriesResultsPath = @"D:\yohay14.1";
        }

        private void ReadQueries()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (!checkDates())
                return;

            string[] lines = System.IO.File.ReadAllLines(QueriesFilePath);
            search = new Searcher(sortedTermDictionary, DocsDict);

            for (int u = 1; u <= lines.Length; u++)
            {


                query = lines[u-1];
                int i = 0;
                while (char.IsNumber(query[i]))
                {
                    queryNum += query[i];
                    i++;
                }
                query = query.Substring(queryNum.Length);
                if (!search.checkPercent(query))
                search.ParseQuery(query, stem);

                search.getAllRelatedDocuments(fromMonth, toMonth,queryNum,stem);
            
                queryNum = "";

                this.Dispatcher.Invoke((Action)(() =>
                {
                    textBoxQueryFilePath.Text = QueriesFilePath;

                    ReadPostingsTextBlock.Items.Clear();
                    ReadPostingsTextBlock.Items.Add("Query number " + u + " read.");
                    ReadPostingsTextBlock.Items.Add("read  time: " + stopwatch.Elapsed.ToString());

                                             
                }));


            }
            this.Dispatcher.Invoke((Action)(() =>
            {
                ReadPostingsTextBlock.Items.Clear();

                ReadPostingsTextBlock.Items.Add("finished reading query file. ");
                ReadPostingsTextBlock.Items.Add("read time: " + stopwatch.Elapsed.ToString());
                
                initiateShowResultsWindow();

            }));
        }

        private void initiateShowResultsWindow()
        {

            int i = 1;
            ReadPostingsTextBlock.Items.Clear();
            ReadPostingsTextBlock.Items.Add(search.ans.Count + " relevant documents found: \n");

            if (search.ans.Count > 0)
            {

                foreach (ResultLine doc in search.ans)
                {
                    ReadPostingsTextBlock.Items.Add(i + ". Document " + doc.docID);
                    i++;
                }
            }

            save_result_button.IsEnabled = true;

        }

        public string QueriesResultsPath { get; set; }



    }
}