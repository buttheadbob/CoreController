using System;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace CoreController.Classes
{
    public sealed class LogicalProcessors
    {
        public int ID {get; set;}
        public int GroupRelativeProcessorNumber { get; set; }
        public string AffinityMask { get; set; }
        public int ProcessorGroup { get; set; }
        public int PhysicalProcessor { get; set; }
        public string GetButtonName => "Core " + ID;

        public SolidColorBrush Color
        {
            get
            {
                foreach (LogicalProcessors logicalProcessor in CoreControllerMain.Instance.Config.AllowedProcessors)
                {
                    if (logicalProcessor.ID == ID)
                        return new SolidColorBrush(Colors.Green);
                }
                return new SolidColorBrush(Colors.Firebrick);
            }
        }
    }
}