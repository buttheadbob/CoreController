using System.Collections.Generic;
using CoreController.Classes;
using Torch;

namespace CoreController
{
    public class CoreControllerConfig : ViewModel
    {
        private string _version = "";
        public string Version { get => _version; set => SetValue(ref _version, value); }
        
        private List<LogicalProcessors> _AllowedProcessors = new List<LogicalProcessors>();
        public List<LogicalProcessors> AllowedProcessors { get => _AllowedProcessors; set => SetValue(ref _AllowedProcessors, value); }
        
        
    }
}
