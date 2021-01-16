using MrMeeseeks.NonogramSolver.Model.PersistenceHook;
using MrMeeseeks.Windows;
using System.Runtime.CompilerServices;

namespace MrMeeseeks.NonogramSolver.Model.Settings
{
    public interface ISettings : IModelLayerBase
    {
        string? LastOpenFileName { get; set; }
    }
        
    internal class Settings : ModelLayerBase, ISettings
    {
        private readonly ISaveSettings _saveSettings;
        private string? _lastOpenFileName;

        public Settings(ISaveSettings saveSettings)
        {
            _saveSettings = saveSettings;
        }

        public string? LastOpenFileName
        {
            get => _lastOpenFileName;
            set => SetAndRaiseNotificationAndSaveIfChanged(ref _lastOpenFileName, value);
        }

        private void SetAndRaiseNotificationAndSaveIfChanged<T>(
            ref T field,
            T value,
            [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return;
            field = value;
            this.OnPropertyChanged(propertyName);
            _saveSettings.Save(this);
        }
    }
}