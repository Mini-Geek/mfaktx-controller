using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace MfaktXController
{
    /// <summary>
    /// Interaction logic for ManageProcessListWindow.xaml
    /// </summary>
    public partial class ManageProcessListWindow : Window
    {
        readonly ObservableCollection<string> processes = null;
        bool isSlow;
        bool isDirty;
        ObservableCollection<ProcessSummary> summaries = new ObservableCollection<ProcessSummary>();

        public ManageProcessListWindow(bool isSlow)
        {
            InitializeComponent();

            this.isSlow = isSlow;
            this.processes = new ObservableCollection<string>(isSlow ? Utilities.SlowWhileRunning : Utilities.PauseWhileRunning);
            this.processes.CollectionChanged += (s, e) => isDirty = true;
            this.ProcessDataGrid.ItemsSource = processes;
            this.Title = "Manage " + (isSlow ? "Slow" : "Pause") + " While Running List - MfaktX Controller";
            this.Closing += ManageProcessListWindow_Closing;
            this.Refresh();
        }

        void ManageProcessListWindow_Closing(object sender, CancelEventArgs e)
        {
            if (isDirty)
            {
                var result = MessageBox.Show("You have unsaved changes. Would you like to save these changes?", "Unsaved Changes", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                    Save();
                else if (result == MessageBoxResult.Cancel)
                    e.Cancel = true;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
            this.Close();
        }

        private void Save()
        {
            string settingName = isSlow ? "SlowWhileRunning" : "PauseWhileRunning";
            string newValue = string.Join(",", this.processes);

            Configuration config = ConfigurationManager.OpenExeConfiguration(Assembly.GetEntryAssembly().Location);
            var xdoc = XDocument.Load(config.FilePath, LoadOptions.PreserveWhitespace);
            XNamespace ns = "";
            var appSettings = xdoc.Element(ns + "configuration").Element(ns + "appSettings");
            var whileRunningElement = appSettings.Elements(ns + "add").First(x => x.Attribute(ns + "key").Value == settingName);
            whileRunningElement.Attribute(ns + "value").Value = newValue;
            xdoc.Save(config.FilePath);

            ConfigurationManager.RefreshSection("appSettings");
            isDirty = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            isDirty = false;
            this.Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var processName = (string)((FrameworkElement)sender).DataContext;
            processes.Remove(processName);
            UpdateSummary(processName, false);
        }

        private void ManualAddButton_Click(object sender, RoutedEventArgs e)
        {
            var processName = NewProcessTextBox.Text;
            if (!string.IsNullOrWhiteSpace(processName))
            {
                processes.Add(processName);
                NewProcessTextBox.Text = "";
                UpdateSummary(processName, true);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.Refresh();
        }

        private void Refresh()
        {
            var allProcesses = Process.GetProcesses();
            summaries = allProcesses.Select(x => new ProcessSummary(x.ProcessName, x.PrivateMemorySize64, processes.Contains(x.ProcessName, StringComparer.OrdinalIgnoreCase))).ToObservableCollection();

            ProcessesDataGrid.ItemsSource = summaries;
            ICollectionView dataView = CollectionViewSource.GetDefaultView(ProcessesDataGrid.ItemsSource);
            dataView.SortDescriptions.Clear();
            dataView.SortDescriptions.Add(new SortDescription("MemoryMB", ListSortDirection.Descending));
            dataView.Refresh();
        }

        private void AddProcessButton_Click(object sender, RoutedEventArgs e)
        {
            var summary = (ProcessSummary)((FrameworkElement)sender).DataContext;
            processes.Add(summary.ProcessName);
            UpdateSummary(summary.ProcessName, true);
        }

        void UpdateSummary(string processName, bool isSelected)
        {
            foreach (var summary in summaries.Where(x => string.Equals(x.ProcessName, processName, StringComparison.OrdinalIgnoreCase)))
            {
                summary.IsSelected = isSelected;
            }
        }
    }

    public class ProcessSummary : INotifyPropertyChanged
    {
#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

        public bool IsSelected { get; set; }
        public bool IsNotSelected { get { return !IsSelected; } }
        public string ProcessName { get; private set; }
        public long MemoryBytes { get; private set; }
        public decimal MemoryMB
        {
            get
            {
                return (decimal)MemoryBytes / 1024 / 1024;
            }
        }

        public ProcessSummary(string processName, long memoryBytes, bool isSelected)
        {
            this.ProcessName = processName;
            this.MemoryBytes = memoryBytes;
            this.IsSelected = isSelected;
        }
    }
}
