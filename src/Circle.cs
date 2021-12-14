using Microsoft.Xna.Framework;

namespace Dcrew;

/// <summary>A circle</summary>
public struct Circle {
    internal Vector2 FarthestPoint(Vector2 dir) {
        return XY + Vector2.Normalize(dir) * Radius;
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
    public bool Intersects(Line value) {
        Collision.GJK(FarthestPoint, value.FarthestPoint, out var intersects);
        return intersects;
    }
    /// <summary>Gets whether or not the given <see cref="Line"/> intersects with this <see cref="Circle"/></summary>
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
    public bool Intersects(Quad value) {
        Collision.GJK(FarthestPoint, value.FarthestPoint, out var intersects);
        return intersects;
    }
    /// <summary>Gets whether or not the given <see cref="Quad"/> intersects with this <see cref="Circle"/></summary>
    public bool Intersects(Quad value, out CollisionResolution res) {
        Collision.GJK(FarthestPoint, value.FarthestPoint, out var intersects, out res);
        return intersects;
    }
    /// <summary>Gets whether or not the given <see cref="ConvPoly"/> intersects with this <see cref="Circle"/></summary>
    public bool Intersects(ConvPoly value) {
        Collision.GJK(FarthestPoint, value.FarthestPoint, out var intersects);
        return intersects;
    }
    /// <summary>Gets whether or not the given <see cref="ConvPoly"/> intersects with this <see cref="Circle"/></summary>
    public bool Intersects(ConvPoly value, out CollisionResolution res) {
        Collision.GJK(FarthestPoint, value.FarthestPoint, out var intersects, out res);
        return intersects;
    }

    ///// <summary>Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="Circle"/></summary>
    ///// <param name="value">The coordinates to check for inclusion in this <see cref="Circle"/></param>
    ///// <returns><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="Circle"/>; <c>false</c> otherwise</returns>
    //public bool Contains(Vector2 value) {
    //    float cos = MathF.Cos(Rotation),
    //        sin = MathF.Sin(Rotation),
    //        x = -Origin.X,
    //        y = -Origin.Y,
    //        w = Size.X + x,
    //        h = Size.Y + y,
    //        xcos = x * cos,
    //        ycos = y * cos,
    //        xsin = x * sin,
    //        ysin = y * sin,
    //        wcos = w * cos,
    //        wsin = w * sin,
    //        hcos = h * cos,
    //        hsin = h * sin;
    //    Vector2 tl = new(xcos - ysin + XY.X, xsin + ycos + XY.Y),
    //        tr = new(wcos - ysin + XY.X, wsin + ycos + XY.Y),
    //        br = new(wcos - hsin + XY.X, wsin + hcos + XY.Y),
    //        bl = new(xcos - hsin + XY.X, xsin + hcos + XY.Y);
    //    return IsLeft(tl, tr, value) && IsLeft(tr, br, value) && IsLeft(br, bl, value) && IsLeft(bl, tl, value);
    //}
    ///// <summary>Gets whether or not the provided <see cref="Point"/> lies within the bounds of this <see cref="Circle"/></summary>
    ///// <param name="value">The coordinates to check for inclusion in this <see cref="Circle"/></param>
    ///// <returns><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="Circle"/>; <c>false</c> otherwise</returns>
    //public bool Contains(Point value) => Contains(value.ToVector2());
    ///// <summary>Gets whether or not the provided coordinates lie within the bounds of this <see cref="Circle"/></summary>
    ///// <param name="x">The x coordinate of the point to check for containment</param>
    ///// <param name="y">The y coordinate of the point to check for containment</param>
    ///// <returns><c>true</c> if the provided coordinates lie inside this <see cref="Circle"/>; <c>false</c> otherwise</returns>
    //public bool Contains(int x, int y) => Contains(new Vector2(x, y));
    ///// <summary>Gets whether or not the provided coordinates lie within the bounds of this <see cref="Circle"/></summary>
    ///// <param name="x">The x coordinate of the point to check for containment</param>
    ///// <param name="y">The y coordinate of the point to check for containment</param>
    ///// <returns><c>true</c> if the provided coordinates lie inside this <see cref="Circle"/>; <c>false</c> otherwise</returns>
    //public bool Contains(float x, float y) => Contains(new Vector2(x, y));
    ///// <summary>Gets whether or not the provided <see cref="Rectangle"/> lies within the bounds of this <see cref="Circle"/></summary>
    ///// <param name="value">The <see cref="Rectangle"/> to check for inclusion in this <see cref="Circle"/></param>
    ///// <returns><c>true</c> if the provided <see cref="Rectangle"/>'s bounds lie entirely inside this <see cref="Circle"/>; <c>false</c> otherwise</returns>
    //public bool Contains(Rectangle value) {
    //    float cos = MathF.Cos(Rotation),
    //        sin = MathF.Sin(Rotation),
    //        x = -Origin.X,
    //        y = -Origin.Y,
    //        w = Size.X + x,
    //        h = Size.Y + y,
    //        xcos = x * cos,
    //        ycos = y * cos,
    //        xsin = x * sin,
    //        ysin = y * sin,
    //        wcos = w * cos,
    //        wsin = w * sin,
    //        hcos = h * cos,
    //        hsin = h * sin;
    //    Vector2 tl = new(xcos - ysin + XY.X, xsin + ycos + XY.Y),
    //       tr = new(wcos - ysin + XY.X, wsin + ycos + XY.Y),
    //       br = new(wcos - hsin + XY.X, wsin + hcos + XY.Y),
    //       bl = new(xcos - hsin + XY.X, xsin + hcos + XY.Y),
    //       otl = new(value.Left, value.Top),
    //       otr = new(value.Right, value.Top),
    //       obl = new(value.Left, value.Bottom),
    //       obr = new(value.Right, value.Bottom);
    //    return PointInRect(tl, tr, br, bl, otl) && PointInRect(tl, tr, br, bl, otr) && PointInRect(tl, tr, br, bl, obr) && PointInRect(tl, tr, br, bl, obl);
    //}
    ///// <summary>Gets whether or not the provided <see cref="Circle"/> lies within the bounds of this <see cref="Circle"/></summary>
    ///// <param name="value">The <see cref="Circle"/> to check for inclusion in this <see cref="Circle"/></param>
    ///// <returns><c>true</c> if the provided <see cref="Circle"/>'s bounds lie entirely inside this <see cref="Circle"/>; <c>false</c> otherwise</returns>
    //public bool Contains(Circle value) {
    //    float cos = MathF.Cos(Rotation),
    //        sin = MathF.Sin(Rotation),
    //        x = -Origin.X,
    //        y = -Origin.Y,
    //        w = Size.X + x,
    //        h = Size.Y + y,
    //        xcos = x * cos,
    //        ycos = y * cos,
    //        xsin = x * sin,
    //        ysin = y * sin,
    //        wcos = w * cos,
    //        wsin = w * sin,
    //        hcos = h * cos,
    //        hsin = h * sin;
    //    Vector2 tl = new(xcos - ysin + XY.X, xsin + ycos + XY.Y),
    //       tr = new(wcos - ysin + XY.X, wsin + ycos + XY.Y),
    //       br = new(wcos - hsin + XY.X, wsin + hcos + XY.Y),
    //       bl = new(xcos - hsin + XY.X, xsin + hcos + XY.Y);
    //    cos = MathF.Cos(value.Rotation);
    //    sin = MathF.Sin(value.Rotation);
    //    x = -value.Origin.X;
    //    y = -value.Origin.Y;
    //    w = value.Size.X + x;
    //    h = value.Size.Y + y;
    //    xcos = x * cos;
    //    ycos = y * cos;
    //    xsin = x * sin;
    //    ysin = y * sin;
    //    wcos = w * cos;
    //    wsin = w * sin;
    //    hcos = h * cos;
    //    hsin = h * sin;
    //    Vector2 otl = new(xcos - ysin + value.XY.X, xsin + ycos + value.XY.Y),
    //        otr = new(wcos - ysin + value.XY.X, wsin + ycos + value.XY.Y),
    //        obr = new(wcos - hsin + value.XY.X, wsin + hcos + value.XY.Y),
    //        obl = new(xcos - hsin + value.XY.X, xsin + hcos + value.XY.Y);
    //    return PointInRect(tl, tr, br, bl, otl) && PointInRect(tl, tr, br, bl, otr) && PointInRect(tl, tr, br, bl, obr) && PointInRect(tl, tr, br, bl, obl);
    //}
    ///// <summary>Gets whether or not the provided <see cref="Quad"/> lies within the bounds of this <see cref="Circle"/></summary>
    ///// <param name="value">The <see cref="Quad"/> to check for inclusion in this <see cref="Circle"/></param>
    ///// <returns><c>true</c> if the provided <see cref="Quad"/>'s bounds lie entirely inside this <see cref="Circle"/>; <c>false</c> otherwise</returns>
    //public bool Contains(Quad value) {
    //    float cos = MathF.Cos(Rotation),
    //        sin = MathF.Sin(Rotation),
    //        x = -Origin.X,
    //        y = -Origin.Y,
    //        w = Size.X + x,
    //        h = Size.Y + y,
    //        xcos = x * cos,
    //        ycos = y * cos,
    //        xsin = x * sin,
    //        ysin = y * sin,
    //        wcos = w * cos,
    //        wsin = w * sin,
    //        hcos = h * cos,
    //        hsin = h * sin;
    //    Vector2 tl = new(xcos - ysin + XY.X, xsin + ycos + XY.Y),
    //       tr = new(wcos - ysin + XY.X, wsin + ycos + XY.Y),
    //       br = new(wcos - hsin + XY.X, wsin + hcos + XY.Y),
    //       bl = new(xcos - hsin + XY.X, xsin + hcos + XY.Y);
    //    return PointInRect(tl, tr, br, bl, value.A) && PointInRect(tl, tr, br, bl, value.B) && PointInRect(tl, tr, br, bl, value.C) && PointInRect(tl, tr, br, bl, value.D);
    //}

    ///// <summary>Adjusts the edges of this <see cref="Circle"/> by specified horizontal and vertical amounts</summary>
    ///// <param name="horizontal">Value to adjust the left and right edges</param>
    ///// <param name="vertical">Value to adjust the top and bottom edges</param>
    //public void Inflate(float horizontal, float vertical) {
    //    Size = new Vector2(horizontal * 2 + Size.X, vertical * 2 + Size.Y);
    //    Origin = new Vector2(horizontal + Origin.X, vertical + Origin.Y);
    //}
    ///// <summary>Adjusts the edges of this <see cref="Circle"/> by specified horizontal and vertical amounts</summary>
    ///// <param name="value">Value to adjust all edges</param>
    //public void Inflate(Vector2 value) => Inflate(value.X, value.Y);
    ///// <summary>Adjusts the edges of this <see cref="Circle"/> by specified horizontal and vertical amounts</summary>
    ///// <param name="value">Value to adjust all edges</param>
    //public void Inflate(Point value) => Inflate(value.X, value.Y);

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