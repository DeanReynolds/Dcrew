//using Microsoft.Xna.Framework;

//namespace Dcrew;

///// <summary>An oval</summary>
//public struct Capsule {
//    internal static unsafe Vector2 FarthestPoint(Capsule s, Vector2 dir) {
//        if (Vector2.Dot(s.B, dir) > Vector2.Dot(s.A, dir))
//            return s.B;
//        return s.A;
//    }

//    Vector2 _xy;
//    /// <summary>1st vertex (local)</summary>
//    public Vector2 A;
//    /// <summary>2nd vertex (local)</summary>
//    public Vector2 B;
//    public float Radius;
//    /// <summary>Rotation (in radians)</summary>
//    public float Rotation {
//        get => MathF.Atan2(B.Y - A.Y, B.X - A.X);
//        set {
//            static Vector2 RotatePoint(Vector2 p, float cos, float sin) {
//                float xcos = p.X * cos,
//                    ycos = p.Y * cos,
//                    xsin = p.X * sin,
//                    ysin = p.Y * sin;
//                return new Vector2(xcos - ysin, xsin + ycos);
//            }
//            float old = MathF.Atan2(B.Y - A.Y, B.X - A.X),
//                cos = MathF.Cos(-old),
//                sin = MathF.Sin(-old),
//                cos2 = MathF.Cos(value),
//                sin2 = MathF.Sin(value);
//            A = RotatePoint(A - _xy, cos, sin);
//            A = RotatePoint(A, cos2, sin2) + _xy;
//            B = RotatePoint(B - _xy, cos, sin);
//            B = RotatePoint(B, cos2, sin2) + _xy;
//        }
//    }
//    /// <summary>Center of rotation</summary>
//    public Vector2 Origin;

//    public Capsule(Vector2 a, Vector2 b, float radius, float rotation = 0, Vector2 origin = default) {
//        A = a;
//        B = b;
//        Radius = radius;
//        Origin = origin;
//        _xy = Vector2.Zero;
//        Rotation = rotation;
//    }

//    ///// <summary>Gets whether or not the given <see cref="Line"/> intersects with this <see cref="Capsule"/></summary>
//    //public unsafe bool Intersects(Line value) {
//    //    var shape1 = this;
//    //    Collision.GJK(&shape1, &FarthestPoint, &value, &Line.FarthestPoint, out var intersects);
//    //    return intersects;
//    //}
//    ///// <summary>Gets whether or not the given <see cref="Line"/> intersects with this <see cref="Capsule"/></summary>
//    //public unsafe bool Intersects(Line value, out CollisionResolution res) {
//    //    var shape1 = this;
//    //    Collision.GJK(&shape1, &FarthestPoint, &value, &Line.FarthestPoint, out var intersects, out res);
//    //    return intersects;
//    //}
//    ///// <summary>Gets whether or not the given <see cref="Capsule"/> intersects with this <see cref="Capsule"/></summary>
//    //public unsafe bool Intersects(Capsule value) {
//    //    var shape1 = this;
//    //    Collision.GJK(&shape1, &FarthestPoint, &value, &Capsule.FarthestPoint, out var intersects);
//    //    return intersects;
//    //}
//    ///// <summary>Gets whether or not the given <see cref="Capsule"/> intersects with this <see cref="Capsule"/></summary>
//    //public unsafe bool Intersects(Capsule value, out CollisionResolution res) {
//    //    var shape1 = this;
//    //    Collision.GJK(&shape1, &FarthestPoint, &value, &Capsule.FarthestPoint, out var intersects, out res);
//    //    return intersects;
//    //}
//    ///// <summary>Gets whether or not the given <see cref="Rectangle"/> intersects with this <see cref="Capsule"/></summary>
//    //public bool Intersects(Rectangle value) {
//    //    Vector2 otl = new(value.Left, value.Top),
//    //       otr = new(value.Right, otl.Y),
//    //       obr = new(otr.X, value.Bottom),
//    //       obl = new(otl.X, obr.Y);
//    //    return Intersects(new Quad(otl, otr, obr, obl));
//    //}
//    ///// <summary>Gets whether or not the given <see cref="Rectangle"/> intersects with this <see cref="Capsule"/></summary>
//    //public bool Intersects(Rectangle value, out CollisionResolution res) {
//    //    Vector2 otl = new(value.Left, value.Top),
//    //       otr = new(value.Right, otl.Y),
//    //       obr = new(otr.X, value.Bottom),
//    //       obl = new(otl.X, obr.Y);
//    //    return Intersects(new Quad(otl, otr, obr, obl), out res);
//    //}
//    ///// <summary>Gets whether or not the given <see cref="RotRect"/> intersects with this <see cref="Capsule"/></summary>
//    //public bool Intersects(RotRect value) {
//    //    float cos = MathF.Cos(value.Rotation),
//    //        sin = MathF.Sin(value.Rotation),
//    //        x = -value.Origin.X,
//    //        y = -value.Origin.Y,
//    //        w = value.Size.X + x,
//    //        h = value.Size.Y + y,
//    //        xcos = x * cos,
//    //        ycos = y * cos,
//    //        xsin = x * sin,
//    //        ysin = y * sin,
//    //        wcos = w * cos,
//    //        wsin = w * sin,
//    //        hcos = h * cos,
//    //        hsin = h * sin;
//    //    Vector2 otl = new(xcos - ysin + value.XY.X, xsin + ycos + value.XY.Y),
//    //       otr = new(wcos - ysin + value.XY.X, wsin + ycos + value.XY.Y),
//    //       obr = new(wcos - hsin + value.XY.X, wsin + hcos + value.XY.Y),
//    //       obl = new(xcos - hsin + value.XY.X, xsin + hcos + value.XY.Y);
//    //    return Intersects(new Quad(otl, otr, obr, obl));
//    //}
//    ///// <summary>Gets whether or not the given <see cref="RotRect"/> intersects with this <see cref="Capsule"/></summary>
//    //public bool Intersects(RotRect value, out CollisionResolution res) {
//    //    float cos = MathF.Cos(value.Rotation),
//    //        sin = MathF.Sin(value.Rotation),
//    //        x = -value.Origin.X,
//    //        y = -value.Origin.Y,
//    //        w = value.Size.X + x,
//    //        h = value.Size.Y + y,
//    //        xcos = x * cos,
//    //        ycos = y * cos,
//    //        xsin = x * sin,
//    //        ysin = y * sin,
//    //        wcos = w * cos,
//    //        wsin = w * sin,
//    //        hcos = h * cos,
//    //        hsin = h * sin;
//    //    Vector2 otl = new(xcos - ysin + value.XY.X, xsin + ycos + value.XY.Y),
//    //       otr = new(wcos - ysin + value.XY.X, wsin + ycos + value.XY.Y),
//    //       obr = new(wcos - hsin + value.XY.X, wsin + hcos + value.XY.Y),
//    //       obl = new(xcos - hsin + value.XY.X, xsin + hcos + value.XY.Y);
//    //    return Intersects(new Quad(otl, otr, obr, obl), out res);
//    //}
//    ///// <summary>Gets whether or not the given <see cref="Quad"/> intersects with this <see cref="Capsule"/></summary>
//    //public unsafe bool Intersects(Quad value) {
//    //    var shape1 = this;
//    //    Collision.GJK(&shape1, &FarthestPoint, &value, &Quad.FarthestPoint, out var intersects);
//    //    return intersects;
//    //}
//    ///// <summary>Gets whether or not the given <see cref="Quad"/> intersects with this <see cref="Capsule"/></summary>
//    //public unsafe bool Intersects(Quad value, out CollisionResolution res) {
//    //    var shape1 = this;
//    //    Collision.GJK(&shape1, &FarthestPoint, &value, &Quad.FarthestPoint, out var intersects, out res);
//    //    return intersects;
//    //}
//    /// <summary>Gets whether or not the given <see cref="ConvPoly"/> intersects with this <see cref="Capsule"/></summary>
//    public unsafe bool Intersects(ConvPoly value) {
//        Collision.GJK(this, value, out var intersects, out _);
//        return intersects;
//    }
//    /// <summary>Gets whether or not the given <see cref="ConvPoly"/> intersects with this <see cref="Capsule"/></summary>
//    public unsafe bool Intersects(ConvPoly value, out CollisionResolution res) {
//        Collision.GJK(this, value, out var intersects, out res);
//        return intersects;
//    }

//    /// <summary>Adjusts the radius of this <see cref="Capsule"/> by specified amount</summary>
//    public void Inflate(float amount) => Radius += amount;

//    /// <summary>Changes the <see cref="XY"/> of this <see cref="Capsule"/></summary>
//    public void Offset(float offsetX, float offsetY) {
//        Vector2 v = new(offsetX, offsetY);
//        A += v;
//        B += v;
//        _xy += v;
//    }
//    /// <summary>Changes the <see cref="XY"/> of this <see cref="Capsule"/></summary>
//    public void Offset(Vector2 amount) => Offset(amount.X, amount.Y);
//    /// <summary>Changes the <see cref="XY"/> of this <see cref="Capsule"/></summary>
//    public void Offset(Point amount) => Offset(amount.X, amount.Y);

//    ///// <summary>Find the closest point to the given position from within this <see cref="Oval"/></summary>
//    ///// <param name="xy">Position</param>
//    ///// <returns><see cref="Vector2"/> closest to <paramref name="xy"/> that lies within this <see cref="Oval"/></returns>
//    //public Vector2 ClosestPoint(Vector2 xy) {
//    //    float cos = MathF.Cos(-Rotation),
//    //        sin = MathF.Sin(-Rotation),
//    //        x = xy.X - XY.X,
//    //        y = xy.Y - XY.Y,
//    //        xcos = x * cos,
//    //        ycos = y * cos,
//    //        xsin = x * sin,
//    //        ysin = y * sin;
//    //    var p = new Vector2(xcos - ysin + XY.X, xsin + ycos + XY.Y);
//    //    p.X = MathHelper.Clamp(p.X, XY.X - Origin.X, XY.X + Size.X - Origin.X);
//    //    p.Y = MathHelper.Clamp(p.Y, XY.Y - Origin.Y, XY.Y + Size.Y - Origin.Y);
//    //    cos = MathF.Cos(Rotation);
//    //    sin = MathF.Sin(Rotation);
//    //    x = p.X - XY.X;
//    //    y = p.Y - XY.Y;
//    //    xcos = x * cos;
//    //    ycos = y * cos;
//    //    xsin = x * sin;
//    //    ysin = y * sin;
//    //    return new Vector2(xcos - ysin + XY.X, xsin + ycos + XY.Y);
//    //}
//}