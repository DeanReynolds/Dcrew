namespace Dcrew;

public struct Anim {
    float _frame;
    readonly ushort _frames;
    public float SecPerFrame;

    public int CurrentFrame => (int)MathF.Min(MathF.Abs(_frame), _frames - 1);
    public float SecPerCycle {
        get => 1 / SecPerFrame;
        set => SecPerFrame = _frames / value;
    }
    public bool Loop {
        get => MathF.Sign(_frame) >= 0;
        set {
            if (value)
                _frame = MathF.Abs(_frame);
            else
                _frame = -MathF.Abs(_frame);
        }
    }
    public bool Finished => !Loop && MathF.Abs(_frame) == _frames;

    public Anim(ushort frames, float secPerCycle, bool loop) {
        _frames = frames;
        SecPerFrame = frames / secPerCycle;
        if (loop)
            _frame = 0;
        else
            _frame = -.001f; ;
    }

    public void Update() {
        if (Loop)
            _frame = (MathF.Abs(_frame) + (SecPerFrame * Time.Delta)) % _frames;
        else
            _frame = -MathF.Min(MathF.Abs(_frame) + (SecPerFrame * Time.Delta), _frames);
    }

    /// <summary>Reset frame to 0</summary>
    public void Reset() {
        _frame = 0;
    }
}