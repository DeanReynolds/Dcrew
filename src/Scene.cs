namespace Dcrew;

public abstract class Scene {
    public abstract void Load();
    public abstract void Unload();
    public abstract void Update();
    public abstract void Draw();
}