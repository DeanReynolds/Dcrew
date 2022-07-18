﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Dcrew.Camera;

/// <summary>An efficient 2D Camera</summary>
public class Camera2D {
    float _x, _y, _originX, _originY, _z, _rot, _scaleX, _scaleY, _rotCos, _rotSin, _targetScale, _mouseX, _mouseY;
    int _virtualWidth, _virtualHeight, _gameWidth, _gameHeight;
    Flags _flags;
    Matrix _view, _viewInvert, _proj;
    long _lastFrameUpdate;
    /// <summary>X position</summary>
    public float X {
        get => _x;
        set {
            _x = value;
            _flags |= Flags.IsDirty;
        }
    }
    /// <summary>Y position</summary>
    public float Y {
        get => _y;
        set {
            _y = value;
            _flags |= Flags.IsDirty;
        }
    }
    public float Z {
        get => _z;
        set {
            _z = value;
            _flags |= Flags.IsDirty;
        }
    }
    public Vector2 XY {
        get => new(_x, _y);
        set {
            _x = value.X;
            _y = value.Y;
            _flags |= Flags.IsDirty;
        }
    }
    public Vector3 XYZ {
        get => new(_x, _y, _z);
        set {
            _x = value.X;
            _y = value.Y;
            _z = value.Z;
            _flags |= Flags.IsDirty;
        }
    }
    public float Scale {
        get => _scaleX;
        set {
            _scaleX = _scaleY = value;
            _flags |= Flags.IsDirty;
        }
    }
    public float ScaleX {
        get => _scaleX;
        set {
            _scaleX = value;
            _flags |= Flags.IsDirty;
        }
    }
    public float ScaleY {
        get => _scaleY;
        set {
            _scaleY = value;
            _flags |= Flags.IsDirty;
        }
    }
    public Vector2 ScaleXY {
        get => new(_scaleX, _scaleY);
        set {
            _scaleX = value.X;
            _scaleY = value.Y;
            _flags |= Flags.IsDirty;
        }
    }
    public float Zoom {
        get => 1 / _scaleX;
        set {
            _scaleX = _scaleY = 1 / value;
            _flags |= Flags.IsDirty;
        }
    }
    public float ZoomX {
        get => 1 / _scaleX;
        set {
            _scaleX = 1 / value;
            _flags |= Flags.IsDirty;
        }
    }
    public float ZoomY {
        get => 1 / _scaleY;
        set {
            _scaleY = 1 / value;
            _flags |= Flags.IsDirty;
        }
    }
    public Vector2 ZoomXY {
        get => new(1 / _scaleX, 1 / _scaleY);
        set {
            _scaleX = 1 / value.X;
            _scaleY = 1 / value.Y;
            _flags |= Flags.IsDirty;
        }
    }
    public float WorldToScreenScale => WorldToScreenScaleAt(0);
    public float ScreenToWorldScale => ScreenToWorldScaleAt(0);
    /// <summary>Angle (in radians)</summary>
    public float Rotation {
        get => _rot;
        set {
            _rot = value;
            _rotCos = MathF.Cos(-_rot);
            _rotSin = MathF.Sin(-_rot);
            _flags |= Flags.IsDirty;
        }
    }
    /// <summary>Position offset (unaffected by angle and zoom)</summary>
    public Vector2 Origin {
        get => new(_originX, _originY);
        set {
            _originX = value.X;
            _originY = value.Y;
            _flags |= Flags.IsDirty;
        }
    }
    /// <summary>Virtual resolution</summary>
    public (int Width, int Height) TargetRes {
        get => (_virtualWidth, _virtualHeight);
        set {
            if (value.Width > 0 && value.Height > 0) {
                _virtualWidth = value.Width;
                _virtualHeight = value.Height;
                _flags |= Flags.HasVirtualRes;
                _flags |= Flags.IsDirty;
                return;
            }
            _virtualWidth = _virtualHeight = 0;
            _flags &= ~Flags.HasVirtualRes;
            _flags |= Flags.IsDirty;
        }
    }

    [Flags] enum Flags { IsDirty = 1, HasVirtualRes = 2, IsMainCamera = 4 }
    /// <summary>Virtual resolution scale/zoom</summary>
    public float TargetScale {
        get {
            UpdateDirty();
            return _targetScale;
        }
    }
    public Rectangle ViewRect => ViewRectAt(0);
    /// <summary>View transformation matrix</summary>
    public Matrix View {
        get {
            UpdateDirty();
            return _view;
        }
    }
    /// <summary>Inverted view transformation matrix</summary>
    public Matrix ViewInvert {
        get {
            UpdateDirty();
            return _viewInvert;
        }
    }
    /// <summary>Projection matrix</summary>
    public Matrix Projection {
        get {
            UpdateDirty();
            return _proj;
        }
    }
    public float MouseX {
        get {
            UpdateDirty();
            return _mouseX;
        }
    }
    public float MouseY {
        get {
            UpdateDirty();
            return _mouseY;
        }
    }
    public Vector2 MouseXY {
        get {
            UpdateDirty();
            return new(_mouseX, _mouseY);
        }
    }

    public Camera2D(bool isMainCamera, Vector2 xy = new()) {
        _proj = new Matrix {
            M33 = -1,
            M41 = -1,
            M42 = 1,
            M44 = 1
        };
        _x = xy.X;
        _y = xy.Y;
        _rot = _rotSin = 0;
        _z = _rotCos = _scaleX = _scaleY = 1;
        _virtualWidth = _virtualHeight = _gameWidth = _gameHeight = 0;
        _flags = Flags.IsDirty;
        Origin = Vector2.Zero;
        _targetScale = 1;
        if (isMainCamera)
            _flags |= Flags.IsMainCamera;
    }
    /// <summary></summary>
    /// <param name="targetRes">Virtual resolution to maintain</param>
    public Camera2D(bool isMainCamera, (int Width, int Height) targetRes, Vector2 xy = new()) : this(isMainCamera, xy) {
        TargetRes = targetRes;
        Origin = new(targetRes.Width * .5f, targetRes.Height * .5f);
    }

    /// <summary>Convert the given position from screen to world</summary>
    public Vector2 ScreenToWorld(float x, float y) {
        var invert = ViewInvert;
        x -= Global.Game.GraphicsDevice.Viewport.X;
        y -= Global.Game.GraphicsDevice.Viewport.Y;
        return new Vector2(x * invert.M11 + (y * invert.M21) + invert.M41, x * invert.M12 + (y * invert.M22) + invert.M42);
    }
    /// <summary>Convert the given position from screen to world at <paramref name="z"/></summary>
    public Vector2 ScreenToWorld(float x, float y, float z) {
        var invert = ViewInvertAt(z);
        x -= Global.Game.GraphicsDevice.Viewport.X;
        y -= Global.Game.GraphicsDevice.Viewport.Y;
        return new Vector2(x * invert.M11 + (y * invert.M21) + invert.M41, x * invert.M12 + (y * invert.M22) + invert.M42);
    }
    /// <summary>Convert the given position from screen to world</summary>
    public Vector2 ScreenToWorld(Vector2 xy) => ScreenToWorld(xy.X, xy.Y);
    /// <summary>Convert the given position from screen to world at <paramref name="z"/></summary>
    public Vector2 ScreenToWorld(Vector2 xy, float z) => ScreenToWorld(xy.X, xy.Y, z);

    /// <summary>Convert the given position from world to screen</summary>
    public Vector2 WorldToScreen(float x, float y) {
        var view = View;
        var r = new Vector2(x * view.M11 + (y * view.M21) + view.M41 + x, x * view.M12 + (y * view.M22) + view.M42 + y);
        r.X += Global.Game.GraphicsDevice.Viewport.X;
        r.Y += Global.Game.GraphicsDevice.Viewport.Y;
        return r;
    }
    /// <summary>Convert the given position from world to screen at <paramref name="z"/></summary>
    public Vector2 WorldToScreen(float x, float y, float z) {
        var view = ViewAt(z);
        var r = new Vector2(x * view.M11 + (y * view.M21) + view.M41 + x, x * view.M12 + (y * view.M22) + view.M42 + y);
        r.X += Global.Game.GraphicsDevice.Viewport.X;
        r.Y += Global.Game.GraphicsDevice.Viewport.Y;
        return r;
    }
    /// <summary>Convert the given position from world to screen</summary>
    public Vector2 WorldToScreen(Vector2 xy) => WorldToScreen(xy.X, xy.Y);
    /// <summary>Convert the given position from world to screen at <paramref name="z"/></summary>
    public Vector2 WorldToScreen(Vector2 xy, float z) => WorldToScreen(xy.X, xy.Y, z);

    /// <summary>View transformation matrix at <paramref name="z"/></summary>
    public Matrix ViewAt(float z) {
        var view = new Matrix { M33 = 1, M44 = 1 };
        float zoomFromZ = ZToScale(_z, z),
            scaleM11 = _scaleX * _targetScale * zoomFromZ,
            scaleM22 = _scaleY * _targetScale * zoomFromZ,
            m41 = -_x * scaleM11,
            m42 = -_y * scaleM22;
        view.M41 = (m41 * _rotCos) + (m42 * -_rotSin) + (_originX * _targetScale);
        view.M42 = (m41 * _rotSin) + (m42 * _rotCos) + (_originY * _targetScale);
        view.M11 = scaleM11 * _rotCos;
        view.M12 = scaleM22 * _rotSin;
        view.M21 = scaleM11 * -_rotSin;
        view.M22 = scaleM22 * _rotCos;
        return view;
    }

    /// <summary>View transformation matrix at <paramref name="z"/></summary>
    public Matrix ViewAt(out Matrix invert, float z) {
        var view = ViewAt(z);
        invert = new Matrix { M33 = 1, M44 = 1 };
        float n24 = -view.M21, n27 = (float)(1 / (view.M11 * (double)view.M22 + view.M12 * (double)n24));
        invert.M41 = (float)-(view.M21 * (double)-view.M42 - view.M22 * (double)-view.M41) * n27;
        invert.M42 = (float)(view.M11 * (double)-view.M42 - view.M12 * (double)-view.M41) * n27;
        invert.M11 = view.M22 * n27;
        invert.M12 = -view.M12 * n27;
        invert.M21 = n24 * n27;
        invert.M22 = view.M11 * n27;
        return view;
    }
    /// <summary>Inverted view transformation matrix at <paramref name="z"/></summary>
    public Matrix ViewInvertAt(float z = 0) {
        ViewAt(out var invert, z);
        return invert;
    }

    public float WorldToScreenScaleAt(float z) => Vector2.Distance(WorldToScreen(0, 0, z), WorldToScreen(1, 0, z));
    public float ScreenToWorldScaleAt(float z) => Vector2.Distance(ScreenToWorld(0, 0, z), ScreenToWorld(1, 0, z));

    /// <summary>A rectangle covering the view given <paramref name="z"/> (in world coords).</summary>
    public Rectangle ViewRectAt(float z) {
        var frustum = BoundingFrustum(z);
        var corners = frustum.GetCorners();
        var a = corners[0];
        var b = corners[1];
        var c = corners[2];
        var d = corners[3];

        int left = (int)MathF.Min(MathF.Min(a.X, b.X), MathF.Min(c.X, d.X)),
            right = (int)MathF.Ceiling(MathF.Max(MathF.Max(a.X, b.X), MathF.Max(c.X, d.X))),
            top = (int)MathF.Min(MathF.Min(a.Y, b.Y), MathF.Min(c.Y, d.Y)),
            bottom = (int)MathF.Ceiling(MathF.Max(MathF.Max(a.Y, b.Y), MathF.Max(c.Y, d.Y)));

        var width = right - left;
        var height = bottom - top;

        return new Rectangle(left, top, width, height);
    }

    public bool IsZVisible(float z, float minDistance = 0.1f) {
        float scaleZ = ZToScale(Z, z);
        float maxScale = ZToScale(minDistance, 0f);

        return scaleZ > 0 && scaleZ < maxScale;
    }

    public BoundingFrustum BoundingFrustum(float z = 0) => new(ViewAt(z) * _proj);

    void UpdateDirty() {
        if (_lastFrameUpdate != Time.Ticks) {
            if ((_flags & Flags.IsMainCamera) != 0 && (_flags & Flags.HasVirtualRes) != 0) {
                _targetScale = MathF.Min(Global.Game.GraphicsDevice.PresentationParameters.BackBufferWidth / (float)TargetRes.Width,
                    Global.Game.GraphicsDevice.PresentationParameters.BackBufferHeight / (float)TargetRes.Height);

                if (_gameWidth != Global.Game.GraphicsDevice.PresentationParameters.BackBufferWidth ||
                    _gameHeight != Global.Game.GraphicsDevice.PresentationParameters.BackBufferHeight) {
                    var targetAspectRatio = _virtualWidth / (float)_virtualHeight;
                    var width2 = Global.Game.GraphicsDevice.PresentationParameters.BackBufferWidth;
                    var height2 = (int)(width2 / targetAspectRatio + .5f);
                    if (height2 > Global.Game.GraphicsDevice.PresentationParameters.BackBufferHeight) {
                        height2 = Global.Game.GraphicsDevice.PresentationParameters.BackBufferHeight;
                        width2 = (int)(height2 * targetAspectRatio + .5f);
                    }
                    Global.Game.GraphicsDevice.Viewport = new Viewport((Global.Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 2) - (width2 / 2),
                        (Global.Game.GraphicsDevice.PresentationParameters.BackBufferHeight / 2) - (height2 / 2), width2, height2);

                    _gameWidth = Global.Game.GraphicsDevice.PresentationParameters.BackBufferWidth;
                    _gameHeight = Global.Game.GraphicsDevice.PresentationParameters.BackBufferHeight;
                }

                _flags |= Flags.IsDirty;
            }

            _proj.M11 = (float)(2d / Global.Game.GraphicsDevice.Viewport.Width);
            _proj.M22 = (float)(2d / -Global.Game.GraphicsDevice.Viewport.Height);

            _lastFrameUpdate = Time.Ticks;

            if ((_flags & Flags.IsDirty) == 0) {
                UpdateMouse();
                return;
            } else
                goto skipDirtyCheck;
        }

        if ((_flags & Flags.IsDirty) == 0)
            return;
        skipDirtyCheck:;
        _flags &= ~Flags.IsDirty;

        _view = ViewAt(out _viewInvert, 0);
        UpdateMouse();
    }
    void UpdateMouse() {
        var mouse = ScreenToWorld(Input.MouseX, Input.MouseY, 0);
        _mouseX = mouse.X;
        _mouseY = mouse.Y;
    }

    static float ZToScale(float z, float targetZ) => z - targetZ == 0 ? 0 : 1 / (z - targetZ);
    static float ScaleToZ(float zoom, float targetZ) => 1 / zoom + targetZ;
}