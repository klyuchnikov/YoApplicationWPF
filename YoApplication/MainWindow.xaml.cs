using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.ComponentModel;
using System.Threading;

namespace YoApplication
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker bw = new BackgroundWorker { WorkerSupportsCancellation = true };
        private bool isExecuteble = false;
        private YoClass yo = new YoClass();
        public MainWindow()
        {
            InitializeComponent();
            /*   int Ret; 
               int Res; 
               string FontPath; 
               const int WM_FONTCHANGE = 0x001D; 
               const int HWND_BROADCAST = 0xffff;
               var fontFileName = "PFSquareSansPro-Thin.ttf";
               FontPath = Directory.GetCurrentDirectory() + @"\Fonts\" + fontFileName; 
               Ret = FontInstall.AddFontResource(FontPath); 
               Res = FontInstall.SendMessage(HWND_BROADCAST, WM_FONTCHANGE, 0, 0);
               Ret = FontInstall.WriteProfileString("fonts", "PF Square Sans Pro" + " (TrueType)", fontFileName);*/

        }

        private void Window_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                try
                {
                    this.DragMove();
                }
                catch (Exception ee) { }
        }

        private void ControlMin_Click(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void ControlMax_Click(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Maximized)
            {
                contrlMax.Visibility = System.Windows.Visibility.Visible;
                contrlNorm.Visibility = System.Windows.Visibility.Hidden;
                this.WindowState = System.Windows.WindowState.Normal;
            }
            else
            {
                contrlMax.Visibility = System.Windows.Visibility.Hidden;
                contrlNorm.Visibility = System.Windows.Visibility.Visible;
                this.WindowState = System.Windows.WindowState.Maximized;
            }
        }

        private void ControlCloce_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            rect.Rect = new Rect(0, 0, Width, Height);
        }

        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            rect.Rect = new Rect(0, 0, ActualWidth, ActualHeight);
        }

        private void Image_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://ulmic.ru");
        }

        private void StackPanel_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            isExecuteble = false;
            bw.DoWork += delegate
            {
                //Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

                try
                {
                    if (isExecuteble)
                        return;
                    var list = new List<Run>();
                    foreach (var block in rtb.Document.Blocks)
                        GetChildresRun(block, list);
                    int count = 0;
                    Dispatcher.Invoke(new Action(() =>
                    {
                        ReplaceButton.Visibility = System.Windows.Visibility.Hidden;
                        rtb.IsReadOnly = true;
                        prog.Width = 0;
                        progB.Visibility = System.Windows.Visibility.Visible;
                        ResMessage.Visibility = System.Windows.Visibility.Hidden;
                    }), DispatcherPriority.Normal, null);

                    int value = 0;
                    List<ExceptionView> listExceptions = new List<ExceptionView>();
                    foreach (var run in list)
                    {
                        List<ReplaceWord> wordsExceptions;
                        var ress = yo.IsContainsException(run.Text, out wordsExceptions);
                        var str = yo.PasteLetter(run.Text);
                        foreach (var a in wordsExceptions)
                        {
                            var arr = run.Text.GetBlocks();
                            for (int i = 0; i < arr.Length; i++)
                                if (arr[i].ToLower() == a.Word)
                                    arr[i] = string.Format("<Bold>{0}</Bold>", arr[i]);
                            listExceptions.Add(new ExceptionView { Word = a.Word, Index = a.Index, SourceRun = run, ViewString = string.Join("", arr) });
                        }
                        Dispatcher.Invoke(new Action(() =>
                        {
                            run.Text = str;
                            float res = (++value / (float)list.Count);
                            prog.Width = res * progB.Width;
                        }), DispatcherPriority.Normal, null);
                    }
                    Dispatcher.Invoke(new Action(() =>
                    {
                        exceptionsList.ItemsSource = listExceptions;
                        if (exceptionsGrid.Height == 0 && listExceptions.Count != 0)
                            (FindResource("excOpen") as Storyboard).Begin();
                        progB.Visibility = System.Windows.Visibility.Hidden;
                        ResMessage.Visibility = System.Windows.Visibility.Visible;
                        ResMessage.Text = string.Format("Заменено {0} слов.", count);
                        (FindResource("progressbar") as Storyboard).Begin();
                        rtb.IsReadOnly = false;
                    }), DispatcherPriority.Normal, null);
                    isExecuteble = true;
                }
                catch (Exception ee)
                {
                    MessageBox.Show(/*ee.Message*/"Возникла ошибка", "Ошибка!");
                    Dispatcher.Invoke(new Action(() =>
                    {
                        progB.Visibility = System.Windows.Visibility.Hidden;
                        ResMessage.Visibility = System.Windows.Visibility.Hidden;
                        ReplaceButton.Visibility = System.Windows.Visibility.Visible;
                        rtb.IsReadOnly = false;
                    }), DispatcherPriority.Normal, null);
                }
            };
            bw.RunWorkerAsync();
        }
        public class ExceptionView
        {
            public Run SourceRun { get; set; }
            public string ViewString { get; set; }
            public string Word { get; set; }
            public int Index { get; set; }
        }

        public void GetChildresRun(DependencyObject parent, List<Run> list)
        {
            foreach (DependencyObject child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is Run)
                    list.Add(child as Run);
                else
                    GetChildresRun(child, list);

            }
        }

        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                ReplaceButton.Visibility = System.Windows.Visibility.Visible;
            }));
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            (FindResource("excClose") as Storyboard).Begin();
        }

        private void replaceExceptionWord(object sender, RoutedEventArgs e)
        {
            var item = ((sender as Button).DataContext as ExceptionView);
            item.SourceRun.Text = yo.PasteLetterExceptions(item.SourceRun.Text, new ReplaceWord { Word = item.Word, Index = item.Index });
            (exceptionsList.ItemsSource as List<ExceptionView>).Remove(item);
            exceptionsList.ItemsSource = new List<ExceptionView>(exceptionsList.ItemsSource as List<ExceptionView>);
            if ((exceptionsList.ItemsSource as List<ExceptionView>).Count == 0)
                (FindResource("excClose") as Storyboard).Begin();
        }
    }


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
            string[] tokens = value.Split(' ');
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

    public static class StringExtentions
    {
        public static string[] GetBlocks(this string s)
        {
            string separators = " '.,!:;?\n\r\t()-\'\"";
            var list = new List<string>();
            for (var i = 0; i < s.Length; i++)
            {
                if (!separators.Contains(s[i]))
                {
                    var countCharNotSeparators = 0;
                    while (!separators.Contains(s[i + countCharNotSeparators]))
                    {
                        countCharNotSeparators++;
                        if (i + countCharNotSeparators >= s.Length)
                            break;
                    }
                    if (countCharNotSeparators != 0)
                        list.Add(s.Substring(i, countCharNotSeparators));
                    if (countCharNotSeparators != 0)
                        i += countCharNotSeparators - 1;
                }
                else
                {
                    var countCharSeparators = 0;
                    while (separators.Contains(s[i + countCharSeparators]))
                    {
                        countCharSeparators++;
                        if (i + countCharSeparators >= s.Length)
                            break;
                    }
                    if (countCharSeparators != 0)
                        list.Add(s.Substring(i, countCharSeparators));
                    if (countCharSeparators != 0)
                        i += countCharSeparators - 1;
                }
            }
            return list.ToArray(); ;
        }
    }
}
