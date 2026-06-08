using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TrafficIntersection.Models
{
    public enum TurnDirection { Straight, Left, Right }

    public class Vehicle : INotifyPropertyChanged
    {
        private double _x;
        private double _y;
        private string _direction;
        private bool _isMoving;
        private double _angle;

        public int Id { get; set; }
        public string Color { get; set; } = "Blue";
        public TurnDirection IntendedTurn { get; set; } = TurnDirection.Straight;
        public double Speed { get; set; } = 4.0;
        public double MaxSpeed { get; set; } = 4.0;
        public bool HasCompletedTurn { get; set; } = false;

        public double Angle
        {
            get => _angle;
            set
            {
                if (_angle != value)
                {
                    _angle = value;
                    OnPropertyChanged();
                }
            }
        }

        public double X
        {
            get => _x;
            set
            {
                if (_x != value)
                {
                    _x = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Y
        {
            get => _y;
            set
            {
                if (_y != value)
                {
                    _y = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Direction
        {
            get => _direction;
            set
            {
                if (_direction != value)
                {
                    _direction = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsMoving
        {
            get => _isMoving;
            set
            {
                if (_isMoving != value)
                {
                    _isMoving = value;
                    OnPropertyChanged();
                }
            }
        }

        public Vehicle()
        {
            _direction = "North";
            _isMoving = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
