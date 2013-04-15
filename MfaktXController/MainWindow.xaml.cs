using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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

namespace MfaktXController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Controller controller = null;
        Queue<string> messages = new Queue<string>();
        bool lastScreenInactive = false;

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            controller = Controller.Instance;
            controller.PropertyChanged += controller_PropertyChanged;
            controller.DataReceived += controller_DataReceived;
            this.DataContext = controller;
            UpdateButtonState();
            if (Utilities.StartupSpeed != null)
                SetSpeed(Utilities.StartupSpeed.Value);

            if (Utilities.EnableIdleDetection)
            {
                lastScreenInactive = Utilities.ScreenInactive;
                var idleTimer = new Timer(Utilities.IdleDetectionInterval);
                idleTimer.Elapsed += idleTimer_Elapsed;
                idleTimer.Start();
            }

            if (Utilities.PauseWhileRunning.Any())
            {
                var pauseTimer = new Timer(500);
                pauseTimer.Elapsed += pauseTimer_Elapsed;
                pauseTimer.Start();
            }

            OutputTextBox.FontFamily = Utilities.OutputLogFontFamily;
        }

        void idleTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool screenInactive = Utilities.ScreenInactive;
            if (lastScreenInactive != screenInactive)
            {
                lastScreenInactive = screenInactive;
                if (controller.Status == MfaktXStatus.Running)
                {
                    if (screenInactive)
                    {
                        if (controller.CurrentSpeed != Speed.Fast)
                            SetSpeed(Speed.Fast);
                    }
                    else
                    {
                        if (controller.CurrentSpeed == Speed.Fast)
                            SetSpeed(Speed.Medium);
                    }
                }
            }
        }

        void pauseTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool pause = Utilities.PauseWhileRunning.Any(x => Process.GetProcessesByName(x).Any());
            if (pause && controller.Status == MfaktXStatus.Running)
            {
                controller.Paused = true;
                controller.Stop().Wait();
            }
            else if (!pause && controller.Paused && controller.Status == MfaktXStatus.Stopped)
            {
                controller.Paused = false;
                SetSpeed(controller.CurrentSpeed);
            }
        }

        async void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (controller != null && controller.Status != MfaktXStatus.Stopped)
            {
                e.Cancel = true;
                await controller.Stop();
                this.Close();
            }
        }

        void controller_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (Dispatcher.CheckAccess())
                ReceiveData(e);
            else
                Dispatcher.BeginInvoke(() => ReceiveData(e));
        }

        private void ReceiveData(DataReceivedEventArgs e)
        {
            if (e != null && e.Data != null)
            {
                messages.Enqueue(e.Data);
                if (messages.Count > Utilities.MaxLines)
                {
                    messages.Dequeue();
                }
            }

            if (FreezeCheckBox.IsChecked != true)
            {
                OutputTextBox.Text = string.Join(Environment.NewLine, messages);

                OutputTextBox.ScrollToEnd();
                OutputTextBox.CaretIndex = OutputTextBox.Text.Length;
            }
        }

        void controller_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Status":
                case "CurrentSpeed":
                    if (Dispatcher.CheckAccess())
                        UpdateButtonState();
                    else
                        Dispatcher.BeginInvoke(UpdateButtonState);
                    break;
                case "StatusText":
                    if (Dispatcher.CheckAccess())
                        UpdateStatus();
                    else
                        Dispatcher.BeginInvoke(UpdateStatus);
                    break;
            }
        }

        private void UpdateButtonState()
        {
            StopButton.IsEnabled = controller.Status == MfaktXStatus.Running;
            SlowButton.IsEnabled = controller.Status != MfaktXStatus.Stopping && (controller.Status != MfaktXStatus.Running || controller.CurrentSpeed != Speed.Slow);
            MediumButton.IsEnabled = controller.Status != MfaktXStatus.Stopping && (controller.Status != MfaktXStatus.Running || controller.CurrentSpeed != Speed.Medium);
            FastButton.IsEnabled = controller.Status != MfaktXStatus.Stopping && (controller.Status != MfaktXStatus.Running || controller.CurrentSpeed != Speed.Fast);
        }

        private void UpdateStatus()
        {
            this.Title = "MfaktX Controller" + Utilities.InstanceIdentifier.WithBeginningSpace() + " - " + controller.StatusText;
            this.StatusTextBlock.Text = Utilities.InstanceIdentifier.WithEndingSpace() + "Status: " + controller.StatusText;
        }

        private async void StopButton_Click(object sender , RoutedEventArgs e )
        {
            await controller.Stop();
        }

        private void SlowButton_Click(object sender, RoutedEventArgs e)
        {
            SetSpeed(Speed.Slow);
        }

        private void MediumButton_Click(object sender, RoutedEventArgs e)
        {
            SetSpeed(Speed.Medium);
        }

        private void FastButton_Click(object sender, RoutedEventArgs e)
        {
            SetSpeed(Speed.Fast);
        }

        private async void SetSpeed(Speed speed)
        {
            await controller.Start(speed);
        }

        private void FreezeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            controller_DataReceived(null, null);
        }
    }
}
