namespace Supono.Core
{
    /// <summary>
    /// Source of movement decisions for an animal body.
    /// Player input and AI are interchangeable implementations.
    /// </summary>
    public interface IAnimalBrain
    {
        MoveCommand Think(float deltaTime);
    }
}
