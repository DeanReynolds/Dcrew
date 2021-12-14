using Microsoft.Xna.Framework;

namespace Dcrew;

/// <summary>A convex polygon</summary>
public struct ConvPoly {
    internal Vector2 FarthestPoint(Vector2 dir) {
        var p = Verts[0];
        var d = Vector2.Dot(p, dir);
        for (int i = 1; i < Verts.Length; i++) {
            var p2 = Verts[i];
            var dp2 = Vector2.Dot(p2, dir);
            if (dp2 > d) {
                p = p2;
                d = dp2;
            }
        }
        return p;
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

    /// <summary>Gets whether or not the given <see cref="Line"/> intersects with this <see cref="ConvPoly"/></summary>
    public bool Intersects(Line value) {
        Collision.GJK(FarthestPoint, value.FarthestPoint, out var intersects);
        return intersects;
    }
    /// <summary>Gets whether or not the given <see cref="Line"/> intersects with this <see cref="ConvPoly"/></summary>
    public bool Intersects(Line value, out CollisionResolution res) {
        Collision.GJK(FarthestPoint, value.FarthestPoint, out var intersects, out res);
        return intersects;
    }
    /// <summary>Gets whether or not the given <see cref="Circle"/> intersects with this <see cref="Circle"/></summary>
    public bool Intersects(Circle value) {
        Collision.GJK(FarthestPoint, value.FarthestPoint, out var intersects);
        return intersects;
    }
    /// <summary>Gets whether or not the given <see cref="Circle"/> intersects with this <see cref="Circle"/></summary>
    public bool Intersects(Circle value, out CollisionResolution res) {
        Collision.GJK(FarthestPoint, value.FarthestPoint, out var intersects, out res);
        return intersects;
    }
    /// <summary>Gets whether or not the given <see cref="Rectangle"/> intersects with this <see cref="ConvPoly"/></summary>
    public bool Intersects(Rectangle value) {
        Vector2 otl = new(value.Left, value.Top),
            otr = new(value.Right, otl.Y),
            obr = new(otr.X, value.Bottom),
            obl = new(otl.X, obr.Y);
        return Intersects(new Quad(otl, otr, obl, obr));
    }
    /// <summary>Gets whether or not the given <see cref="Rectangle"/> intersects with this <see cref="ConvPoly"/></summary>
    public bool Intersects(Rectangle value, out CollisionResolution res) {
        Vector2 otl = new(value.Left, value.Top),
            otr = new(value.Right, otl.Y),
            obr = new(otr.X, value.Bottom),
            obl = new(otl.X, obr.Y);
        return Intersects(new Quad(otl, otr, obr, obl), out res);
    }
    /// <summary>Gets whether or not the given <see cref="RotRect"/> intersects with this <see cref="ConvPoly"/></summary>
    public bool Intersects(RotRect value) => value.Intersects(this);
    /// <summary>Gets whether or not the given <see cref="RotRect"/> intersects with this <see cref="ConvPoly"/></summary>
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
    /// <summary>Gets whether or not the given <see cref="Quad"/> intersects with this <see cref="ConvPoly"/></summary>
    public bool Intersects(Quad value) => value.Intersects(this);
    /// <summary>Gets whether or not the given <see cref="Quad"/> intersects with this <see cref="ConvPoly"/></summary>
    public bool Intersects(Quad value, out CollisionResolution res) {
        Collision.GJK(FarthestPoint, value.FarthestPoint, out var intersects, out res);
        return intersects;
    }
    /// <summary>Gets whether or not the given <see cref="ConvPoly"/> intersects with this <see cref="ConvPoly"/></summary>
    public bool Intersects(ConvPoly value) {
        Collision.GJK(FarthestPoint, value.FarthestPoint, out var intersects);
        return intersects;
    }
    /// <summary>Gets whether or not the given <see cref="ConvPoly"/> intersects with this <see cref="ConvPoly"/></summary>
    public bool Intersects(ConvPoly value, out CollisionResolution res) {
        Collision.GJK(FarthestPoint, value.FarthestPoint, out var intersects, out res);
        return intersects;
    }

    /// <summary>Gets whether or not the given <see cref="Vector2"/> lies within the bounds of this <see cref="ConvPoly"/></summary>
    public bool Contains(Vector2 value) {
        for (int i = 0; i < Verts.Length - 1; i++) {
            if (!IsLeft(Verts[i], Verts[i + 1], value))
                return false;
        }
        return !IsLeft(Verts[0], Verts[^1], value);
    }
    /// <summary>Gets whether or not the given <see cref="Point"/> lies within the bounds of this <see cref="ConvPoly"/></summary>
    public bool Contains(Point value) => Contains(value.ToVector2());
    /// <summary>Gets whether or not the given coordinates lie within the bounds of this <see cref="ConvPoly"/></summary>
    public bool Contains(int x, int y) => Contains(new Vector2(x, y));
    /// <summary>Gets whether or not the given coordinates lie within the bounds of this <see cref="ConvPoly"/></summary>
    public bool Contains(float x, float y) => Contains(new Vector2(x, y));
    /// <summary>Gets whether or not the given <see cref="Rectangle"/> lies within the bounds of this <see cref="ConvPoly"/></summary>
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
    /// <summary>Gets whether or not the given <see cref="RotRect"/> lies within the bounds of this <see cref="ConvPoly"/></summary>
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
    /// <summary>Gets whether or not the given <see cref="ConvPoly"/> lies within the bounds of this <see cref="ConvPoly"/></summary>
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
    public void Offset(float offsetX, float offsetY) {
        for (int i = 0; i < Verts.Length; i++)
            Verts[i] += new Vector2(offsetX, offsetY);
        _xy += new Vector2(offsetX, offsetY);
    }
    /// <summary>Changes all corners of this <see cref="ConvPoly"/></summary>
    public void Offset(Vector2 amount) => Offset(amount.X, amount.Y);
    /// <summary>Changes all corners of this <see cref="ConvPoly"/></summary>
    public void Offset(Point amount) => Offset(amount.X, amount.Y);
}