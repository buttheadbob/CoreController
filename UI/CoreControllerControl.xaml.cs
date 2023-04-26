using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using CoreController.Classes;

namespace CoreController.UI
{
    public partial class CoreControllerControl : UserControl
    {
        public ObservableCollection<LogicalProcessorsRaw> LogicalCores
        {
            get => CoreControllerMain.LogicalCores;
        }
        
        private CoreControllerMain Plugin { get; }

        private CoreControllerControl()
        {
            InitializeComponent();
        }

        public CoreControllerControl(CoreControllerMain plugin) : this()
        {
            Plugin = plugin;
            DataContext = this;
            dGridProcessors.ItemsSource = LogicalCores;
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            Plugin.Save();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button?.Tag == null) return;
            int ID = (int) button.Tag;
            LogicalProcessorsRaw processor = null;

            for (int index = CoreControllerMain.LogicalCores.Count - 1; index >= 0; index--)
            {
                if (CoreControllerMain.LogicalCores[index].ID != ID) continue;
                processor = CoreControllerMain.LogicalCores[index];
                break;
            }

            if (processor == null) return;

            bool found = false;
            for (int index = CoreControllerMain.Instance.Config.AllowedProcessors.Count - 1; index >= 0; index--)
            {
                if (CoreControllerMain.Instance.Config.AllowedProcessors[index].ID != ID) continue;
                if (CoreControllerMain.Instance.Config.AllowedProcessors.Count == 1) continue;
                CoreControllerMain.Instance.Config.AllowedProcessors.RemoveAt(index);
                dGridProcessors.Items.Refresh();
                found = true;
                break;
            }

            if (!found)
            {
                CoreControllerMain.Instance.Config.AllowedProcessors.Add(processor.ConvertToUnRaw());
                dGridProcessors.Items.Refresh();
            }
            CoreControllerMain.Instance.Save();
            CoreControllerMain.Instance.ResetAffinity();
        }
    }
}
