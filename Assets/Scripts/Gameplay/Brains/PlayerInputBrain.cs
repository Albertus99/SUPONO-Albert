using Supono.Animals;
using Supono.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Supono.Brains
{
    /// <summary>Drives an animal from the Input System, relative to the camera view.</summary>
    [RequireComponent(typeof(Animal))]
    public sealed class PlayerInputBrain : MonoBehaviour, IAnimalBrain
    {
        [SerializeField] string actionMap = "Player";
        [SerializeField] string moveAction = "Move";
        [SerializeField] string sprintAction = "Sprint";

        InputAction move;
        InputAction sprint;
        Transform view;

        [Inject]
        public void Construct(InputActionAsset actions, Camera viewCamera)
        {
            InputActionMap map = actions.FindActionMap(actionMap, throwIfNotFound: true);
            move = map.FindAction(moveAction, throwIfNotFound: true);
            sprint = map.FindAction(sprintAction, throwIfNotFound: true);
            view = viewCamera.transform;
            map.Enable();
        }

        public MoveCommand Think(float deltaTime)
        {
            if (move == null) return MoveCommand.Stop;

            Vector2 input = move.ReadValue<Vector2>();
            if (input.sqrMagnitude < 0.0001f) return MoveCommand.Stop;

            Vector3 forward = Flatten(view.forward);
            if (forward.sqrMagnitude < 0.01f) forward = Flatten(view.up);
            Vector3 right = Flatten(view.right);
            return new MoveCommand(forward * input.y + right * input.x, sprint.IsPressed());
        }

        static Vector3 Flatten(Vector3 v) => Vector3.ProjectOnPlane(v, Vector3.up).normalized;
    }
}
