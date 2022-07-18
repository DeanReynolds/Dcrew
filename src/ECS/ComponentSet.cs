namespace Dcrew.ECS;

internal class ComponentSet {
    SparseSet _set;

    public int Count => _set.Count;
    public int MaxComponents => _set.MaxComponents;
    public int MaxEntities => _set.MaxEntities;

    public ComponentSet(int maxEntities, int maxComponents) => _set = new SparseSet(maxEntities, maxComponents);
    public ComponentSet() : this(0, 0) { }

    public void Add(int i) => _set.Add(i);
    public void EnsureFits(int i, int expandBy = 1) => _set.EnsureFits(i, expandBy);
    public void Del(int i) => _set.Del(i);
    public bool TryDel(int i) => _set.TryDel(i);
    public ReadOnlySpan<int> All => _set.All;
    public bool Has(int i) => _set.Has(i);
    public void Clear() => _set.Clear();
    public void Resize(int maxEntities, int maxComponents) => _set.Resize(maxEntities, maxComponents);
}