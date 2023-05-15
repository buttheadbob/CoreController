using NLog;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Session;
using CoreController.UI;
using CoreController.Classes;
using static CoreController.CoreManager;

namespace CoreController
{
    public class CoreControllerMain : TorchPluginBase, IWpfPlugin
    {
        public static readonly Logger Log = LogManager.GetLogger("CoreController");
        private static readonly string CONFIG_FILE_NAME = "CoreControllerConfig.cfg";
        public bool firstrun;
        public static CoreControllerMain Instance;
        public static ObservableCollection<LogicalProcessorRaw> LogicalCores = new ObservableCollection<LogicalProcessorRaw>();
        private Timer _enforcementTimer = new Timer();

        public CoreControllerControl _control;
        public UserControl GetControl() => _control ?? (_control = new CoreControllerControl(this));

        private Persistent<CoreControllerConfig> _config;
        public CoreControllerConfig Config => _config?.Data;

        public override void Init(ITorchBase torch)
        {
            Instance = this;
            base.Init(torch);
            SetupConfig();
            if (!Config.AllowedProcessors.Any())
            {
                firstrun = true;
                Log.Warn("This is either your first time running CoreController or you have removed all allowed processors.  Resetting Processors to all allowed.");
            }
            TorchSessionManager sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if (sessionManager != null)
                sessionManager.SessionStateChanged += SessionChanged;
            else
                Log.Warn("No session manager loaded!");
            Save();
            GetLogicalCores();
            NumaManager.UpdateNumaTopology();
            _enforcementTimer.Elapsed += EnforcementTimerOnElapsed;
            _enforcementTimer.Interval = Config.EnforcementFrequency * 1000;
            _enforcementTimer.Start();
        }

        private void EnforcementTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (!Config.EnabledTimer) return;
            Process currentProcess = Process.GetCurrentProcess();
            long bitmask = 0;

            for (int index = Instance.Config.AllowedProcessors.Count - 1; index >= 0; index--)
            {
                bitmask |= (1L << Instance.Config.AllowedProcessors[index].ID);
            }

            currentProcess.ProcessorAffinity = (IntPtr) bitmask;
        }

        private void SessionChanged(ITorchSession session, TorchSessionState state)
        {
            switch (state)
            {
                case TorchSessionState.Loaded:
                    Log.Info("Session Loaded!");
                    break;

                case TorchSessionState.Unloading:
                    Log.Info("Session Unloading!");
                    break;
            }
        }

        private void SetupConfig()
        {
            var configFile = Path.Combine(StoragePath, CONFIG_FILE_NAME);

            try
            {
                _config = Persistent<CoreControllerConfig>.Load(configFile);
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }

            if (_config?.Data == null)
            {
                Log.Info("Create Default Config, because none was found!");
                firstrun = true;
                _config = new Persistent<CoreControllerConfig>(configFile, new CoreControllerConfig());
                _config.Save();
            }
        }

        public void Save()
        {
            try
            {
                _config.Save();
                Log.Info("Configuration Saved.");
            }
            catch (IOException e)
            {
                Log.Warn(e, "Configuration failed to save");
            }
        } 
    }
    
    public static class ObjectPrinter
    {
        public static StringBuilder PrintObjectProperties(object obj)
        {
            StringBuilder debug = new StringBuilder();
            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(obj);
                debug.AppendLine($"{property.Name}: {value}");
            }
            return debug;
        }
    }
}
