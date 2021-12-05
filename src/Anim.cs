namespace Dcrew;

public struct Anim {
    [Flags] enum Flags { IsPlaying = 1, IsLooped = 2 }
    float _secPerFrame, _startTime, _pauseTime;
    Flags _flags;
    readonly int _frames;
    public int CurrentFrame {
        get {
            if ((_flags & Flags.IsPlaying) == 0)
                return 0;
            if (_pauseTime != -1) {
                if ((_flags & Flags.IsLooped) != 0)
                    return (int)((_pauseTime - _startTime) / _secPerFrame) % _frames;
                return Math.Min((int)((_pauseTime - _startTime) / _secPerFrame), _frames - 1);
            }
            if ((_flags & Flags.IsLooped) != 0)
                return (int)((Time.Total - _startTime) / _secPerFrame) % _frames;
            return Math.Min((int)((Time.Total - _startTime) / _secPerFrame), _frames - 1);
        }
    }

    public float SecPerCycle {
        get => _secPerFrame * _frames;
        set => SecPerFrame = 1 / value;
    }
    public float SecPerFrame {
        get => _secPerFrame;
        set {
            var oldTime = _secPerFrame;
            _secPerFrame = value;
            var delta = Time.Total - _startTime;
            _startTime = Time.Total - (delta * (_secPerFrame / oldTime));
        }
    }
    public bool IsPlaying {
        get => (_flags & Flags.IsPlaying) != 0 && _pauseTime == -1;
        set {
            if (value) {
                Play();
                return;
            }
            Pause();
        }
    }
    public bool IsPaused {
        get => (_flags & Flags.IsPlaying) != 0 && _pauseTime != -1;
        set {
            if (value) {
                Pause();
                return;
            }
            Play();
        }
    }
    public bool IsStopped {
        get => (_flags & Flags.IsPlaying) == 0;
        set {
            if (value) {
                Stop();
                return;
            }
            Play();
        }
    }
    public bool IsLooped {
        get => (_flags & Flags.IsLooped) != 0;
        set {
            if (value) {
                if ((_flags & Flags.IsLooped) == 0) {
                    var delta = Time.Total - _startTime;
                    if (delta > SecPerCycle)
                        _startTime = Time.Total - SecPerCycle;
                }
                _flags |= Flags.IsLooped;
                return;
            }
            if ((_flags & Flags.IsLooped) != 0) {
                var delta = Time.Total - _startTime;
                if (delta > SecPerCycle)
                    _startTime = Time.Total - ((int)(delta / SecPerCycle) * SecPerCycle);
            }
            _flags &= ~Flags.IsLooped;
        }
    }

    public Anim(int frames, float secPerCycle, bool loop, bool startPlaying = true) {
        _frames = frames;
        _secPerFrame = 1 / secPerCycle;
        _startTime = Time.Total;
        _pauseTime = -1;
        _flags = 0;
        if (loop)
            _flags |= Flags.IsLooped;
        if (startPlaying)
            _flags |= Flags.IsPlaying;
    }

    /// <summary>Play or resume (if paused)</summary>
    public void Play() {
        if (_pauseTime != -1)
            _startTime = Time.Total - (_pauseTime - _startTime);
        _flags |= Flags.IsPlaying;
        _pauseTime = -1;
    }
    /// <summary>Pause at current frame</summary>
    public void Pause() {
        if ((_flags & Flags.IsPlaying) == 0)
            return;
        _pauseTime = Time.Total;
    }
    /// <summary>Stop playing</summary>
    public void Stop() {
        _flags &= ~Flags.IsPlaying;
        _pauseTime = -1;
    }
    /// <summary>Reset frame to 0 and start playing</summary>
    public void Restart() {
        _pauseTime = -1;
        _startTime = Time.Total;
        _flags |= Flags.IsPlaying;
    }
    /// <summary>Reset frame to 0 and stop playing</summary>
    public void Reset() {
        _pauseTime = -1;
        _startTime = Time.Total;
        _flags &= ~Flags.IsPlaying;
    }
}