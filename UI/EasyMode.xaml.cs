using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CoreController;
using CoreController.Classes;

namespace UI
{
    public partial class EasyMode : UserControl
    {
        private ObservableCollection<LogicalProcessorRaw> LogicalCores => CoreControllerMain.LogicalCores;
        
        public EasyMode()
        {
            InitializeComponent();
            UpdateAffinity();
        }
        
        public void UpdateAffinity()
        {
            WrapPanel.Children.Clear();
            foreach (LogicalProcessorRaw core in LogicalCores)
            {
                Button LP = new Button
                {
                    Content = core.GetButtonName,
                    Tag = core.PID,
                    Width = 60,
                    Foreground = (SolidColorBrush) new BrushConverter().ConvertFrom("#FF27E915")
                };
                LP.Click += ActionController_ButtonBase_OnClick;
                LP.Background = core.Color;
                LP.Margin = new Thickness(5);

                WrapPanel.Children.Add(LP);
            }
        }
        
        private void ActionController_ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            CoreManager.ButtonBase_OnClick(sender, e);
            UpdateAffinity();
            CoreControllerMain.Instance._control.ppp.UpdateAffinity();
            CoreControllerMain.Instance._control.pnn.UpdateAffinity();
        }
    }
}