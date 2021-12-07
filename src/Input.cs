using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Dcrew;

public enum GamePadButton {
    A,
    B,
    Back,
    X,
    Y,
    Start,
    LeftShoulder,
    LeftStick,
    RightShoulder,
    RightStick,
    BigButton,
    Down,
    Left,
    Right,
    Up
}
public enum GamePadSensor {
    LeftTrigger,
    RightTrigger,
    LeftStick,
    RightStick
}

public static class Input {
    static long _lastMousePoll = -1, _lastKeyboardPoll = -1, _lastGamePadPoll = -1;
    static MouseState _newMouse, _oldMouse;
    static KeyboardState _newKeyboard, _oldKeyboard;
    static readonly Dictionary<GamePadButton, Func<GamePadState, ButtonState>> _gamePadButtons = new Dictionary<GamePadButton, Func<GamePadState, ButtonState>> { { GamePadButton.A, (s) => s.Buttons.A },
        { GamePadButton.B, (s) => s.Buttons.B },
        { GamePadButton.Back, (s) => s.Buttons.Back },
        { GamePadButton.X, (s) => s.Buttons.X },
        { GamePadButton.Y, (s) => s.Buttons.Y },
        { GamePadButton.Start, (s) => s.Buttons.Start },
        { GamePadButton.LeftShoulder, (s) => s.Buttons.LeftShoulder },
        { GamePadButton.LeftStick, (s) => s.Buttons.LeftStick },
        { GamePadButton.RightShoulder, (s) => s.Buttons.RightShoulder },
        { GamePadButton.RightStick, (s) => s.Buttons.RightStick },
        { GamePadButton.BigButton, (s) => s.Buttons.BigButton },
        { GamePadButton.Down, (s) => s.DPad.Down },
        { GamePadButton.Left, (s) => s.DPad.Left },
        { GamePadButton.Right, (s) => s.DPad.Right },
        { GamePadButton.Up, (s) => s.DPad.Up },
    };
    static readonly GamePadState[] _newGamePad, _oldGamePad;
    public static KeyboardState OldKeyboard {
        get {
            if (_lastKeyboardPoll != Time.Ticks) {
                _oldKeyboard = _newKeyboard;
                _newKeyboard = Keyboard.GetState();
                _lastKeyboardPoll = Time.Ticks;
            }
            return _oldKeyboard;
        }
    }
    public static KeyboardState NewKeyboard {
        get {
            if (_lastKeyboardPoll != Time.Ticks) {
                _oldKeyboard = _newKeyboard;
                _newKeyboard = Keyboard.GetState();
                _lastKeyboardPoll = Time.Ticks;
            }
            return _newKeyboard;
        }
    }
    public static MouseState OldMouse {
        get {
            if (_lastMousePoll != Time.Ticks) {
                _oldMouse = _newMouse;
                _newMouse = Mouse.GetState();
                _lastMousePoll = Time.Ticks;
            }
            return _oldMouse;
        }
    }
    public static MouseState NewMouse {
        get {
            if (_lastMousePoll != Time.Ticks) {
                _oldMouse = _newMouse;
                _newMouse = Mouse.GetState();
                _lastMousePoll = Time.Ticks;
            }
            return _newMouse;
        }
    }
    public static ReadOnlySpan<GamePadState> OldGamePad {
        get {
            var span = new Span<GamePadState>(_oldGamePad);
            span[0] = GetOldGamePad(0);
            span[1] = GetOldGamePad(1);
            span[2] = GetOldGamePad(2);
            span[3] = GetOldGamePad(3);
            return span;
        }
    }
    public static ReadOnlySpan<GamePadState> NewGamePad {
        get {
            var span = new Span<GamePadState>(_newGamePad);
            span[0] = GetNewGamePad(0);
            span[1] = GetNewGamePad(1);
            span[2] = GetNewGamePad(2);
            span[3] = GetNewGamePad(3);
            return span;
        }
    }

    static Input() {
        _newGamePad = new GamePadState[4];
        _oldGamePad = new GamePadState[4];
    }

    public static int MouseX => NewMouse.X;
    public static int MouseY => NewMouse.Y;

    public static bool LMBHeld => NewMouse.LeftButton == ButtonState.Pressed;
    public static bool LMBPressed => NewMouse.LeftButton == ButtonState.Pressed && OldMouse.LeftButton != ButtonState.Pressed;
    public static bool LMBReleased => NewMouse.LeftButton != ButtonState.Pressed && OldMouse.LeftButton == ButtonState.Pressed;
    public static bool MMBHeld => NewMouse.MiddleButton == ButtonState.Pressed;
    public static bool MMBPressed => NewMouse.MiddleButton == ButtonState.Pressed && OldMouse.MiddleButton != ButtonState.Pressed;
    public static bool MMBReleased => NewMouse.MiddleButton != ButtonState.Pressed && OldMouse.MiddleButton == ButtonState.Pressed;
    public static bool RMBHeld => NewMouse.RightButton == ButtonState.Pressed;
    public static bool RMBPressed => NewMouse.RightButton == ButtonState.Pressed && OldMouse.RightButton != ButtonState.Pressed;
    public static bool RMBReleased => NewMouse.RightButton != ButtonState.Pressed && OldMouse.RightButton == ButtonState.Pressed;
    public static bool MMBScrolledUp => NewMouse.ScrollWheelValue > OldMouse.ScrollWheelValue;
    public static bool MMBScrolledDown => NewMouse.ScrollWheelValue < OldMouse.ScrollWheelValue;

    public static bool KeyHeld(Keys key) => NewKeyboard.IsKeyDown(key);
    public static bool KeyPressed(Keys key) => NewKeyboard.IsKeyDown(key) && !OldKeyboard.IsKeyDown(key);
    public static bool KeyReleased(Keys key) => !NewKeyboard.IsKeyDown(key) && OldKeyboard.IsKeyDown(key);
    public static bool AnyKeyHeld(params Keys[] keys) {
        foreach (var key in keys)
            if (NewKeyboard.IsKeyDown(key))
                return true;
        return false;
    }
    public static bool AnyKeyPressed(params Keys[] keys) {
        foreach (var key in keys)
            if (NewKeyboard.IsKeyDown(key) && !OldKeyboard.IsKeyDown(key))
                return true;
        return false;
    }
    public static bool AnyKeyReleased(params Keys[] keys) {
        foreach (var key in keys)
            if (!NewKeyboard.IsKeyDown(key) && OldKeyboard.IsKeyDown(key))
                return true;
        return false;
    }

    static GamePadState GetOldGamePad(int i) {
        if (_lastGamePadPoll != Time.Ticks) {
            _newGamePad.CopyTo(_oldGamePad, 0);
            for (int j = 0; j < 4; j++)
                _newGamePad[j] = GamePad.GetState((PlayerIndex)j, GamePadDeadZone.Circular);
            _lastGamePadPoll = Time.Ticks;
        }
        return _oldGamePad[i];
    }
    static GamePadState GetNewGamePad(int i) {
        if (_lastGamePadPoll != Time.Ticks) {
            _newGamePad.CopyTo(_oldGamePad, 0);
            for (int j = 0; j < 4; j++)
                _newGamePad[j] = GamePad.GetState((PlayerIndex)j, GamePadDeadZone.Circular);
            _lastGamePadPoll = Time.Ticks;
        }
        return _newGamePad[i];
    }
    public static bool Pressed(GamePadButton button, int gamePadIndex) {
        return _gamePadButtons[button](GetNewGamePad(gamePadIndex)) == ButtonState.Pressed &&
            _gamePadButtons[button](GetOldGamePad(gamePadIndex)) == ButtonState.Released;
    }
    public static bool Held(GamePadButton button, int gamePadIndex) {
        return _gamePadButtons[button](GetNewGamePad(gamePadIndex)) == ButtonState.Pressed;
    }
    public static bool HeldOnly(GamePadButton button, int gamePadIndex) {
        return _gamePadButtons[button](GetNewGamePad(gamePadIndex)) == ButtonState.Pressed &&
            _gamePadButtons[button](GetOldGamePad(gamePadIndex)) == ButtonState.Pressed;
    }
    public static bool Released(GamePadButton button, int gamePadIndex) {
        return _gamePadButtons[button](GetNewGamePad(gamePadIndex)) == ButtonState.Released &&
            _gamePadButtons[button](GetOldGamePad(gamePadIndex)) == ButtonState.Pressed;
    }
}