using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProjectPart1
{
    /// <summary>
    /// Interaction logic for DictWindow.xaml
    /// </summary>
    public partial class DictWindow : Window
    {
        public string path;

        public DictWindow()
        {
            InitializeComponent();


        }



        public void AddTerms()
        {

            foreach(string term in data)
                    listBoxInvertedIndex.Items.Add(term);
                



            }

        


            public List<string> data { get; set; }
    }
}
