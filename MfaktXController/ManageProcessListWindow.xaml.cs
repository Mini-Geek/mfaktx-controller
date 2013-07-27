using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
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
        ObservableCollection<string> processes = null;
        bool isSlow;

        public ManageProcessListWindow(bool isSlow)
        {
            InitializeComponent();

            this.isSlow = isSlow;
            this.processes = new ObservableCollection<string>(isSlow ? Utilities.SlowWhileRunning : Utilities.PauseWhileRunning);
            this.ProcessDataGrid.ItemsSource = processes;
            this.Title = "Manage " + (isSlow ? "Slow" : "Pause") + " While Running List - MfaktX Controller";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
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
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var processName = (string)((FrameworkElement)sender).DataContext;
            processes.Remove(processName);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var processName = NewProcessTextBox.Text;
            if (!string.IsNullOrWhiteSpace(processName))
            {
                processes.Add(processName);
                NewProcessTextBox.Text = "";
            }
        }

        private void ListButton_Click(object sender, RoutedEventArgs e)
        {
            var listProcesses = new ListProcesses(processes);
            listProcesses.ShowDialog();
            if (!string.IsNullOrEmpty(listProcesses.SelectedProcess))
                processes.Add(listProcesses.SelectedProcess);
        }
    }
}
