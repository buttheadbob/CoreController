using System.Windows.Controls;
using UI;

namespace CoreController.UI
{
    public partial class CoreControllerControl : UserControl
    {
        private CoreControllerMain Plugin { get; }

        private CoreControllerControl()
        {
            InitializeComponent();
        }

        public CoreControllerControl(CoreControllerMain plugin) : this()
        {
            Plugin = plugin;
            DataContext = this;
            EasyModeTab.Content = new EasyMode();
        }
    }
}
