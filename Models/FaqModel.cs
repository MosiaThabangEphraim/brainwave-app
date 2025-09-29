using System.ComponentModel;

namespace BrainWave.APP.Models
{
    public class FaqModel : INotifyPropertyChanged
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}








