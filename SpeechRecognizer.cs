using System.Windows;
using System.Speech.Recognition;

namespace VoiceCalculator
{
    public interface ISpeechRecognizer
    {
        void LoadGrammar(string words);
    }

    public class SpeechRecognizer : ISpeechRecognizer
    {
        // 'SpeechRecognizer' singleton.
        public static ISpeechRecognizer Instance()
        {
            if (s_speechRecognizer == null)
            {
                s_speechRecognizer = new SpeechRecognizer();
            }

            return s_speechRecognizer;
        }

        public static void Destroy()
        {
            if (s_speechRecognizer != null)
            {
                var speechRecognizer = s_speechRecognizer._speechRecognizer;
                speechRecognizer.RecognizeAsyncStop();
                speechRecognizer.UnloadAllGrammars();
                speechRecognizer.Dispose();

                s_speechRecognizer = null;
            }
        }

        // Constructor is private, created via singleton 'Instance'.
        private SpeechRecognizer()
        {
            _speechRecognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));

            // Configure input to the speech recognizer.
            _speechRecognizer.SetInputToDefaultAudioDevice();

            // Add a handler for the speech recognized event.
            _speechRecognizer.SpeechRecognized += (object sender, SpeechRecognizedEventArgs e) =>
            {
                if (e.Result != null)
                {
                    if (Application.Current.MainWindow is UIUpdater uiUpdater)
                    {
                        var resultText = e.Result.Text;

                        // The thread in the callback is the same as 'MainWindow' thread, can be called without switching thread context.
                        uiUpdater.UpdateUI(resultText);
                    }
                }
            };
        }

        // Implements 'ISpeechRecognizer'.
        public void LoadGrammar(string words)
        {
            var grammarBuilder = new GrammarBuilder();
            grammarBuilder.Append(new Choices(new string[] { words }));
            var grammar = new Grammar(grammarBuilder);

            if (_speechRecognitionStarted)
            {
                // Wait for the RecognizerUpdateReached event
                _speechRecognizer.RecognizerUpdateReached += (_, _) => {
                    // Unload previous grammar.
                    _speechRecognizer.UnloadAllGrammars();

                    _speechRecognizer.LoadGrammar(grammar);
                };

                // Changing grammar via 'UnloadAllGrammars/LoadGrammar' should be executed
                // in callback 'RecognizerUpdateReached' above that is triggered by 'RequestRecognizerUpdate'.
                _speechRecognizer.RequestRecognizerUpdate();
            }
            else
            {
                _speechRecognitionStarted = true;

                _speechRecognizer.LoadGrammar(grammar);

                // A grammar should be loaded before calling 'RecognizeAsync' and it should be called once.
                _speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
        }

        private SpeechRecognitionEngine _speechRecognizer;
        private bool _speechRecognitionStarted = false;

        private static SpeechRecognizer s_speechRecognizer;
    }
}
