﻿using Microsoft.Xna.Framework;

namespace Dcrew;

internal static class Collision {
    internal ref struct Simplex {
        internal Vector2 _a, _b, _c;

        public Simplex(Vector2 a, Vector2 b) {
            _a = a;
            _b = b;
            _c = default;
        }

        public void Add(Vector2 v) {
            _c = v;
        }
        public void Expand(Vector2 v) {
            _c = _b;
            _b = v;
        }

        public Vector2? CalcDir2C() {
            Vector2 ab = _a - _b, abPerp = new(ab.Y, -ab.X);
            if ((abPerp.X * -_b.X) + (abPerp.Y * -_b.Y) <= 0)
                abPerp = -abPerp;
            return abPerp;
        }

        public Vector2? CalcDir3C() {
            Vector2 a = _c, b = _b, c = _a, ab = b - a, abPerp = new(ab.Y, -ab.X);
            if ((abPerp.X * c.X) + (abPerp.Y * c.Y) >= 0)
                abPerp = -abPerp;
            if ((abPerp.X * -_c.X) + (abPerp.Y * -_c.Y) > 0) {
                _a = _b;
                _b = _c;
                return abPerp;
            }
            Vector2 ac = c - a, acPerp = new(ac.Y, -ac.X);
            if ((acPerp.X * b.X) + (acPerp.Y * b.Y) >= 0)
                acPerp = -acPerp;
            if ((acPerp.X * -_c.X) + (acPerp.Y * -_c.Y) > 0) {
                _b = _c;
                return acPerp;
            }
            return null;
        }
    }

    internal ref struct Polytope {
        readonly List<Vector2> _verts;

        public Polytope(Simplex simplex) {
            _verts = new List<Vector2>(4) {
                simplex._a,
                simplex._b
            };
            if (simplex._c != default)
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

    internal ref struct Edge {
        public float Distance;
        public Vector2 Normal;
        public int Index;
    }

    public static unsafe void GJK(void* shape1, delegate*<void*, Vector2, Vector2> farthestPoint1, void* shape2, delegate*<void*, Vector2, Vector2> farthestPoint2, out bool intersects) {
        intersects = false;
        Vector2 dir = new(0, 1);
        var spa = farthestPoint1(shape1, dir) - farthestPoint2(shape2, -dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return;
        dir = -dir;
        var spb = farthestPoint1(shape1, dir) - farthestPoint2(shape2, -dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return;
        var simplex = new Simplex(spa, spb);
        var nd = simplex.CalcDir2C();
        if (!nd.HasValue)
            return;
        dir = nd.Value;
        do {
            spa = farthestPoint1(shape1, dir) - farthestPoint2(shape2, -dir);
            if (Vector2.Dot(spa, dir) <= 0)
                return;
            simplex.Add(spa);
            nd = simplex.CalcDir3C();
            if (!nd.HasValue)
                break;
            dir = nd.Value;
        } while (true);
        intersects = true;
    }

    public static unsafe void GJK(void* shape1, delegate*<void*, Vector2, Vector2> farthestPoint1, void* shape2, delegate*<void*, Vector2, Vector2> farthestPoint2, out bool intersects, out CollisionResolution res) {
        intersects = false;
        res = new CollisionResolution();
        Vector2 dir = new(0, 1);
        var spa = farthestPoint1(shape1, dir) - farthestPoint2(shape2, -dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return;
        dir = -dir;
        var spb = farthestPoint1(shape1, dir) - farthestPoint2(shape2, -dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return;
        var simplex = new Simplex(spa, spb);
        var nd = simplex.CalcDir2C();
        if (!nd.HasValue)
            return;
        dir = nd.Value;
        do {
            spa = farthestPoint1(shape1, dir) - farthestPoint2(shape2, -dir);
            if (Vector2.Dot(spa, dir) <= 0)
                return;
            simplex.Add(spa);
            nd = simplex.CalcDir3C();
            if (!nd.HasValue)
                break;
            dir = nd.Value;
        } while (true);
        intersects = true;
        Edge edge;
        Polytope polytope = new(simplex);
        do {
            edge = polytope.GetClosestEdge();
            var sp = farthestPoint1(shape1, edge.Normal) - farthestPoint2(shape2, -edge.Normal);
            var d = Vector2.Dot(sp, edge.Normal);
            if (MathF.Abs(d - edge.Distance) > .001f) {
                edge.Distance = float.PositiveInfinity;
                polytope.Insert(edge.Index, sp);
            }
        } while (edge.Distance == float.PositiveInfinity);
        res.Normal = edge.Normal;
        res.Depth = edge.Distance + .001f;
    }

    public unsafe static void GJKPoly(ConvPoly shape1, void* shape2, delegate*<void*, Vector2, Vector2> farthestPoint2, out bool intersects) {
        intersects = false;
        Vector2 dir = new(0, 1);
        var spa = ConvPoly.FarthestPoint(shape1, dir) - farthestPoint2(shape2, -dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return;
        dir = -dir;
        var spb = ConvPoly.FarthestPoint(shape1, dir) - farthestPoint2(shape2, -dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return;
        var simplex = new Simplex(spa, spb);
        var nd = simplex.CalcDir2C();
        if (!nd.HasValue)
            return;
        dir = nd.Value;
        do {
            spa = ConvPoly.FarthestPoint(shape1, dir) - farthestPoint2(shape2, -dir);
            if (Vector2.Dot(spa, dir) <= 0)
                return;
            simplex.Add(spa);
            nd = simplex.CalcDir3C();
            if (!nd.HasValue)
                break;
            dir = nd.Value;
        } while (true);
        intersects = true;
    }

    public unsafe static void GJKPoly(ConvPoly shape1, void* shape2, delegate*<void*, Vector2, Vector2> farthestPoint2, out bool intersects, out CollisionResolution res) {
        intersects = false;
        res = new CollisionResolution();
        Vector2 dir = new(0, 1);
        var spa = ConvPoly.FarthestPoint(shape1, dir) - farthestPoint2(shape2, -dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return;
        dir = -dir;
        var spb = ConvPoly.FarthestPoint(shape1, dir) - farthestPoint2(shape2, -dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return;
        var simplex = new Simplex(spa, spb);
        var nd = simplex.CalcDir2C();
        if (!nd.HasValue)
            return;
        dir = nd.Value;
        do {
            spa = ConvPoly.FarthestPoint(shape1, dir) - farthestPoint2(shape2, -dir);
            if (Vector2.Dot(spa, dir) <= 0)
                return;
            simplex.Add(spa);
            nd = simplex.CalcDir3C();
            if (!nd.HasValue)
                break;
            dir = nd.Value;
        } while (true);
        intersects = true;
        Edge edge;
        Polytope polytope = new(simplex);
        do {
            edge = polytope.GetClosestEdge();
            var sp = ConvPoly.FarthestPoint(shape1, edge.Normal) - farthestPoint2(shape2, -edge.Normal);
            var d = Vector2.Dot(sp, edge.Normal);
            if (MathF.Abs(d - edge.Distance) > .001f) {
                edge.Distance = float.PositiveInfinity;
                polytope.Insert(edge.Index, sp);
            }
        } while (edge.Distance == float.PositiveInfinity);
        res.Normal = edge.Normal;
        res.Depth = edge.Distance + .001f;
    }

    public unsafe static void GJKPoly(void* shape1, delegate*<void*, Vector2, Vector2> farthestPoint1, ConvPoly shape2, out bool intersects) {
        intersects = false;
        Vector2 dir = new(0, 1);
        var spa = farthestPoint1(shape1, dir) - ConvPoly.FarthestPoint(shape2, -dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return;
        dir = -dir;
        var spb = farthestPoint1(shape1, dir) - ConvPoly.FarthestPoint(shape2, -dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return;
        var simplex = new Simplex(spa, spb);
        var nd = simplex.CalcDir2C();
        if (!nd.HasValue)
            return;
        dir = nd.Value;
        do {
            spa = farthestPoint1(shape1, dir) - ConvPoly.FarthestPoint(shape2, -dir);
            if (Vector2.Dot(spa, dir) <= 0)
                return;
            simplex.Add(spa);
            nd = simplex.CalcDir3C();
            if (!nd.HasValue)
                break;
            dir = nd.Value;
        } while (true);
        intersects = true;
    }

    public unsafe static void GJKPoly(void* shape1, delegate*<void*, Vector2, Vector2> farthestPoint1, ConvPoly shape2, out bool intersects, out CollisionResolution res) {
        intersects = false;
        res = new CollisionResolution();
        Vector2 dir = new(0, 1);
        var spa = farthestPoint1(shape1, dir) - ConvPoly.FarthestPoint(shape2, -dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return;
        dir = -dir;
        var spb = farthestPoint1(shape1, dir) - ConvPoly.FarthestPoint(shape2, -dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return;
        var simplex = new Simplex(spa, spb);
        var nd = simplex.CalcDir2C();
        if (!nd.HasValue)
            return;
        dir = nd.Value;
        do {
            spa = farthestPoint1(shape1, dir) - ConvPoly.FarthestPoint(shape2, -dir);
            if (Vector2.Dot(spa, dir) <= 0)
                return;
            simplex.Add(spa);
            nd = simplex.CalcDir3C();
            if (!nd.HasValue)
                break;
            dir = nd.Value;
        } while (true);
        intersects = true;
        Edge edge;
        Polytope polytope = new(simplex);
        do {
            edge = polytope.GetClosestEdge();
            var sp = farthestPoint1(shape1, edge.Normal) - ConvPoly.FarthestPoint(shape2, -edge.Normal);
            var d = Vector2.Dot(sp, edge.Normal);
            if (MathF.Abs(d - edge.Distance) > .001f) {
                edge.Distance = float.PositiveInfinity;
                polytope.Insert(edge.Index, sp);
            }
        } while (edge.Distance == float.PositiveInfinity);
        res.Normal = edge.Normal;
        res.Depth = edge.Distance + .001f;
    }

    public unsafe static void GJKPoly(ConvPoly shape1, ConvPoly shape2, out bool intersects) {
        intersects = false;
        Vector2 dir = new(0, 1);
        var spa = ConvPoly.FarthestPoint(shape1, dir) - ConvPoly.FarthestPoint(shape2, -dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return;
        dir = -dir;
        var spb = ConvPoly.FarthestPoint(shape1, dir) - ConvPoly.FarthestPoint(shape2, -dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return;
        var simplex = new Simplex(spa, spb);
        var nd = simplex.CalcDir2C();
        if (!nd.HasValue)
            return;
        dir = nd.Value;
        do {
            spa = ConvPoly.FarthestPoint(shape1, dir) - ConvPoly.FarthestPoint(shape2, -dir);
            if (Vector2.Dot(spa, dir) <= 0)
                return;
            simplex.Add(spa);
            nd = simplex.CalcDir3C();
            if (!nd.HasValue)
                break;
            dir = nd.Value;
        } while (true);
        intersects = true;
    }

    public unsafe static void GJKPoly(ConvPoly shape1, ConvPoly shape2, out bool intersects, out CollisionResolution res) {
        intersects = false;
        res = new CollisionResolution();
        Vector2 dir = new(0, 1);
        var spa = ConvPoly.FarthestPoint(shape1, dir) - ConvPoly.FarthestPoint(shape2, -dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return;
        dir = -dir;
        var spb = ConvPoly.FarthestPoint(shape1, dir) - ConvPoly.FarthestPoint(shape2, -dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return;
        var simplex = new Simplex(spa, spb);
        var nd = simplex.CalcDir2C();
        if (!nd.HasValue)
            return;
        dir = nd.Value;
        do {
            spa = ConvPoly.FarthestPoint(shape1, dir) - ConvPoly.FarthestPoint(shape2, -dir);
            if (Vector2.Dot(spa, dir) <= 0)
                return;
            simplex.Add(spa);
            nd = simplex.CalcDir3C();
            if (!nd.HasValue)
                break;
            dir = nd.Value;
        } while (true);
        intersects = true;
        Edge edge;
        Polytope polytope = new(simplex);
        do {
            edge = polytope.GetClosestEdge();
            var sp = ConvPoly.FarthestPoint(shape1, edge.Normal) - ConvPoly.FarthestPoint(shape2, -edge.Normal);
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