using System.Buffers;
using Microsoft.Xna.Framework;

namespace Dcrew;

public struct Pathfinder {
    struct PriorityQueue {
        SparseSet _i;
        readonly PriorityQueue<int, float> _queue;
        public int Count => _queue.Count;

        public PriorityQueue(int maxNodes = 10) {
            _i = new SparseSet(maxNodes + 1, maxNodes + 1);
            _queue = new PriorityQueue<int, float>(maxNodes + 1);
        }

        public void Clear() {
            _queue.Clear();
            _i.Clear();
        }
        public bool Has(int i) => _i.Has(i);

        public void Enqueue(int i, float priority) {
            _queue.Enqueue(i, priority);
            _i.Add(i);
        }
        public int Dequeue() => _queue.Dequeue();
    }

    struct MemoryMap {
        public uint MapVer;
        public bool Built;
        public ushort[] Map;
    }

    readonly byte[] _cost;
    readonly Flags[] _flags;
    SparseSet _closed;
    PriorityQueue _open;
    SparseSet<int> _gCost;
    readonly int[] _parent;
    int _sX, _sY, _eX, _eY;
    uint _mapVer;
    readonly MemoryMap[] _memMap;
    readonly Queue<int> _discovered;
    public int Width { get; private set; }
    public int Height { get; private set; }

    [Flags] enum Flags : byte { IsWall = 1 }

    public Pathfinder(int width, int height) {
        Width = width;
        Height = height;
        int sq = width * height;
        _cost = new byte[sq];
        _flags = new Flags[sq];
        _closed = new SparseSet(sq, sq);
        _open = new PriorityQueue(sq);
        _gCost = new SparseSet<int>(sq, sq);
        _parent = new int[sq];
        _sX = _sY = _eX = _eY = 0;
        _mapVer = 0;
        _memMap = new MemoryMap[sq];
        _discovered = new Queue<int>(sq);
    }

    public void SetWall(int x, int y, bool r) {
        ref var flags = ref _flags[y * Width + x];
        var old = flags & Flags.IsWall;
        if (r) {
            flags |= Flags.IsWall;
            return;
        }
        flags &= ~Flags.IsWall;
        if (old != (flags & Flags.IsWall))
            _mapVer++;
    }
    public bool IsWall(int x, int y) => (_flags[y * Width + x] & Flags.IsWall) != 0;

    public void BuildMemoryMap(int eX, int eY) {
        _eX = eX;
        _eY = eY;
        int end = eY * Width + eX;
        var map = new ushort[Width * Height];
        map[end] = 1;
        _closed.Clear();
        _discovered.Clear();
        _closed.Add(end);
        _discovered.Enqueue(end);
        do {
            int cur = _discovered.Dequeue();
            var d = map[cur];
            foreach (int n in Get4DirNeighbours(cur)) {
                map[n] = (ushort)(d + 1);
                _discovered.Enqueue(n);
                _closed.Add(n);
            }
        } while (_discovered.Count > 0);
        _memMap[end] = new MemoryMap {
            MapVer = _mapVer,
            Built = true,
            Map = map
        };
    }
    public void ClearBadMemoryMaps() {
        for (int i = 0; i < _memMap.Length; i++) {
            ref var memoryMap = ref _memMap[i];
            if (memoryMap.Built && memoryMap.MapVer != _mapVer) {
                Array.Resize(ref memoryMap.Map, 0);
                memoryMap.Built = false;
            }
        }
    }
    public ReadOnlySpan<Point> FindPath(int sX, int sY, int eX, int eY) {
        _sX = sX;
        _sY = sY;
        _eX = eX;
        _eY = eY;
        var cur = sY * Width + sX;
        var end = eY * Width + eX;
        _closed.Clear();
        ref var memoryMap = ref _memMap[end];
        if (memoryMap.Built && memoryMap.MapVer == _mapVer) {
            var arr = ArrayPool<Point>.Shared.Rent(Width * Height);
            int c = 0,
                d = memoryMap.Map[cur],
                best = cur;
            while (d > 1) {
                foreach (int n in Get8DirNeighbours(cur)) {
                    var nd = memoryMap.Map[n];
                    if (nd < d) {
                        d = nd;
                        best = n;
                    }
                }
                cur = best;
                arr[c++] = new Point(cur % Width, cur / Width);
            }
            ArrayPool<Point>.Shared.Return(arr);
            return new ReadOnlySpan<Point>(arr, 0, c);
        }
        _open.Clear();
        _open.Enqueue(cur, 1);
        _gCost.Clear();
        _gCost.Add(cur, 0);
        do {
            cur = _open.Dequeue();
            if (cur == end)
                return BuildPath();
            _closed.Add(cur);
            foreach (var n in Get8DirNeighbours(cur)) {
                var gCost = _gCost.GetIfHas(cur, int.MaxValue) + Cost(cur, n);
                if (!_open.Has(n)) {
                    _gCost.Set(n, gCost);
                    _parent[n] = cur + 1;
                    _open.Enqueue(n, gCost + OctileCrossedHeuristic(n));
                    continue;
                }
                if (gCost >= _gCost.GetIfHas(n, int.MaxValue))
                    continue;
                _gCost.Set(n, gCost);
                _parent[n] = cur + 1;
                // _open.UpdatePriority(node, gCost + OctileCrossedHeuristic(n));
            }
        } while (_open.Count > 0);
        return new ReadOnlySpan<Point>(Array.Empty<Point>());
    }
    ReadOnlySpan<Point> BuildPath() {
        var start = _sY * Width + _sX;
        var arr = ArrayPool<Point>.Shared.Rent(Width * Height);
        var pathLength = 0;
        var cur = _eY * Width + _eX;
        while (cur != start) {
            int x = cur % Width, y = cur / Width;
            arr[pathLength++] = new Point(x, y);
            cur = _parent[cur] - 1;
        }
        ArrayPool<Point>.Shared.Return(arr);
        var r = new Span<Point>(arr, 0, pathLength);
        MemoryExtensions.Reverse(r);
        return r;
    }

    ReadOnlySpan<int> Get4DirNeighbours(int i) {
        var arr = ArrayPool<int>.Shared.Rent(4);
        int count = 0,
            x = i % Width,
            y = i / Width;
        int n = i - Width; // top
        if (y > 0 && (_flags[n] & Flags.IsWall) == 0 && !_closed.Has(n))
            arr[count++] = n;
        n = i + Width; // bottom
        if (y < Height - 1 && (_flags[n] & Flags.IsWall) == 0 && !_closed.Has(n))
            arr[count++] = n;
        n = i - 1; // left
        if (x > 0 && (_flags[n] & Flags.IsWall) == 0 && !_closed.Has(n))
            arr[count++] = n;
        n = i + 1; // right
        if (x < Width - 1 && (_flags[n] & Flags.IsWall) == 0 && !_closed.Has(n))
            arr[count++] = n;
        ArrayPool<int>.Shared.Return(arr);
        return new ReadOnlySpan<int>(arr, 0, count);
    }
    ReadOnlySpan<int> Get8DirNeighbours(int i) {
        var arr = ArrayPool<int>.Shared.Rent(7);
        int count = 0,
            x = i % Width,
            y = i / Width;
        bool isWallLeft = true,
            isWallRight = true;
        int n = i - 1; // left
        if (x > 0 && (_flags[n] & Flags.IsWall) == 0) {
            if (!_closed.Has(n))
                arr[count++] = n;
            isWallLeft = false;
        }
        n = i + 1; // right
        if (x < Width - 1 && (_flags[n] & Flags.IsWall) == 0) {
            if (!_closed.Has(n))
                arr[count++] = n;
            isWallRight = false;
        }
        n = i - Width;
        if (y > 0 && (_flags[n] & Flags.IsWall) == 0) {
            if (!_closed.Has(n))
                arr[count++] = n;
            if (!isWallLeft) {
                n = (i - Width) - 1; // top-left
                if ((_flags[n] & Flags.IsWall) == 0 && !_closed.Has(n))
                    arr[count++] = n;
            }
            if (!isWallRight) {
                n = (i - Width) + 1; // top-right
                if ((_flags[n] & Flags.IsWall) == 0 && !_closed.Has(n))
                    arr[count++] = n;
            }
        }
        n = i + Width; // bottom
        if (y < Height - 1 && (_flags[n] & Flags.IsWall) == 0) {
            if (!_closed.Has(n))
                arr[count++] = n;
            if (!isWallLeft) {
                n = (i + Width) - 1; // bottom-left
                if ((_flags[n] & Flags.IsWall) == 0 && !_closed.Has(n))
                    arr[count++] = n;
            }
            if (!isWallRight) {
                n = (i + Width) + 1; // bottom-right
                if ((_flags[n] & Flags.IsWall) == 0 && !_closed.Has(n))
                    arr[count++] = n;
            }
        }
        ArrayPool<int>.Shared.Return(arr);
        return new ReadOnlySpan<int>(arr, 0, count);
    }
    int Cost(int a, int b) {
        var d = Math.Abs(a - b);
        var cost = _cost[a];
        if (d <= 1 || d == Width)
            return 10 + cost;
        return 14 + cost;
    }
    float OctileCrossedHeuristic(int i) {
        int x = i % Width,
            y = i / Width,
            dX1 = x - _eX,
            dY1 = y - _eY,
            dX = Math.Abs(dX1),
            dY = Math.Abs(dY1),
            dX2 = _sX - _eX,
            dY2 = _sY - _eY,
            cross = Math.Abs((dX1 * dY2) - (dX2 * dY1)),
            straight = Math.Abs(dX - dY);
        return (10 * straight) + (14 * (Math.Max(dX, dY) - straight)) + (cross * .001f);
    }
}