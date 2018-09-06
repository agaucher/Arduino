using System;
using System.Collections.Generic;
using System.Linq;

class Player
{
    public static List<Infos> HistoInfos = new List<Infos>();

    static void Main(string[] args)
    {
        // game loop
        while (true)
        {
            var infos = GetInfos();
            HistoInfos.Add(infos);

            infos.Ship.Direction = GetNextDirection(infos);
            infos.Ship.Thrust = GetThrust(infos);

            var distanceToBrake = infos.Ship.Speed > 100
                ? Helper.DistanceToBrake(infos.Ship.Speed, 100).ToString()
                : "NA";

            Console.Error.WriteLine($"Speed: {infos.Ship.Speed}, Thrust: {infos.Ship.Thrust}, Target: X={infos.Ship.Direction.X};Y={infos.Ship.Direction.Y}, Distance={infos.NextCheckPoint.Distance}, DistanceToBreak: {distanceToBrake}");
            Console.WriteLine(infos.Ship.Direction.X + " " + infos.Ship.Direction.Y + " " + infos.Ship.Thrust);
        }
    }

    static int GetThrust(Infos infos)
    {
        if (Math.Abs(infos.NextCheckPoint.Angle) >= 90)
        {
            // Wrong direction
            return 0;
        }

        var distanceToBrake = Helper.DistanceToBrake(infos.Ship.Speed, Const.TargetSpeedOnBreakPoint);

        if (infos.NextCheckPoint.Distance > distanceToBrake)
        {
            return 100;
        }

        return 0;
    }

    static Point GetNextDirection(Infos infos)
    {
        var target = new Point();
        target.X = infos.NextCheckPoint.Position.X;
        target.Y = infos.NextCheckPoint.Position.Y;
        return target;
    }

    static Infos GetInfos()
    {
        var inputs = Console.ReadLine().Split(' ');
        int x = int.Parse(inputs[0]);
        int y = int.Parse(inputs[1]);
        int nextCheckpointX = int.Parse(inputs[2]); // x position of the next check point
        int nextCheckpointY = int.Parse(inputs[3]); // y position of the next check point
        int nextCheckpointDist = int.Parse(inputs[4]); // distance to the next checkpoint
        int nextCheckpointAngle = int.Parse(inputs[5]); // angle between your pod orientation and the direction of the next checkpoint
        var inputs2 = Console.ReadLine().Split(' ');
        int opponentX = int.Parse(inputs2[0]);
        int opponentY = int.Parse(inputs2[1]);

        int previousX = HistoInfos.Count > 0 ? HistoInfos.Last().Ship.Position.X : x;
        int previousY = HistoInfos.Count > 0 ? HistoInfos.Last().Ship.Position.Y : y;

        return new Infos
        {
            Ship = new Ship
            {
                Position = new Point { X = x, Y = y },
                Speed = (int) Math.Sqrt(Math.Pow(x - previousX, 2) + Math.Pow(y - previousY, 2)), // px/cycle
            },
            NextCheckPoint = new CheckPoint
            {
                Position = new Point { X = nextCheckpointX, Y = nextCheckpointY },
                Angle = nextCheckpointAngle,
                Distance = nextCheckpointDist - Const.SizeCheckpoint / 2,
            },
            Opponent = new Ship
            {
                Position = new Point { X = opponentX, Y = opponentY }
            },
        };
    }
}


#region Others

public class Helper
{
    public static int DistanceToBrake(int currentSpeed, int targetSpeed)
    {
        if (currentSpeed < 0 || targetSpeed < 0)
        {
            throw new ArgumentOutOfRangeException($"[{nameof(DistanceToBrake)}] invalid parameters: {currentSpeed} / {targetSpeed}");
        }
        if (currentSpeed < targetSpeed)
        {
            return 0;
        }

        int distance = 0;
        while (currentSpeed > targetSpeed)
        {
            distance += currentSpeed;
            currentSpeed = (int) Math.Floor(currentSpeed * Const.FrictionCoef);
        }
        return distance;
    }
}

public struct Const
{
    // Physical constants
    public const double FrictionCoef = 0.847933884298;
    public const int SizeCheckpoint = 1200;
    public const int SizeMap = 16000;

    // Parameters
    public const int TargetSpeedOnBreakPoint = 300;
}

public class Infos
{
    public Ship Ship { get; set; }
    public CheckPoint NextCheckPoint { get; set; }
    public Ship Opponent { get; set; }
}

public class Ship
{
    public Point Position { get; set; }
    public int Speed { get; set; }
    public int Thrust { get; set; }
    public Point Direction { get; set; }
}

public class CheckPoint
{
    public Point Position { get; set; }
    public int Distance { get; set; }
    public int Angle { get; set; }
}

public struct Point
{
    public int X, Y;
}

#endregion