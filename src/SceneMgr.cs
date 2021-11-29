using System.Reflection;

namespace Dcrew;

public static class SceneMgr {
    static readonly Dictionary<Type, Scene?> _scene = new();
    static Scene? _currentScene;

    static SceneMgr() {
        foreach (var s in Assembly.GetEntryAssembly().GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(Scene))))
            _scene.Add(s, Activator.CreateInstance(s) as Scene);
    }

    public static Scene? CurrentScene => _currentScene;

    public static void SetScene<T>() where T : Scene {
        _currentScene?.Unload();
        _currentScene = _scene[typeof(T)];
        _currentScene.Load();
    }

    public static void Update() => _currentScene.Update();
    public static void Draw() => _currentScene.Draw();
}