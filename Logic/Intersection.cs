using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using TrafficIntersection.Models;

namespace TrafficIntersection.Logic
{
    public class TrafficNode
    {
        public int Id { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public TrafficLight[] Lights { get; set; } = new TrafficLight[4];
        public int CyclePhase { get; set; }
    }

    public class Intersection
    {
        private const int GREEN_TIME = 20;
        private const int YELLOW_TIME = 3;
        private const int RED_TIME = 23;

        private readonly Timer _timer;
        private Random _random = new();

        public ObservableCollection<TrafficNode> Nodes { get; }
        public ObservableCollection<TrafficLight> Lights { get; }
        public ObservableCollection<Vehicle> Vehicles { get; }

        public Intersection()
        {
            Nodes = new ObservableCollection<TrafficNode>();
            Lights = new ObservableCollection<TrafficLight>();
            Vehicles = new ObservableCollection<Vehicle>();

            int id = 0;
            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    var node = new TrafficNode
                    {
                        Id = id,
                        CenterX = 250 + col * 400,
                        CenterY = 250 + row * 400,
                        CyclePhase = (row + col) % 2
                    };

                    node.Lights[0] = new TrafficLight { Id = id * 4 + 0, Direction = "North", NodeId = id };
                    node.Lights[1] = new TrafficLight { Id = id * 4 + 1, Direction = "South", NodeId = id };
                    node.Lights[2] = new TrafficLight { Id = id * 4 + 2, Direction = "East", NodeId = id };
                    node.Lights[3] = new TrafficLight { Id = id * 4 + 3, Direction = "West", NodeId = id };

                    if (node.CyclePhase == 0)
                    {
                        node.Lights[0].CurrentState = LightState.Green; node.Lights[0].TimeRemaining = GREEN_TIME;
                        node.Lights[1].CurrentState = LightState.Green; node.Lights[1].TimeRemaining = GREEN_TIME;
                        node.Lights[2].CurrentState = LightState.Red; node.Lights[2].TimeRemaining = RED_TIME;
                        node.Lights[3].CurrentState = LightState.Red; node.Lights[3].TimeRemaining = RED_TIME;
                    }
                    else
                    {
                        node.Lights[0].CurrentState = LightState.Red; node.Lights[0].TimeRemaining = RED_TIME;
                        node.Lights[1].CurrentState = LightState.Red; node.Lights[1].TimeRemaining = RED_TIME;
                        node.Lights[2].CurrentState = LightState.Green; node.Lights[2].TimeRemaining = GREEN_TIME;
                        node.Lights[3].CurrentState = LightState.Green; node.Lights[3].TimeRemaining = GREEN_TIME;
                    }

                    Nodes.Add(node);
                    foreach (var l in node.Lights) Lights.Add(l);
                    id++;
                }
            }

            _timer = new Timer(1000);
            _timer.Elapsed += UpdateCycle;
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void UpdateCycle(object? sender, ElapsedEventArgs e)
        {
            foreach (var light in Lights)
            {
                light.TimeRemaining = Math.Max(0, light.TimeRemaining - 1);
            }

            foreach (var node in Nodes)
            {
                if (node.Lights.Any(l => l.TimeRemaining == 0))
                {
                    TransitionLights(node);
                }
            }

            if (_random.Next(100) < 60) // Increased spawn rate for 8 intersections
            {
                SpawnVehicle();
            }
        }

        private void TransitionLights(TrafficNode node)
        {
            if (node.Lights.Any(l => l.CurrentState == LightState.Green && l.TimeRemaining == 0))
            {
                foreach (var light in node.Lights.Where(l => l.CurrentState == LightState.Green))
                {
                    light.CurrentState = LightState.Yellow;
                    light.TimeRemaining = YELLOW_TIME;
                }
            }
            else if (node.Lights.Any(l => l.CurrentState == LightState.Yellow && l.TimeRemaining == 0))
            {
                node.CyclePhase = (node.CyclePhase + 1) % 2;

                foreach (var light in node.Lights)
                {
                    light.CurrentState = LightState.Red;
                }

                if (node.CyclePhase == 0)
                {
                    node.Lights[0].CurrentState = LightState.Green; node.Lights[1].CurrentState = LightState.Green;
                    node.Lights[0].TimeRemaining = GREEN_TIME; node.Lights[1].TimeRemaining = GREEN_TIME;
                    node.Lights[2].TimeRemaining = RED_TIME; node.Lights[3].TimeRemaining = RED_TIME;
                }
                else
                {
                    node.Lights[2].CurrentState = LightState.Green; node.Lights[3].CurrentState = LightState.Green;
                    node.Lights[2].TimeRemaining = GREEN_TIME; node.Lights[3].TimeRemaining = GREEN_TIME;
                    node.Lights[0].TimeRemaining = RED_TIME; node.Lights[1].TimeRemaining = RED_TIME;
                }
            }
        }

        private void SpawnVehicle()
        {
            string[] directions = { "North", "South", "East", "West" };
            string direction = directions[_random.Next(4)];

            double x = 0, y = 0, angle = 0;
            
            var spawnNode = Nodes[_random.Next(Nodes.Count)];

            switch (direction)
            {
                case "North":
                    x = spawnNode.CenterX + 25; y = 1100; angle = 270;
                    break;
                case "South":
                    x = spawnNode.CenterX - 25; y = -100; angle = 90;
                    break;
                case "East":
                    x = -100; y = spawnNode.CenterY + 25; angle = 0;
                    break;
                case "West":
                    x = 1800; y = spawnNode.CenterY - 25; angle = 180;
                    break;
            }

            int turnChance = _random.Next(100);
            TurnDirection turn = TurnDirection.Straight;
            if (turnChance < 20) turn = TurnDirection.Left;
            else if (turnChance < 40) turn = TurnDirection.Right;

            if (Vehicles.Any(v => v.Direction == direction && Math.Abs(v.X - x) < 60 && Math.Abs(v.Y - y) < 60))
            {
                return;
            }

            string color = direction switch { "North" => "Blue", "South" => "Red", "East" => "Green", "West" => "Orange", _ => "Gray" };

            var vehicle = new Vehicle
            {
                Id = Vehicles.Count > 0 ? Vehicles.Max(v => v.Id) + 1 : 0,
                Direction = direction, Color = color, X = x, Y = y, Angle = angle,
                IntendedTurn = turn, IsMoving = true,
                MaxSpeed = 2.0 + _random.NextDouble() * 3.0,
                Speed = 0
            };
            vehicle.Speed = vehicle.MaxSpeed;

            System.Windows.Application.Current.Dispatcher.Invoke(() => { Vehicles.Add(vehicle); });
        }

        private TrafficNode? GetActiveNode(Vehicle v)
        {
            if (v.Direction == "East") return Nodes.Where(n => Math.Abs(n.CenterY - v.Y) < 50 && n.CenterX - v.X > -70).OrderBy(n => n.CenterX).FirstOrDefault();
            if (v.Direction == "West") return Nodes.Where(n => Math.Abs(n.CenterY - v.Y) < 50 && v.X - n.CenterX > -70).OrderByDescending(n => n.CenterX).FirstOrDefault();
            if (v.Direction == "South") return Nodes.Where(n => Math.Abs(n.CenterX - v.X) < 50 && n.CenterY - v.Y > -70).OrderBy(n => n.CenterY).FirstOrDefault();
            if (v.Direction == "North") return Nodes.Where(n => Math.Abs(n.CenterX - v.X) < 50 && v.Y - n.CenterY > -70).OrderByDescending(n => n.CenterY).FirstOrDefault();
            return null;
        }

        public void UpdatePositions()
        {
            var vehiclesToRemove = new List<Vehicle>();
            double safeDistance = 50.0;

            foreach (var vehicle in Vehicles)
            {
                var activeNode = GetActiveNode(vehicle);
                bool canMove = true;
                bool isInsideIntersection = false;
                TrafficLight? light = null;

                if (activeNode != null)
                {
                    var lightIndex = vehicle.Direction switch { "North" => 0, "South" => 1, "East" => 2, "West" => 3, _ => 0 };
                    light = activeNode.Lights[lightIndex];
                    isInsideIntersection = Math.Abs(vehicle.X - activeNode.CenterX) < 70 && Math.Abs(vehicle.Y - activeNode.CenterY) < 70;

                    bool atStopLine = false;
                    switch (vehicle.Direction)
                    {
                        case "North": atStopLine = vehicle.Y <= activeNode.CenterY + 75 && vehicle.Y > activeNode.CenterY + 45; break;
                        case "South": atStopLine = vehicle.Y >= activeNode.CenterY - 75 && vehicle.Y < activeNode.CenterY - 45; break;
                        case "East": atStopLine = vehicle.X >= activeNode.CenterX - 75 && vehicle.X < activeNode.CenterX - 45; break;
                        case "West": atStopLine = vehicle.X <= activeNode.CenterX + 75 && vehicle.X > activeNode.CenterX + 45; break;
                    }

                    if (atStopLine)
                    {
                        string targetDirection = vehicle.IntendedTurn switch {
                            TurnDirection.Left => GetLeftTurnDirection(vehicle.Direction),
                            TurnDirection.Right => GetRightTurnDirection(vehicle.Direction),
                            _ => vehicle.Direction
                        };

                        if (CountVehiclesOnSegment(activeNode, targetDirection) >= 6)
                        {
                            canMove = false;
                        }
                        else if (light.CurrentState != LightState.Green)
                        {
                            if (vehicle.IntendedTurn == TurnDirection.Right)
                            {
                                string trafficFromLeft = vehicle.Direction switch { "North" => "East", "South" => "West", "East" => "South", "West" => "North", _ => "" };
                                bool leftTrafficApproaching = Vehicles.Any(v => 
                                    v.Direction == trafficFromLeft && Math.Abs(v.X - activeNode.CenterX) < 150 && Math.Abs(v.Y - activeNode.CenterY) < 150 &&
                                    ((trafficFromLeft == "East" && v.X < activeNode.CenterX) || (trafficFromLeft == "West" && v.X > activeNode.CenterX) ||
                                     (trafficFromLeft == "South" && v.Y < activeNode.CenterY) || (trafficFromLeft == "North" && v.Y > activeNode.CenterY))
                                );
                                bool crossTrafficInside = Vehicles.Any(v => v.Id != vehicle.Id && Math.Abs(v.X - activeNode.CenterX) < 60 && Math.Abs(v.Y - activeNode.CenterY) < 60);
                                if (leftTrafficApproaching || crossTrafficInside) canMove = false;
                            }
                            else
                            {
                                canMove = false;
                            }
                        }
                        else
                        {
                            bool crossTrafficInside = Vehicles.Any(v => 
                                v.Id != vehicle.Id && v.Direction != vehicle.Direction && v.Direction != GetOppositeDirection(vehicle.Direction) &&
                                Math.Abs(v.X - activeNode.CenterX) < 60 && Math.Abs(v.Y - activeNode.CenterY) < 60);

                            if (crossTrafficInside)
                            {
                                canMove = false;
                            }
                            else if (vehicle.IntendedTurn == TurnDirection.Left)
                            {
                                string oncomingDir = GetOppositeDirection(vehicle.Direction);
                                var firstOncoming = Vehicles
                                    .Where(v => v.Direction == oncomingDir && Math.Abs(v.X - activeNode.CenterX) < 150 && Math.Abs(v.Y - activeNode.CenterY) < 150)
                                    .OrderBy(v => oncomingDir == "North" ? v.Y : oncomingDir == "South" ? -v.Y : oncomingDir == "East" ? -v.X : v.X)
                                    .FirstOrDefault();

                                if (firstOncoming != null && (firstOncoming.IntendedTurn == TurnDirection.Straight || firstOncoming.IntendedTurn == TurnDirection.Right))
                                {
                                    bool isClose = false;
                                    switch(oncomingDir) {
                                        case "South": isClose = firstOncoming.Y > activeNode.CenterY - 150 && firstOncoming.Y < activeNode.CenterY + 60; break;
                                        case "North": isClose = firstOncoming.Y < activeNode.CenterY + 150 && firstOncoming.Y > activeNode.CenterY - 60; break;
                                        case "East": isClose = firstOncoming.X > activeNode.CenterX - 150 && firstOncoming.X < activeNode.CenterX + 60; break;
                                        case "West": isClose = firstOncoming.X < activeNode.CenterX + 150 && firstOncoming.X > activeNode.CenterX - 60; break;
                                    }
                                    if (isClose) canMove = false;
                                }
                            }
                        }
                    }
                }

                if (canMove)
                {
                    // Advanced coordinate-projection collision avoidance
                    Vehicle? vehicleAhead = null;
                    double minDistance = double.MaxValue;

                    foreach (var other in Vehicles)
                    {
                        if (other.Id == vehicle.Id) continue;

                        double dx = other.X - vehicle.X;
                        double dy = other.Y - vehicle.Y;
                        double dist = Math.Sqrt(dx * dx + dy * dy);

                        if (dist >= safeDistance) continue;

                        // Project other vehicle into vehicle's local coordinate system
                        double rad = -vehicle.Angle * Math.PI / 180.0;
                        double localX = dx * Math.Cos(rad) - dy * Math.Sin(rad);
                        double localY = dx * Math.Sin(rad) + dy * Math.Cos(rad);

                        bool shouldAvoid = false;

                        // If other is generally in front of us
                        if (localX > 0)
                        {
                            // If they are directly in our path laterally (within 20 pixels)
                            if (Math.Abs(localY) < 20)
                            {
                                shouldAvoid = true;
                            }
                            // Or if we are very close to them and they are in front
                            else if (dist < 35)
                            {
                                shouldAvoid = true;
                            }
                        }
                        // Side-by-side or rear overlap (very close)
                        else if (dist < 28)
                        {
                            // As a tiebreaker, the vehicle with the larger ID yields
                            shouldAvoid = vehicle.Id > other.Id;
                        }

                        // Also, if they are perpendicular or opposite directions and not in front of us yet,
                        // check if they are in the intersection zone and might cross our path
                        if (!shouldAvoid && activeNode != null && vehicle.Direction != other.Direction)
                        {
                            bool otherInIntersection = Math.Abs(other.X - activeNode.CenterX) < 55 && Math.Abs(other.Y - activeNode.CenterY) < 55;
                            if (otherInIntersection)
                            {
                                double angleToOther = Math.Atan2(dy, dx) * 180 / Math.PI;
                                double angleDiff = AngleDifference(angleToOther, vehicle.Angle);
                                if (angleDiff < 60)
                                {
                                    shouldAvoid = true;
                                }
                            }
                        }

                        if (shouldAvoid && dist < minDistance)
                        {
                            minDistance = dist;
                            vehicleAhead = other;
                        }
                    }

                    if (vehicleAhead != null)
                    {
                        if (minDistance < 38) canMove = false; // Too close, stop entirely
                        else vehicle.Speed = Math.Min(vehicle.Speed, vehicleAhead.Speed * 0.9); // Slow down
                    }
                    else
                    {
                        // Accelerate back to max speed if clear
                        vehicle.Speed = Math.Min(vehicle.Speed + 0.2, vehicle.MaxSpeed);
                    }
                }

                if (!canMove)
                {
                    vehicle.Speed = 0;
                }

                if (vehicle.Speed > 0)
                {
                    if (isInsideIntersection && activeNode != null && vehicle.IntendedTurn != TurnDirection.Straight && !vehicle.HasCompletedTurn)
                    {
                        bool canStartTurn = vehicle.Direction switch {
                            "East" => vehicle.X >= activeNode.CenterX - 50,
                            "West" => vehicle.X <= activeNode.CenterX + 50,
                            "North" => vehicle.Y <= activeNode.CenterY + 50,
                            "South" => vehicle.Y >= activeNode.CenterY - 50,
                            _ => false
                        };

                        if (canStartTurn)
                        {
                            double turnRate = vehicle.IntendedTurn == TurnDirection.Right ? 2.29 * vehicle.Speed : 0.764 * vehicle.Speed;
                            double targetAngleOffset = vehicle.IntendedTurn == TurnDirection.Right ? 90 : -90;
                            
                            double startAngle = vehicle.Direction switch { "East" => 0, "South" => 90, "West" => 180, "North" => 270, _ => 0 };
                            double targetAngle = NormalizeAngle(startAngle + targetAngleOffset);

                            if (Math.Abs(NormalizeAngle(vehicle.Angle) - targetAngle) <= turnRate)
                            {
                                vehicle.Angle = targetAngle;
                                vehicle.HasCompletedTurn = true;
                                vehicle.IntendedTurn = TurnDirection.Straight;
                                vehicle.Direction = vehicle.Angle switch {
                                    0 => "East", 90 => "South", 180 => "West", 270 => "North", _ => vehicle.Direction
                                };
                                
                                switch (vehicle.Direction)
                                {
                                    case "East": vehicle.Y = activeNode.CenterY + 25; break;
                                    case "West": vehicle.Y = activeNode.CenterY - 25; break;
                                    case "North": vehicle.X = activeNode.CenterX + 25; break;
                                    case "South": vehicle.X = activeNode.CenterX - 25; break;
                                }
                            }
                            else
                            {
                                vehicle.Angle = NormalizeAngle(vehicle.Angle + (vehicle.IntendedTurn == TurnDirection.Right ? turnRate : -turnRate));
                            }
                        }
                    }
                    else if (!isInsideIntersection && vehicle.HasCompletedTurn)
                    {
                        vehicle.HasCompletedTurn = false;
                    }

                    double rad = vehicle.Angle * Math.PI / 180.0;
                    vehicle.X += Math.Cos(rad) * vehicle.Speed;
                    vehicle.Y += Math.Sin(rad) * vehicle.Speed;
                }

                if (vehicle.X < -200 || vehicle.X > 3400 || vehicle.Y < -200 || vehicle.Y > 1000)
                {
                    vehiclesToRemove.Add(vehicle);
                }
            }

            if (vehiclesToRemove.Any())
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => 
                {
                    foreach (var vehicle in vehiclesToRemove) Vehicles.Remove(vehicle);
                });
            }
        }

        private string GetLeftTurnDirection(string dir) => dir switch
        {
            "North" => "West",
            "West" => "South",
            "South" => "East",
            "East" => "North",
            _ => dir
        };

        private string GetRightTurnDirection(string dir) => dir switch
        {
            "North" => "East",
            "East" => "South",
            "South" => "West",
            "West" => "North",
            _ => dir
        };

        private int CountVehiclesOnSegment(TrafficNode activeNode, string targetDirection)
        {
            double minX = 0, maxX = 0, minY = 0, maxY = 0;
            bool isHorizontal = targetDirection == "East" || targetDirection == "West";
            double targetLaneCoord = 0;

            if (targetDirection == "East")
            {
                var next = Nodes.Where(n => n.CenterY == activeNode.CenterY && n.CenterX > activeNode.CenterX).OrderBy(n => n.CenterX).FirstOrDefault();
                minX = activeNode.CenterX + 50;
                maxX = next != null ? next.CenterX - 50 : 3500;
                targetLaneCoord = activeNode.CenterY + 25;
            }
            else if (targetDirection == "West")
            {
                var prev = Nodes.Where(n => n.CenterY == activeNode.CenterY && n.CenterX < activeNode.CenterX).OrderByDescending(n => n.CenterX).FirstOrDefault();
                minX = prev != null ? prev.CenterX + 50 : -250;
                maxX = activeNode.CenterX - 50;
                targetLaneCoord = activeNode.CenterY - 25;
            }
            else if (targetDirection == "South")
            {
                var next = Nodes.Where(n => n.CenterX == activeNode.CenterX && n.CenterY > activeNode.CenterY).OrderBy(n => n.CenterY).FirstOrDefault();
                minY = activeNode.CenterY + 50;
                maxY = next != null ? next.CenterY - 50 : 1100;
                targetLaneCoord = activeNode.CenterX - 25;
            }
            else if (targetDirection == "North")
            {
                var prev = Nodes.Where(n => n.CenterX == activeNode.CenterX && n.CenterY < activeNode.CenterY).OrderByDescending(n => n.CenterY).FirstOrDefault();
                minY = prev != null ? prev.CenterY + 50 : -250;
                maxY = activeNode.CenterY - 50;
                targetLaneCoord = activeNode.CenterX + 25;
            }

            int count = 0;
            foreach (var v in Vehicles)
            {
                if (isHorizontal)
                {
                    if (Math.Abs(v.Y - targetLaneCoord) < 15 && v.X >= minX && v.X <= maxX)
                    {
                        count++;
                    }
                }
                else
                {
                    if (Math.Abs(v.X - targetLaneCoord) < 15 && v.Y >= minY && v.Y <= maxY)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private double NormalizeAngle(double a)
        {
            while (a < 0) a += 360;
            while (a >= 360) a -= 360;
            return a;
        }

        private double AngleDifference(double a, double b)
        {
            double diff = Math.Abs(NormalizeAngle(a) - NormalizeAngle(b));
            if (diff > 180) diff = 360 - diff;
            return diff;
        }

        private string GetOppositeDirection(string dir) => dir switch { "North" => "South", "South" => "North", "East" => "West", "West" => "East", _ => dir };
    }
}
