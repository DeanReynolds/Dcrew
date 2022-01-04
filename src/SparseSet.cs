namespace Dcrew;

/// <summary>A simple sparse set</summary>
public struct SparseSet {
    int[] _dense;
    int[] _sparse;
    public int Count { get; private set; }
    public int MaxComponents => _dense.Length;
    public int MaxEntities => _sparse.Length;

    public SparseSet(int maxEntities, int maxComponents) {
        _dense = new int[maxComponents];
        _sparse = new int[maxEntities];
        Count = 0;
    }

    /// <summary>Add <paramref name="i"/> to this set
    /// 
    /// Call <see cref="EnsureFits(int, int)"/> before this if you may need to expand</summary>
    public void Add(int i) {
        _dense[Count] = i;
        _sparse[i] = Count;
        Count++;
    }
    /// <summary>Auto expand this set by <paramref name="expandBy"/> if <paramref name="i"/> won't fit</summary>
    public void EnsureFits(int i, int expandBy = 1) {
        if (_sparse.Length <= i)
            Array.Resize(ref _sparse, i + expandBy);
        if (Count >= _dense.Length)
            Array.Resize(ref _dense, Count + expandBy);
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
public struct SparseSet<T> {
    int[] _dense;
    T[] _item;
    int[] _sparse;
    public int Count { get; private set; }
    public int MaxComponents => _dense.Length;
    public int MaxEntities => _sparse.Length;

    public SparseSet(int maxEntities, int maxComponents) {
        _dense = new int[maxComponents];
        _item = new T[maxComponents];
        _sparse = new int[maxEntities];
        Count = 0;
    }

    public ref T Set(int i, T item) {
        int v = _sparse[i];
        if (v >= Count || _dense[v] != i) {
            _dense[Count] = i;
            v = _sparse[i] = Count++;
        }
        ref var r = ref _item[v];
        r = item;
        return ref r;
    }
    /// <summary>Add <paramref name="i"/> to this set and set its item to <paramref name="item"/> and return a ref to it
    /// 
    /// Call <see cref="EnsureFits(int, int)"/> before this if you may need to expand</summary>
    public ref T Add(int i, T item) {
        _dense[Count] = i;
        ref var r = ref _item[Count];
        r = item;
        _sparse[i] = Count++;
        return ref r;
    }
    /// <summary>Add <paramref name="i"/> to this set and return a ref to its item
    /// 
    /// Call <see cref="EnsureFits(int, int)"/> before this if you may need to expand</summary>
    public ref T Add(int i) {
        _dense[Count] = i;
        _sparse[i] = Count;
        return ref _item[Count++];
    }
    /// <summary>Auto expand this set by <paramref name="expandBy"/> if <paramref name="i"/> won't fit</summary>
    public void EnsureFits(int i, int expandBy = 1) {
        if (_sparse.Length <= i)
            Array.Resize(ref _sparse, i + expandBy);
        if (Count >= _dense.Length)
            Array.Resize(ref _dense, Count + expandBy);
    }
    /// <summary>Remove <paramref name="i"/> from this set</summary>
    public void Remove(int i) {
        int v = _sparse[i];
        int temp = _dense[--Count];
        _dense[v] = temp;
        ref var r = ref _item[v];
        _item[v] = _item[Count];
        _item[Count] = default;
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
    public ref T GetIfHas(int i, T ifNot) {
        int v = _sparse[i];
        ref var r = ref _item[v];
        if (v >= Count || _dense[v] != i)
            r = ifNot;
        return ref r;
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