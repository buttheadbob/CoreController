using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CoreController;
using CoreController.Classes;

namespace UI
{
    public partial class PerPhysicalProcessor : UserControl
    {
        private ObservableCollection<LogicalProcessorRaw> LogicalCores => CoreControllerMain.LogicalCores;
        
        public PerPhysicalProcessor()
        {
            InitializeComponent();
            UpdateAffinity();
        }

        public void UpdateAffinity()
        {
            MainStackPanel.Children.Clear();
            IEnumerable<IGrouping<int, LogicalProcessorRaw>> groupedProcessors = LogicalCores.GroupBy(p => p.PhysicalProcessorID);
            foreach (IGrouping<int, LogicalProcessorRaw> group in groupedProcessors)
            {
                GroupBox groupBox = new GroupBox
                {
                    Header = $"Physical Processor {group.Key}",
                    Margin = new Thickness(5)
                };

                WrapPanel wrapPanel = new WrapPanel
                {
                    Orientation = Orientation.Horizontal
                };

                foreach (LogicalProcessorRaw processor in group)
                {
                    Button LP = new Button
                    {
                        Content = processor.ID,
                        Tag = processor.PID,
                        Width = 60,
                        Foreground = (SolidColorBrush) new BrushConverter().ConvertFrom("#FF27E915")
                    };
                    LP.Click += ActionController_ButtonBase_OnClick;
                    LP.Background = processor.Color;
                    LP.Margin = new Thickness(5);

                    wrapPanel.Children.Add(LP);
                }

                groupBox.Content = wrapPanel;
                MainStackPanel.Children.Add(groupBox);
            }
        }

        private void ActionController_ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            CoreManager.ButtonBase_OnClick(sender, e);
            UpdateAffinity();
            CoreControllerMain.Instance._control.easyWindow.UpdateAffinity();
            CoreControllerMain.Instance._control.pnn.UpdateAffinity();
        }
    }
}