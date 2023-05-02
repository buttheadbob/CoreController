using System.Windows.Controls;
using UI;

namespace CoreController.UI
{
    public partial class CoreControllerControl : UserControl
    {
        private CoreControllerMain Plugin { get; }
        public EasyMode easyWindow = new EasyMode();
        public PerPhysicalProcessor ppp = new PerPhysicalProcessor();
        public NumaMode pnn = new NumaMode();
        
        private CoreControllerControl()
        {
            InitializeComponent();
        }

        public CoreControllerControl(CoreControllerMain plugin) : this()
        {
            Plugin = plugin;
            DataContext = this;
            EasyModeTab.Content = easyWindow;
            PerPhysicalProcessor.Content = ppp;
            PerNumaNode.Content = pnn;
        }
    }
}
