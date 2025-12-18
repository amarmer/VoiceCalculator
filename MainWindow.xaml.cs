using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Speech.Synthesis;
using System.Runtime.InteropServices;

namespace VoiceCalculator
{
    public interface UIUpdater
    {
        void UpdateUI(string result);
        void ShowError();
    }

    public partial class MainWindow : Window, UIUpdater
    {
        private const string PronounceResult = "Pronounce result of sum";
        private const string CorrectResult = "Correct! Modify digits and try again";
        private const string NoResult = "?";

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        public MainWindow()
        {
            InitializeComponent();

            // Uncomment `AllocConsole` and 'Console.Writeline' will write output to the console.
            // AllocConsole();

            Loaded += (_, _) =>
            {
            };

            Closing += (_, _) =>
            {
                SpeechRecognizer.Destroy();
            };

            try
            {
                _speechRecognizer = SpeechRecognizer.Instance();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);

                Application.Current.Shutdown();
            }
        }

        private void LoadGrammar(string text)
        {
            try
            {
                _speechRecognizer.LoadGrammar(text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("LoadGrammar exception: " + ex.Message);

                Application.Current.Shutdown();
            }
        }


        public void UpdateUI(string result)
        {
            Info.Text = "Correct! Click on any word and pronounce it";
        }

        public void ShowError()
        {
            Info.Text = "Incorrect! Try pronounce '" + _word + "' again";
        }

        private ISpeechRecognizer _speechRecognizer;

        SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var text = Words.Text;

            if (Words.Text == "Type any text here")
            {
                Words.Text = "";
                Words.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void TextBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                // Get the mouse click position relative to the TextBox
                Point clickPoint = e.GetPosition(textBox);

                // Get the character index from the click position
                int charIndex = textBox.GetCharacterIndexFromPoint(clickPoint, true);

                if (charIndex >= 0 && charIndex < textBox.Text.Length)
                {
                    string text = textBox.Text;

                    // Find the start and end of the word at the given index
                    int start = charIndex;
                    while (start > 0 && !char.IsWhiteSpace(text[start - 1]))
                    {
                        start--;
                    }

                    int end = charIndex;
                    while (end < text.Length - 1 && !char.IsWhiteSpace(text[end + 1]))
                    {
                        end++;
                    }

                    // Extract the word
                    _word = text.Substring(start, end - start + 1);

                    Info.Text = "Pronounce '" + _word + "'";

                    LoadGrammar(_word);
                }
            }
        }

        string _word;
    }
}
