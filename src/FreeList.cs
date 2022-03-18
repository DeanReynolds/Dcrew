namespace Dcrew;

/// <summary>A <see cref="SparseSet"/> with the ability to add without giving an index</summary>
public class FreeList {
    readonly SparseSet _set;
    int _free;
    int[] _next;
    public int Count => _set.Count;
    public int Max => _set.MaxEntities;

    public FreeList(int capacity) {
        _set = new SparseSet(capacity, capacity);
        _free = -1;
        _next = new int[capacity];
        for (int i = 0; i < _next.Length; i++)
            _next[i] = -1;
    }

    /// <summary>Adds an item at the next free index and returns that index</summary>
    public int Add() {
        int i;
        if (_free != -1) {
            i = _free;
            _free = _next[i];
        } else {
            int j = 0;
            do {
                i = Count + j++;
            } while (_set.Has(i));
        }
        _set.Add(i);
        return i;
    }
    /// <summary>Remove index <paramref name="i"/> from this set</summary>
    public void Remove(int i) {
        _set.Remove(i);
        _next[i] = _free;
        _free = i;
    }
    /// <summary>Get all indeces in this set</summary>
    public ReadOnlySpan<int> All => _set.All;
    /// <summary>True if index <paramref name="i"/> is in this set</summary>
    public bool Has(int i) => _set.Has(i);
    /// <summary>Empty this set</summary>
    public void Clear() {
        _set.Clear();
        _free = -1;
        for (int i = 0; i < _next.Length; i++)
            _next[i] = -1;
    }
    /// <summary>Resize this set's capacity to <paramref name="capacity"/></summary>
    public void Resize(int capacity) {
        int oldLength = _set.MaxEntities;
        _set.Resize(capacity, capacity);
        Array.Resize(ref _next, capacity);
        for (int j = oldLength; j < capacity; j++)
            _next[j] = -1;
    }
}

/// <summary>A <see cref="SparseSet"/> with the ability to add without giving an index</summary>
public class FreeList<T> {
    readonly SparseSet<T> _set;
    int _free;
    int[] _next;
    int[] _refer;
    public int Count => _set.Count;
    public int Max => _set.MaxEntities;

    public FreeList(int capacity) {
        _set = new SparseSet<T>(capacity, capacity);
        _free = -1;
        _next = new int[capacity];
        _refer = new int[capacity];
        for (int i = 0; i < _next.Length; i++)
            _next[i] = -1;
    }

    /// <summary>Adds <paramref name="item"/> at the next free index and returns that index</summary>
    public int Add(ref T item) {
        int i;
        if (_free != -1) {
            i = _free;
            _free = _next[i];
        } else {
            int j = 0;
            do {
                i = Count + j++;
            } while (_set.Has(i));
        }
        item = ref _set.Add(i, item);
        return i;
    }
    /// <summary>Adds <paramref name="item"/> at the next free index and returns that index</summary>
    public int Add(T item = default) {
        int i;
        if (_free != -1) {
            i = _free;
            _free = _next[i];
        } else {
            int j = 0;
            do {
                i = Count + j++;
            } while (_set.Has(i));
        }
        _set.Add(i, item);
        return i;
    }
    /// <summary>Adds an item at the next free index and returns a ref to that item</summary>
    public ref T Add(out int i) {
        if (_free != -1) {
            i = _free;
            _free = _next[i];
        } else {
            int j = 0;
            do {
                i = Count + j++;
            } while (_set.Has(i));
        }
        return ref _set.Add(i);
    }
    /// <summary>Set index <paramref name="i"/> to <paramref name="item"/> and return a ref to it</summary>
    public ref T Set(int i, T item = default) {
        if (_free == i)
            _free = _next[i];
        else {
            int refferal = _refer[i];
            if (refferal != -1 && _next[refferal] == i) {
                if ((_next[refferal] = _next[i]) != -1)
                    _refer[_next[i]] = refferal;
                _refer[i] = -1;
            }
        }
        return ref _set.Set(i, item);
    }
    /// <summary>Remove index <paramref name="i"/> from this set</summary>
    public void Remove(int i) {
        _set.Remove(i);
        if ((_next[i] = _free) != -1)
            _refer[_free] = i;
        _free = i;
    }
    /// <summary>Get a ref to the item at index <paramref name="i"/></summary>
    public ref T this[int i] => ref _set[i];
    /// <summary>Get all indeces in this set</summary>
    public ReadOnlySpan<int> All => _set.All;
    /// <summary>True if index <paramref name="i"/> is in this set</summary>
    public bool Has(int i) => _set.Has(i);
    /// <summary>Empty this set</summary>
    public void Clear() {
        _set.Clear();
        _free = -1;
        for (int i = 0; i < _next.Length; i++)
            _next[i] = -1;
    }
    /// <summary>Resize this set's capacity to <paramref name="capacity"/></summary>
    public void Resize(int capacity) {
        int oldLength = _set.MaxEntities;
        _set.Resize(capacity, capacity);
        Array.Resize(ref _next, capacity);
        Array.Resize(ref _refer, capacity);
        for (int j = oldLength; j < capacity; j++)
            _next[j] = -1;
    }
}