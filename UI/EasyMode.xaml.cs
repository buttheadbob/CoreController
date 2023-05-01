using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CoreController;
using CoreController.Classes;

namespace UI
{
    public partial class EasyMode : UserControl
    {
        
        public ObservableCollection<LogicalProcessorRaw> LogicalCores => CoreControllerMain.LogicalCores;
        
        public EasyMode()
        {
            InitializeComponent();
            SetProcessorGrid();
        }
        
        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button?.Tag == null) return;
            int ID = (int) button.Tag;
            LogicalProcessorRaw processor = null;

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
            CoreManager.ResetAffinity();
            await SetProcessorGrid();
        }
        
        private Task SetProcessorGrid()
        {
            WrapPanel.Children.Clear();
            foreach (LogicalProcessorRaw core in LogicalCores)
            {
                Button LP = new Button
                {
                    Content = core.GetButtonName,
                    Tag = core.ID,
                    Width = 60,
                    Foreground = (SolidColorBrush) new BrushConverter().ConvertFrom("#FF27E915")
                };
                LP.Click += ButtonBase_OnClick;
                LP.Background = core.Color;
                LP.Margin = new Thickness(5);
                LP.Style = (Style) FindResource("MyCoresButtonStyle");

                WrapPanel.Children.Add(LP);
            }
            return Task.CompletedTask;
        }
    }
}