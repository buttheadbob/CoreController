using System;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace CoreController.Classes
{
    public sealed class LogicalProcessorRaw
    {
        public string PID { get; set; }
        public int ID {get; set;}
        public IntPtr AffinityMask { get; set; }
        public int PhysicalProcessorID { get; set; }
        public int Node { get; set; }
        public string GetButtonName
        {
            get
            {
                return ID < 10 ? $"‎{ID}" : ID.ToString();
            }
        }

        public SolidColorBrush Color
        {
            get
            {
                for (int index = CoreControllerMain.Instance.Config.AllowedProcessors.Count - 1; index >= 0; index--)
                {
                    if (CoreControllerMain.Instance.Config.AllowedProcessors[index].ID == ID)
                        return new SolidColorBrush(Colors.Green);
                }

                return new SolidColorBrush(Colors.DarkSlateGray);
            }
        }

        public LogicalProcessors ConvertToUnRaw()
        {
            return new LogicalProcessors
            {
                ID = ID,
                AffinityMask = Marshal.PtrToStringAuto(AffinityMask)
            };
        }
    }
}