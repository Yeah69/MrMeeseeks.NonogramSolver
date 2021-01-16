using MrMeeseeks.Extensions;
using MrMeeseeks.NonogramSolver.Model.PersistenceHook;
using MrMeeseeks.NonogramSolver.Model.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace MrMeeseeks.NonogramSolver.Persistence
{
    internal class LoadSaveSettings : ILoadSettings, ISaveSettings
    {
        private readonly Func<ISaveSettings, ISettings> _settingsFactory;
        private readonly string _filePath = Path.Combine(
            new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName ?? "", 
            "settings.json");
        
        public LoadSaveSettings(Func<ISaveSettings, ISettings> settingsFactory) => _settingsFactory = settingsFactory;

        public ISettings Load()
        {
            var settingsDto = File.Exists(_filePath) 
                ? JsonConvert.DeserializeObject<Settings>(File.ReadAllText(_filePath))
                : new Settings(null);
            var settings = _settingsFactory(this);
            settings.LastOpenFileName = settingsDto.LastOpenFileName;
            if (File.Exists(_filePath).Not())
                Save(settings);
            return settings;
        }

        public void Save(ISettings settings)
        {
            var settingsDto = new Settings(settings.LastOpenFileName);
            File.WriteAllText(_filePath, JsonConvert.SerializeObject(settingsDto));
        }

        private record Settings(string? LastOpenFileName);
    }
}