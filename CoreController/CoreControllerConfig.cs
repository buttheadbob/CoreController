using System.Collections.Generic;
using CoreController.Classes;
using Torch;

namespace CoreController
{
    public class CoreControllerConfig : ViewModel
    {
        private bool _enable;
        public bool Enable { get => _enable; set => SetValue(ref _enable, value); }
        
        private List<LogicalProcessors> _AllowedProcessors = new List<LogicalProcessors>();
        public List<LogicalProcessors> AllowedProcessors { get => _AllowedProcessors; set => SetValue(ref _AllowedProcessors, value); }
        
        
    }
}
