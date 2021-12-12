using Microsoft.Xna.Framework;

namespace Dcrew;

/// <summary>A rotated rectangle</summary>
public struct RotRect {
    static bool IsLeft(Vector2 a, Vector2 b, Vector2 p) => (b.X - a.X) * (p.Y - a.Y) - (p.X - a.X) * (b.Y - a.Y) > 0;
    static bool PointInRect(Vector2 x, Vector2 y, Vector2 z, Vector2 w, Vector2 p) => IsLeft(x, y, p) && IsLeft(y, z, p) && IsLeft(z, w, p) && IsLeft(w, x, p);

    /// <summary>Coordinates of this <see cref="RotRect"/></summary>
    public Vector2 XY;
    /// <summary>Size of bounds</summary>
    public Vector2 Size;
    /// <summary>Rotation (in radians) of this <see cref="RotRect"/></summary>
    public float Rotation;
    /// <summary>Center of rotation of this <see cref="RotRect"/></summary>
    public Vector2 Origin;

    /// <summary>X coordinate of this <see cref="RotRect"/></summary>
    public float X {
        get => XY.X;
        set => XY.X = value;
    }
    /// <summary>Y coordinate of this <see cref="RotRect"/></summary>
    public float Y {
        get => XY.Y;
        set => XY.Y = value;
    }
    /// <summary>X size of this <see cref="RotRect"/></summary>
    public float Width {
        get => Size.X;
        set => Size.X = value;
    }
    /// <summary>Y size coordinate of this <see cref="RotRect"/></summary>
    public float Height {
        get => Size.Y;
        set => Size.Y = value;
    }

    /// <summary>A <see cref="Vector2"/> located in the center of this <see cref="RotRect"/></summary>
    public Vector2 Center {
        get {
            float cos = MathF.Cos(Rotation),
                sin = MathF.Sin(Rotation),
                x = -Origin.X,
                y = -Origin.Y,
                w = Size.X + x,
                h = Size.Y + y,
                xcos = x * cos,
                ycos = y * cos,
                xsin = x * sin,
                ysin = y * sin,
                wcos = w * cos,
                wsin = w * sin,
                hcos = h * cos,
                hsin = h * sin;
            return new Vector2((((xcos - ysin) + (wcos - ysin) + (wcos - hsin) + (xcos - hsin)) / 4) + XY.X, (((xsin + ycos) + (wsin + ycos) + (wsin + hcos) + (xsin + hcos)) / 4) + XY.Y);
        }
    }
    /// <summary>A <see cref="Rectangle"/> covering the min/max coordinates (bounds) of this <see cref="RotRect"/></summary>
    public Rectangle AABB {
        get {
            float cos = MathF.Cos(Rotation),
                sin = MathF.Sin(Rotation),
                x = -Origin.X,
                y = -Origin.Y,
                w = Size.X + x,
                h = Size.Y + y,
                xcos = x * cos,
                ycos = y * cos,
                xsin = x * sin,
                ysin = y * sin,
                wcos = w * cos,
                wsin = w * sin,
                hcos = h * cos,
                hsin = h * sin,
                tlx = xcos - ysin,
                tly = xsin + ycos,
                trx = wcos - ysin,
                tr_y = wsin + ycos,
                brx = wcos - hsin,
                bry = wsin + hcos,
                blx = xcos - hsin,
                bly = xsin + hcos,
                minx = tlx,
                miny = tly,
                maxx = minx,
                maxy = miny;
            if (trx < minx)
                minx = trx;
            if (brx < minx)
                minx = brx;
            if (blx < minx)
                minx = blx;
            if (tr_y < miny)
                miny = tr_y;
            if (bry < miny)
                miny = bry;
            if (bly < miny)
                miny = bly;
            if (trx > maxx)
                maxx = trx;
            if (brx > maxx)
                maxx = brx;
            if (blx > maxx)
                maxx = blx;
            if (tr_y > maxy)
                maxy = tr_y;
            if (bry > maxy)
                maxy = bry;
            if (bly > maxy)
                maxy = bly;
            var r = new Rectangle((int)minx, (int)miny, (int)MathF.Ceiling(maxx - minx), (int)MathF.Ceiling(maxy - miny));
            r.Offset(XY.X, XY.Y);
            return r;
        }
    }

    /// <summary>Creates a new instance of <see cref="Rectangle"/> struct, with the specified position, width, height, angle, and origin</summary>
    /// <param name="x">X coordinate of the created <see cref="RotRect"/></param>
    /// <param name="y">Y coordinate of the created <see cref="RotRect"/></param>
    /// <param name="width">X size of the created <see cref="RotRect"/></param>
    /// <param name="height">Y size of the created <see cref="RotRect"/></param>
    /// <param name="rotation">Rotation (in radians) of the created <see cref="RotRect"/></param>
    /// <param name="origin">Center of rotation of the created <see cref="RotRect"/></param>
    public RotRect(float x, float y, float width, float height, float rotation = default, Vector2 origin = default) {
        XY = new Vector2(x, y);
        Size = new Vector2(width, height);
        Rotation = rotation;
        Origin = origin;
    }
    /// <summary>Creates a new instance of <see cref="Rectangle"/> struct, with the specified position, width, height, angle, and origin</summary>
    /// <param name="x">X coordinate of the created <see cref="RotRect"/></param>
    /// <param name="y">Y coordinate of the created <see cref="RotRect"/></param>
    /// <param name="size">Size of the created <see cref="RotRect"/></param>
    /// <param name="rotation">Rotation (in radians) of the created <see cref="RotRect"/></param>
    /// <param name="origin">Center of rotation of the created <see cref="RotRect"/></param>
    public RotRect(float x, float y, Vector2 size, float rotation = default, Vector2 origin = default) : this(x, y, size.X, size.Y, rotation, origin) { }
    /// <summary>Creates a new instance of <see cref="Rectangle"/> struct, with the specified position, width, height, angle, and origin</summary>
    /// <param name="xy">Coordinates of the created <see cref="RotRect"/></param>
    /// <param name="size">Size of the created <see cref="RotRect"/></param>
    /// <param name="rotation">Rotation (in radians) of the created <see cref="RotRect"/></param>
    /// <param name="origin">Center of rotation of the created <see cref="RotRect"/></param>
    public RotRect(Vector2 xy, Vector2 size, float rotation = default, Vector2 origin = default) : this(xy.X, xy.Y, size.X, size.Y, rotation, origin) { }
    /// <summary>Creates a new instance of <see cref="Rectangle"/> struct, with the specified position, width, height, angle, and origin</summary>
    /// <param name="rectangle">Area of the created <see cref="RotRect"/></param>
    /// <param name="rotation">Rotation (in radians) of the created <see cref="RotRect"/></param>
    /// <param name="origin">Center of rotation of the created <see cref="RotRect"/></param>
    public RotRect(Rectangle rectangle, float rotation = default, Vector2 origin = default) : this(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, rotation, origin) { }

    /// <summary>Gets whether or not the other <see cref="Line"/> intersects with this <see cref="RotRect"/></summary>
    /// <param name="value">The other line for testing</param>
    /// <returns><c>true</c> if other <see cref="Line"/> intersects with this <see cref="RotRect"/>; <c>false</c> otherwise</returns>
    public bool Intersects(Line value) {
        float cos = MathF.Cos(Rotation),
            sin = MathF.Sin(Rotation),
            x = -Origin.X,
            y = -Origin.Y,
            w = Size.X + x,
            h = Size.Y + y,
            xcos = x * cos,
            ycos = y * cos,
            xsin = x * sin,
            ysin = y * sin,
            wcos = w * cos,
            wsin = w * sin,
            hcos = h * cos,
            hsin = h * sin;
        Vector2 tl = new(xcos - ysin + XY.X, xsin + ycos + XY.Y),
           tr = new(wcos - ysin + XY.X, wsin + ycos + XY.Y),
           br = new(wcos - hsin + XY.X, wsin + hcos + XY.Y),
           bl = new(xcos - hsin + XY.X, xsin + hcos + XY.Y);
        return new Quad(tl, tr, br, bl).Intersects(value);
    }
    /// <summary>Gets whether or not the other <see cref="Line"/> intersects with this <see cref="RotRect"/></summary>
    /// <param name="value">The other line for testing</param>
    /// <returns><c>true</c> if other <see cref="Line"/> intersects with this <see cref="RotRect"/>; <c>false</c> otherwise</returns>
    public bool Intersects(Line value, out CollisionResolution res) {
        float cos = MathF.Cos(Rotation),
            sin = MathF.Sin(Rotation),
            x = -Origin.X,
            y = -Origin.Y,
            w = Size.X + x,
            h = Size.Y + y,
            xcos = x * cos,
            ycos = y * cos,
            xsin = x * sin,
            ysin = y * sin,
            wcos = w * cos,
            wsin = w * sin,
            hcos = h * cos,
            hsin = h * sin;
        Vector2 tl = new(xcos - ysin + XY.X, xsin + ycos + XY.Y),
           tr = new(wcos - ysin + XY.X, wsin + ycos + XY.Y),
           br = new(wcos - hsin + XY.X, wsin + hcos + XY.Y),
           bl = new(xcos - hsin + XY.X, xsin + hcos + XY.Y);
        return new Quad(tl, tr, br, bl).Intersects(value, out res);
    }
    /// <summary>Gets whether or not the other <see cref="Rectangle"/> intersects with this <see cref="RotRect"/></summary>
    /// <param name="value">The other rectangle for testing</param>
    /// <returns><c>true</c> if other <see cref="Rectangle"/> intersects with this <see cref="RotRect"/>; <c>false</c> otherwise</returns>
    public bool Intersects(Rectangle value) {
        float cos = MathF.Cos(Rotation),
            sin = MathF.Sin(Rotation),
            x = -Origin.X,
            y = -Origin.Y,
            w = Size.X + x,
            h = Size.Y + y,
            xcos = x * cos,
            ycos = y * cos,
            xsin = x * sin,
            ysin = y * sin,
            wcos = w * cos,
            wsin = w * sin,
            hcos = h * cos,
            hsin = h * sin;
        Vector2 tl = new(xcos - ysin + XY.X, xsin + ycos + XY.Y),
           tr = new(wcos - ysin + XY.X, wsin + ycos + XY.Y),
           br = new(wcos - hsin + XY.X, wsin + hcos + XY.Y),
           bl = new(xcos - hsin + XY.X, xsin + hcos + XY.Y),
           otl = new(value.Left, value.Top),
           otr = new(value.Right, otl.Y),
           obr = new(otr.X, value.Bottom),
           obl = new(otl.X, obr.Y);
        return new Quad(tl, tr, br, bl).Intersects(new Quad(otl, otr, obr, obl));
    }
    /// <summary>Gets whether or not the other <see cref="Rectangle"/> intersects with this <see cref="RotRect"/></summary>
    /// <param name="value">The other rectangle for testing</param>
    /// <returns><c>true</c> if other <see cref="Rectangle"/> intersects with this <see cref="RotRect"/>; <c>false</c> otherwise</returns>
    public bool Intersects(Rectangle value, out CollisionResolution res) {
        float cos = MathF.Cos(Rotation),
            sin = MathF.Sin(Rotation),
            x = -Origin.X,
            y = -Origin.Y,
            w = Size.X + x,
            h = Size.Y + y,
            xcos = x * cos,
            ycos = y * cos,
            xsin = x * sin,
            ysin = y * sin,
            wcos = w * cos,
            wsin = w * sin,
            hcos = h * cos,
            hsin = h * sin;
        Vector2 tl = new(xcos - ysin + XY.X, xsin + ycos + XY.Y),
           tr = new(wcos - ysin + XY.X, wsin + ycos + XY.Y),
           br = new(wcos - hsin + XY.X, wsin + hcos + XY.Y),
           bl = new(xcos - hsin + XY.X, xsin + hcos + XY.Y),
           otl = new(value.Left, value.Top),
           otr = new(value.Right, otl.Y),
           obr = new(otr.X, value.Bottom),
           obl = new(otl.X, obr.Y);
        return new Quad(tl, tr, br, bl).Intersects(new Quad(otl, otr, obr, obl), out res);
    }
    /// <summary>Gets whether or not the other <see cref="RotRect"/> intersects with this <see cref="RotRect"/></summary>
    /// <param name="value">The other rectangle for testing</param>
    /// <returns><c>true</c> if other <see cref="RotRect"/> intersects with this <see cref="RotRect"/>; <c>false</c> otherwise</returns>
    public bool Intersects(RotRect value) {
        float cos = MathF.Cos(Rotation),
            sin = MathF.Sin(Rotation),
            x = -Origin.X,
            y = -Origin.Y,
            w = Size.X + x,
            h = Size.Y + y,
            xcos = x * cos,
            ycos = y * cos,
            xsin = x * sin,
            ysin = y * sin,
            wcos = w * cos,
            wsin = w * sin,
            hcos = h * cos,
            hsin = h * sin;
        Vector2 tl = new(xcos - ysin + XY.X, xsin + ycos + XY.Y),
           tr = new(wcos - ysin + XY.X, wsin + ycos + XY.Y),
           br = new(wcos - hsin + XY.X, wsin + hcos + XY.Y),
           bl = new(xcos - hsin + XY.X, xsin + hcos + XY.Y);
        cos = MathF.Cos(value.Rotation);
        sin = MathF.Sin(value.Rotation);
        x = -value.Origin.X;
        y = -value.Origin.Y;
        w = value.Size.X + x;
        h = value.Size.Y + y;
        xcos = x * cos;
        ycos = y * cos;
        xsin = x * sin;
        ysin = y * sin;
        wcos = w * cos;
        wsin = w * sin;
        hcos = h * cos;
        hsin = h * sin;
        Vector2 otl = new(xcos - ysin + value.XY.X, xsin + ycos + value.XY.Y),
            otr = new(wcos - ysin + value.XY.X, wsin + ycos + value.XY.Y),
            obr = new(wcos - hsin + value.XY.X, wsin + hcos + value.XY.Y),
            obl = new(xcos - hsin + value.XY.X, xsin + hcos + value.XY.Y);
        return new Quad(tl, tr, br, bl).Intersects(new Quad(otl, otr, obr, obl));
    }
    /// <summary>Gets whether or not the other <see cref="RotRect"/> intersects with this <see cref="RotRect"/></summary>
    /// <param name="value">The other rectangle for testing</param>
    /// <returns><c>true</c> if other <see cref="RotRect"/> intersects with this <see cref="RotRect"/>; <c>false</c> otherwise</returns>
    public bool Intersects(RotRect value, out CollisionResolution res) {
        float cos = MathF.Cos(Rotation),
            sin = MathF.Sin(Rotation),
            x = -Origin.X,
            y = -Origin.Y,
            w = Size.X + x,
            h = Size.Y + y,
            xcos = x * cos,
            ycos = y * cos,
            xsin = x * sin,
            ysin = y * sin,
            wcos = w * cos,
            wsin = w * sin,
            hcos = h * cos,
            hsin = h * sin;
        Vector2 tl = new(xcos - ysin + XY.X, xsin + ycos + XY.Y),
           tr = new(wcos - ysin + XY.X, wsin + ycos + XY.Y),
           br = new(wcos - hsin + XY.X, wsin + hcos + XY.Y),
           bl = new(xcos - hsin + XY.X, xsin + hcos + XY.Y);
        cos = MathF.Cos(value.Rotation);
        sin = MathF.Sin(value.Rotation);
        x = -value.Origin.X;
        y = -value.Origin.Y;
        w = value.Size.X + x;
        h = value.Size.Y + y;
        xcos = x * cos;
        ycos = y * cos;
        xsin = x * sin;
        ysin = y * sin;
        wcos = w * cos;
        wsin = w * sin;
        hcos = h * cos;
        hsin = h * sin;
        Vector2 otl = new(xcos - ysin + value.XY.X, xsin + ycos + value.XY.Y),
            otr = new(wcos - ysin + value.XY.X, wsin + ycos + value.XY.Y),
            obr = new(wcos - hsin + value.XY.X, wsin + hcos + value.XY.Y),
            obl = new(xcos - hsin + value.XY.X, xsin + hcos + value.XY.Y);
        return new Quad(tl, tr, br, bl).Intersects(new Quad(otl, otr, obr, obl), out res);
    }
    /// <summary>Gets whether or not the other <see cref="Quad"/> intersects with this <see cref="RotRect"/></summary>
    /// <param name="value">The other quad for testing</param>
    /// <returns><c>true</c> if other <see cref="Quad"/> intersects with this <see cref="RotRect"/>; <c>false</c> otherwise</returns>
    public bool Intersects(Quad value) {
        float cos = MathF.Cos(Rotation),
            sin = MathF.Sin(Rotation),
            x = -Origin.X,
            y = -Origin.Y,
            w = Size.X + x,
            h = Size.Y + y,
            xcos = x * cos,
            ycos = y * cos,
            xsin = x * sin,
            ysin = y * sin,
            wcos = w * cos,
            wsin = w * sin,
            hcos = h * cos,
            hsin = h * sin;
        Vector2 tl = new(xcos - ysin + XY.X, xsin + ycos + XY.Y),
           tr = new(wcos - ysin + XY.X, wsin + ycos + XY.Y),
           br = new(wcos - hsin + XY.X, wsin + hcos + XY.Y),
           bl = new(xcos - hsin + XY.X, xsin + hcos + XY.Y);
        return new Quad(tl, tr, br, bl).Intersects(value);
    }
    /// <summary>Gets whether or not the other <see cref="Quad"/> intersects with this <see cref="RotRect"/></summary>
    /// <param name="value">The other quad for testing</param>
    /// <returns><c>true</c> if other <see cref="Quad"/> intersects with this <see cref="RotRect"/>; <c>false</c> otherwise</returns>
    public bool Intersects(Quad value, out CollisionResolution res) {
        float cos = MathF.Cos(Rotation),
            sin = MathF.Sin(Rotation),
            x = -Origin.X,
            y = -Origin.Y,
            w = Size.X + x,
            h = Size.Y + y,
            xcos = x * cos,
            ycos = y * cos,
            xsin = x * sin,
            ysin = y * sin,
            wcos = w * cos,
            wsin = w * sin,
            hcos = h * cos,
            hsin = h * sin;
        Vector2 tl = new(xcos - ysin + XY.X, xsin + ycos + XY.Y),
           tr = new(wcos - ysin + XY.X, wsin + ycos + XY.Y),
           br = new(wcos - hsin + XY.X, wsin + hcos + XY.Y),
           bl = new(xcos - hsin + XY.X, xsin + hcos + XY.Y);
        return new Quad(tl, tr, br, bl).Intersects(value, out res);
    }
    /// <summary>Gets whether or not the other <see cref="ConvPoly"/> intersects with this <see cref="RotRect"/></summary>
    /// <param name="value">The other convex polygon for testing</param>
    /// <returns><c>true</c> if other <see cref="ConvPoly"/> intersects with this <see cref="RotRect"/>; <c>false</c> otherwise</returns>
    public bool Intersects(ConvPoly value) {
        float cos = MathF.Cos(Rotation),
            sin = MathF.Sin(Rotation),
            x = -Origin.X,
            y = -Origin.Y,
            w = Size.X + x,
            h = Size.Y + y,
            xcos = x * cos,
            ycos = y * cos,
            xsin = x * sin,
            ysin = y * sin,
            wcos = w * cos,
            wsin = w * sin,
            hcos = h * cos,
            hsin = h * sin;
        Vector2 tl = new(xcos - ysin + XY.X, xsin + ycos + XY.Y),
           tr = new(wcos - ysin + XY.X, wsin + ycos + XY.Y),
           br = new(wcos - hsin + XY.X, wsin + hcos + XY.Y),
           bl = new(xcos - hsin + XY.X, xsin + hcos + XY.Y);
        return new Quad(tl, tr, br, bl).Intersects(value);
    }
    /// <summary>Gets whether or not the other <see cref="ConvPoly"/> intersects with this <see cref="RotRect"/></summary>
    /// <param name="value">The other convex polygon for testing</param>
    /// <returns><c>true</c> if other <see cref="ConvPoly"/> intersects with this <see cref="RotRect"/>; <c>false</c> otherwise</returns>
    public bool Intersects(ConvPoly value, out CollisionResolution res) {
        float cos = MathF.Cos(Rotation),
            sin = MathF.Sin(Rotation),
            x = -Origin.X,
            y = -Origin.Y,
            w = Size.X + x,
            h = Size.Y + y,
            xcos = x * cos,
            ycos = y * cos,
            xsin = x * sin,
            ysin = y * sin,
            wcos = w * cos,
            wsin = w * sin,
            hcos = h * cos,
            hsin = h * sin;
        Vector2 tl = new(xcos - ysin + XY.X, xsin + ycos + XY.Y),
           tr = new(wcos - ysin + XY.X, wsin + ycos + XY.Y),
           br = new(wcos - hsin + XY.X, wsin + hcos + XY.Y),
           bl = new(xcos - hsin + XY.X, xsin + hcos + XY.Y);
        return new Quad(tl, tr, br, bl).Intersects(value, out res);
    }

    /// <summary>Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="RotRect"/></summary>
    /// <param name="value">The coordinates to check for inclusion in this <see cref="RotRect"/></param>
    /// <returns><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="RotRect"/>; <c>false</c> otherwise</returns>
    public bool Contains(Vector2 value) {
        float cos = MathF.Cos(Rotation),
            sin = MathF.Sin(Rotation),
            x = -Origin.X,
            y = -Origin.Y,
            w = Size.X + x,
            h = Size.Y + y,
            xcos = x * cos,
            ycos = y * cos,
            xsin = x * sin,
            ysin = y * sin,
            wcos = w * cos,
            wsin = w * sin,
            hcos = h * cos,
            hsin = h * sin;
        Vector2 tl = new(xcos - ysin + XY.X, xsin + ycos + XY.Y),
            tr = new(wcos - ysin + XY.X, wsin + ycos + XY.Y),
            br = new(wcos - hsin + XY.X, wsin + hcos + XY.Y),
            bl = new(xcos - hsin + XY.X, xsin + hcos + XY.Y);
        return IsLeft(tl, tr, value) && IsLeft(tr, br, value) && IsLeft(br, bl, value) && IsLeft(bl, tl, value);
    }
    /// <summary>Gets whether or not the provided <see cref="Point"/> lies within the bounds of this <see cref="RotRect"/></summary>
    /// <param name="value">The coordinates to check for inclusion in this <see cref="RotRect"/></param>
    /// <returns><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="RotRect"/>; <c>false</c> otherwise</returns>
    public bool Contains(Point value) => Contains(value.ToVector2());
    /// <summary>Gets whether or not the provided coordinates lie within the bounds of this <see cref="RotRect"/></summary>
    /// <param name="x">The x coordinate of the point to check for containment</param>
    /// <param name="y">The y coordinate of the point to check for containment</param>
    /// <returns><c>true</c> if the provided coordinates lie inside this <see cref="RotRect"/>; <c>false</c> otherwise</returns>
    public bool Contains(int x, int y) => Contains(new Vector2(x, y));
    /// <summary>Gets whether or not the provided coordinates lie within the bounds of this <see cref="RotRect"/></summary>
    /// <param name="x">The x coordinate of the point to check for containment</param>
    /// <param name="y">The y coordinate of the point to check for containment</param>
    /// <returns><c>true</c> if the provided coordinates lie inside this <see cref="RotRect"/>; <c>false</c> otherwise</returns>
    public bool Contains(float x, float y) => Contains(new Vector2(x, y));
    /// <summary>Gets whether or not the provided <see cref="Rectangle"/> lies within the bounds of this <see cref="RotRect"/></summary>
    /// <param name="value">The <see cref="Rectangle"/> to check for inclusion in this <see cref="RotRect"/></param>
    /// <returns><c>true</c> if the provided <see cref="Rectangle"/>'s bounds lie entirely inside this <see cref="RotRect"/>; <c>false</c> otherwise</returns>
    public bool Contains(Rectangle value) {
        float cos = MathF.Cos(Rotation),
            sin = MathF.Sin(Rotation),
            x = -Origin.X,
            y = -Origin.Y,
            w = Size.X + x,
            h = Size.Y + y,
            xcos = x * cos,
            ycos = y * cos,
            xsin = x * sin,
            ysin = y * sin,
            wcos = w * cos,
            wsin = w * sin,
            hcos = h * cos,
            hsin = h * sin;
        Vector2 tl = new(xcos - ysin + XY.X, xsin + ycos + XY.Y),
           tr = new(wcos - ysin + XY.X, wsin + ycos + XY.Y),
           br = new(wcos - hsin + XY.X, wsin + hcos + XY.Y),
           bl = new(xcos - hsin + XY.X, xsin + hcos + XY.Y),
           otl = new(value.Left, value.Top),
           otr = new(value.Right, value.Top),
           obl = new(value.Left, value.Bottom),
           obr = new(value.Right, value.Bottom);
        return PointInRect(tl, tr, br, bl, otl) && PointInRect(tl, tr, br, bl, otr) && PointInRect(tl, tr, br, bl, obr) && PointInRect(tl, tr, br, bl, obl);
    }
    /// <summary>Gets whether or not the provided <see cref="RotRect"/> lies within the bounds of this <see cref="RotRect"/></summary>
    /// <param name="value">The <see cref="RotRect"/> to check for inclusion in this <see cref="RotRect"/></param>
    /// <returns><c>true</c> if the provided <see cref="RotRect"/>'s bounds lie entirely inside this <see cref="RotRect"/>; <c>false</c> otherwise</returns>
    public bool Contains(RotRect value) {
        float cos = MathF.Cos(Rotation),
            sin = MathF.Sin(Rotation),
            x = -Origin.X,
            y = -Origin.Y,
            w = Size.X + x,
            h = Size.Y + y,
            xcos = x * cos,
            ycos = y * cos,
            xsin = x * sin,
            ysin = y * sin,
            wcos = w * cos,
            wsin = w * sin,
            hcos = h * cos,
            hsin = h * sin;
        Vector2 tl = new(xcos - ysin + XY.X, xsin + ycos + XY.Y),
           tr = new(wcos - ysin + XY.X, wsin + ycos + XY.Y),
           br = new(wcos - hsin + XY.X, wsin + hcos + XY.Y),
           bl = new(xcos - hsin + XY.X, xsin + hcos + XY.Y);
        cos = MathF.Cos(value.Rotation);
        sin = MathF.Sin(value.Rotation);
        x = -value.Origin.X;
        y = -value.Origin.Y;
        w = value.Size.X + x;
        h = value.Size.Y + y;
        xcos = x * cos;
        ycos = y * cos;
        xsin = x * sin;
        ysin = y * sin;
        wcos = w * cos;
        wsin = w * sin;
        hcos = h * cos;
        hsin = h * sin;
        Vector2 otl = new(xcos - ysin + value.XY.X, xsin + ycos + value.XY.Y),
            otr = new(wcos - ysin + value.XY.X, wsin + ycos + value.XY.Y),
            obr = new(wcos - hsin + value.XY.X, wsin + hcos + value.XY.Y),
            obl = new(xcos - hsin + value.XY.X, xsin + hcos + value.XY.Y);
        return PointInRect(tl, tr, br, bl, otl) && PointInRect(tl, tr, br, bl, otr) && PointInRect(tl, tr, br, bl, obr) && PointInRect(tl, tr, br, bl, obl);
    }
    /// <summary>Gets whether or not the provided <see cref="Quad"/> lies within the bounds of this <see cref="RotRect"/></summary>
    /// <param name="value">The <see cref="Quad"/> to check for inclusion in this <see cref="RotRect"/></param>
    /// <returns><c>true</c> if the provided <see cref="Quad"/>'s bounds lie entirely inside this <see cref="RotRect"/>; <c>false</c> otherwise</returns>
    public bool Contains(Quad value) {
        float cos = MathF.Cos(Rotation),
            sin = MathF.Sin(Rotation),
            x = -Origin.X,
            y = -Origin.Y,
            w = Size.X + x,
            h = Size.Y + y,
            xcos = x * cos,
            ycos = y * cos,
            xsin = x * sin,
            ysin = y * sin,
            wcos = w * cos,
            wsin = w * sin,
            hcos = h * cos,
            hsin = h * sin;
        Vector2 tl = new(xcos - ysin + XY.X, xsin + ycos + XY.Y),
           tr = new(wcos - ysin + XY.X, wsin + ycos + XY.Y),
           br = new(wcos - hsin + XY.X, wsin + hcos + XY.Y),
           bl = new(xcos - hsin + XY.X, xsin + hcos + XY.Y);
        return PointInRect(tl, tr, br, bl, value.A) && PointInRect(tl, tr, br, bl, value.B) && PointInRect(tl, tr, br, bl, value.C) && PointInRect(tl, tr, br, bl, value.D);
    }

    /// <summary>Adjusts the edges of this <see cref="RotRect"/> by specified horizontal and vertical amounts</summary>
    /// <param name="horizontal">Value to adjust the left and right edges</param>
    /// <param name="vertical">Value to adjust the top and bottom edges</param>
    public void Inflate(float horizontal, float vertical) {
        Size = new Vector2(horizontal * 2 + Size.X, vertical * 2 + Size.Y);
        Origin = new Vector2(horizontal + Origin.X, vertical + Origin.Y);
    }
    /// <summary>Adjusts the edges of this <see cref="RotRect"/> by specified horizontal and vertical amounts</summary>
    /// <param name="value">Value to adjust all edges</param>
    public void Inflate(Vector2 value) => Inflate(value.X, value.Y);
    /// <summary>Adjusts the edges of this <see cref="RotRect"/> by specified horizontal and vertical amounts</summary>
    /// <param name="value">Value to adjust all edges</param>
    public void Inflate(Point value) => Inflate(value.X, value.Y);

    /// <summary>Changes the <see cref="XY"/> of this <see cref="RotRect"/></summary>
    /// <param name="offsetX">The x coordinate to add to this <see cref="RotRect"/></param>
    /// <param name="offsetY">The y coordinate to add to this <see cref="RotRect"/></param>
    public void Offset(float offsetX, float offsetY) => XY = new Vector2(XY.X + offsetX, XY.Y + offsetY);
    /// <summary>Changes the <see cref="XY"/> of this <see cref="RotRect"/></summary>
    /// <param name="amount">The x and y components to add to this <see cref="RotRect"/></param>
    public void Offset(Vector2 amount) => Offset(amount.X, amount.Y);
    /// <summary>Changes the <see cref="XY"/> of this <see cref="RotRect"/></summary>
    /// <param name="amount">The x and y components to add to this <see cref="RotRect"/></param>
    public void Offset(Point amount) => Offset(amount.X, amount.Y);

    /// <summary>Find the closest point to the given position from within this <see cref="RotRect"/></summary>
    /// <param name="xy">Position</param>
    /// <returns><see cref="Vector2"/> closest to <paramref name="xy"/> that lies within this <see cref="RotRect"/></returns>
    public Vector2 ClosestPoint(Vector2 xy) {
        float cos = MathF.Cos(-Rotation),
            sin = MathF.Sin(-Rotation),
            x = xy.X - XY.X,
            y = xy.Y - XY.Y,
            xcos = x * cos,
            ycos = y * cos,
            xsin = x * sin,
            ysin = y * sin;
        var p = new Vector2(xcos - ysin + XY.X, xsin + ycos + XY.Y);
        p.X = MathHelper.Clamp(p.X, XY.X - Origin.X, XY.X + Size.X - Origin.X);
        p.Y = MathHelper.Clamp(p.Y, XY.Y - Origin.Y, XY.Y + Size.Y - Origin.Y);
        cos = MathF.Cos(Rotation);
        sin = MathF.Sin(Rotation);
        x = p.X - XY.X;
        y = p.Y - XY.Y;
        xcos = x * cos;
        ycos = y * cos;
        xsin = x * sin;
        ysin = y * sin;
        return new Vector2(xcos - ysin + XY.X, xsin + ycos + XY.Y);
    }
    /// <summary>Find the closest corner point of this <see cref="RotRect"/> to the given position</summary>
    /// <param name="xy">Position</param>
    /// <returns><see cref="Vector2"/> closest corner of this <see cref="RotRect"/> to <paramref name="xy"/></returns>
    public Vector2 ClosestCornerPoint(Vector2 xy) {
        float cos = MathF.Cos(-Rotation),
            sin = MathF.Sin(-Rotation),
            x = xy.X - XY.X,
            y = xy.Y - XY.Y,
            xcos = x * cos,
            ycos = y * cos,
            xsin = x * sin,
            ysin = y * sin;
        var p = new Vector2(xcos - ysin + XY.X, xsin + ycos + XY.Y);
        float cXMin = XY.X - Origin.X,
            cXMax = XY.X + Size.X - Origin.X,
            cYMin = XY.Y - Origin.Y,
            cYMax = XY.Y + Size.Y - Origin.Y;
        if (p.X - cXMin < cXMax - p.X)
            p.X = cXMin;
        else
            p.X = cXMax;
        if (p.Y - cYMin < cYMax - p.Y)
            p.Y = cYMin;
        else
            p.Y = cYMax;
        cos = MathF.Cos(Rotation);
        sin = MathF.Sin(Rotation);
        x = p.X - XY.X;
        y = p.Y - XY.Y;
        xcos = x * cos;
        ycos = y * cos;
        xsin = x * sin;
        ysin = y * sin;
        return new Vector2(xcos - ysin + XY.X, xsin + ycos + XY.Y);
    }
}