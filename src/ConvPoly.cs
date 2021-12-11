using Microsoft.Xna.Framework;

namespace Dcrew;

/// <summary>A convex polygon</summary>
public struct ConvPoly {
    internal ref struct Simplex {
        internal int _count;
        internal Vector2 _a, _b, _c;

        public Simplex(Vector2 a, Vector2 b) {
            _a = a;
            _b = b;
            _c = default;
            _count = 2;
        }

        public void Add(Vector2 v) {
            _c = v;
            _count = 3;
        }
        public void Expand(Vector2 v) {
            _c = _b;
            _b = v;
            _count = 3;
        }

        public Vector2? CalcDir() {
            Vector2 a, b, ab, abPerp;
            if (_count == 3) {
                a = _c;
                var ao = -a;
                b = _b;
                var c = _a;
                ab = b - a;
                var ac = c - a;
                abPerp = new Vector2(ab.Y, -ab.X);
                if ((abPerp.X * c.X) + (abPerp.Y * c.Y) >= 0)
                    abPerp = -abPerp;
                if ((abPerp.X * ao.X) + (abPerp.Y * ao.Y) > 0) {
                    _a = _b;
                    _b = _c;
                    _count = 2;
                    return abPerp;
                }
                var acPerp = new Vector2(ac.Y, -ac.X);
                if ((acPerp.X * b.X) + (acPerp.Y * b.Y) >= 0)
                    acPerp = -acPerp;
                if ((acPerp.X * ao.X) + (acPerp.Y * ao.Y) > 0) {
                    _b = _c;
                    _count = 2;
                    return acPerp;
                }
                return null;
            }
            a = _b;
            b = _a;
            ab = b - a;
            abPerp = new Vector2(ab.Y, -ab.X);
            if ((abPerp.X * -a.X) + (abPerp.Y * -a.Y) <= 0)
                abPerp = -abPerp;
            return abPerp;
        }
    }

    internal ref struct Polytope {
        readonly List<Vector2> _verts;

        public Polytope(Simplex simplex) {
            _verts = new List<Vector2>(4) {
                simplex._a,
                simplex._b
            };
            if (simplex._count == 3)
                _verts.Add(simplex._c);
        }

        public void Insert(int i, Vector2 v) => _verts.Insert(i, v);

        public Edge GetClosestEdge() {
            Edge closest = new();
            closest.Distance = float.PositiveInfinity;
            for (int i = 0; i < _verts.Count; i++) {
                int j = i == _verts.Count - 1 ? 0 : i + 1;
                Vector2 a = _verts[i], b = _verts[j];
                var e = b - a;
                var normal = Vector2.Normalize(new Vector2(e.Y, -e.X));
                var d = Vector2.Dot(normal, a);
                if (d < 0) {
                    d = -d;
                    normal *= -1;
                }
                if (d < closest.Distance) {
                    closest.Distance = d;
                    closest.Normal = normal;
                    closest.Index = j;
                }
            }
            return closest;
        }
    }

    internal struct Edge {
        public float Distance;
        public Vector2 Normal;
        public int Index;
    }

    internal static Vector2 FarthestPInDir(ReadOnlySpan<Vector2> verts, Vector2 dir) {
        var p = verts[0];
        var d = Vector2.Dot(p, dir);
        for (int i = 1; i < verts.Length; i++) {
            var p2 = verts[i];
            var dp2 = Vector2.Dot(p2, dir);
            if (dp2 > d) {
                p = p2;
                d = dp2;
            }
        }
        return p;
    }

    internal static Vector2 Support(Vector2[] v1, (Vector2 a, Vector2 b, Vector2 c, Vector2 d) v2, Vector2 dir) {
        var a = FarthestPInDir(v1, dir);
        var b = Quad.FarthestPInDir(v2.a, v2.b, v2.c, v2.d, -dir);
        return a - b;
    }
    internal static Vector2 Support(ReadOnlySpan<Vector2> verts1, ReadOnlySpan<Vector2> verts2, Vector2 dir) {
        var a = FarthestPInDir(verts1, dir);
        var b = FarthestPInDir(verts2, -dir);
        return a - b;
    }

    internal static bool IsLeft(Vector2 a, Vector2 b, Vector2 p) => (b.X - a.X) * (p.Y - a.Y) - (p.X - a.X) * (b.Y - a.Y) > 0;

    float _rotation;
    Vector2 _xy;
    public Vector2[] Verts;
    public float Rotation {
        get => _rotation;
        set {
            static Vector2 RotatePoint(Vector2 p, float cos, float sin) {
                float xcos = p.X * cos,
                    ycos = p.Y * cos,
                    xsin = p.X * sin,
                    ysin = p.Y * sin;
                return new Vector2(xcos - ysin, xsin + ycos);
            }
            float cos = MathF.Cos(-_rotation),
                sin = MathF.Sin(-_rotation),
                cos2 = MathF.Cos(value),
                sin2 = MathF.Sin(value);
            for (int i = 0; i < Verts.Length; i++) {
                ref Vector2 v = ref Verts[i];
                v = RotatePoint(v - _xy, cos, sin);
                v = RotatePoint(v, cos2, sin2) + _xy;
            }
            _rotation = value;
        }
    }

    /// <summary>A <see cref="Vector2"/> located in the center of this <see cref="ConvPoly"/></summary>
    public Vector2 Center {
        get {
            Vector2 v = new(0, 0);
            for (int i = 0; i < Verts.Length; i++)
                v += Verts[i];
            return v / Verts.Length;
        }
    }

    public ConvPoly(params Vector2[] verts) {
        Verts = verts;
        _rotation = 0;
        _xy = Vector2.Zero;
    }

    /// <summary>Gets whether or not the other <see cref="Rectangle"/> intersects with this <see cref="ConvPoly"/></summary>
    /// <param name="value">The other rectangle for testing</param>
    /// <returns><c>true</c> if other <see cref="Rectangle"/> intersects with this <see cref="ConvPoly"/>; <c>false</c> otherwise</returns>
    public bool Intersects(Rectangle value) {
        Vector2 otl = new(value.Left, value.Top),
            otr = new(value.Right, otl.Y),
            obr = new(otr.X, value.Bottom),
            obl = new(otl.X, obr.Y);
        return Intersects(new Quad(otl, otr, obl, obr));
    }
    /// <summary>Gets whether or not the other <see cref="Rectangle"/> intersects with this <see cref="ConvPoly"/></summary>
    /// <param name="value">The other rectangle for testing</param>
    /// <returns><c>true</c> if other <see cref="Rectangle"/> intersects with this <see cref="ConvPoly"/>; <c>false</c> otherwise</returns>
    public bool Intersects(Rectangle value, out CollisionResolution res) {
        Vector2 otl = new(value.Left, value.Top),
            otr = new(value.Right, otl.Y),
            obr = new(otr.X, value.Bottom),
            obl = new(otl.X, obr.Y);
        return Intersects(new Quad(otl, otr, obr, obl), out res);
    }
    /// <summary>Gets whether or not the other <see cref="RotRect"/> intersects with this <see cref="ConvPoly"/></summary>
    /// <param name="value">The other rect for testing</param>
    /// <returns><c>true</c> if other <see cref="RotRect"/> intersects with this <see cref="ConvPoly"/>; <c>false</c> otherwise</returns>
    public bool Intersects(RotRect value) => value.Intersects(this);
    /// <summary>Gets whether or not the other <see cref="RotRect"/> intersects with this <see cref="ConvPoly"/></summary>
    /// <param name="value">The other rect for testing</param>
    /// <returns><c>true</c> if other <see cref="RotRect"/> intersects with this <see cref="ConvPoly"/>; <c>false</c> otherwise</returns>
    public bool Intersects(RotRect value, out CollisionResolution res) {
        float cos = MathF.Cos(value.Rotation),
            sin = MathF.Sin(value.Rotation),
            x = -value.Origin.X,
            y = -value.Origin.Y,
            w = value.Size.X + x,
            h = value.Size.Y + y,
            xcos = x * cos,
            ycos = y * cos,
            xsin = x * sin,
            ysin = y * sin,
            wcos = w * cos,
            wsin = w * sin,
            hcos = h * cos,
            hsin = h * sin;
        Vector2 otl = new(xcos - ysin + value.XY.X, xsin + ycos + value.XY.Y),
            otr = new(wcos - ysin + value.XY.X, wsin + ycos + value.XY.Y),
            obr = new(wcos - hsin + value.XY.X, wsin + hcos + value.XY.Y),
            obl = new(xcos - hsin + value.XY.X, xsin + hcos + value.XY.Y);
        return Intersects(new Quad(otl, otr, obr, obl), out res);
    }
    /// <summary>Gets whether or not the other <see cref="Quad"/> intersects with this <see cref="ConvPoly"/></summary>
    /// <param name="value">The other quad for testing</param>
    /// <returns><c>true</c> if other <see cref="Quad"/> intersects with this <see cref="ConvPoly"/>; <c>false</c> otherwise</returns>
    public bool Intersects(Quad value) => value.Intersects(this);
    /// <summary>Gets whether or not the other <see cref="Quad"/> intersects with this <see cref="ConvPoly"/></summary>
    /// <param name="value">The other quad for testing</param>
    /// <returns><c>true</c> if other <see cref="Quad"/> intersects with this <see cref="ConvPoly"/>; <c>false</c> otherwise</returns>
    public bool Intersects(Quad value, out CollisionResolution res) {
        res = new CollisionResolution();
        Vector2 dir = new(0, 1);
        var v1 = Verts;
        var v2 = (value.A, value.B, value.C, value.D);
        var spa = Support(v1, v2, dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return false;
        dir = -dir;
        var spb = Support(v1, v2, dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return false;
        var simplex = new Simplex(spa, spb);
        var nd = simplex.CalcDir();
        if (!nd.HasValue)
            return false;
        dir = nd.Value;
        do {
            spa = Support(v1, v2, dir);
            if (Vector2.Dot(spa, dir) <= 0)
                return false;
            simplex.Add(spa);
            nd = simplex.CalcDir();
            if (!nd.HasValue)
                break;
            dir = nd.Value;
        } while (true);
        Edge edge;
        Polytope polytope = new(simplex);
        do {
            edge = polytope.GetClosestEdge();
            var sp = Support(v1, v2, edge.Normal);
            var d = Vector2.Dot(sp, edge.Normal);
            if (MathF.Abs(d - edge.Distance) > .001f) {
                edge.Distance = float.PositiveInfinity;
                polytope.Insert(edge.Index, sp);
            }
        } while (edge.Distance == float.PositiveInfinity);
        res.Normal = edge.Normal;
        res.Depth = edge.Distance + .001f;
        return true;
    }
    /// <summary>Gets whether or not the other <see cref="ConvPoly"/> intersects with this <see cref="ConvPoly"/></summary>
    /// <param name="value">The other rectangle for testing</param>
    /// <returns><c>true</c> if other <see cref="ConvPoly"/> intersects with this <see cref="ConvPoly"/>; <c>false</c> otherwise</returns>
    public bool Intersects(ConvPoly value) {
        Vector2 dir = new(0, 1);
        var v1 = Verts;
        var v2 = value.Verts;
        var spa = Support(v1, v2, dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return false;
        dir = -dir;
        var spb = Support(v1, v2, dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return false;
        var simplex = new Simplex(spa, spb);
        var nd = simplex.CalcDir();
        if (!nd.HasValue)
            return false;
        dir = nd.Value;
        do {
            spa = Support(v1, v2, dir);
            if (Vector2.Dot(spa, dir) <= 0)
                return false;
            simplex.Add(spa);
            nd = simplex.CalcDir();
            if (!nd.HasValue)
                break;
            dir = nd.Value;
        } while (true);
        return true;
    }
    /// <summary>Gets whether or not the other <see cref="ConvPoly"/> intersects with this <see cref="ConvPoly"/></summary>
    /// <param name="value">The other rectangle for testing</param>
    /// <returns><c>true</c> if other <see cref="ConvPoly"/> intersects with this <see cref="ConvPoly"/>; <c>false</c> otherwise</returns>
    public bool Intersects(ConvPoly value, out CollisionResolution res) {
        res = new CollisionResolution();
        Vector2 dir = new(0, 1);
        var v1 = Verts;
        var v2 = value.Verts;
        var spa = Support(v1, v2, dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return false;
        dir = -dir;
        var spb = Support(v1, v2, dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return false;
        var simplex = new Simplex(spa, spb);
        var nd = simplex.CalcDir();
        if (!nd.HasValue)
            return false;
        dir = nd.Value;
        do {
            spa = Support(v1, v2, dir);
            if (Vector2.Dot(spa, dir) <= 0)
                return false;
            simplex.Add(spa);
            nd = simplex.CalcDir();
            if (!nd.HasValue)
                break;
            dir = nd.Value;
        } while (true);
        Edge edge;
        Polytope polytope = new(simplex);
        do {
            edge = polytope.GetClosestEdge();
            var sp = Support(v1, v2, edge.Normal);
            var d = Vector2.Dot(sp, edge.Normal);
            if (MathF.Abs(d - edge.Distance) > .001f) {
                edge.Distance = float.PositiveInfinity;
                polytope.Insert(edge.Index, sp);
            }
        } while (edge.Distance == float.PositiveInfinity);
        res.Normal = edge.Normal;
        res.Depth = edge.Distance + .001f;
        return true;
    }

    /// <summary>Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="ConvPoly"/></summary>
    /// <param name="value">The coordinates to check for inclusion in this <see cref="ConvPoly"/></param>
    /// <returns><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="ConvPoly"/>; <c>false</c> otherwise</returns>
    public bool Contains(Vector2 value) {
        for (int i = 0; i < Verts.Length - 1; i++) {
            if (!IsLeft(Verts[i], Verts[i + 1], value))
                return false;
        }
        return !IsLeft(Verts[0], Verts[^1], value);
    }
    /// <summary>Gets whether or not the provided <see cref="Point"/> lies within the bounds of this <see cref="ConvPoly"/></summary>
    /// <param name="value">The coordinates to check for inclusion in this <see cref="ConvPoly"/></param>
    /// <returns><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="ConvPoly"/>; <c>false</c> otherwise</returns>
    public bool Contains(Point value) => Contains(value.ToVector2());
    /// <summary>Gets whether or not the provided coordinates lie within the bounds of this <see cref="ConvPoly"/></summary>
    /// <param name="x">The x coordinate of the point to check for containment</param>
    /// <param name="y">The y coordinate of the point to check for containment</param>
    /// <returns><c>true</c> if the provided coordinates lie inside this <see cref="ConvPoly"/>; <c>false</c> otherwise</returns>
    public bool Contains(int x, int y) => Contains(new Vector2(x, y));
    /// <summary>Gets whether or not the provided coordinates lie within the bounds of this <see cref="ConvPoly"/></summary>
    /// <param name="x">The x coordinate of the point to check for containment</param>
    /// <param name="y">The y coordinate of the point to check for containment</param>
    /// <returns><c>true</c> if the provided coordinates lie inside this <see cref="ConvPoly"/>; <c>false</c> otherwise</returns>
    public bool Contains(float x, float y) => Contains(new Vector2(x, y));
    /// <summary>Gets whether or not the provided <see cref="Rectangle"/> lies within the bounds of this <see cref="ConvPoly"/></summary>
    /// <param name="value">The <see cref="Rectangle"/> to check for inclusion in this <see cref="ConvPoly"/></param>
    /// <returns><c>true</c> if the provided <see cref="Rectangle"/>'s bounds lie entirely inside this <see cref="ConvPoly"/>; <c>false</c> otherwise</returns>
    public bool Contains(Rectangle value) {
        Vector2 otl = new(value.Left, value.Top),
            otr = new(value.Right, value.Top),
            obl = new(value.Left, value.Bottom),
            obr = new(value.Right, value.Bottom);
        for (int i = 0; i < Verts.Length - 1; i++) {
            if (!IsLeft(Verts[i], Verts[i + 1], otl))
                return false;
            if (!IsLeft(Verts[i], Verts[i + 1], otr))
                return false;
            if (!IsLeft(Verts[i], Verts[i + 1], obl))
                return false;
            if (!IsLeft(Verts[i], Verts[i + 1], obr))
                return false;
        }
        return !IsLeft(Verts[0], Verts[^1], otl) || !IsLeft(Verts[0], Verts[^1], otr) || !IsLeft(Verts[0], Verts[^1], obl) || !IsLeft(Verts[0], Verts[^1], obr);
    }
    /// <summary>Gets whether or not the provided <see cref="RotRect"/> lies within the bounds of this <see cref="ConvPoly"/></summary>
    /// <param name="value">The <see cref="RotRect"/> to check for inclusion in this <see cref="ConvPoly"/></param>
    /// <returns><c>true</c> if the provided <see cref="RotRect"/>'s bounds lie entirely inside this <see cref="ConvPoly"/>; <c>false</c> otherwise</returns>
    public bool Contains(RotRect value) {
        float cos = MathF.Cos(value.Rotation),
            sin = MathF.Sin(value.Rotation),
            x = -value.Origin.X,
            y = -value.Origin.Y,
            w = value.Size.X + x,
            h = value.Size.Y + y,
            xcos = x * cos,
            ycos = y * cos,
            xsin = x * sin,
            ysin = y * sin,
            wcos = w * cos,
            wsin = w * sin,
            hcos = h * cos,
            hsin = h * sin;
        Vector2 otl = new(xcos - ysin + value.XY.X, xsin + ycos + value.XY.Y),
            otr = new(wcos - ysin + value.XY.X, wsin + ycos + value.XY.Y),
            obr = new(wcos - hsin + value.XY.X, wsin + hcos + value.XY.Y),
            obl = new(xcos - hsin + value.XY.X, xsin + hcos + value.XY.Y);
        for (int i = 0; i < Verts.Length - 1; i++) {
            if (!IsLeft(Verts[i], Verts[i + 1], otl))
                return false;
            if (!IsLeft(Verts[i], Verts[i + 1], otr))
                return false;
            if (!IsLeft(Verts[i], Verts[i + 1], obl))
                return false;
            if (!IsLeft(Verts[i], Verts[i + 1], obr))
                return false;
        }
        return !IsLeft(Verts[0], Verts[^1], otl) || !IsLeft(Verts[0], Verts[^1], otr) || !IsLeft(Verts[0], Verts[^1], obl) || !IsLeft(Verts[0], Verts[^1], obr);
    }
    /// <summary>Gets whether or not the provided <see cref="ConvPoly"/> lies within the bounds of this <see cref="ConvPoly"/></summary>
    /// <param name="value">The <see cref="ConvPoly"/> to check for inclusion in this <see cref="ConvPoly"/></param>
    /// <returns><c>true</c> if the provided <see cref="ConvPoly"/>'s bounds lie entirely inside this <see cref="ConvPoly"/>; <c>false</c> otherwise</returns>
    public bool Contains(ConvPoly value) {
        for (int j = 0; j < value.Verts.Length; j++) {
            var v = value.Verts[j];
            for (int i = 0; i < Verts.Length - 1; i++) {
                if (!IsLeft(Verts[i], Verts[i + 1], v))
                    return false;
            }
            return !IsLeft(Verts[0], Verts[^1], v);
        }
        return false;
    }

    /// <summary>Changes all corners of this <see cref="ConvPoly"/></summary>
    /// <param name="offsetX">The x coordinate to add to this <see cref="ConvPoly"/></param>
    /// <param name="offsetY">The y coordinate to add to this <see cref="ConvPoly"/></param>
    public void Offset(float offsetX, float offsetY) {
        for (int i = 0; i < Verts.Length; i++)
            Verts[i] += new Vector2(offsetX, offsetY);
        _xy += new Vector2(offsetX, offsetY);
    }
    /// <summary>Changes all corners of this <see cref="ConvPoly"/></summary>
    /// <param name="amount">The x and y components to add to this <see cref="ConvPoly"/></param>
    public void Offset(Vector2 amount) => Offset(amount.X, amount.Y);
    /// <summary>Changes all corners of this <see cref="ConvPoly"/></summary>
    /// <param name="amount">The x and y components to add to this <see cref="ConvPoly"/></param>
    public void Offset(Point amount) => Offset(amount.X, amount.Y);
}