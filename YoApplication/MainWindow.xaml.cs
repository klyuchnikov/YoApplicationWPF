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
using System.Deployment.Application;

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
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 5, 0);
            timer.Tick += UpdateTick;
            timer.Start();
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
             {
                 MessageBox.Show((e.ExceptionObject as Exception).Message);
             };
        }

        private void UpdateTick(object sender, EventArgs e)
        {
            try
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                var info = ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate();
                if (info.UpdateAvailable)
                {
                    if (!info.IsUpdateRequired)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            IsUpdate.Content = "Доступна новая версия: " + info.AvailableVersion.ToString();
                            IsUpdate.Visibility = System.Windows.Visibility.Visible;
                            UpdateButton.Visibility = System.Windows.Visibility.Visible;
                        }), DispatcherPriority.Normal, null);
                    }
                }
                else
                    Dispatcher.Invoke(new Action(() =>
                    {
                        IsUpdate.Visibility = System.Windows.Visibility.Hidden;
                        UpdateButton.Visibility = System.Windows.Visibility.Hidden;
                    }), DispatcherPriority.Normal, null);
            }
            catch { }
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

        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetColumnWidth();
        }

        private void Image_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://ulmic.ru");
        }

        void Process()
        {
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
                    string text = null;
                    Dispatcher.Invoke(new Action(() =>
                    {
                        text = run.Text;
                    }), DispatcherPriority.Normal, null);
                    var ress = yo.IsContainsException(text, out wordsExceptions);
                    var str = yo.PasteLetter(text);
                    foreach (var a in wordsExceptions)
                    {
                        var arr = text.GetBlocks();
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
                    {
                        (FindResource("excOpen") as Storyboard).Begin();
                        SetColumnWidth();
                    }
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
                MessageBox.Show("Возникла ошибка", "Ошибка!");
                Dispatcher.Invoke(new Action(() =>
                {
                    progB.Visibility = System.Windows.Visibility.Hidden;
                    ResMessage.Visibility = System.Windows.Visibility.Hidden;
                    ReplaceButton.Visibility = System.Windows.Visibility.Visible;
                    rtb.IsReadOnly = false;
                }), DispatcherPriority.Normal, null);
            }
        }

        private void SetColumnWidth()
        {
            var width1 = (exceptionsList.View as GridView).Columns[1].ActualWidth;
            var width2 = (exceptionsList.View as GridView).Columns[2].ActualWidth;
            (exceptionsList.View as GridView).Columns[0].Width = exceptionsList.ActualWidth - width1 - width2 - 45;
        }

        private void StackPanel_MouseDown_1(object sender, RoutedEventArgs e)
        {
            isExecuteble = false;
            var tread = new Thread(Process);
            tread.SetApartmentState(ApartmentState.STA);
            tread.Start();
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

        private void UpdateButton_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ApplicationDeployment.CurrentDeployment.UpdateCompleted += delegate
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        IsUpdate.Content = "Программа обновлена. Требуется перегрузка.";
                        MessageBox.Show("Программа была обновлена. Ё-приложение будет перегружено...");
                        System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                        Application.Current.Shutdown();
                    }), DispatcherPriority.Normal, null);
                };
                IsUpdate.Content = "...Обновление";
                ApplicationDeployment.CurrentDeployment.UpdateAsync();
            }
            catch { }
        }
    }
}
