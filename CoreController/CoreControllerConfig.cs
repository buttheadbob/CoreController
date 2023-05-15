using System.Collections.Generic;
using CoreController.Classes;
using Torch;

namespace CoreController
{
    public class CoreControllerConfig : ViewModel
    {
        private bool _enabledTimer = true;
        public bool EnabledTimer { get => _enabledTimer; set => SetValue(ref _enabledTimer, value); }
        
        private int enforcementFrequency = 60; // In seconds
        public int EnforcementFrequency { get => enforcementFrequency; set => SetValue(ref enforcementFrequency, value); }
        
        private List<LogicalProcessors> _AllowedProcessors = new List<LogicalProcessors>();
        public List<LogicalProcessors> AllowedProcessors { get => _AllowedProcessors; set => SetValue(ref _AllowedProcessors, value); }
        
        
    }
}
