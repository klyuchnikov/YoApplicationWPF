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
        public MainWindow()
        {
            InitializeComponent();
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
                try
                {
                    if (isExecuteble)
                        return;
                    var list = new List<Run>();
                    foreach (var block in rtb.Document.Blocks)
                        GetChildresRun(block, list);
                    double TotalMilliseconds = 0;
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
                    foreach (var run in list)
                    {
                        //  Properties.Resources.bases_constu
                        var basesE = Properties.Resources.basesE_constu.Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();// File.ReadAllLines(@"Resource/basesE.constu", Encoding.Default).ToList();
                        var bases = Properties.Resources.bases_constu.Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList(); // File.ReadAllLines(@"Resource/bases.constu", Encoding.Default).ToList();
                        var chars = "йцукенгшщзхъфывапролджэячсмитьбю";
                        var qwe = chars.Select(a => new { Letter = a, Arr = new List<string>() });
                        var letters = new List<string>[32];
                        var lettersE = new List<string>[32];
                        for (var i = 0; i < 32; i++)
                        {
                            letters[i] = new List<string>();
                            lettersE[i] = new List<string>();
                        }
                        for (var i = 0; i < bases.Count; i++)
                        {
                            var ind = chars.ToList().IndexOf(bases[i][0]);
                            letters[ind].Add(bases[i]);
                            lettersE[ind].Add(basesE[i]);
                        }
                        var arr = run.Text.GetBlocks().ToList();//Split(separators.ToCharArray());
                        var start = DateTime.Now;
                        for (var i = 0; i < arr.Count; i++)
                        {
                            if (chars.Contains(arr[i].ToLower()[0]))
                            {
                                var ind = 0;
                                try
                                {
                                    ind = letters[chars.ToList().IndexOf(arr[i].ToLower()[0])].IndexOf(arr[i].ToLower());
                                }
                                catch (Exception ee)
                                { }
                                if (ind != -1)
                                {
                                    arr[i] = lettersE[chars.ToList().IndexOf(arr[i].ToLower()[0])][ind];
                                    count++;
                                }
                            }
                        }
                        var end = DateTime.Now;
                        TotalMilliseconds += (end - start).TotalMilliseconds;
                        var str = string.Join("", arr.ToArray());
                        Dispatcher.Invoke(new Action(() =>
                        {
                            run.Text = str;
                            float res = (++value / (float)list.Count);
                            prog.Width = res * progB.Width;
                        }), DispatcherPriority.Normal, null);
                    }
                    Dispatcher.Invoke(new Action(() =>
                    {
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
