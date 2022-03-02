using System.Buffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Dcrew.Spatial;

public struct Quadtree {
    struct Node {
        public float X, Y, Width, Height;
        public readonly float CenterX, CenterY;
        public readonly int Parent;
        public int Child;
        public readonly byte Depth;

        public Node(float x, float y, byte depth, int parent) {
            X = float.MaxValue;
            Y = float.MaxValue;
            Width = float.MinValue;
            Height = float.MinValue;
            CenterX = x;
            CenterY = y;
            Parent = parent;
            Child = 0;
            Depth = depth;
        }
    }

    struct TreeItem {
        public float X, Y, Width, Height;
        public int Next, Node;
    }

    public struct QueryList : IDisposable {
        readonly int[] _item;
        readonly int _count;

        public int Count => _count;
        public ReadOnlySpan<int> All => new(_item, 0, _count);

        public QueryList(int[] item, int count) {
            _item = item;
            _count = count;
        }

        public bool Has(int i) {
            for (int j = 0; j < _count; j++)
                if (_item[j] == i)
                    return true;
            return false;
        }

        public void Dispose() {
            ArrayPool<int>.Shared.Return(_item);
        }
    }

    float _x, _y, _width, _height;
    readonly byte _maxDepth;
    readonly Node[] _node;
    int _freeNode;
    TreeItem[] _item;
    readonly Stack<int> _toProcess, _toProcess2;
    readonly HashSet<int> _nodesToResize;
    float _newX, _newY, _newWidth, _newHeight;

    /// <summary>Get/set the max items that can be set</summary>
    public int MaxItems {
        get { return _item.Length; }
        set {
            if (value < _item.Length)
                for (var i = value; i < _item.Length; i++)
                    if (_item[i].Next != -2)
                        Remove(i);
            Array.Resize(ref _item, value);
        }
    }

    /// <summary>Create a new tree with the desired bounds, max items and max depth</summary>
    /// <param name="maxItems">Initial max items that can be set</param>
    /// <param name="maxDepth">Amount of layers this tree can subdivide into</param>
    public Quadtree(float x, float y, float width, float height, int maxItems, byte maxDepth = 8) {
        _newX = _x = x;
        _newY = _y = y;
        _newWidth = _width = width;
        _newHeight = _height = height;
        _maxDepth = maxDepth;
        var nodes = 1;
        for (var i = 0; i < maxDepth; i++)
            nodes += (int)Math.Pow(4, i + 1);
        _node = new Node[nodes];
        _node[0] = new Node(x + (width * .5f), y + (height * .5f), 0, -1);
        _freeNode = 1;
        _item = new TreeItem[maxItems];
        for (var i = 0; i < _item.Length; i++)
            _item[i].Next = -2;
        _toProcess = new Stack<int>();
        _toProcess2 = new Stack<int>();
        _nodesToResize = new HashSet<int>();
    }

    /// <summary>Return the latest bounds of item <paramref name="i"/></summary>
    public (float X, float Y, float Width, float Height) this[int i] {
        get {
            ref readonly var item = ref _item[i];
            return (item.X, item.Y, item.Width, item.Height);
        }
    }

    /// <summary>Set id <paramref name="i"/> to the given bounds</summary>
    public void Update(int i, Rectangle rect) { Update(i, rect.X, rect.Y, rect.Width, rect.Height); }
    /// <summary>Set id <paramref name="i"/> to the given bounds</summary>
    public void Update(int i, float x, float y, float width, float height) {
        if (x + (width * .5f) < _newX)
            _newX = x + (width * .5f);
        if (y + (height * .5f) < _newY)
            _newY = y + (height * .5f);
        if ((x + (width * .5f)) - _newX > _newWidth)
            _newWidth = (x + (width * .5f)) - _newX;
        if ((y + (height * .5f)) - _newY > _newHeight)
            _newHeight = (y + (height * .5f)) - _newY;
        ref var item = ref _item[i];
        item.X = x;
        item.Y = y;
        item.Width = width;
        item.Height = height;
        var newNode = FindNode(x + (width * .5f), y + (height * .5f));
        if (item.Next != -2) {
            ref var n = ref _node[item.Node];
            if (newNode == item.Node) {
                PropagateUnion(ref n, x, y, width, height);
                return;
            }
            int o = n.Child - 1;
            if (o == i) {
                n.Child = item.Next + 1;
            } else {
                int prev;
                do {
                    prev = o;
                    o = _item[o].Next;
                } while (o != i);
                ref var j = ref _item[prev];
                j.Next = item.Next;
            }
        }
        item.Next = -1;
        item.Node = newNode;
        ref var node = ref _node[newNode];
        int itemCount = 1;
        if (node.Child == 0)
            node.Child = i + 1;
        else {
            ref var j = ref _item[node.Child - 1];
            itemCount++;
            while (j.Next != -1) {
                j = ref _item[j.Next];
                itemCount++;
            }
            j.Next = i;
        }
        if (itemCount >= 8 && node.Depth < _maxDepth)
            _nodesToResize.Add(newNode);
        PropagateUnion(ref node, x, y, width, height);
    }
    /// <summary>Remove id <paramref name="i"/>, no re-ordering, <paramref name="i"/> will be available for you to re-use</summary>
    public void Remove(int i) {
        ref var item = ref _item[i];
        ref var node = ref _node[item.Node];
        int o = node.Child - 1;
        if (o == i) {
            node.Child = item.Next + 1;
        } else {
            int prev;
            do {
                prev = o;
                o = _item[o].Next;
            } while (o != i);
            ref var j = ref _item[prev];
            j.Next = item.Next;
        }
        item.Next = -2;
    }
    /// <summary>Returns true if id <paramref name="i"/> has been added</summary>
    public bool Contains(int i) {
        return _item[i].Next != -2;
    }
    /// <summary>Clear all items and nodes</summary>
    public void Clear() {
        for (var i = 0; i < _item.Length; i++) {
            ref var item = ref _item[i];
            item.Next = -2;
        }
        _node[0].Child = 0;
        _freeNode = 1;
    }
    /// <summary>Call this once per frame preferably at the end of the Update call. This manages sub-dividing and updates node bounds</summary>
    public void Update() {
        if (_newX != _x || _newY != _y || _newWidth != _width || _newHeight != _height) {
            _x = _newX;
            _y = _newY;
            _width = _newWidth;
            _height = _newHeight;
            var toAdd = ArrayPool<int>.Shared.Rent(_item.Length);
            var toAddCount = 0;
            for (var i = 0; i < _item.Length; i++) {
                ref var item = ref _item[i];
                if (item.Next != -2) {
                    toAdd[toAddCount++] = i;
                    item.Next = -2;
                }
            }
            _node[0].Child = 0;
            _freeNode = 1;
            for (var i = 0; i < toAddCount; i++) {
                ref readonly var item = ref _item[toAdd[i]];
                Update(toAdd[i], item.X, item.Y, item.Width, item.Height);
            }
            ArrayPool<int>.Shared.Return(toAdd);
        }
        foreach (int j in _nodesToResize)
            Subdivide(j);
        _nodesToResize.Clear();
    }
    /// <summary>Optimize (minimize) all node bounds. Can be called as frequently/infrequently as wanted</summary>
    public void Optimize() {
        int ni = 0;
        ref var n = ref _node[ni];
        do {
            n.X = float.MaxValue;
            n.Y = float.MaxValue;
            n.Width = float.MinValue;
            n.Height = float.MinValue;
            if (n.Child < 0) {
                int c = Math.Abs(n.Child);
                _toProcess.Push(c);
                _toProcess.Push(c + 1);
                _toProcess.Push(c + 2);
                _toProcess.Push(c + 3);
                Node nw = _node[c],
                    ne = _node[c + 1],
                    sw = _node[c + 2],
                    se = _node[c + 3];
                if (nw.Child != 0) {
                    if (nw.X < n.X)
                        n.X = nw.X;
                    if (nw.Y < n.Y)
                        n.Y = nw.Y;
                    if (nw.X + nw.Width > n.Width)
                        n.Width = nw.X + nw.Width;
                    if (nw.Y + nw.Height > n.Height)
                        n.Height = nw.Y + nw.Height;
                }
                if (ne.Child != 0) {
                    if (ne.X < n.X)
                        n.X = ne.X;
                    if (ne.Y < n.Y)
                        n.Y = ne.Y;
                    if (ne.X + ne.Width > n.Width)
                        n.Width = ne.X + ne.Width;
                    if (ne.Y + ne.Height > n.Height)
                        n.Height = ne.Y + ne.Height;
                }
                if (sw.Child != 0) {
                    if (sw.X < n.X)
                        n.X = sw.X;
                    if (sw.Y < n.Y)
                        n.Y = sw.Y;
                    if (sw.X + sw.Width > n.Width)
                        n.Width = sw.X + sw.Width;
                    if (sw.Y + sw.Height > n.Height)
                        n.Height = sw.Y + sw.Height;
                }
                if (se.Child != 0) {
                    if (se.X < n.X)
                        n.X = se.X;
                    if (se.Y < n.Y)
                        n.Y = se.Y;
                    if (se.X + se.Width > n.Width)
                        n.Width = se.X + se.Width;
                    if (se.Y + se.Height > n.Height)
                        n.Height = se.Y + se.Height;
                }
            } else if (n.Child > 0) {
                ref readonly var i = ref _item[n.Child - 1];
                do {
                    if (i.X < n.X)
                        n.X = i.X;
                    if (i.Y < n.Y)
                        n.Y = i.Y;
                    if (i.X + i.Width > n.Width)
                        n.Width = i.X + i.Width;
                    if (i.Y + i.Height > n.Height)
                        n.Height = i.Y + i.Height;
                    if (i.Next == -1)
                        break;
                    i = ref _item[i.Next];
                } while (true);
            }
            n.Width -= n.X;
            n.Height -= n.Y;
            if (_toProcess.Count <= 0)
                break;
            ni = _toProcess.Pop();
            n = ref _node[ni];
        } while (true);
    }
    public void Draw(SpriteBatch spriteBatch, RectStyle style, float thickness = 1, float opacity = .5f) {
        int ni = 0;
        ref readonly var n = ref _node[ni];
        do {
            if (n.Child < 0) {
                int c = Math.Abs(n.Child);
                _toProcess.Push(c);
                _toProcess.Push(c + 1);
                _toProcess.Push(c + 2);
                _toProcess.Push(c + 3);
            } else if (n.Child > 0) {
                ref readonly var j = ref _item[n.Child - 1];
                spriteBatch.DrawRectangle(j.X, j.Y, j.Width, j.Height, Color.LawnGreen * opacity, style, thickness: thickness, layerDepth: 1);
                while (j.Next != -1) {
                    j = ref _item[j.Next];
                    spriteBatch.DrawRectangle(j.X, j.Y, j.Width, j.Height, Color.LawnGreen * opacity, style, thickness: thickness, layerDepth: 1);
                }
            }
            spriteBatch.DrawRectangle(n.X, n.Y, n.Width, n.Height, Color.Blue * opacity, style, thickness: thickness, layerDepth: 1);
            if (_toProcess.Count <= 0)
                break;
            ni = _toProcess.Pop();
            n = ref _node[ni];
        } while (true);
    }
    public void Draw(SpriteBatch spriteBatch, Rectangle area, RectStyle style, float thickness = 1, float opacity = .5f) => Draw(spriteBatch, new Quad(area), style, thickness, opacity);
    public void Draw(SpriteBatch spriteBatch, RotRect area, RectStyle style, float thickness = 1, float opacity = .5f) {
        int ni = 0;
        ref readonly var n = ref _node[ni];
        do {
            if (n.Child < 0) {
                int c = Math.Abs(n.Child);
                Node nw = _node[c],
                    ne = _node[c + 1],
                    sw = _node[c + 2],
                    se = _node[c + 3];
                if (area.Intersects(new Quad(nw.X, nw.Y, nw.Width, nw.Height)))
                    _toProcess.Push(c);
                if (area.Intersects(new Quad(ne.X, ne.Y, ne.Width, ne.Height)))
                    _toProcess.Push(c + 1);
                if (area.Intersects(new Quad(sw.X, sw.Y, sw.Width, sw.Height)))
                    _toProcess.Push(c + 2);
                if (area.Intersects(new Quad(se.X, se.Y, se.Width, se.Height)))
                    _toProcess.Push(c + 3);
            } else if (n.Child > 0) {
                int i = n.Child - 1;
                do {
                    ref readonly var item = ref _item[i];
                    if (area.Intersects(new Quad(item.X, item.Y, item.Width, item.Height)))
                        spriteBatch.DrawRectangle(item.X, item.Y, item.Width, item.Height, Color.LawnGreen * opacity, style, thickness: thickness, layerDepth: 1);
                    i = item.Next;
                } while (i != -1);
            }
            spriteBatch.DrawRectangle(n.X, n.Y, n.Width, n.Height, Color.Blue * opacity, style, thickness: thickness, layerDepth: 1);
            if (_toProcess.Count <= 0)
                break;
            ni = _toProcess.Pop();
            n = ref _node[ni];
        } while (true);
    }
    public void Draw(SpriteBatch spriteBatch, Quad area, RectStyle style, float thickness = 1, float opacity = .5f) {
        int ni = 0;
        ref readonly var n = ref _node[ni];
        do {
            if (n.Child < 0) {
                int c = Math.Abs(n.Child);
                Node nw = _node[c],
                    ne = _node[c + 1],
                    sw = _node[c + 2],
                    se = _node[c + 3];
                if (area.Intersects(new Quad(nw.X, nw.Y, nw.Width, nw.Height)))
                    _toProcess.Push(c);
                if (area.Intersects(new Quad(ne.X, ne.Y, ne.Width, ne.Height)))
                    _toProcess.Push(c + 1);
                if (area.Intersects(new Quad(sw.X, sw.Y, sw.Width, sw.Height)))
                    _toProcess.Push(c + 2);
                if (area.Intersects(new Quad(se.X, se.Y, se.Width, se.Height)))
                    _toProcess.Push(c + 3);
            } else if (n.Child > 0) {
                int i = n.Child - 1;
                do {
                    ref readonly var item = ref _item[i];
                    if (area.Intersects(new Quad(item.X, item.Y, item.Width, item.Height)))
                        spriteBatch.DrawRectangle(item.X, item.Y, item.Width, item.Height, Color.LawnGreen * opacity, style, thickness: thickness, layerDepth: 1);
                    i = item.Next;
                } while (i != -1);
            }
            spriteBatch.DrawRectangle(n.X, n.Y, n.Width, n.Height, Color.Blue * opacity, style, thickness: thickness, layerDepth: 1);
            if (_toProcess.Count <= 0)
                break;
            ni = _toProcess.Pop();
            n = ref _node[ni];
        } while (true);
    }
    public void Draw(SpriteBatch spriteBatch, ConvPoly area, RectStyle style, float thickness = 1, float opacity = .5f) {
        int ni = 0;
        ref readonly var n = ref _node[ni];
        do {
            if (n.Child < 0) {
                int c = Math.Abs(n.Child);
                Node nw = _node[c],
                    ne = _node[c + 1],
                    sw = _node[c + 2],
                    se = _node[c + 3];
                if (area.Intersects(new Quad(nw.X, nw.Y, nw.Width, nw.Height)))
                    _toProcess.Push(c);
                if (area.Intersects(new Quad(ne.X, ne.Y, ne.Width, ne.Height)))
                    _toProcess.Push(c + 1);
                if (area.Intersects(new Quad(sw.X, sw.Y, sw.Width, sw.Height)))
                    _toProcess.Push(c + 2);
                if (area.Intersects(new Quad(se.X, se.Y, se.Width, se.Height)))
                    _toProcess.Push(c + 3);
            } else if (n.Child > 0) {
                int i = n.Child - 1;
                do {
                    ref readonly var item = ref _item[i];
                    if (area.Intersects(new Quad(item.X, item.Y, item.Width, item.Height)))
                        spriteBatch.DrawRectangle(item.X, item.Y, item.Width, item.Height, Color.LawnGreen * opacity, style, thickness: thickness, layerDepth: 1);
                    i = item.Next;
                } while (i != -1);
            }
            spriteBatch.DrawRectangle(n.X, n.Y, n.Width, n.Height, Color.Blue * opacity, style, thickness: thickness, layerDepth: 1);
            if (_toProcess.Count <= 0)
                break;
            ni = _toProcess.Pop();
            n = ref _node[ni];
        } while (true);
    }

    /// <summary>Query and return a collection of ids that intersect the given rectangle</summary>
    public QueryList Query(Rectangle rect) { return Query(rect.X, rect.Y, rect.Width, rect.Height); }
    /// <summary>Query and return a collection of ids that intersect the given rectangle</summary>
    public QueryList Query(float x, float y, float width, float height) {
        float right = x + width,
            bottom = y + height;
        var yield = ArrayPool<int>.Shared.Rent(_item.Length);
        var totalItems = 0;
        ref readonly var n = ref _node[0];
        do {
            if (n.Child < 0) {
                int c = Math.Abs(n.Child);
                Node nw = _node[c],
                    ne = _node[c + 1],
                    sw = _node[c + 2],
                    se = _node[c + 3];
                if (new Quad(x, y, width, height).Contains(new Quad(n.X, n.Y, n.Width, n.Height))) {
                    _toProcess2.Push(c);
                    _toProcess2.Push(c + 1);
                    _toProcess2.Push(c + 2);
                    _toProcess2.Push(c + 3);
                } else {
                    if (nw.X < right && x < nw.X + nw.Width && nw.Y < bottom && y < nw.Y + nw.Height)
                        _toProcess.Push(c);
                    if (ne.X < right && x < ne.X + ne.Width && ne.Y < bottom && y < ne.Y + ne.Height)
                        _toProcess.Push(c + 1);
                    if (sw.X < right && x < sw.X + sw.Width && sw.Y < bottom && y < sw.Y + sw.Height)
                        _toProcess.Push(c + 2);
                    if (se.X < right && x < se.X + se.Width && se.Y < bottom && y < se.Y + se.Height)
                        _toProcess.Push(c + 3);
                }
            } else if (n.Child > 0) {
                int i = n.Child - 1;
                do {
                    ref readonly var item = ref _item[i];
                    if (item.X < right && x < item.X + item.Width && item.Y < bottom && y < item.Y + item.Height)
                        yield[totalItems++] = i;
                    i = item.Next;
                } while (i != -1);
            }
            if (_toProcess.Count <= 0)
                break;
            n = ref _node[_toProcess.Pop()];
        } while (true);
        while (_toProcess2.Count > 0) {
            n = ref _node[_toProcess2.Pop()];
            if (n.Child < 0) {
                int c = Math.Abs(n.Child);
                _toProcess2.Push(c);
                _toProcess2.Push(c + 1);
                _toProcess2.Push(c + 2);
                _toProcess2.Push(c + 3);
            } else if (n.Child > 0) {
                int i = n.Child - 1;
                do {
                    ref readonly var item = ref _item[i];
                    if (new Quad(x, y, width, height).Intersects(new Quad(item.X, item.Y, item.Width, item.Height)))
                        yield[totalItems++] = i;
                    i = item.Next;
                } while (i != -1);
            }
        }
        return new QueryList(yield, totalItems);
    }
    /// <summary>Query and return a collection of ids that intersect the given rectangle</summary>
    public QueryList Query(Rectangle rect, float rotation, float originX, float originY) { return Query(rect.X, rect.Y, rect.Width, rect.Height, rotation, originX, originY); }
    /// <summary>Query and return a collection of ids that intersect the given rectangle</summary>
    public QueryList Query(Rectangle rect, float rotation, Vector2 origin) { return Query(rect.X, rect.Y, rect.Width, rect.Height, rotation, origin.X, origin.Y); }
    /// <summary>Query and return a collection of ids that intersect the given rectangle</summary>
    public QueryList Query(float x, float y, float width, float height, float rotation, Vector2 origin) { return Query(x, y, width, height, rotation, origin.X, origin.Y); }
    /// <summary>Query and return a collection of ids that intersect the given rectangle</summary>
    public QueryList Query(float x, float y, float width, float height, float rotation, float originX, float originY) {
        RotRect rect = new(x, y, width, height, rotation, new Vector2(originX, originY));
        var yield = ArrayPool<int>.Shared.Rent(_item.Length);
        var totalItems = 0;
        ref readonly var n = ref _node[0];
        do {
            if (n.Child < 0) {
                int c = Math.Abs(n.Child);
                Node nw = _node[c],
                    ne = _node[c + 1],
                    sw = _node[c + 2],
                    se = _node[c + 3];
                if (rect.Contains(new Quad(n.X, n.Y, n.Width, n.Height))) {
                    _toProcess2.Push(c);
                    _toProcess2.Push(c + 1);
                    _toProcess2.Push(c + 2);
                    _toProcess2.Push(c + 3);
                } else {
                    if (rect.Intersects(new Quad(nw.X, nw.Y, nw.Width, nw.Height)))
                        _toProcess.Push(c);
                    if (rect.Intersects(new Quad(ne.X, ne.Y, ne.Width, ne.Height)))
                        _toProcess.Push(c + 1);
                    if (rect.Intersects(new Quad(sw.X, sw.Y, sw.Width, sw.Height)))
                        _toProcess.Push(c + 2);
                    if (rect.Intersects(new Quad(se.X, se.Y, se.Width, se.Height)))
                        _toProcess.Push(c + 3);
                }
            } else if (n.Child > 0) {
                int i = n.Child - 1;
                do {
                    ref readonly var item = ref _item[i];
                    if (rect.Intersects(new Quad(item.X, item.Y, item.Width, item.Height)))
                        yield[totalItems++] = i;
                    i = item.Next;
                } while (i != -1);
            }
            if (_toProcess.Count <= 0)
                break;
            n = ref _node[_toProcess.Pop()];
        } while (true);
        while (_toProcess2.Count > 0) {
            n = ref _node[_toProcess2.Pop()];
            if (n.Child < 0) {
                int c = Math.Abs(n.Child);
                _toProcess2.Push(c);
                _toProcess2.Push(c + 1);
                _toProcess2.Push(c + 2);
                _toProcess2.Push(c + 3);
            } else if (n.Child > 0) {
                int i = n.Child - 1;
                do {
                    ref readonly var item = ref _item[i];
                    if (rect.Intersects(new Quad(item.X, item.Y, item.Width, item.Height)))
                        yield[totalItems++] = i;
                    i = item.Next;
                } while (i != -1);
            }
        }
        return new QueryList(yield, totalItems);
    }
    /// <summary>Query and return a collection of ids that intersect the given point/radius</summary>
    public QueryList Query(Point p, float radius = 1) { return Query(p.X, p.Y, radius); }
    /// <summary>Query and return a collection of ids that intersect the given point/radius</summary>
    public QueryList Query(Vector2 p, float radius = 1) { return Query(p.X, p.Y, radius); }
    /// <summary>Query and return a collection of ids that intersect the given point/radius</summary>
    public QueryList Query(float x, float y, float radius = 1) {
        bool Intersects((float x, float y, float width, float height) rect) {
            float dx = MathF.Abs(x - (rect.x + (rect.width * .5f))),
                dy = MathF.Abs(y - (rect.y + (rect.height * .5f)));
            if (dx > (rect.width * .5f) + radius || dy > (rect.height * .5f) + radius)
                return false;
            if (dx <= rect.width * .5f || dy <= rect.height * .5f)
                return true;
            float fx = dx - (rect.width * .5f),
                fy = dy - (rect.height * .5f);
            fx *= fx;
            fy *= fy;
            return fx + fy <= radius * radius;
        }
        var yield = ArrayPool<int>.Shared.Rent(_item.Length);
        var totalItems = 0;
        ref readonly var n = ref _node[0];
        do {
            if (n.Child < 0) {
                int c = Math.Abs(n.Child);
                Node nw = _node[c],
                    ne = _node[c + 1],
                    sw = _node[c + 2],
                    se = _node[c + 3];
                if (Intersects((nw.X, nw.Y, nw.Width, nw.Height)))
                    _toProcess.Push(c);
                if (Intersects((ne.X, ne.Y, ne.Width, ne.Height)))
                    _toProcess.Push(c + 1);
                if (Intersects((sw.X, sw.Y, sw.Width, sw.Height)))
                    _toProcess.Push(c + 2);
                if (Intersects((se.X, se.Y, se.Width, se.Height)))
                    _toProcess.Push(c + 3);
            } else if (n.Child > 0) {
                int i = n.Child - 1;
                do {
                    ref readonly var item = ref _item[i];
                    if (Intersects((item.X, item.Y, item.Width, item.Height)))
                        yield[totalItems++] = i;
                    i = item.Next;
                } while (i != -1);
            }
            if (_toProcess.Count <= 0)
                break;
            n = ref _node[_toProcess.Pop()];
        } while (true);
        return new QueryList(yield, totalItems);
    }
    /// <summary>Query and return a collection of ids that intersect the given ray</summary>
    public QueryList Raycast(Vector2 position, Point direction, float thickness = 1) { return Raycast(position.X, position.Y, direction.X, direction.Y, thickness); }
    /// <summary>Query and return a collection of ids that intersect the given ray</summary>
    public QueryList Raycast(Point position, Point direction, float thickness = 1) { return Raycast(position.X, position.Y, direction.X, direction.Y, thickness); }
    /// <summary>Query and return a collection of ids that intersect the given ray</summary>
    public QueryList Raycast(Point position, Vector2 direction, float thickness = 1) { return Raycast(position.X, position.Y, direction.X, direction.Y, thickness); }
    /// <summary>Query and return a collection of ids that intersect the given ray</summary>
    public QueryList Raycast(Point position, float directionX, float directionY, float thickness = 1) { return Raycast(position.X, position.Y, directionX, directionY, thickness); }
    /// <summary>Query and return a collection of ids that intersect the given ray</summary>
    public QueryList Raycast(Point position, float rotation, float thickness = 1) { return Raycast(position.X, position.Y, rotation, thickness); }
    /// <summary>Query and return a collection of ids that intersect the given ray</summary>
    public QueryList Raycast(Vector2 position, Vector2 direction, float thickness = 1) { return Raycast(position.X, position.Y, direction.X, direction.Y, thickness); }
    /// <summary>Query and return a collection of ids that intersect the given ray</summary>
    public QueryList Raycast(Vector2 position, float directionX, float directionY, float thickness = 1) { return Raycast(position.X, position.Y, directionX, directionY, thickness); }
    /// <summary>Query and return a collection of ids that intersect the given ray</summary>
    public QueryList Raycast(Vector2 position, float rotation, float thickness = 1) { return Raycast(position.X, position.Y, rotation, thickness); }
    /// <summary>Query and return a collection of ids that intersect the given ray</summary>
    public QueryList Raycast(float x, float y, Vector2 direction, float thickness = 1) { return Raycast(x, y, direction.X, direction.Y, thickness); }
    /// <summary>Query and return a collection of ids that intersect the given ray</summary>
    public QueryList Raycast(float x, float y, float directionX, float directionY, float thickness = 1) {
        var rotation = MathF.Atan2(directionY, directionX);
        return Query(x, y, float.MaxValue, thickness, rotation, 0, thickness * .5f);
    }
    /// <summary>Query and return a collection of ids that intersect the given ray</summary>
    public QueryList Raycast(float x, float y, float rotation, float thickness = 1) { return Query(x, y, float.MaxValue, thickness, rotation, 0, thickness * .5f); }
    /// <summary>Query and return a collection of ids that intersect the given line</summary>
    public QueryList Linecast(Point a, Point b, float thickness = 1) { return Linecast(a.X, a.Y, b.X, b.Y, thickness); }
    /// <summary>Query and return a collection of ids that intersect the given line</summary>
    public QueryList Linecast(Point a, Vector2 b, float thickness = 1) { return Linecast(a.X, a.Y, b.X, b.Y, thickness); }
    /// <summary>Query and return a collection of ids that intersect the given line</summary>
    public QueryList Linecast(Vector2 a, Point b, float thickness = 1) { return Linecast(a.X, a.Y, b.X, b.Y, thickness); }
    /// <summary>Query and return a collection of ids that intersect the given line</summary>
    public QueryList Linecast(float aX, float aY, Point b, float thickness = 1) { return Linecast(aX, aY, b.X, b.Y, thickness); }
    /// <summary>Query and return a collection of ids that intersect the given line</summary>
    public QueryList Linecast(Point a, float bX, float bY, float thickness = 1) { return Linecast(a.X, a.Y, bX, bY, thickness); }
    /// <summary>Query and return a collection of ids that intersect the given line</summary>
    public QueryList Linecast(Vector2 a, Vector2 b, float thickness = 1) { return Linecast(a.X, a.Y, b.X, b.Y, thickness); }
    /// <summary>Query and return a collection of ids that intersect the given line</summary>
    public QueryList Linecast(float aX, float aY, Vector2 b, float thickness = 1) { return Linecast(aX, aY, b.X, b.Y, thickness); }
    /// <summary>Query and return a collection of ids that intersect the given line</summary>
    public QueryList Linecast(Vector2 a, float bX, float bY, float thickness = 1) { return Linecast(a.X, a.Y, bX, bY, thickness); }
    /// <summary>Query and return a collection of ids that intersect the given line</summary>
    public QueryList Linecast(float x1, float y1, float x2, float y2, float thickness = 1) {
        var rotation = MathF.Atan2(y2 - y1, x2 - x1);
        return Query(x1, y1, Vector2.Distance(new Vector2(x1, y1), new Vector2(x2, y2)), thickness, rotation, 0, thickness * .5f);
    }
    /// <summary>Query and return a collection of ids that have been added</summary>
    public QueryList All {
        get {
            var yield = ArrayPool<int>.Shared.Rent(_item.Length);
            var totalItems = 0;
            for (var i = 0; i < _item.Length; i++)
                if (_item[i].Next != -2)
                    yield[totalItems++] = i;
            return new QueryList(yield, totalItems);
        }
    }

    int FindNode(float x, float y, int i = 0) {
        ref readonly var n = ref _node[i];
        while (n.Child < 0) {
            i = x > n.CenterX ?
                y > n.CenterY ?
                Math.Abs(n.Child) + 3 :
                Math.Abs(n.Child) + 1 :
                y > n.CenterY ?
                Math.Abs(n.Child) + 2 :
                Math.Abs(n.Child);
            n = ref _node[i];
        }
        return i;
    }

    void Subdivide(int node) {
        ref var n = ref _node[node];
        var d = (byte)(n.Depth + 1);
        var dp2 = 1 << d;
        float w = _width * .5f / dp2,
            h = _height * .5f / dp2;
        Node nw = new Node(n.CenterX - w, n.CenterY - h, d, node),
            ne = new Node(n.CenterX + w, nw.CenterY, d, node),
            sw = new Node(nw.CenterX, n.CenterY + h, d, node),
            se = new Node(ne.CenterX, sw.CenterY, d, node);
        int i = n.Child - 1;
        n.Child = -_freeNode;
        _node[_freeNode++] = nw;
        _node[_freeNode++] = ne;
        _node[_freeNode++] = sw;
        _node[_freeNode++] = se;
        do {
            if (i == -1)
                break;
            ref var item = ref _item[i];
            int next = item.Next;
            item.Next = -1;
            int ni = FindNode(item.X + (item.Width * .5f), item.Y + (item.Height * .5f), node);
            n = ref _node[ni];
            item.Node = ni;
            int itemCount = 1;
            if (n.Child == 0)
                n.Child = i + 1;
            else {
                ref var j = ref _item[n.Child - 1];
                itemCount++;
                while (j.Next != -1) {
                    j = ref _item[j.Next];
                    itemCount++;
                }
                j.Next = i;
            }
            i = next;
            PropagateUnion(ref n, item.X, item.Y, item.Width, item.Height);
            if (itemCount >= 8 && n.Depth < _maxDepth)
                Subdivide(ni);
        } while (i != -1);
    }

    void PropagateUnion(ref Node n, float x, float y, float width, float height) {
        do {
            var resized = false;
            if (x < n.X) {
                n.Width = n.X + n.Width - x;
                n.X = x;
                resized = true;
            }
            if (y < n.Y) {
                n.Height = n.Y + n.Height - y;
                n.Y = y;
                resized = true;
            }
            if (x + width > n.X + n.Width) {
                n.Width = x + width - n.X;
                resized = true;
            }
            if (y + height > n.Y + n.Height) {
                n.Height = y + height - n.Y;
                resized = true;
            }
            if (n.Parent == -1 || !resized)
                break;
            n = ref _node[n.Parent];
        } while (true);
    }
}