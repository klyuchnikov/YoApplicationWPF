using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace YoApplication
{

    public static class FormattedTextBehavior
    {
        public static string GetFormattedText(DependencyObject obj)
        {
            return (string)obj.GetValue(FormattedTextProperty);
        }

        public static void SetFormattedText(DependencyObject obj, string value)
        {
            obj.SetValue(FormattedTextProperty, value);
        }

        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.RegisterAttached("FormattedText",
                                                typeof(string),
                                                typeof(FormattedTextBehavior),
                                                new UIPropertyMetadata("", FormattedTextChanged));

        private static void FormattedTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            string value = e.NewValue as string;
            string[] tokens = value.Split(" ,.%)(*&^$#!@-=".ToCharArray());
            foreach (string token in tokens)
            {
                if (token.StartsWith("<Bold>") && token.EndsWith("</Bold>"))
                {
                    textBlock.Inlines.Add(new Bold(new Run(token.Replace("<Bold>", "").Replace("</Bold>", "") + " ")));
                }
                else
                {
                    textBlock.Inlines.Add(new Run(token + " "));
                }
            }
        }
    }

}
