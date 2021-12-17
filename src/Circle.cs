using Microsoft.Xna.Framework;

namespace Dcrew;

/// <summary>A circle</summary>
public struct Circle {
    internal static Vector2 FarthestPoint(Span<Vector2> verts, Vector2 dir) {
        return verts[0] + Vector2.Normalize(dir) * verts[1].X;
    }

    /// <summary>Coordinates of this <see cref="Circle"/></summary>
    public Vector2 XY;
    /// <summary>Size of bounds</summary>
    public float Radius;
    /// <summary>Center of rotation of this <see cref="Circle"/></summary>
    public Vector2 Origin;

    /// <summary>X coordinate of this <see cref="Circle"/></summary>
    public float X {
        get => XY.X;
        set => XY.X = value;
    }
    /// <summary>Y coordinate of this <see cref="Circle"/></summary>
    public float Y {
        get => XY.Y;
        set => XY.Y = value;
    }

    /// <summary>Creates a new instance of <see cref="Circle"/> struct, with the specified position, width, height, angle, and origin</summary>
    /// <param name="x">X coordinate of the created <see cref="Circle"/></param>
    /// <param name="y">Y coordinate of the created <see cref="Circle"/></param>
    /// <param name="width">X size of the created <see cref="Circle"/></param>
    /// <param name="height">Y size of the created <see cref="Circle"/></param>
    /// <param name="rotation">Rotation (in radians) of the created <see cref="Circle"/></param>
    /// <param name="origin">Center of rotation of the created <see cref="Circle"/></param>
    public Circle(float x, float y, float radius, Vector2 origin = default) {
        XY = new Vector2(x, y);
        Radius = radius;
        Origin = origin;
    }
    /// <summary>Creates a new instance of <see cref="Circle"/> struct, with the specified position, width, height, angle, and origin</summary>
    /// <param name="xy">Coordinates of the created <see cref="Circle"/></param>
    /// <param name="size">Size of the created <see cref="Circle"/></param>
    /// <param name="rotation">Rotation (in radians) of the created <see cref="Circle"/></param>
    /// <param name="origin">Center of rotation of the created <see cref="Circle"/></param>
    public Circle(Vector2 xy, float radius, Vector2 origin = default) : this(xy.X, xy.Y, radius, origin) { }

    /// <summary>Gets whether or not the given <see cref="Line"/> intersects with this <see cref="Circle"/></summary>
    public unsafe bool Intersects(Line value) => Collision.GJK(stackalloc[] { XY, new Vector2(Radius, 0) }, &FarthestPoint, stackalloc[] { value.A, value.B }, &Line.FarthestPoint);
    /// <summary>Gets whether or not the given <see cref="Line"/> intersects with this <see cref="Circle"/></summary>
    public unsafe bool Intersects(Line value, out CollisionResolution res) => Collision.GJK(stackalloc[] { XY, new Vector2(Radius, 0) }, &FarthestPoint, stackalloc[] { value.A, value.B }, &Line.FarthestPoint, out res);
    /// <summary>Gets whether or not the given <see cref="Circle"/> intersects with this <see cref="Circle"/></summary>
    public unsafe bool Intersects(Circle value) => Collision.GJK(stackalloc[] { XY, new Vector2(Radius, 0) }, &FarthestPoint, stackalloc[] { value.XY, new Vector2(value.Radius, 0) }, &Circle.FarthestPoint);
    /// <summary>Gets whether or not the given <see cref="Circle"/> intersects with this <see cref="Circle"/></summary>
    
    public unsafe bool Intersects(Circle value, out CollisionResolution res) => Collision.GJK(stackalloc[] { XY, new Vector2(Radius, 0) }, &FarthestPoint, stackalloc[] { value.XY, new Vector2(value.Radius, 0) }, &Circle.FarthestPoint, out res);
    /// <summary>Gets whether or not the given <see cref="Rectangle"/> intersects with this <see cref="Circle"/></summary>
    public bool Intersects(Rectangle value) {
        Vector2 otl = new(value.Left, value.Top),
           otr = new(value.Right, otl.Y),
           obr = new(otr.X, value.Bottom),
           obl = new(otl.X, obr.Y);
        return Intersects(new Quad(otl, otr, obr, obl));
    }
    /// <summary>Gets whether or not the given <see cref="Rectangle"/> intersects with this <see cref="Circle"/></summary>
    public bool Intersects(Rectangle value, out CollisionResolution res) {
        Vector2 otl = new(value.Left, value.Top),
           otr = new(value.Right, otl.Y),
           obr = new(otr.X, value.Bottom),
           obl = new(otl.X, obr.Y);
        return Intersects(new Quad(otl, otr, obr, obl), out res);
    }
    /// <summary>Gets whether or not the given <see cref="RotRect"/> intersects with this <see cref="Circle"/></summary>
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
    /// <summary>Gets whether or not the given <see cref="RotRect"/> intersects with this <see cref="Circle"/></summary>
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
    /// <summary>Gets whether or not the given <see cref="Quad"/> intersects with this <see cref="Circle"/></summary>
    public unsafe bool Intersects(Quad value) => Collision.GJK(stackalloc[] { XY, new Vector2(Radius, 0) }, &FarthestPoint, stackalloc[] { value.A, value.B, value.C, value.D }, &Quad.FarthestPoint);
    /// <summary>Gets whether or not the given <see cref="Quad"/> intersects with this <see cref="Circle"/></summary>
    public unsafe bool Intersects(Quad value, out CollisionResolution res) => Collision.GJK(stackalloc[] { XY, new Vector2(Radius, 0) }, &FarthestPoint, stackalloc[] { value.A, value.B, value.C, value.D }, &Quad.FarthestPoint, out res);
    /// <summary>Gets whether or not the given <see cref="ConvPoly"/> intersects with this <see cref="Circle"/></summary>
    public unsafe bool Intersects(ConvPoly value) => Collision.GJK(stackalloc[] { XY, new Vector2(Radius, 0) }, &FarthestPoint, value.Verts, &ConvPoly.FarthestPoint);
    /// <summary>Gets whether or not the given <see cref="ConvPoly"/> intersects with this <see cref="Circle"/></summary>
    public unsafe bool Intersects(ConvPoly value, out CollisionResolution res) => Collision.GJK(stackalloc[] { XY, new Vector2(Radius, 0) }, &FarthestPoint, value.Verts, &ConvPoly.FarthestPoint, out res);

    /// <summary>Gets whether or not the given <see cref="Vector2"/> lies within the bounds of this <see cref="Circle"/></summary>
    public bool Contains(Vector2 value) => Vector2.Distance(value, XY) <= Radius;
    /// <summary>Gets whether or not the given <see cref="Point"/> lies within the bounds of this <see cref="Circle"/></summary>
    public bool Contains(Point value) => Contains(value.ToVector2());
    /// <summary>Gets whether or not the given coordinates lie within the bounds of this <see cref="Circle"/></summary>
    public bool Contains(int x, int y) => Contains(new Vector2(x, y));
    /// <summary>Gets whether or not the given coordinates lie within the bounds of this <see cref="Circle"/></summary>
    public bool Contains(float x, float y) => Contains(new Vector2(x, y));
    /// <summary>Gets whether or not the given <see cref="Rectangle"/> lies within the bounds of this <see cref="Circle"/></summary>
    public bool Contains(Rectangle value) {
        Vector2 otl = new(value.Left, value.Top),
           otr = new(value.Right, value.Top),
           obl = new(value.Left, value.Bottom),
           obr = new(value.Right, value.Bottom);
        return Vector2.Distance(otl, XY) <= Radius && Vector2.Distance(otr, XY) <= Radius && Vector2.Distance(obr, XY) <= Radius && Vector2.Distance(obl, XY) <= Radius;
    }
    /// <summary>Gets whether or not the given <see cref="Circle"/> lies within the bounds of this <see cref="Circle"/></summary>
    public bool Contains(Circle value) {
        return false;
    }
    /// <summary>Gets whether or not the given <see cref="Quad"/> lies within the bounds of this <see cref="Circle"/></summary>
    public bool Contains(Quad value) => Vector2.Distance(value.A, XY) <= Radius && Vector2.Distance(value.B, XY) <= Radius && Vector2.Distance(value.C, XY) <= Radius && Vector2.Distance(value.D, XY) <= Radius;

    /// <summary>Adjusts the radius of this <see cref="Circle"/> by specified amount</summary>
    public void Inflate(float amount) => Radius += amount;

    /// <summary>Changes the <see cref="XY"/> of this <see cref="Circle"/></summary>
    /// <param name="offsetX">The x coordinate to add to this <see cref="Circle"/></param>
    /// <param name="offsetY">The y coordinate to add to this <see cref="Circle"/></param>
    public void Offset(float offsetX, float offsetY) => XY = new Vector2(XY.X + offsetX, XY.Y + offsetY);
    /// <summary>Changes the <see cref="XY"/> of this <see cref="Circle"/></summary>
    /// <param name="amount">The x and y components to add to this <see cref="Circle"/></param>
    public void Offset(Vector2 amount) => Offset(amount.X, amount.Y);
    /// <summary>Changes the <see cref="XY"/> of this <see cref="Circle"/></summary>
    /// <param name="amount">The x and y components to add to this <see cref="Circle"/></param>
    public void Offset(Point amount) => Offset(amount.X, amount.Y);

    ///// <summary>Find the closest point to the given position from within this <see cref="Circle"/></summary>
    ///// <param name="xy">Position</param>
    ///// <returns><see cref="Vector2"/> closest to <paramref name="xy"/> that lies within this <see cref="Circle"/></returns>
    //public Vector2 ClosestPoint(Vector2 xy) {
    //    float cos = MathF.Cos(-Rotation),
    //        sin = MathF.Sin(-Rotation),
    //        x = xy.X - XY.X,
    //        y = xy.Y - XY.Y,
    //        xcos = x * cos,
    //        ycos = y * cos,
    //        xsin = x * sin,
    //        ysin = y * sin;
    //    var p = new Vector2(xcos - ysin + XY.X, xsin + ycos + XY.Y);
    //    p.X = MathHelper.Clamp(p.X, XY.X - Origin.X, XY.X + Size.X - Origin.X);
    //    p.Y = MathHelper.Clamp(p.Y, XY.Y - Origin.Y, XY.Y + Size.Y - Origin.Y);
    //    cos = MathF.Cos(Rotation);
    //    sin = MathF.Sin(Rotation);
    //    x = p.X - XY.X;
    //    y = p.Y - XY.Y;
    //    xcos = x * cos;
    //    ycos = y * cos;
    //    xsin = x * sin;
    //    ysin = y * sin;
    //    return new Vector2(xcos - ysin + XY.X, xsin + ycos + XY.Y);
    //}
}