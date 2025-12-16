using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VoiceCalculator
{
    public static class VisualTreeHelpers
    {
        /// Finds the first parent of a specific type
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            if (parent == null)
                return null;

            if (parent is T result)
                return result;

            return FindParent<T>(parent);
        }

        /// Gets text from any element, handling TextBoxView
        public static string GetTextFromElement(DependencyObject element)
        {
            if (element == null)
                return null;

            // If it's a TextBoxView, find the parent TextBox
            if (element.GetType().Name == "TextBoxView")
            {
                TextBox textBox = FindParent<TextBox>(element);
                return textBox?.Text;
            }

            // Handle direct TextBox
            if (element is TextBox tb)
            {
                return tb.Text;
            }

            // Handle other text controls
            if (element is TextBlock textBlock)
            {
                return textBlock.Text;
            }

            if (element is ContentControl contentControl)
            {
                return contentControl.Content?.ToString();
            }

            return null;
        }
    }
}
