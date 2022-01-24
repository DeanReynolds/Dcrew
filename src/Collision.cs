using Microsoft.Xna.Framework;
using System.Buffers;

namespace Dcrew;

internal static class Collision {
    internal ref struct Simplex {
        internal Vector2 _a, _b, _c;

        public Simplex(Vector2 a, Vector2 b) {
            _a = a;
            _b = b;
            _c = default;
        }

        public bool CalcDir(out Vector2 r) {
            Vector2 b = _b, c = _a, abPerp = new(b.Y - _c.Y, _c.X - b.X);
            if ((abPerp.X * c.X) + (abPerp.Y * c.Y) >= 0)
                abPerp = -abPerp;
            if ((abPerp.X * -_c.X) + (abPerp.Y * -_c.Y) > 0) {
                _a = _b;
                _b = _c;
                r = abPerp;
                return true;
            }
            Vector2 acPerp = new(c.Y - _c.Y, _c.X - c.X);
            if ((acPerp.X * b.X) + (acPerp.Y * b.Y) >= 0)
                acPerp = -acPerp;
            if ((acPerp.X * -_c.X) + (acPerp.Y * -_c.Y) > 0) {
                _b = _c;
                r = acPerp;
                return true;
            }
            r = Vector2.Zero;
            return false;
        }
    }

    internal unsafe ref struct Polytope {
        public fixed float X[256];
        public fixed float Y[256];
        public byte Count;

        public Polytope(Simplex simplex) {
            X[0] = simplex._a.X;
            Y[0] = simplex._a.Y;
            X[1] = simplex._b.X;
            Y[1] = simplex._b.Y;
            X[2] = simplex._c.X;
            Y[2] = simplex._c.Y;
            Count = 3;
        }

        public void Insert(int i, Vector2 v) {
            for (int j = Count - 1; j >= i; j--) {
                var k = j + 1;
                X[k] = X[j];
                Y[k] = Y[j];
            }
            X[i] = v.X;
            Y[i] = v.Y;
            Count++;
        }

        public Edge GetClosestEdge() {
            Edge closest = new();
            closest.Distance = float.PositiveInfinity;
            for (int i = 0; i < Count; i++) {
                int j = i == Count - 1 ? 0 : i + 1;
                Vector2 a = new Vector2(X[i], Y[i]), b = new Vector2(X[j], Y[j]);
                var normal = Vector2.Normalize(new Vector2(b.Y - a.Y, a.X - b.X));
                var d = (normal.X * a.X) + (normal.Y * a.Y);
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

    public static unsafe bool GJK(Span<Vector2> verts1, delegate*<Span<Vector2>, Vector2, Vector2> farthestPoint1, Span<Vector2> verts2, delegate*<Span<Vector2>, Vector2, Vector2> farthestPoint2) {
        Vector2 dir = new(0, 1);
        var spa = farthestPoint1(verts1, dir) - farthestPoint2(verts2, -dir);
        if (spa.Y <= 0)
            return false;
        var spb = farthestPoint1(verts1, -dir) - farthestPoint2(verts2, dir);
        if (-spb.Y <= 0)
            return false;
        Vector2 abPerp = new(spa.Y - spb.Y, spb.X - spa.X);
        dir = (abPerp.X * -spb.X) + (abPerp.Y * -spb.Y) <= 0 ? -abPerp : abPerp;
        var simplex = new Simplex(spa, spb);
        do {
            spa = farthestPoint1(verts1, dir) - farthestPoint2(verts2, -dir);
            if ((spa.X * dir.X) + (spa.Y * dir.Y) <= 0)
                return false;
            simplex._c = spa;
            if (!simplex.CalcDir(out dir))
                break;
        } while (true);
        return true;
    }

    public static unsafe bool GJK(Span<Vector2> verts1, delegate*<Span<Vector2>, Vector2, Vector2> farthestPoint1, Span<Vector2> verts2, delegate*<Span<Vector2>, Vector2, Vector2> farthestPoint2, out CollisionResolution res) {
        res = new CollisionResolution();
        Vector2 dir = new(0, 1);
        var spa = farthestPoint1(verts1, dir) - farthestPoint2(verts2, -dir);
        if (spa.Y <= 0)
            return false;
        var spb = farthestPoint1(verts1, -dir) - farthestPoint2(verts2, dir);
        if (-spb.Y <= 0)
            return false;
        Vector2 abPerp = new(spa.Y - spb.Y, spb.X - spa.X);
        dir = (abPerp.X * -spb.X) + (abPerp.Y * -spb.Y) <= 0 ? -abPerp : abPerp;
        var simplex = new Simplex(spa, spb);
        do {
            spa = farthestPoint1(verts1, dir) - farthestPoint2(verts2, -dir);
            if ((spa.X * dir.X) + (spa.Y * dir.Y) <= 0)
                return false;
            simplex._c = spa;
            if (!simplex.CalcDir(out dir))
                break;
        } while (true);
        Edge edge;
        Polytope polytope = new(simplex);
        do {
            edge = polytope.GetClosestEdge();
            spa = farthestPoint1(verts1, edge.Normal) - farthestPoint2(verts2, -edge.Normal);
            if (MathF.Abs((spa.X * edge.Normal.X) + (spa.Y * edge.Normal.Y) - edge.Distance) > .001f) {
                edge.Distance = float.PositiveInfinity;
                polytope.Insert(edge.Index, spa);
            }
        } while (edge.Distance == float.PositiveInfinity);
        res.Normal = edge.Normal;
        res.Depth = edge.Distance + .001f;
        return true;
    }
}