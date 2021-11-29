namespace Dcrew;

/// <summary>A simple sparse set</summary>
public struct SparseSet {
    int[] _dense;
    int[] _sparse;
    public int Count { get; private set; }
    public int Max => _sparse.Length;

    public SparseSet(int maxEntities, int maxComponents) {
        _dense = new int[maxComponents];
        _sparse = new int[maxEntities];
        Count = 0;
    }

    /// <summary>Add <paramref name="i"/> to this set</summary>
    public void Add(int i) {
        _dense[Count] = i;
        _sparse[i] = Count;
        Count++;
    }
    /// <summary>Remove <paramref name="i"/> from this set</summary>
    public void Remove(int i) {
        int v = _sparse[i];
        int temp = _dense[--Count];
        _dense[v] = temp;
        _sparse[temp] = v;
    }
    /// <summary>Get all ints added to this set</summary>
    public ReadOnlySpan<int> All => new(_dense, 0, Count);
    /// <summary>True if <paramref name="i"/> is in this set</summary>
    public bool Has(int i) {
        int v = _sparse[i];
        return v < Count && _dense[v] == i;
    }
    /// <summary>Empty this set</summary>
    public void Clear() => Count = 0;
    /// <summary>Resize this set to fit the given capacities</summary>
    public void Resize(int maxEntities, int maxComponents) {
        Array.Resize(ref _sparse, maxEntities);
        Array.Resize(ref _dense, maxComponents);
    }
}

/// <summary>A simple sparse set with items</summary>
public struct SparseSet<T> where T : struct {
    int[] _dense;
    T[] _item;
    int[] _sparse;
    public int Count { get; private set; }
    public int Max => _sparse.Length;

    public SparseSet(int maxEntities, int maxComponents) {
        _dense = new int[maxComponents];
        _item = new T[maxComponents];
        _sparse = new int[maxEntities];
        Count = 0;
    }

    /// <summary>Add <paramref name="i"/> to this set and set its item to <paramref name="item"/> and return a ref to it</summary>
    public ref T Add(int i, T item) {
        _dense[Count] = i;
        ref var r = ref _item[Count];
        r = item;
        _sparse[i] = Count++;
        return ref r;
    }
    /// <summary>Add <paramref name="i"/> to this set and return a ref to its item</summary>
    public ref T Add(int i) {
        _dense[Count] = i;
        _sparse[i] = Count;
        return ref _item[Count++];
    }
    /// <summary>Remove <paramref name="i"/> from this set</summary>
    public void Remove(int i) {
        int v = _sparse[i];
        int temp = _dense[--Count];
        _dense[v] = temp;
        _item[v] = _item[Count];
        _sparse[temp] = v;
    }
    public ref T this[int i] => ref _item[_sparse[i]];
    /// <summary>Get all ints added to this set</summary>
    public ReadOnlySpan<int> All => new(_dense, 0, Count);
    /// <summary>True if <paramref name="i"/> is in this set</summary>
    public bool Has(int i) {
        int v = _sparse[i];
        return v < Count && _dense[v] == i;
    }
    /// <summary>Empty this set</summary>
    public void Clear() => Count = 0;
    /// <summary>Resize this set to fit the given capacities</summary>
    public void Resize(int maxEntities, int maxComponents) {
        Array.Resize(ref _sparse, maxEntities);
        Array.Resize(ref _dense, maxComponents);
        Array.Resize(ref _item, maxComponents);
    }
}