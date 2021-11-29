using System.Reflection;
using Microsoft.Xna.Framework;

namespace Dcrew;

public static class Global {
    public static Game? Game { get; private set; }

    static Global() => Game = typeof(Game).GetField("_instance", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Game;
}