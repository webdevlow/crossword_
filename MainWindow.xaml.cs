using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;

namespace CrosswordCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly List<Button> _buttons;
        private readonly List<string> _words = new List<string>();
        private List<string> _order;

        Crossword.Crossword _board = new Crossword.Crossword(13, 17);
        Random _rand = new Random();

        public MainWindow()
        {
            InitializeComponent();
            _buttons = new List<Button> { bcolor2, bcolor1, bcolor3, button4, button5, button6 };

            for (var i = 0; i < _board.N; i++)
            {
                for (var j = 0; j < _board.M; j++)
                {
                    var b = new Button { Background = _buttons[0].Background, Content = "" };

                    Grid.SetRow(b, i);
                    Grid.SetColumn(b, j);
                    grid1.Children.Add(b);

                }
            }

            blackSquaresLabel.Content = (_board.N*_board.M).ToString();
        }


        private void Button2Click(object sender, RoutedEventArgs e)
        {
            var word = newWordTextBox.Text.Trim();
            if (word.Length != 0)
            {

                if (_words.Contains(word))
                {
                    MessageBox.Show("This word already exists in the crossword", "Alert!!", MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    return;
                }

                _words.Add(word);
                listView1.Items.Add(word);
            }
            
            newWordTextBox.Text = "";
            newWordTextBox.Focus();


        }


        static int Comparer(string a, string b)
        {
            var temp = a.Length.CompareTo(b.Length);
            return temp == 0 ? a.CompareTo(b) : temp;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            _words.Sort(Comparer);
            _words.Reverse();
            _order = _words;
            GenCrossword();
            newWordTextBox.Focus();
        }

        void GenCrossword()
        {
            horizontalWordsListView.Items.Clear();
            verticalWordsListView.Items.Clear();
            notUsedListView.Items.Clear();
            _board.Reset();
            ClearBoard();

            _board.inRTL = (bool) RTLRadioButton.IsChecked;
            foreach (var word in _order)
            {
                //var wordToInsert = ((bool)RTLRadioButton.IsChecked) ? word.Reverse().Aggregate("",(x,y) => x + y) : word;

                switch (_board.AddWord(word))
                {
                    case 0:
                        horizontalWordsListView.Items.Add(word);
                        break;
                    case 1:
                        verticalWordsListView.Items.Add(word);
                        break;
                    default:
                        notUsedListView.Items.Add(word);
                        break;

                }
            }
            
            ActualizeData();
        }
        
        void ActualizeData()
        {

            var count = _board.N * _board.M;

            var board = _board.GetBoard;
            var p = 0;

            for (var i = 0; i < _board.N; i++)
            {
                for (var j = 0; j < _board.M; j++)
                {
                    var letter = board[i, j] == '*' ? ' ' : board[i, j];
                    if (letter != ' ') count--;
                    ((Button)grid1.Children[p]).Content = letter.ToString();
                    ((Button)grid1.Children[p]).Background = letter != ' ' ? _buttons[4].Background : _buttons[0].Background;
                    p++;
                }
            }

            blackSquaresLabel.Content = count.ToString();
        }

        void GenOrder()
        {
            _order = _words.Where(word => _rand.NextDouble() > 0.3).ToList();// (_words);
        }

        void ClearBoard()
        {
            var p = 0;
            for (var i = 0; i < _board.N; i++)
            {
                for (var j = 0; j < _board.M; j++)
                {
                    ((Button)grid1.Children[p]).Content = "";
                    ((Button)grid1.Children[p]).Background = _buttons[0].Background;
                    p++;
                }
            }
            blackSquaresLabel.Content = (_board.N*_board.M).ToString();
        }

        private void NewButton_Click_1(object sender, RoutedEventArgs e)
        {
            horizontalWordsListView.Items.Clear();
            verticalWordsListView.Items.Clear();
            notUsedListView.Items.Clear();
            listView1.Items.Clear();
            _words.Clear();
            newWordTextBox.Text = "";
            ClearBoard();
            newWordTextBox.Focus();
        }

        private void LoadWordsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openfileDialog = (OpenFileDialog) Resources["loadWordsOpenFileDialog"];

                if ((bool) openfileDialog.ShowDialog())
                {
                    NewButton_Click_1(null, null);
                    var reader = new StreamReader(openfileDialog.FileName, true);

                    while (!reader.EndOfStream)
                    {
                        var word = reader.ReadLine().Trim();
                        listView1.Items.Add(word);
                        _words.Add(word);
                    }
                }
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message, "Error reading file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            newWordTextBox.Focus();
        }

        private void LatinRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            //newWordTextBox.FlowDirection = FlowDirection.LeftToRight;
            newWordTextBox.Focus();
        }

        private void RTLRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            //newWordTextBox.FlowDirection = FlowDirection.RightToLeft;
            
            newWordTextBox.Focus();
        }

        private void NewWordTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Button2Click(null,null);
            }
        }

        private void OptimizeButtonClick(object sender, RoutedEventArgs e)
        {
            var initialTime = DateTime.Now;

            var min = 0;

            int number = _words.Sum(x => x.Length);

            if (number >= _board.N * _board.M - 60)
            {

                GenerateButton_Click(null, null);
                Crossword.Crossword temp = _board;
                min = int.Parse((string)blackSquaresLabel.Content);
                while ((DateTime.Now - initialTime).Minutes < 1)
                {
                    if (int.Parse((string)blackSquaresLabel.Content) <= 70)
                        break;
                    GenOrder();
                    _board = new Crossword.Crossword(13, 17);
                    GenCrossword();
                    if (int.Parse((string)blackSquaresLabel.Content) < min)
                    {
                        temp = _board;
                        min = int.Parse((string)blackSquaresLabel.Content);
                    }
                }

                _board = temp;
                blackSquaresLabel.Content = min.ToString();



                ActualizeData();
            }

            //_board.inRTL =(bool) RTLRadioButton.IsChecked;

            //_board.AddWords(_words);

            //ActualizeData();

            if (int.Parse((string)blackSquaresLabel.Content) > 95)
                MessageBox.Show("Not enough words", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
            newWordTextBox.Focus();
        }
    }
}