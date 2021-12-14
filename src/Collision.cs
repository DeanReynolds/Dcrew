using Microsoft.Xna.Framework;

namespace Dcrew;

internal static class Collision {
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

    public static void GJK(Func<Vector2, Vector2> a, Func<Vector2, Vector2> b, out bool intersects) {
        intersects = false;
        Vector2 dir = new(0, 1);
        var spa = a(dir) - b(-dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return;
        dir = -dir;
        var spb = a(dir) - b(-dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return;
        var simplex = new Simplex(spa, spb);
        var nd = simplex.CalcDir();
        if (!nd.HasValue)
            return;
        dir = nd.Value;
        do {
            spa = a(dir) - b(-dir);
            if (Vector2.Dot(spa, dir) <= 0)
                return;
            simplex.Add(spa);
            nd = simplex.CalcDir();
            if (!nd.HasValue)
                break;
            dir = nd.Value;
        } while (true);
        intersects = true;
    }

    public static void GJK(Func<Vector2, Vector2> a, Func<Vector2, Vector2> b, out bool intersects, out CollisionResolution res) {
        intersects = false;
        res = new CollisionResolution();
        Vector2 dir = new(0, 1);
        var spa = a(dir) - b(-dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return;
        dir = -dir;
        var spb = a(dir) - b(-dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return;
        var simplex = new Simplex(spa, spb);
        var nd = simplex.CalcDir();
        if (!nd.HasValue)
            return;
        dir = nd.Value;
        do {
            spa = a(dir) - b(-dir);
            if (Vector2.Dot(spa, dir) <= 0)
                return;
            simplex.Add(spa);
            nd = simplex.CalcDir();
            if (!nd.HasValue)
                break;
            dir = nd.Value;
        } while (true);
        intersects = true;
        Edge edge;
        Polytope polytope = new(simplex);
        do {
            edge = polytope.GetClosestEdge();
            var sp = a(edge.Normal) - b(-edge.Normal);
            var d = Vector2.Dot(sp, edge.Normal);
            if (MathF.Abs(d - edge.Distance) > .001f) {
                edge.Distance = float.PositiveInfinity;
                polytope.Insert(edge.Index, sp);
            }
        } while (edge.Distance == float.PositiveInfinity);
        res.Normal = edge.Normal;
        res.Depth = edge.Distance + .001f;
    }
}