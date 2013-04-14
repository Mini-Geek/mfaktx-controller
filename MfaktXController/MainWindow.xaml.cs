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
        bool lastScreenSaverRunning = false;

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateController();
            if (Utilities.EnableIdleDetection && Utilities.ScreenSaverActive)
            {
                lastScreenSaverRunning = Utilities.ScreenSaverRunning;
                var timer = new Timer(Utilities.IdleDetectionInterval);
                timer.Elapsed += timer_Elapsed;
                timer.Start();
            }
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool screenSaverRunning = Utilities.ScreenSaverRunning;
            if (lastScreenSaverRunning != screenSaverRunning)
            {
                lastScreenSaverRunning = screenSaverRunning;
                if (controller.Status == MfaktXStatus.Running)
                {
                    if (screenSaverRunning)
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

        async void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (controller != null && controller.Status != MfaktXStatus.Stopped)
            {
                e.Cancel = true;
                await controller.Stop();
                this.Close();
            }
        }

        private void UpdateController()
        {
            controller = Controller.Instance;
            controller.PropertyChanged += controller_PropertyChanged;
            controller.DataReceived += controller_DataReceived;
            StatusTextBlock.DataContext = controller;
            UpdateButtonState();
            SetSpeed(Speed.Fast);
        }

        void controller_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    if (e.Data != null)
                    {
                        messages.Enqueue(e.Data);
                        if (messages.Count > Utilities.MaxLines)
                            messages.Dequeue();
                        OutputTextBox.Text = string.Join(Environment.NewLine, messages);
                    }
                    OutputTextBox.ScrollToEnd();
                });
        }

        void controller_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Status":
                case "CurrentSpeed":
                    UpdateButtonState();
                    break;
            }
        }

        private void UpdateButtonState()
        {
            Dispatcher.BeginInvoke(() =>
            {
                StopButton.IsEnabled = controller.Status == MfaktXStatus.Running;
                SlowButton.IsEnabled = controller.Status != MfaktXStatus.Stopping && (controller.Status != MfaktXStatus.Running || controller.CurrentSpeed != Speed.Slow);
                MediumButton.IsEnabled = controller.Status != MfaktXStatus.Stopping && (controller.Status != MfaktXStatus.Running || controller.CurrentSpeed != Speed.Medium);
                FastButton.IsEnabled = controller.Status != MfaktXStatus.Stopping && (controller.Status != MfaktXStatus.Running || controller.CurrentSpeed != Speed.Fast);
            });
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
    }
}
