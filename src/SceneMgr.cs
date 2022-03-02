using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace Dcrew;

public static class SceneMgr {
    static readonly Dictionary<Type, Scene?> _scene = new();

    static SceneMgr() {
        foreach (var s in Assembly.GetEntryAssembly().GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(Scene))))
            _scene.Add(s, Activator.CreateInstance(s) as Scene);
    }

    public static Scene? CurrentScene { get; private set; }

    public static void SetScene<T>() where T : Scene {
        CurrentScene?.Unload();
        CurrentScene = _scene[typeof(T)];
        CurrentScene.Load();
    }

    public static void Update() => CurrentScene.Update();
    public static void Draw(SpriteBatch s) => CurrentScene.Draw(s);
}