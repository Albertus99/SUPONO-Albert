using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Supono.Animals
{
    /// <summary>
    /// Blends idle/walk/run clips by movement speed using a small PlayableGraph,
    /// so no AnimatorController asset is needed per animal.
    /// </summary>
    [RequireComponent(typeof(Animal), typeof(AnimalMotor))]
    public sealed class AnimalAnimator : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] AnimationClip idle;
        [SerializeField] AnimationClip walk;
        [SerializeField] AnimationClip run;
        [SerializeField] float blendSharpness = 10f;

        readonly AnimationClipPlayable[] slots = new AnimationClipPlayable[3];
        PlayableGraph graph;
        AnimationMixerPlayable mixer;
        Vector3 weights = new Vector3(1f, 0f, 0f);
        Animal animal;
        AnimalMotor motor;

        void Awake()
        {
            animal = GetComponent<Animal>();
            motor = GetComponent<AnimalMotor>();
        }

        void Start()
        {
            if (animator == null || idle == null || walk == null || run == null)
            {
                enabled = false;
                return;
            }

            animator.applyRootMotion = false;
            graph = PlayableGraph.Create($"{name}.Locomotion");
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            var output = AnimationPlayableOutput.Create(graph, "Locomotion", animator);
            mixer = AnimationMixerPlayable.Create(graph, slots.Length);

            AnimationClip[] clips = { idle, walk, run };
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = AnimationClipPlayable.Create(graph, clips[i]);
                graph.Connect(slots[i], 0, mixer, i);
            }

            output.SetSourcePlayable(mixer);
            ApplyWeights();
            graph.Play();
        }

        void Update()
        {
            if (!graph.IsValid()) return;

            float speed = animal.IsAlive ? motor.PlanarSpeed : 0f;
            float walkSpeed = Mathf.Max(0.01f, animal.WalkSpeed);
            float sprintSpeed = Mathf.Max(walkSpeed + 0.01f, animal.SprintSpeed);
            float moving = Mathf.Clamp01(speed / (walkSpeed * 0.6f));
            float running = Mathf.Clamp01((speed - walkSpeed) / (sprintSpeed - walkSpeed));
            var target = new Vector3(1f - moving, moving * (1f - running), moving * running);

            weights = Vector3.Lerp(weights, target, 1f - Mathf.Exp(-blendSharpness * Time.deltaTime));
            ApplyWeights();

            // Imported clips aren't flagged as looping, so wrap them manually.
            foreach (var slot in slots)
            {
                double length = slot.GetAnimationClip().length;
                double time = slot.GetTime();
                if (length > 0d && time >= length) slot.SetTime(time % length);
            }
        }

        void ApplyWeights()
        {
            mixer.SetInputWeight(0, weights.x);
            mixer.SetInputWeight(1, weights.y);
            mixer.SetInputWeight(2, weights.z);
        }

        void OnDestroy()
        {
            if (graph.IsValid()) graph.Destroy();
        }
    }
}
