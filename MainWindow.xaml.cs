using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TextProcessor
{
    public partial class MainWindow : Window
    {
        private bool isEncoded = false;
        private string CurrentText => new TextRange(rtbText.Document.ContentStart, rtbText.Document.ContentEnd).Text.TrimEnd();

        public MainWindow()
        {
            InitializeComponent();
            UpdateWordCount();
        }

        private void rtbText_TextChanged(object sender, TextChangedEventArgs e) => UpdateWordCount();
        private string ShiftText(string text, int shift) => new string(text.Select(c => (char)(c + shift)).ToArray());

        private void UpdateWordCount()
        {
            if (lblWordCount == null)
                return;
            lblWordCount.Content = $"Words total: {Regex.Split(CurrentText, @"\s+").Count(s => !string.IsNullOrWhiteSpace(s))}";
        }

        private void SetRichText(string text)
        {
            rtbText.Document.Blocks.Clear();
            rtbText.Document.Blocks.Add(new Paragraph(new Run(text)));
        }

        private Paragraph BuildParagraph(string keyword, Func<string, bool> isMatch, Func<string, Inline> processMatch)
        {
            string pattern = $"({Regex.Escape(keyword)})";
            var parts = Regex.Split(CurrentText, pattern, RegexOptions.IgnoreCase);
            var paragraph = new Paragraph();
            parts.ToList().ForEach(p =>
            {
                paragraph.Inlines.Add(isMatch(p) ? processMatch(p) : new Run(p));
            });
            return paragraph;
        }

        private void DeleteWord_Click(object sender, RoutedEventArgs e)
        {
            string wordToDelete = tbDeleteWord.Text;
            if (string.IsNullOrWhiteSpace(wordToDelete))
            {
                MessageBox.Show("Enter something to delete");
                return;
            }
            string pattern = $@"\b{Regex.Escape(wordToDelete)}\b";
            string newText = Regex.Replace(CurrentText, pattern, "", RegexOptions.IgnoreCase);
            SetRichText(newText);
        }

        private void ReverseWord_Click(object sender, RoutedEventArgs e)
        {
            string wordToReverse = tbReverseWord.Text;
            if (string.IsNullOrEmpty(wordToReverse))
            {
                MessageBox.Show("Enter something to reverse");
                return;
            }

            var paragraph = BuildParagraph(
                wordToReverse,
                part => string.Equals(part, wordToReverse, StringComparison.CurrentCultureIgnoreCase),
                part =>
                {
                    string reversed = new string(part.Reverse().ToArray());
                    return new Run(reversed) { Foreground = Brushes.Red };
                });
            rtbText.Document.Blocks.Clear();
            rtbText.Document.Blocks.Add(paragraph);
        }

        private void FindWordCount_Click(object sender, RoutedEventArgs e)
        {
            string wordToFind = tbFindWord.Text;
            if (string.IsNullOrWhiteSpace(wordToFind))
            {
                MessageBox.Show("Enter something to search");
                return;
            }

            var paragraph = BuildParagraph(
                wordToFind,
                part => string.Equals(part, wordToFind, StringComparison.CurrentCultureIgnoreCase),
                part => new Run(part) { Background = Brushes.Yellow }
            );
            string pattern = $"({Regex.Escape(wordToFind)})";
            var parts = Regex.Split(CurrentText, pattern, RegexOptions.IgnoreCase);
            int count = parts.Count(part => string.Equals(part, wordToFind, StringComparison.CurrentCultureIgnoreCase));
            lblCount.Content = count.ToString();
            rtbText.Document.Blocks.Clear();
            rtbText.Document.Blocks.Add(paragraph);
        }

        private void CleanSpaces_Click(object sender, RoutedEventArgs e)
        {
            string cleaned = Regex.Replace(CurrentText, @"\s+", " ").Trim();
            SetRichText(cleaned);
        }

        private void CodeDecode_Click(object sender, RoutedEventArgs e)
        {
            string newText = !isEncoded ? ShiftText(CurrentText, 2) : ShiftText(CurrentText, -2);
            btnCodeDecode.Content = !isEncoded ? "Decode" : "Code";
            isEncoded = !isEncoded;
            SetRichText(newText);
        }
    }
}
