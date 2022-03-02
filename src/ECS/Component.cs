namespace Dcrew.ECS;

public static class Component<T> where T : struct, IComponent {
    static internal SparseSet _set;
    static internal T[] _item;

    public static void Init(int maxEntities, int maxComponents) {
        _set = new SparseSet(maxEntities, maxComponents);
        _item = new T[maxEntities];
    }

    public static ref T Add(int entity, T component = default) {
        _set.Add(entity);
        ref T r = ref _item[entity];
        r = component;
        return ref r;
    }
    public static void Remove(int entity) {
        _set.Remove(entity);
        _item[entity] = default;
    }
    public static ref T Get(int entity) => ref _item[entity];
    public static ReadOnlySpan<int> All => _set.All;
    public static bool Has(int entity) => _set.Has(entity);
    public static void Clear() => _set.Clear();
}