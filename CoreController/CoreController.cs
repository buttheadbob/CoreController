using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Session;
using System.Management;
using System.Threading.Tasks;
using CoreController.UI;
using System.Diagnostics;
using System.Text;
using CoreController.Classes;

namespace CoreController
{
    public class CoreControllerMain : TorchPluginBase, IWpfPlugin
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly string CONFIG_FILE_NAME = "CoreControllerConfig.cfg";
        private bool firstrun;
        
        public static CoreControllerMain Instance;
        public static ObservableCollection<LogicalProcessorsRaw> LogicalCores = new ObservableCollection<LogicalProcessorsRaw>();

        private CoreControllerControl _control;
        public UserControl GetControl() => _control ?? (_control = new CoreControllerControl(this));

        private Persistent<CoreControllerConfig> _config;
        public CoreControllerConfig Config => _config?.Data;

        public override async void Init(ITorchBase torch)
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
            await GetLogicalCores();
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

        public Task GetLogicalCores()
        {
            // Get the number of logical processors
            int numberOfLogicalProcessors = Environment.ProcessorCount;

            // Display the list of logical processors
            for (int i = 0; i < numberOfLogicalProcessors; i++)
            {
                Process proc = Process.GetCurrentProcess();
                IntPtr affinityMask = proc.ProcessorAffinity;
                bool isOn = ((affinityMask.ToInt64() >> i) & 1) == 1;
            }

            for (int p = 0; p < numberOfLogicalProcessors; p++)
            {
                Process proc = Process.GetCurrentProcess();
                IntPtr affinityMask = proc.ProcessorAffinity;
                LogicalProcessorsRaw core = new LogicalProcessorsRaw { ID = p + 1, AffinityMask = affinityMask };
                LogicalCores.Add(core);
                if (firstrun)
                    Instance.Config.AllowedProcessors.Add(core.ConvertToUnRaw());
            }
            Save();
            ResetAffinity();

            return Task.CompletedTask;
        }
        
        public void ResetAffinity()
        {
            // Get the current process
            Process currentProcess = Process.GetCurrentProcess();
            long bitmask = 0;
            foreach (LogicalProcessors core in Config.AllowedProcessors)
            {
                bitmask |= (1L << (core.ID - 1));
            }
            currentProcess.ProcessorAffinity = (IntPtr)bitmask;
        }
    }
}
