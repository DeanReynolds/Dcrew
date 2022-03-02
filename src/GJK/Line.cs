using Microsoft.Xna.Framework;

namespace Dcrew;

/// <summary>A Line</summary>
public struct Line {
    internal static Vector2 FarthestPoint(Span<Vector2> verts, Vector2 dir) {
        if (Vector2.Dot(verts[1], dir) > Vector2.Dot(verts[0], dir))
            return verts[1];
        return verts[0];
    }

    Vector2 _xy;
    /// <summary>1st vertex (local)</summary>
    public Vector2 A;
    /// <summary>2nd vertex (local)</summary>
    public Vector2 B;
    public float Rotation {
        get => MathF.Atan2(B.Y - A.Y, B.X - A.X);
        set {
            static Vector2 RotatePoint(Vector2 p, float cos, float sin) {
                float xcos = p.X * cos,
                    ycos = p.Y * cos,
                    xsin = p.X * sin,
                    ysin = p.Y * sin;
                return new Vector2(xcos - ysin, xsin + ycos);
            }
            float old = MathF.Atan2(B.Y - A.Y, B.X - A.X),
                cos = MathF.Cos(-old),
                sin = MathF.Sin(-old),
                cos2 = MathF.Cos(value),
                sin2 = MathF.Sin(value);
            A = RotatePoint(A - _xy, cos, sin);
            A = RotatePoint(A, cos2, sin2) + _xy;
            B = RotatePoint(B - _xy, cos, sin);
            B = RotatePoint(B, cos2, sin2) + _xy;
        }
    }

    /// <summary>A <see cref="Vector2"/> located in the center of this <see cref="Line"/></summary>
    public Vector2 Center => new((A.X + B.X) / 4, (A.Y + B.Y) / 4);

    public Line(Vector2 a, Vector2 b) {
        A = a;
        B = b;
        _xy = Vector2.Zero;
    }

    /// <summary>Gets whether or not the given <see cref="Line"/> intersects with this <see cref="Line"/></summary>
    public unsafe bool Intersects(Line value) => Collision.GJK(stackalloc[] { A, B }, &FarthestPoint, stackalloc[] { value.A, value.B }, &Line.FarthestPoint);
    /// <summary>Gets whether or not the given <see cref="Line"/> intersects with this <see cref="Line"/></summary>
    public unsafe bool Intersects(Line value, out CollisionResolution res) => Collision.GJK(stackalloc[] { A, B }, &FarthestPoint, stackalloc[] { value.A, value.B }, &Line.FarthestPoint, out res);
    /// <summary>Gets whether or not the given <see cref="Circle"/> intersects with this <see cref="Line"/></summary>
    public unsafe bool Intersects(Circle value) => Collision.GJK(stackalloc[] { A, B }, &FarthestPoint, stackalloc[] { value.XY, new Vector2(value.Radius, 0) }, &Circle.FarthestPoint);
    /// <summary>Gets whether or not the given <see cref="Circle"/> intersects with this <see cref="Line"/></summary>
    public unsafe bool Intersects(Circle value, out CollisionResolution res) => Collision.GJK(stackalloc[] { A, B }, &FarthestPoint, stackalloc[] { value.XY, new Vector2(value.Radius, 0) }, &Circle.FarthestPoint, out res);
    /// <summary>Gets whether or not the given <see cref="Rectangle"/> intersects with this <see cref="Line"/></summary>
    public bool Intersects(Rectangle value) {
        Vector2 otl = new(value.Left, value.Top),
           otr = new(value.Right, otl.Y),
           obr = new(otr.X, value.Bottom),
           obl = new(otl.X, obr.Y);
        return Intersects(new Quad(otl, otr, obr, obl));
    }
    /// <summary>Gets whether or not the given <see cref="Rectangle"/> intersects with this <see cref="Line"/></summary>
    public bool Intersects(Rectangle value, out CollisionResolution res) {
        Vector2 otl = new(value.Left, value.Top),
           otr = new(value.Right, otl.Y),
           obr = new(otr.X, value.Bottom),
           obl = new(otl.X, obr.Y);
        return Intersects(new Quad(otl, otr, obr, obl), out res);
    }
    /// <summary>Gets whether or not the given <see cref="RotRect"/> intersects with this <see cref="Line"/></summary>
    public bool Intersects(RotRect value) {
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
        return Intersects(new Quad(otl, otr, obr, obl));
    }
    /// <summary>Gets whether or not the given <see cref="RotRect"/> intersects with this <see cref="Line"/></summary>
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
    /// <summary>Gets whether or not the given <see cref="Line"/> intersects with this <see cref="Line"/></summary>
    public unsafe bool Intersects(Quad value) => Collision.GJK(stackalloc[] { A, B }, &FarthestPoint, stackalloc[] { value.A, value.B, value.C, value.D }, &Quad.FarthestPoint);
    /// <summary>Gets whether or not the given <see cref="Line"/> intersects with this <see cref="Line"/></summary>
    public unsafe bool Intersects(Quad value, out CollisionResolution res) => Collision.GJK(stackalloc[] { A, B }, &FarthestPoint, stackalloc[] { value.A, value.B, value.C, value.D }, &Quad.FarthestPoint, out res);
    /// <summary>Gets whether or not the given <see cref="ConvPoly"/> intersects with this <see cref="Line"/></summary>
    public unsafe bool Intersects(ConvPoly value) => Collision.GJK(stackalloc[] { A, B }, &FarthestPoint, value.Verts, &ConvPoly.FarthestPoint);
    /// <summary>Gets whether or not the given <see cref="ConvPoly"/> intersects with this <see cref="Line"/></summary>
    public unsafe bool Intersects(ConvPoly value, out CollisionResolution res) => Collision.GJK(stackalloc[] { A, B }, &FarthestPoint, value.Verts, &ConvPoly.FarthestPoint, out res);

    /// <summary>Changes all corners of this <see cref="Line"/></summary>
    public void Offset(float offsetX, float offsetY) {
        Vector2 v = new(offsetX, offsetY);
        A += v;
        B += v;
        _xy += v;
    }
    /// <summary>Changes all corners of this <see cref="Line"/></summary>
    public void Offset(Vector2 amount) => Offset(amount.X, amount.Y);
    /// <summary>Changes all corners of this <see cref="Line"/></summary>
    public void Offset(Point amount) => Offset(amount.X, amount.Y);
}