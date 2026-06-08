using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Timers;
using System.Collections.Specialized;
using System.Linq;
using TrafficIntersection.Logic;
using TrafficIntersection.Models;

namespace TrafficIntersection
{
    public partial class MainWindow : Window
    {
        private Intersection _intersection = null!;
        private Timer? _renderTimer;

        public MainWindow()
        {
            InitializeComponent();
            _intersection = new Intersection();
            DrawRoads();
            UpdateTrafficLightColors();
        }

        private void DrawRoads()
        {
            // Draw 2 Horizontal Roads
            for (int row = 0; row < 2; row++)
            {
                double centerY = 250 + row * 400;
                var hRoad = new Rectangle { Width = 1800, Height = 100, Fill = new SolidColorBrush(Color.FromRgb(44, 44, 44)) };
                Canvas.SetLeft(hRoad, -50); Canvas.SetTop(hRoad, centerY - 50);
                IntersectionCanvas.Children.Add(hRoad);

                // center dashed line
                IntersectionCanvas.Children.Add(new Line { X1 = -50, Y1 = centerY, X2 = 1750, Y2 = centerY, Stroke = Brushes.White, StrokeThickness = 2, StrokeDashArray = new DoubleCollection { 4, 4 } });
                
                // Edge lines
                IntersectionCanvas.Children.Add(new Line { X1 = -50, Y1 = centerY - 50, X2 = 1750, Y2 = centerY - 50, Stroke = Brushes.White, StrokeThickness = 2 });
                IntersectionCanvas.Children.Add(new Line { X1 = -50, Y1 = centerY + 50, X2 = 1750, Y2 = centerY + 50, Stroke = Brushes.White, StrokeThickness = 2 });
            }

            // Draw 4 Vertical Roads
            for (int col = 0; col < 4; col++)
            {
                double centerX = 250 + col * 400;
                var vRoad = new Rectangle { Width = 100, Height = 1000, Fill = new SolidColorBrush(Color.FromRgb(44, 44, 44)) };
                Canvas.SetLeft(vRoad, centerX - 50); Canvas.SetTop(vRoad, -50);
                IntersectionCanvas.Children.Add(vRoad);

                // center dashed line
                IntersectionCanvas.Children.Add(new Line { X1 = centerX, Y1 = -50, X2 = centerX, Y2 = 950, Stroke = Brushes.White, StrokeThickness = 2, StrokeDashArray = new DoubleCollection { 4, 4 } });

                // Edge lines
                IntersectionCanvas.Children.Add(new Line { X1 = centerX - 50, Y1 = -50, X2 = centerX - 50, Y2 = 950, Stroke = Brushes.White, StrokeThickness = 2 });
                IntersectionCanvas.Children.Add(new Line { X1 = centerX + 50, Y1 = -50, X2 = centerX + 50, Y2 = 950, Stroke = Brushes.White, StrokeThickness = 2 });
            }

            // Draw intersections (to cover the crossed lines) and lights
            foreach (var node in _intersection.Nodes)
            {
                var centerBox = new Rectangle { Width = 98, Height = 98, Fill = new SolidColorBrush(Color.FromRgb(44, 44, 44)) };
                Canvas.SetLeft(centerBox, node.CenterX - 49);
                Canvas.SetTop(centerBox, node.CenterY - 49);
                IntersectionCanvas.Children.Add(centerBox);

                // Stop lines
                IntersectionCanvas.Children.Add(new Line { X1 = node.CenterX, Y1 = node.CenterY + 50, X2 = node.CenterX + 50, Y2 = node.CenterY + 50, Stroke = Brushes.White, StrokeThickness = 4 }); // Northbound
                IntersectionCanvas.Children.Add(new Line { X1 = node.CenterX - 50, Y1 = node.CenterY - 50, X2 = node.CenterX, Y2 = node.CenterY - 50, Stroke = Brushes.White, StrokeThickness = 4 }); // Southbound
                IntersectionCanvas.Children.Add(new Line { X1 = node.CenterX - 50, Y1 = node.CenterY, X2 = node.CenterX - 50, Y2 = node.CenterY + 50, Stroke = Brushes.White, StrokeThickness = 4 }); // Eastbound
                IntersectionCanvas.Children.Add(new Line { X1 = node.CenterX + 50, Y1 = node.CenterY - 50, X2 = node.CenterX + 50, Y2 = node.CenterY, Stroke = Brushes.White, StrokeThickness = 4 }); // Westbound

                CreateLightHousing($"Light{node.Id}_North", node.CenterX + 60, node.CenterY - 120, false);
                CreateLightHousing($"Light{node.Id}_South", node.CenterX - 84, node.CenterY + 55, false);
                CreateLightHousing($"Light{node.Id}_East", node.CenterX + 60, node.CenterY + 60, true);
                CreateLightHousing($"Light{node.Id}_West", node.CenterX - 125, node.CenterY - 84, true);
            }
        }

        private void CreateLightHousing(string namePrefix, double x, double y, bool horizontal)
        {
            var border = new Border
            {
                Width = horizontal ? 64 : 24,
                Height = horizontal ? 24 : 64,
                Background = new SolidColorBrush(Color.FromRgb(17, 17, 17)),
                CornerRadius = new CornerRadius(4),
                BorderBrush = new SolidColorBrush(Color.FromRgb(85, 85, 85)),
                BorderThickness = new Thickness(1)
            };

            var stack = new StackPanel
            {
                Orientation = horizontal ? Orientation.Horizontal : Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var red = new Ellipse { Name = $"{namePrefix}_Red", Width = 14, Height = 14, Margin = horizontal ? new Thickness(2,0,2,0) : new Thickness(0,2,0,2) };
            var yellow = new Ellipse { Name = $"{namePrefix}_Yellow", Width = 14, Height = 14, Margin = horizontal ? new Thickness(2,0,2,0) : new Thickness(0,2,0,2) };
            var green = new Ellipse { Name = $"{namePrefix}_Green", Width = 14, Height = 14, Margin = horizontal ? new Thickness(2,0,2,0) : new Thickness(0,2,0,2) };

            RegisterName(red.Name, red);
            RegisterName(yellow.Name, yellow);
            RegisterName(green.Name, green);

            stack.Children.Add(red);
            stack.Children.Add(yellow);
            stack.Children.Add(green);
            border.Child = stack;

            Canvas.SetLeft(border, x);
            Canvas.SetTop(border, y);
            IntersectionCanvas.Children.Add(border);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _intersection.Start();
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            ((INotifyCollectionChanged)_intersection.Vehicles).CollectionChanged += Vehicles_CollectionChanged;

            _renderTimer = new Timer(50);
            _renderTimer.Elapsed += RenderUpdate;
            _renderTimer.Start();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _intersection.Stop();
            _renderTimer?.Stop();
            _renderTimer?.Dispose();

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;

            var vehiclesToRemove = IntersectionCanvas.Children.OfType<Grid>()
                .Where(el => el.Tag?.ToString() == "VehicleContainer").ToList();

            foreach (var element in vehiclesToRemove)
            {
                IntersectionCanvas.Children.Remove(element);
            }
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            if (StartButton.IsEnabled == false) StopButton_Click(sender, e);
            _intersection.Vehicles.Clear();
            StartButton_Click(sender, e);
        }

        private void Vehicles_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                VehicleCountText.Text = _intersection.Vehicles.Count.ToString();
            });
        }

        private void RenderUpdate(object? sender, ElapsedEventArgs e)
        {
            _intersection.UpdatePositions();

            Dispatcher.Invoke(() =>
            {
                var vehiclesToRemove = IntersectionCanvas.Children.OfType<Grid>()
                    .Where(el => el.Tag?.ToString() == "VehicleContainer").ToList();

                foreach (var element in vehiclesToRemove)
                {
                    IntersectionCanvas.Children.Remove(element);
                }

                foreach (var vehicle in _intersection.Vehicles)
                {
                    var carContainer = new Grid
                    {
                        Width = 36,
                        Height = 18,
                        Tag = "VehicleContainer"
                    };

                    var car = new Rectangle
                    {
                        Width = 36,
                        Height = 18,
                        RadiusX = 4,
                        RadiusY = 4,
                        Fill = GetVehicleColor(vehicle.Color),
                        Stroke = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)),
                        StrokeThickness = 1
                    };
                    carContainer.Children.Add(car);

                    var windshield = new Rectangle
                    {
                        Width = 6,
                        Height = 14,
                        Fill = new SolidColorBrush(Color.FromArgb(200, 200, 230, 255)),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Margin = new Thickness(0, 0, 8, 0),
                        RadiusX = 2,
                        RadiusY = 2
                    };
                    carContainer.Children.Add(windshield);

                    // Add blinkers (restored from previous implementation, no headlights)
                    if (!vehicle.HasCompletedTurn && vehicle.IntendedTurn != TurnDirection.Straight)
                    {
                        long currentMillis = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        if ((currentMillis / 400) % 2 == 0)
                        {
                            var indicator = new Ellipse
                            {
                                Width = 8,
                                Height = 8,
                                Fill = new SolidColorBrush(Colors.Yellow),
                                Stroke = new SolidColorBrush(Colors.DarkOrange),
                                StrokeThickness = 1,
                                HorizontalAlignment = HorizontalAlignment.Right,
                                VerticalAlignment = vehicle.IntendedTurn == TurnDirection.Left ? VerticalAlignment.Top : VerticalAlignment.Bottom,
                                Margin = new Thickness(0, vehicle.IntendedTurn == TurnDirection.Left ? -2 : 0, -2, vehicle.IntendedTurn == TurnDirection.Right ? -2 : 0)
                            };
                            carContainer.Children.Add(indicator);
                        }
                    }

                    carContainer.RenderTransform = new RotateTransform(vehicle.Angle, 18, 9);
                    Canvas.SetLeft(carContainer, vehicle.X - 18);
                    Canvas.SetTop(carContainer, vehicle.Y - 9);

                    IntersectionCanvas.Children.Add(carContainer);
                }

                UpdateTrafficLightColors();
            });
        }

        private SolidColorBrush GetVehicleColor(string colorName)
        {
            return colorName switch
            {
                "Blue" => new SolidColorBrush(Color.FromRgb(59, 130, 246)), 
                "Red" => new SolidColorBrush(Color.FromRgb(239, 68, 68)),  
                "Green" => new SolidColorBrush(Color.FromRgb(16, 185, 129)),  
                "Orange" => new SolidColorBrush(Color.FromRgb(245, 158, 11)),  
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        private void UpdateTrafficLightColors()
        {
            foreach (var node in _intersection.Nodes)
            {
                foreach (var light in node.Lights)
                {
                    SetLightHousing(node.Id, light.Direction, light.CurrentState);
                }
            }
        }

        private void SetLightHousing(int nodeId, string direction, LightState state)
        {
            var redBulb = (Ellipse)FindName($"Light{nodeId}_{direction}_Red");
            var yellowBulb = (Ellipse)FindName($"Light{nodeId}_{direction}_Yellow");
            var greenBulb = (Ellipse)FindName($"Light{nodeId}_{direction}_Green");

            if (redBulb == null || yellowBulb == null || greenBulb == null) return;

            redBulb.Fill = new SolidColorBrush(Color.FromRgb(51, 0, 0));
            yellowBulb.Fill = new SolidColorBrush(Color.FromRgb(51, 51, 0));
            greenBulb.Fill = new SolidColorBrush(Color.FromRgb(0, 51, 0));

            switch (state)
            {
                case LightState.Red:
                    redBulb.Fill = new SolidColorBrush(Colors.Red);
                    break;
                case LightState.Yellow:
                    yellowBulb.Fill = new SolidColorBrush(Colors.Yellow);
                    break;
                case LightState.Green:
                    greenBulb.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                    break;
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Feature removed by request
        }
    }
}
