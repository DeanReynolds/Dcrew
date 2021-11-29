using System.Reflection;
using Microsoft.Xna.Framework;

namespace Dcrew;

public static class Time {
    static readonly GameTime? _gameTime;

    static Time() => _gameTime = typeof(Game).GetField("_gameTime", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Global.Game) as GameTime;

    public static float Delta => (float)_gameTime.ElapsedGameTime.TotalSeconds;
    public static float Total => (float)_gameTime.TotalGameTime.TotalSeconds;
    public static double DeltaD => _gameTime.ElapsedGameTime.TotalSeconds;
    public static double TotalD => _gameTime.TotalGameTime.TotalSeconds;
}