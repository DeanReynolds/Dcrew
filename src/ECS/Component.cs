namespace Dcrew.ECS;

public static class Component<T> where T : struct, IComponent {
    static internal SparseSet _set = new(0, 0);
    static internal T[] _item = Array.Empty<T>();
    public static int MaxComponents => _set.MaxComponents;
    public static int MaxEntities => _set.MaxEntities;

    public static void Init(int maxEntities, int maxComponents) {
        _set.Resize(maxEntities, maxComponents);
        Array.Resize(ref _item, maxEntities);
    }

    public static ref T Add(int entity, T component = default) {
        if (_set.MaxEntities < entity + 1) {
            if (_set.Count >= _set.MaxComponents)
                _set.Resize(entity + 1, (_set.MaxComponents + 1) << 1);
            else
                _set.Resize(entity + 1, _set.MaxComponents);
            Array.Resize(ref _item, entity + 1);
        } else if (_set.Count >= _set.MaxComponents)
            _set.Resize(_set.MaxEntities, (_set.MaxComponents + 1) << 1);
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