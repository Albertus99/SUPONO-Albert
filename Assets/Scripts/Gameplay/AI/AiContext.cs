using Supono.Animals;
using Supono.Core;
using Supono.Food;
using UnityEngine;

namespace Supono.AI
{
    /// <summary>What an AI behaviour can perceive about itself and the world.</summary>
    public sealed class AiContext
    {
        public readonly Animal Self;
        public readonly Stamina Stamina;
        public readonly WorldRegistry Registry;
        public readonly LevelBounds Bounds;
        /// <summary>Distance from walls at which fleeing starts steering back inside.</summary>
        public readonly float WallMargin;

        public AiContext(Animal self, WorldRegistry registry, LevelBounds bounds, float wallMargin)
        {
            Self = self;
            Stamina = self.GetComponent<Stamina>();
            Registry = registry;
            Bounds = bounds;
            WallMargin = wallMargin;
        }

        public bool IsExhausted => Stamina != null && Stamina.IsExhausted;

        public float Time => UnityEngine.Time.time;

        public float DistanceTo(Vector3 point) => Vector3.Distance(Flat(Self.Position), Flat(point));

        /// <summary>Nearest animal in sense range that is able to eat us.</summary>
        public Animal FindNearestThreat()
        {
            Animal best = null;
            float bestSqr = Sqr(Self.SenseRadius);
            foreach (Animal other in Registry.Animals)
            {
                if (other == Self || !other.IsAlive || !other.CanEat(Self)) continue;
                float sqr = SqrDistance(other.Position);
                if (sqr >= bestSqr) continue;
                bestSqr = sqr;
                best = other;
            }
            return best;
        }

        /// <summary>
        /// Nearest animal in sense range that we can eat.
        /// <paramref name="playerBias"/> &lt; 1 makes the player look closer (more attractive).
        /// </summary>
        public Animal FindNearestPrey(float playerBias = 1f)
        {
            Animal best = null;
            float range = Self.SenseRadius;
            float bestScore = float.MaxValue;
            foreach (Animal other in Registry.Animals)
            {
                if (other == Self || !Self.CanEat(other)) continue;
                float distance = DistanceTo(other.Position);
                if (distance > range) continue;
                float score = other == Registry.Player ? distance * playerBias : distance;
                if (score >= bestScore) continue;
                bestScore = score;
                best = other;
            }
            return best;
        }

        public FoodPickup FindNearestFood()
        {
            FoodPickup best = null;
            float bestSqr = Sqr(Self.SenseRadius);
            foreach (FoodPickup food in Registry.Food)
            {
                if (!Self.CanEat(food)) continue;
                float sqr = SqrDistance(food.Position);
                if (sqr >= bestSqr) continue;
                bestSqr = sqr;
                best = food;
            }
            return best;
        }

        float SqrDistance(Vector3 point) => (Flat(point) - Flat(Self.Position)).sqrMagnitude;
        static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
        static float Sqr(float v) => v * v;
    }
}
