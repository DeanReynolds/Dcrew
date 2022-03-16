using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Dcrew.Camera;

/// <summary>An efficient 2D Camera</summary>
public class Camera2D {
    float _x, _y, _rot, _zoom, _rotCos, _rotSin, _targetScale, _mouseX, _mouseY;
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
    public Vector2 Pos {
        get => new(_x, _y);
        set {
            _x = value.X;
            _y = value.Y;
            _flags |= Flags.IsDirty;
        }
    }
    /// <summary>Camera zoom/object scale</summary>
    public float Zoom {
        get => _zoom;
        set {
            _zoom = value;
            _flags |= Flags.IsDirty;
        }
    }
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
    public Vector2 Origin;
    /// <summary>Virtual resolution to maintain. See <see cref="FixBlackBars()"/></summary>
    public (int Width, int Height) TargetRes {
        get => (_virtualWidth, _virtualHeight);
        set {
            if (value.Width > 0 && value.Height > 0) {
                _flags |= Flags.HasVirtualRes;
                _virtualWidth = value.Width;
                _virtualHeight = value.Height;
                return;
            }
            _flags &= ~Flags.HasVirtualRes;
        }
    }

    [Flags] enum Flags { IsDirty = 1, HasVirtualRes = 2 }
    /// <summary>Virtual resolution scale/zoom</summary>
    public float TargetScale => _targetScale;

    public Camera2D() {
        _view = new Matrix { M33 = 1, M44 = 1 };
        _viewInvert = new Matrix { M33 = 1, M44 = 1 };
        _proj = new Matrix {
            M33 = -1,
            M41 = -1,
            M42 = 1,
            M44 = 1
        };
        _x = _y = _rot = _rotSin = 0;
        _rotCos = _zoom = 1;
        _virtualWidth = _virtualHeight = _gameWidth = _gameHeight = 0;
        _flags = Flags.IsDirty;
        Origin = Vector2.Zero;
        _targetScale = 1;
    }
    /// <summary></summary>
    /// <param name="targetRes">Virtual resolution to maintain. See <see cref="FixBlackBars()"/></param>
    public Camera2D((int Width, int Height) targetRes) : this() {
        TargetRes = targetRes;
        Origin = new(targetRes.Width * .5f, targetRes.Height * .5f);
    }

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
    public Vector2 MousePos {
        get {
            UpdateDirty();
            return new(_mouseX, _mouseY);
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

    /// <summary>Convert the given position from screen to world</summary>
    public Vector2 ScreenToWorld(float x, float y) {
        var invert = ViewInvert;
        x -= Global.Game.GraphicsDevice.Viewport.X;
        y -= Global.Game.GraphicsDevice.Viewport.Y;
        return new Vector2(x * invert.M11 + (y * invert.M21) + invert.M41, x * invert.M12 + (y * invert.M22) + invert.M42);
    }
    /// <summary>Convert the given position from screen to world</summary>
    public Vector2 ScreenToWorld(Vector2 xy) => ScreenToWorld(xy.X, xy.Y);
    /// <summary>Convert the given position from world to screen</summary>
    public Vector2 WorldToScreen(float x, float y) {
        var view = View;
        var r = new Vector2(x * view.M11 + (y * view.M21) + view.M41 + x, x * view.M12 + (y * view.M22) + view.M42 + y);
        r.X += Global.Game.GraphicsDevice.Viewport.X;
        r.Y += Global.Game.GraphicsDevice.Viewport.Y;
        return r;
    }
    /// <summary>Convert the given position from world to screen</summary>
    public Vector2 WorldToScreen(Vector2 xy) => WorldToScreen(xy.X, xy.Y);

    void UpdateDirty() {
        if (_lastFrameUpdate != Time.Ticks) {
            if ((_flags & Flags.HasVirtualRes) != 0 &&
                (_gameWidth != Global.Game.GraphicsDevice.PresentationParameters.BackBufferWidth ||
                _gameHeight != Global.Game.GraphicsDevice.PresentationParameters.BackBufferHeight)) {

                _targetScale = MathF.Min(Global.Game.GraphicsDevice.PresentationParameters.BackBufferWidth / (float)TargetRes.Width,
                    Global.Game.GraphicsDevice.PresentationParameters.BackBufferHeight / (float)TargetRes.Height);

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

            _proj.M11 = (float)(2d / Global.Game.GraphicsDevice.Viewport.Width);
            _proj.M22 = (float)(2d / -Global.Game.GraphicsDevice.Viewport.Height);

            _lastFrameUpdate = Time.Ticks;

            var mouse = ScreenToWorld(Input.MouseX, Input.MouseY);
            _mouseX = mouse.X;
            _mouseY = mouse.Y;
        }

        if ((_flags & Flags.IsDirty) == 0)
            return;
        _flags &= ~Flags.IsDirty;

        float scaleM11 = 1 * _zoom * _targetScale,
            scaleM22 = 1 * _zoom * _targetScale,
            m41 = -X * scaleM11,
            m42 = -Y * scaleM22;
        _view.M41 = (m41 * _rotCos) + (m42 * -_rotSin) + (Origin.X * _targetScale);
        _view.M42 = (m41 * _rotSin) + (m42 * _rotCos) + (Origin.Y * _targetScale);
        _view.M11 = scaleM11 * _rotCos;
        _view.M12 = scaleM22 * _rotSin;
        _view.M21 = scaleM11 * -_rotSin;
        _view.M22 = scaleM22 * _rotCos;
        float n24 = -_view.M21, n27 = (float)(1 / (_view.M11 * (double)_view.M22 + _view.M12 * (double)n24));
        _viewInvert.M41 = (float)-(_view.M21 * (double)-_view.M42 - _view.M22 * (double)-_view.M41) * n27;
        _viewInvert.M42 = (float)(_view.M11 * (double)-_view.M42 - _view.M12 * (double)-_view.M41) * n27;
        _viewInvert.M11 = _view.M22 * n27;
        _viewInvert.M12 = -_view.M12 * n27;
        _viewInvert.M21 = n24 * n27;
        _viewInvert.M22 = _view.M11 * n27;
    }
}