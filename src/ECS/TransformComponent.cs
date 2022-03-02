using Microsoft.Xna.Framework;

namespace Dcrew.ECS;

public struct TransformComponent : IComponent {
    public Vector2 Position;
    public float Rotation;
    public Vector2 Scale;
}