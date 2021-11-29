using Microsoft.Xna.Framework;

namespace Dcrew;

public struct RNG {
    public uint Seed { get; private set; }

    public RNG(uint seed) => Seed = seed;
    public RNG(int seed) : this((uint)seed) { }

    uint Lehmer32() {
        Seed += 0xe120fc15;
        ulong tmp = (ulong)Seed * 0x4a39b70d;
        uint m1 = (uint)((tmp >> 32) ^ tmp);
        tmp = (ulong)m1 * 0x12fad5c9;
        uint m2 = (uint)((tmp >> 32) ^ tmp);
        return m2;
    }

    /// <summary>Returns the next pseudo-random <see cref="uint"/> in the range of <see cref="uint.MinValue"/> (inclusive) to <see cref="uint.MaxValue"/> (exlusive)</summary>
    public uint RndUInt() => Lehmer32();
    /// <summary>Returns the next pseudo-random <see cref="uint"/> in the range of <paramref name="min"/> (inclusive) to <paramref name="max"/> (exclusive)</summary>
    public uint RndInt(uint min, uint max) => (Lehmer32() % (max - min)) + min;
    /// <summary>Returns the next pseudo-random <see cref="int"/> in the range of <see cref="int.MinValue"/> (inclusive) to <see cref="int.MaxValue"/> (exlusive)</summary>
    public int RndInt() => (int)Lehmer32();
    /// <summary>Returns the next pseudo-random <see cref="int"/> in the range of <paramref name="min"/> (inclusive) to <paramref name="max"/> (exclusive)</summary>
    public int RndInt(int min, int max) => (int)((Lehmer32() % (max - min)) + min);

    /// <summary>Returns the next pseudo-random <see cref="double"/> in the range of 0 (inclusive) to 1 (exclusive)</summary>
    public double RndDouble() => Lehmer32() / (double)uint.MaxValue;
    /// <summary>Returns the next pseudo-random <see cref="double"/> in the range of <paramref name="min"/> (inclusive) to <paramref name="max"/> (exclusive)</summary>
    public double RndDouble(double min, double max) => Lehmer32() / (double)uint.MaxValue * (max - min) + min;

    /// <summary>Returns the next pseudo-random <see cref="float"/> in the range of 0 (inclusive) to 1 (exclusive)</summary>
    public float RndFloat() => Lehmer32() / (float)uint.MaxValue;
    /// <summary>Returns the next pseudo-random <see cref="float"/> in the range of <paramref name="min"/> (inclusive) to <paramref name="max"/> (exclusive)</summary>
    public float RndFloat(float min, float max) => Lehmer32() / (float)uint.MaxValue * (max - min) + min;

    public Vector2 RndVec2(float x1, float x2, float y1, float y2) => new Vector2(RndFloat(x1, x2), RndFloat(y1, y2));

    public bool CoinToss(float chanceToSucceed) => RndFloat() < chanceToSucceed;
    public T RndPick<T>(params T[] values) => values[RndInt(0, values.Length)];
}