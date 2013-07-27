using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

namespace MfaktXController
{
    /// <summary>
    /// Interaction logic for ListProcesses.xaml
    /// </summary>
    public partial class ListProcesses : Window
    {
        public string SelectedProcess { get; set; }

        private IList<string> selectedProcesses;

        public ListProcesses(IEnumerable<string> selectedProcesses)
        {
            InitializeComponent();

            this.selectedProcesses = selectedProcesses.ToList();
            this.Refresh();
        }

        private void Refresh()
        {
            var allProcesses = Process.GetProcesses();
            var summaries = allProcesses.Select(x => new ProcessSummary(x.ProcessName, x.PrivateMemorySize64, !selectedProcesses.Contains(x.ProcessName, StringComparer.OrdinalIgnoreCase))).ToList();

            ProcessesDataGrid.ItemsSource = summaries;
            ICollectionView dataView = CollectionViewSource.GetDefaultView(ProcessesDataGrid.ItemsSource);
            dataView.SortDescriptions.Clear();
            dataView.SortDescriptions.Add(new SortDescription("MemoryMB", ListSortDirection.Descending));
            dataView.Refresh();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            var summary = (ProcessSummary)((FrameworkElement)sender).DataContext;
            SelectedProcess = summary.ProcessName;
            this.Close();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.Refresh();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class ProcessSummary
    {
        public bool NotAlreadySelected { get; private set; }
        public string ProcessName { get; private set; }
        public long MemoryBytes { get; private set; }
        public decimal MemoryMB
        {
            get
            {
                return (decimal)MemoryBytes / 1024 / 1024;
            }
        }

        public ProcessSummary(string processName, long memoryBytes, bool notAlreadySelected)
        {
            this.ProcessName = processName;
            this.MemoryBytes = memoryBytes;
            this.NotAlreadySelected = notAlreadySelected;
        }
    }
}
