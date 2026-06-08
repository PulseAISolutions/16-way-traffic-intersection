using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TrafficIntersection.Models
{
    public enum LightState { Red, Yellow, Green }

    public class TrafficLight : INotifyPropertyChanged
    {
        private LightState _currentState;
        private int _timeRemaining;

        public int Id { get; set; }
        public int NodeId { get; set; }
        public string Direction { get; set; } = string.Empty;

        public LightState CurrentState
        {
            get => _currentState;
            set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    OnPropertyChanged();
                }
            }
        }

        public int TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                if (_timeRemaining != value)
                {
                    _timeRemaining = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
