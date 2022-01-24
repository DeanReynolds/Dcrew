using Microsoft.Xna.Framework;

namespace Dcrew;

/// <summary>A quad</summary>
public struct Quad {
    internal static Vector2 FarthestPoint(Span<Vector2> verts, Vector2 dir) {
        var fp = verts[0];
        var di = Vector2.Dot(fp, dir);
        var dp2 = Vector2.Dot(verts[1], dir);
        if (dp2 > di) {
            fp = verts[1];
            di = dp2;
        }
        dp2 = Vector2.Dot(verts[2], dir);
        if (dp2 > di) {
            fp = verts[2];
            di = dp2;
        }
        dp2 = Vector2.Dot(verts[3], dir);
        if (dp2 > di)
            return verts[3];
        return fp;
    }

    static bool IsLeft(Vector2 a, Vector2 b, Vector2 p) => (b.X - a.X) * (p.Y - a.Y) - (p.X - a.X) * (b.Y - a.Y) > 0;

    float _rotation;
    Vector2 _xy;
    /// <summary>1st vertex (local)</summary>
    public Vector2 A;
    /// <summary>2nd vertex (local)</summary>
    public Vector2 B;
    /// <summary>3rd vertex (local)</summary>
    public Vector2 C;
    /// <summary>4th vertex (local)</summary>
    public Vector2 D;
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
            A = RotatePoint(A - _xy, cos, sin);
            A = RotatePoint(A, cos2, sin2) + _xy;
            B = RotatePoint(B - _xy, cos, sin);
            B = RotatePoint(B, cos2, sin2) + _xy;
            C = RotatePoint(C - _xy, cos, sin);
            C = RotatePoint(C, cos2, sin2) + _xy;
            D = RotatePoint(D - _xy, cos, sin);
            D = RotatePoint(D, cos2, sin2) + _xy;
            _rotation = value;
        }
    }

    /// <summary>A <see cref="Vector2"/> located in the center of this <see cref="Quad"/></summary>
    public Vector2 Center => new((A.X + B.X + D.X + C.X) / 4, (A.Y + B.Y + D.Y + C.Y) / 4);

    public Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) {
        A = a;
        B = b;
        C = c;
        D = d;
        _rotation = 0;
        _xy = Vector2.Zero;
    }
    public Quad(float x, float y, float width, float height) {
        A = new(x, y);
        B = new(x + width, y);
        C = new(B.X, y + height);
        D = new(A.X, C.Y);
        _rotation = 0;
        _xy = Vector2.Zero;
    }
    public Quad(Rectangle rect) : this(rect.X, rect.Y, rect.Width, rect.Height) { }

    /// <summary>Gets whether or not the given <see cref="Line"/> intersects with this <see cref="Quad"/></summary>
    public unsafe bool Intersects(Line value) => Collision.GJK(stackalloc[] { A, B, C, D }, &FarthestPoint, stackalloc[] { value.A, value.B }, &Line.FarthestPoint);
    /// <summary>Gets whether or not the given <see cref="Line"/> intersects with this <see cref="Quad"/></summary>
    public unsafe bool Intersects(Line value, out CollisionResolution res) => Collision.GJK(stackalloc[] { A, B, C, D }, &FarthestPoint, stackalloc[] { value.A, value.B }, &Line.FarthestPoint, out res);
    /// <summary>Gets whether or not the given <see cref="Circle"/> intersects with this <see cref="Quad"/></summary>
    public unsafe bool Intersects(Circle value) => Collision.GJK(stackalloc[] { A, B, C, D }, &FarthestPoint, stackalloc[] { value.XY, new Vector2(value.Radius, 0) }, &Circle.FarthestPoint);
    /// <summary>Gets whether or not the given <see cref="Circle"/> intersects with this <see cref="Quad"/></summary>
    public unsafe bool Intersects(Circle value, out CollisionResolution res) => Collision.GJK(stackalloc[] { A, B, C, D }, &FarthestPoint, stackalloc[] { value.XY, new Vector2(value.Radius, 0) }, &Circle.FarthestPoint, out res);
    /// <summary>Gets whether or not the given <see cref="Rectangle"/> intersects with this <see cref="Quad"/></summary>
    public bool Intersects(Rectangle value) {
        Vector2 otl = new(value.Left, value.Top),
           otr = new(value.Right, otl.Y),
           obr = new(otr.X, value.Bottom),
           obl = new(otl.X, obr.Y);
        return Intersects(new Quad(otl, otr, obr, obl));
    }
    /// <summary>Gets whether or not the given <see cref="Rectangle"/> intersects with this <see cref="Quad"/></summary>
    public bool Intersects(Rectangle value, out CollisionResolution res) {
        Vector2 otl = new(value.Left, value.Top),
           otr = new(value.Right, otl.Y),
           obr = new(otr.X, value.Bottom),
           obl = new(otl.X, obr.Y);
        return Intersects(new Quad(otl, otr, obr, obl), out res);
    }
    /// <summary>Gets whether or not the given <see cref="RotRect"/> intersects with this <see cref="Quad"/></summary>
    public bool Intersects(RotRect value) => value.Intersects(this);
    /// <summary>Gets whether or not the given <see cref="RotRect"/> intersects with this <see cref="Quad"/></summary>
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
    /// <summary>Gets whether or not the given <see cref="Quad"/> intersects with this <see cref="Quad"/></summary>
    public unsafe bool Intersects(Quad value) => Collision.GJK(stackalloc[] { A, B, C, D }, &FarthestPoint, stackalloc[] { value.A, value.B, value.C, value.D }, &Quad.FarthestPoint);
    /// <summary>Gets whether or not the given <see cref="Quad"/> intersects with this <see cref="Quad"/></summary>
    public unsafe bool Intersects(Quad value, out CollisionResolution res) => Collision.GJK(stackalloc[] { A, B, C, D }, &FarthestPoint, stackalloc[] { value.A, value.B, value.C, value.D }, &Quad.FarthestPoint, out res);
    /// <summary>Gets whether or not the given <see cref="ConvPoly"/> intersects with this <see cref="Quad"/></summary>
    public unsafe bool Intersects(ConvPoly value) => Collision.GJK(stackalloc[] { A, B, C, D }, &FarthestPoint, value.Verts, &ConvPoly.FarthestPoint);
    /// <summary>Gets whether or not the given <see cref="ConvPoly"/> intersects with this <see cref="Quad"/></summary>
    public unsafe bool Intersects(ConvPoly value, out CollisionResolution res) => Collision.GJK(stackalloc[] { A, B, C, D }, &FarthestPoint, value.Verts, &ConvPoly.FarthestPoint, out res);

    /// <summary>Gets whether or not the given <see cref="Vector2"/> lies within the bounds of this <see cref="Quad"/></summary>
    public bool Contains(Vector2 value) => IsLeft(A, B, value) && IsLeft(B, D, value) && IsLeft(D, C, value) && IsLeft(C, A, value);
    /// <summary>Gets whether or not the given <see cref="Point"/> lies within the bounds of this <see cref="Quad"/></summary>
    public bool Contains(Point value) => Contains(value.ToVector2());
    /// <summary>Gets whether or not the given coordinates lie within the bounds of this <see cref="Quad"/></summary>
    public bool Contains(int x, int y) => Contains(new Vector2(x, y));
    /// <summary>Gets whether or not the given coordinates lie within the bounds of this <see cref="Quad"/></summary>
    public bool Contains(float x, float y) => Contains(new Vector2(x, y));
    /// <summary>Gets whether or not the given <see cref="Rectangle"/> lies within the bounds of this <see cref="Quad"/></summary>
    public bool Contains(Rectangle value) {
        Vector2 otl = new(value.Left, value.Top),
           otr = new(value.Right, value.Top),
           obl = new(value.Left, value.Bottom),
           obr = new(value.Right, value.Bottom);
        return Contains(otl) && Contains(otr) && Contains(obl) && Contains(obr);
    }
    /// <summary>Gets whether or not the given <see cref="RotRect"/> lies within the bounds of this <see cref="Quad"/></summary>
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
        return Contains(otl) && Contains(otr) && Contains(obl) && Contains(obr);
    }
    /// <summary>Gets whether or not the given <see cref="Quad"/> lies within the bounds of this <see cref="Quad"/></summary>
    public bool Contains(Quad value) => Contains(value.A) && Contains(value.B) && Contains(value.C) && Contains(value.D);

    /// <summary>Changes all corners of this <see cref="Quad"/></summary>
    public void Offset(float offsetX, float offsetY) {
        Vector2 v = new(offsetX, offsetY);
        A += v;
        B += v;
        C += v;
        D += v;
        _xy += v;
    }

    /// <summary>Changes all corners of this <see cref="Quad"/></summary>
    public void Offset(Vector2 amount) => Offset(amount.X, amount.Y);
    /// <summary>Changes all corners of this <see cref="Quad"/></summary>
    public void Offset(Point amount) => Offset(amount.X, amount.Y);
}