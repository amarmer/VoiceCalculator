using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System.Speech.Synthesis;
using System.Runtime.InteropServices;

namespace VoiceCalculator
{
    public interface UIUpdater
    {
        void UpdateUI(string result);
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
                Keyboard.Focus(Number1);

                InitUI();
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

            LoadGrammar(s_numbers[_sum]);
        }

        private void InitUI()
        {
            Result.Text = NoResult;
            Instruction.Text = PronounceResult;
        }

        // 'Delete' and 'Backspace' are disabled, typing new digit will substitute current one. 
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                e.Handled = true;
                return;
            }

            var key = e.Key - Key.D0;

            // Allow only digits 0 - 9.
            if (0 <= key && key <= 9)
            {
                var textBox = sender as TextBox;

                var strKey = key.ToString();

                // If nothing is changed, return.
                if (textBox.Text == strKey)
                {
                    return;
                }

                textBox.Text = strKey;

                InitUI();

                _sum = int.Parse(Number1.Text) + int.Parse(Number2.Text);

                LoadGrammar(s_numbers[_sum]);
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

        private void RestoreCalculator()
        {
            if (_sum != -1)
            {
                LoadGrammar(s_numbers[_sum]);
            }
        }

        public void UpdateUI(string result)
        {
            if (_textUnderCursor == null)
            {
                // Use '_sum == -1' to update UI and speak only once.
                if (_sum != -1 && result == s_numbers[_sum])
                {
                    Result.Text = _sum.ToString();
                    Instruction.Text = CorrectResult;

                    _sum = -1;

                    _speechSynthesizer.SpeakAsync("Correct");
                }
            }
            else
            {
                if (result == _textUnderCursor)
                {
                    _textUnderCursor = null;

                    RestoreCalculator();

                    Console.WriteLine("Reply from speechSynthesizer: " + result);

                    _speechSynthesizer.SpeakAsync("Correct");
                }
            }
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;
            if (source == null)
            {
                return;
            }

            string text = VisualTreeHelpers.GetTextFromElement(source);
            if (!String.IsNullOrEmpty(text))
            {
                _textUnderCursor = text;
                Console.WriteLine("_textUnderCursor: " + text);

                LoadGrammar(_textUnderCursor);

                _speechSynthesizer.SpeakAsync("Pronounce text under the cursor");
            }
        }

        private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _textUnderCursor = null;

            _speechSynthesizer.SpeakAsyncCancelAll();

            RestoreCalculator();
        }

        private ISpeechRecognizer _speechRecognizer;

        // Used in voice calculator.
        private int _sum = 0;

        // Used in recognizing text under the cusror.
        private string _textUnderCursor;

        private static List<string> s_numbers = new List<string>{
            "zero",
            "one",
            "two",
            "three",
            "four",
            "five",
            "six",
            "seven",
            "eight",
            "nine",
            "ten",
            "eleven",
            "twelve",
            "thirteen",
            "fourteen",
            "fifteen",
            "sixteen",
            "seventeen",
            "eighteen"
        };

        SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();
    }
}
