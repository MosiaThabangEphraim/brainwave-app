using System.ComponentModel;

namespace BrainWave.APP.Models
{
    public class BadgeModel : INotifyPropertyChanged
    {
        public int BadgeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Slogan { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int RequiredTasks { get; set; }
        public bool IsAchieved { get; set; }
        public Color BadgeColor { get; set; } = Colors.Gray;
        public Color StatusColor { get; set; } = Colors.Gray;
        public string StatusText { get; set; } = "Locked";

        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

