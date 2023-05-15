using System.Windows.Controls;
using CoreController;

namespace UI
{
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
            DataContext = CoreControllerMain.Instance.Config;
        }
    }
}