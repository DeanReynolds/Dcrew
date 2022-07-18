namespace Dcrew.ECS;

public static class Entity {
    static FreeList _set = new(0);
    static readonly HashSet<ComponentSet> _components = new();
    public static int Count => _set.Count;

    public static void Init(int capacity) => _set = new(capacity);

    public static int Add() {
        if (_set.Count >= _set.Max) {
            int start = _set.Max;
            _set.Resize((_set.Max + 1) << 1);
        }
        return _set.Add();
    }
    public static void Del(int entity) {
        _set.Remove(entity);
        foreach (var s in _components)
            s.TryDel(entity);
    }
    public static ReadOnlySpan<int> All => _set.All;
    public static bool Has(int entity) => _set.Has(entity);
    public static void Clear() {
        foreach (var l in _components)
            l.Clear();
        _components.Clear();
        _set.Clear();
    }

    public static ref T AddComponent<T>(int entity, T component = default) where T : struct, IComponent {
        _components.Add(Component<T>._set);
        return ref Component<T>.Add(entity, component);
    }
    public static void DelComponent<T>(int entity) where T : struct, IComponent {
        Component<T>.Del(entity);
        if (Component<T>.Count == 0)
            _components.Remove(Component<T>._set);
    }
    public static void TryDelComponent<T>(int entity) where T : struct, IComponent {
        if (Component<T>.TryDel(entity) && Component<T>.Count == 0)
            _components.Remove(Component<T>._set);
    }
    public static ref T GetComponent<T>(int entity) where T : struct, IComponent => ref Component<T>.Get(entity);
    public static bool HasComponent<T>(int entity) where T : struct, IComponent => Component<T>.Has(entity);
    public static ReadOnlySpan<int> AllWith<T>() where T : struct, IComponent => Component<T>.All;
    public static ReadOnlySpan<int> AllWith<T1, T2>() where T1 : struct, IComponent where T2 : struct, IComponent {
        return new System<T1, T2>().All;
    }
    public static void ClearComponents(int entity) {
        foreach (var s in _components)
            if (s.TryDel(entity) && s.Count == 0)
                _components.Remove(s);
    }

    public static int CountOf<T>() where T : struct, IComponent => Component<T>.Count;
}