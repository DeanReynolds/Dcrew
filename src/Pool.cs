namespace Dcrew;

public static class Pool<T> where T : class, new() {
    /// <summary>Returns the amount of free <typeparamref name="T"/> objects</summary>
    public static int Count { get; private set; }

    /// <summary>Returns the size of the pool in total</summary>
    public static int Size => _arr.Length;

    static T[] _arr = Array.Empty<T>();

    /// <summary>Ensures there is <paramref name="size"/> amount of free <typeparamref name="T"/> objects</summary>
    public static void EnsureCount(int size) {
        if (_arr.Length < size)
            Array.Resize(ref _arr, size);
        var n = size - Count;
        for (var i = 0; i < n; i++)
            _arr[Count++] = new T();
    }
    /// <summary>Expands the amount of <typeparamref name="T"/> objects by <paramref name="amount"/></summary>
    public static void Expand(int amount) {
        Array.Resize(ref _arr, _arr.Length + amount);
        for (var i = 0; i < amount; i++)
            _arr[Count++] = new T();
    }

    /// <summary>Returns a free instance of <typeparamref name="T"/> and returns a new one if there's none available</summary>
    public static T Spawn() {
        if (Count == 0)
            return new T();
        return Interlocked.Exchange(ref _arr[--Count], default);
    }
    /// <summary>Frees <paramref name="obj"/> for use when <see cref="Spawn"/> is called</summary>
    public static void Free(T obj) {
        if (Count == _arr.Length)
            Array.Resize(ref _arr, (_arr.Length << 1) + 1);
        _arr[Count++] = obj;
    }
}