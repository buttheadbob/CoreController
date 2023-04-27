using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
            SetProcessorGrid();
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            Plugin.Save();
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
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
                //LogicalProcessorGrid.Items.Refresh();
                found = true;
                break;
            }

            if (!found)
            {
                CoreControllerMain.Instance.Config.AllowedProcessors.Add(processor.ConvertToUnRaw());
                //LogicalProcessorGrid.Items.Refresh();
            }
            CoreControllerMain.Instance.Save();
            CoreControllerMain.Instance.ResetAffinity();
            await SetProcessorGrid();
        }

        private Task SetProcessorGrid()
        {
            int rowSize = 10;
            int rowCount = (int)Math.Ceiling((double)LogicalCores.Count / rowSize);

            for (int i = 0; i < rowCount; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(1, GridUnitType.Star);
                LogicalProcessorGrid.RowDefinitions.Add(row);

                for (int j = 0; j < rowSize; j++)
                {
                    int index = (i * rowSize) + j;

                    if (index >= LogicalCores.Count)
                    {
                        break;
                    }

                    LogicalProcessorsRaw button = LogicalCores[index];

                    ColumnDefinition column = new ColumnDefinition();
                    column.Width = new GridLength(1, GridUnitType.Auto);
                    LogicalProcessorGrid.ColumnDefinitions.Add(column);

                    Button b = new Button();
                    b.Content = button.GetButtonName;
                    b.Tag = button.ID;
                    b.Width = 60;
                    b.Foreground = (SolidColorBrush) new BrushConverter().ConvertFrom("#FF27E915");
                    b.Click += ButtonBase_OnClick;
                    b.Background = button.Color;
                    b.Margin = new Thickness(5);
                    b.Style = (Style) FindResource("MyCoresButtonStyle");
                    Grid.SetRow(b, i);
                    Grid.SetColumn(b, j);
                    LogicalProcessorGrid.Children.Add(b);
                }
            }

            return Task.CompletedTask;
        }
    }
}
