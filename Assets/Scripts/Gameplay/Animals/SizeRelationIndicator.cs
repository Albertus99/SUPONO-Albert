using Supono.Core;
using UnityEngine;
using VContainer;

namespace Supono.Animals
{
    /// <summary>
    /// Colored ring under an NPC showing how it relates to the player:
    /// green = you can eat it, red = it can eat you, yellow = neither.
    /// </summary>
    [RequireComponent(typeof(Animal))]
    public sealed class SizeRelationIndicator : MonoBehaviour
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] Renderer ring;
        [SerializeField, Tooltip("Ring diameter as a multiple of body diameter.")] float ringScale = 1.4f;

        Animal self;
        WorldRegistry registry;
        GameSettings settings;
        MaterialPropertyBlock block;
        Color current;

        [Inject]
        public void Construct(WorldRegistry registry, GameSettings settings)
        {
            this.registry = registry;
            this.settings = settings;
        }

        void Awake()
        {
            self = GetComponent<Animal>();
            block = new MaterialPropertyBlock();
        }

        void LateUpdate()
        {
            if (registry == null || ring == null) return;
            if (!self.IsAlive)
            {
                ring.enabled = false;
                return;
            }

            float diameter = self.BodyRadius * 2f * ringScale;
            Transform ringTransform = ring.transform;
            ringTransform.localScale = new Vector3(diameter, ringTransform.localScale.y, diameter);

            Color color = Evaluate();
            if (color == current) return;
            current = color;
            block.SetColor(BaseColorId, color);
            ring.SetPropertyBlock(block);
        }

        Color Evaluate()
        {
            Animal player = registry.Player;
            if (player == null || !player.IsAlive) return settings.neutralColor;
            if (player.CanEat(self)) return settings.edibleColor;
            if (self.CanEat(player)) return settings.dangerColor;
            return settings.neutralColor;
        }
    }
}
