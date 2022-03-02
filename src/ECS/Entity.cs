namespace Dcrew.ECS;

public static class Entity {
    static FreeList _set;
    static List<SparseSet>[] _components;
    static HashSet<SparseSet> _componentsList;

    public static void Init(int capacity) {
        _set = new(capacity);
        _components = new List<SparseSet>[capacity];
        for (int i = 0; i < capacity; i++)
            _components[i] = new();
        _componentsList = new();
    }

    public static int Add() {
        int i = _set.Add();
        return i;
    }
    public static void Remove(int entity) {
        _set.Remove(entity);
        foreach (var s in _components[entity])
            s.Remove(entity);
        _components[entity].Clear();
    }
    public static ReadOnlySpan<int> All => _set.All;
    public static bool Has(int entity) => _set.Has(entity);
    public static void Clear() {
        foreach (var l in _componentsList)
            l.Clear();
        _componentsList.Clear();
        for (int i = 0; i < _components.Length; i++)
            _components[i].Clear();
        _set.Clear();
    }

    public static ref T AddComponent<T>(int entity, T component = default) where T : struct, IComponent {
        _components[entity].Add(Component<T>._set);
        _componentsList.Add(Component<T>._set);
        return ref Component<T>.Add(entity, component);
    }
    public static void RemoveComponent<T>(int entity) where T : struct, IComponent {
        _components[entity].Remove(Component<T>._set);
        Component<T>.Remove(entity);
    }
    public static ref T GetComponent<T>(int entity) where T : struct, IComponent => ref Component<T>.Get(entity);
    public static bool HasComponent<T>(int entity) where T : struct, IComponent => Component<T>.Has(entity);
    public static ReadOnlySpan<int> AllWith<T>() where T : struct, IComponent => Component<T>.All;
    //public static ReadOnlySpan<int> AllWith<T1, T2>()
    //    where T1 : struct, IComponent
    //    where T2 : struct, IComponent {
    //    return Component<T>.All;
    //}
    public static void ClearComponents(int entity) {
        foreach (var s in _components[entity])
            s.Remove(entity);
        _components[entity].Clear();
    }
}