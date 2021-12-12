using Microsoft.Xna.Framework;

namespace Dcrew;

/// <summary>A Line</summary>
public struct Line {
    internal static Vector2 FarthestPInDir(Vector2 a, Vector2 b, Vector2 dir) {
        var p = a;
        var di = Vector2.Dot(p, dir);
        var dp2 = Vector2.Dot(b, dir);
        if (dp2 > di)
            return b;
        return p;
    }

    internal static Vector2 Support((Vector2 a, Vector2 b) v1, Vector2[] v2, Vector2 dir) {
        var a = FarthestPInDir(v1.a, v1.b, dir);
        var b = ConvPoly.FarthestPInDir(v2, -dir);
        return a - b;
    }
    internal static Vector2 Support((Vector2 a, Vector2 b) v1, (Vector2 a, Vector2 b) v2, Vector2 dir) {
        var a = FarthestPInDir(v1.a, v1.b, dir);
        var b = FarthestPInDir(v2.a, v2.b, -dir);
        return a - b;
    }
    internal static Vector2 Support((Vector2 a, Vector2 b) v1, (Vector2 a, Vector2 b, Vector2 c, Vector2 d) v2, Vector2 dir) {
        var a = FarthestPInDir(v1.a, v1.b, dir);
        var b = Quad.FarthestPInDir(v2.a, v2.b, v2.c, v2.d, -dir);
        return a - b;
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

    /// <summary>Gets whether or not the other <see cref="Rectangle"/> intersects with this <see cref="Line"/></summary>
    /// <param name="value">The other rectangle for testing</param>
    /// <returns><c>true</c> if other <see cref="Rectangle"/> intersects with this <see cref="Line"/>; <c>false</c> otherwise</returns>
    public bool Intersects(Rectangle value) {
        Vector2 otl = new(value.Left, value.Top),
           otr = new(value.Right, otl.Y),
           obr = new(otr.X, value.Bottom),
           obl = new(otl.X, obr.Y);
        return Intersects(new Quad(otl, otr, obr, obl));
    }
    /// <summary>Gets whether or not the other <see cref="Rectangle"/> intersects with this <see cref="Line"/></summary>
    /// <param name="value">The other rectangle for testing</param>
    /// <returns><c>true</c> if other <see cref="Rectangle"/> intersects with this <see cref="Line"/>; <c>false</c> otherwise</returns>
    public bool Intersects(Rectangle value, out CollisionResolution res) {
        Vector2 otl = new(value.Left, value.Top),
           otr = new(value.Right, otl.Y),
           obr = new(otr.X, value.Bottom),
           obl = new(otl.X, obr.Y);
        return Intersects(new Quad(otl, otr, obr, obl), out res);
    }
    /// <summary>Gets whether or not the other <see cref="RotRect"/> intersects with this <see cref="Line"/></summary>
    /// <param name="value">The other rect for testing</param>
    /// <returns><c>true</c> if other <see cref="RotRect"/> intersects with this <see cref="Line"/>; <c>false</c> otherwise</returns>
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
    /// <summary>Gets whether or not the other <see cref="RotRect"/> intersects with this <see cref="Line"/></summary>
    /// <param name="value">The other rect for testing</param>
    /// <returns><c>true</c> if other <see cref="RotRect"/> intersects with this <see cref="Line"/>; <c>false</c> otherwise</returns>
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
    /// <summary>Gets whether or not the other <see cref="Line"/> intersects with this <see cref="Line"/></summary>
    /// <param name="value">The other rectangle for testing</param>
    /// <returns><c>true</c> if other <see cref="Line"/> intersects with this <see cref="Line"/>; <c>false</c> otherwise</returns>
    public bool Intersects(Line value) {
        Vector2 dir = new(0, 1);
        var v1 = (A, B);
        var v2 = (value.A, value.B);
        var spa = Support(v1, v2, dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return false;
        dir = -dir;
        var spb = Support(v1, v2, dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return false;
        var simplex = new ConvPoly.Simplex(spa, spb);
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
    /// <summary>Gets whether or not the other <see cref="Line"/> intersects with this <see cref="Line"/></summary>
    /// <param name="value">The other rectangle for testing</param>
    /// <returns><c>true</c> if other <see cref="Line"/> intersects with this <see cref="Line"/>; <c>false</c> otherwise</returns>
    public bool Intersects(Line value, out CollisionResolution res) {
        res = new();
        Vector2 dir = new(0, 1);
        var v1 = (A, B);
        var v2 = (value.A, value.B);
        var spa = Support(v1, v2, dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return false;
        dir = -dir;
        var spb = Support(v1, v2, dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return false;
        var simplex = new ConvPoly.Simplex(spa, spb);
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
        ConvPoly.Edge edge;
        ConvPoly.Polytope polytope = new(simplex);
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
    /// <summary>Gets whether or not the other <see cref="Line"/> intersects with this <see cref="Line"/></summary>
    /// <param name="value">The other rectangle for testing</param>
    /// <returns><c>true</c> if other <see cref="Line"/> intersects with this <see cref="Line"/>; <c>false</c> otherwise</returns>
    public bool Intersects(Quad value) {
        Vector2 dir = new(0, 1);
        var v1 = (A, B);
        var v2 = (value.A, value.B, value.C, value.D);
        var spa = Support(v1, v2, dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return false;
        dir = -dir;
        var spb = Support(v1, v2, dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return false;
        var simplex = new ConvPoly.Simplex(spa, spb);
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
    /// <summary>Gets whether or not the other <see cref="Line"/> intersects with this <see cref="Line"/></summary>
    /// <param name="value">The other rectangle for testing</param>
    /// <returns><c>true</c> if other <see cref="Line"/> intersects with this <see cref="Line"/>; <c>false</c> otherwise</returns>
    public bool Intersects(Quad value, out CollisionResolution res) {
        res = new();
        Vector2 dir = new(0, 1);
        var v1 = (A, B);
        var v2 = (value.A, value.B, value.C, value.D);
        var spa = Support(v1, v2, dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return false;
        dir = -dir;
        var spb = Support(v1, v2, dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return false;
        var simplex = new ConvPoly.Simplex(spa, spb);
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
        ConvPoly.Edge edge;
        ConvPoly.Polytope polytope = new(simplex);
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
    /// <summary>Gets whether or not the other <see cref="ConvPoly"/> intersects with this <see cref="Line"/></summary>
    /// <param name="value">The other convex polygon for testing</param>
    /// <returns><c>true</c> if other <see cref="ConvPoly"/> intersects with this <see cref="Line"/>; <c>false</c> otherwise</returns>
    public bool Intersects(ConvPoly value) {
        Vector2 dir = new(0, 1);
        var v1 = (A, B);
        var v2 = value.Verts;
        var spa = Support(v1, v2, dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return false;
        dir = -dir;
        var spb = Support(v1, v2, dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return false;
        var simplex = new ConvPoly.Simplex(spa, spb);
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
    /// <summary>Gets whether or not the other <see cref="ConvPoly"/> intersects with this <see cref="Line"/></summary>
    /// <param name="value">The other rectangle for testing</param>
    /// <returns><c>true</c> if other <see cref="ConvPoly"/> intersects with this <see cref="Line"/>; <c>false</c> otherwise</returns>
    public bool Intersects(ConvPoly value, out CollisionResolution res) {
        res = new CollisionResolution();
        Vector2 dir = new(0, 1);
        var v1 = (A, B);
        var v2 = value.Verts;
        var spa = Support(v1, v2, dir);
        if (Vector2.Dot(spa, dir) <= 0)
            return false;
        dir = -dir;
        var spb = Support(v1, v2, dir);
        if (Vector2.Dot(spb, dir) <= 0)
            return false;
        var simplex = new ConvPoly.Simplex(spa, spb);
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
        ConvPoly.Edge edge;
        ConvPoly.Polytope polytope = new(simplex);
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

    /// <summary>Changes all corners of this <see cref="Line"/></summary>
    /// <param name="offsetX">The x coordinate to add to this <see cref="Line"/></param>
    /// <param name="offsetY">The y coordinate to add to this <see cref="Line"/></param>
    public void Offset(float offsetX, float offsetY) {
        Vector2 v = new(offsetX, offsetY);
        A += v;
        B += v;
        _xy += v;
    }
    /// <summary>Changes all corners of this <see cref="Line"/></summary>
    /// <param name="amount">The x and y components to add to this <see cref="Line"/></param>
    public void Offset(Vector2 amount) => Offset(amount.X, amount.Y);
    /// <summary>Changes all corners of this <see cref="Line"/></summary>
    /// <param name="amount">The x and y components to add to this <see cref="Line"/></param>
    public void Offset(Point amount) => Offset(amount.X, amount.Y);
}