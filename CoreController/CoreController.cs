using NLog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using System.Text;
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
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly string CONFIG_FILE_NAME = "CoreControllerConfig.cfg";
        public bool firstrun;
        public static CoreControllerMain Instance;
        public static ObservableCollection<LogicalProcessorRaw> LogicalCores = new ObservableCollection<LogicalProcessorRaw>();
        public bool readMessage = false;

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
                firstrun = true;
            var sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if (sessionManager != null)
                sessionManager.SessionStateChanged += SessionChanged;
            else
                Log.Warn("No session manager loaded!");
            Save();
            GetLogicalCores();
            NumaManager.UpdateNumaTopology();
            UpdateCheck.VerifyConfigVersion();
            
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

            Config.Version = UpdateCheck.minVersion;
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

    static class UpdateCheck
    {
        public static readonly string minVersion = "1,1,0,1";
        public static void VerifyConfigVersion()
        {
            if (!string.IsNullOrEmpty(CoreControllerMain.Instance.Config.Version)) return;
            if (CoreControllerMain.Instance.Config.Version == minVersion) return;
            
            int[] logicalCores = CoreControllerMain.Instance.Config.AllowedProcessors.Select(x => x.ID).ToArray();
            CoreControllerMain.Instance.Config.AllowedProcessors.Clear();

            foreach (var core in CoreControllerMain.LogicalCores)
            {
                if (logicalCores.Contains(core.ID))
                    CoreControllerMain.Instance.Config.AllowedProcessors.Add(core.ConvertToUnRaw());
            }
            CoreControllerMain.Instance.Save();
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
