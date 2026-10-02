using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Supono.App.Characters
{
    /// <summary>
    /// Live 3D previews for Character Selection (a prefab: camera + turntable template, set up in the editor).
    /// Every character stands on its own turntable in a hidden grid far below the world; the one camera renders
    /// the whole grid into a single texture and each card shows its tile of it (<see cref="TileRect"/>).
    /// One camera and one texture, however many characters.
    /// </summary>
    public sealed class CharacterPreviewStage : MonoBehaviour
    {
        static readonly int OutlineFocusId = Shader.PropertyToID("_ToonOutlineFocusDistance");

        [SerializeField] Camera previewCamera;
        [SerializeField] CharacterTurntable turntableTemplate;
        [SerializeField, Min(1)] int columns = 4;
        [SerializeField, Min(64)] int tilePixels = 384;
        [SerializeField, Tooltip("World units between turntables.")] float spacing = 2.4f;
        [SerializeField, Tooltip("Largest model dimension after normalizing.")] float modelFit = 1.75f;

        readonly List<CharacterTurntable> turntables = new();
        RenderTexture texture;
        int rows = 1;
        float cameraDistance;
        float outlineFocusOutside;

        public Texture Texture => texture;

        /// <summary>Places every character on a turntable and starts rendering them.</summary>
        public void Show(IReadOnlyList<PlayableCharacter> characters)
        {
            rows = Mathf.Max(1, Mathf.CeilToInt(characters.Count / (float)columns));
            texture = new RenderTexture(columns * tilePixels, rows * tilePixels, 24, RenderTextureFormat.ARGB32)
            {
                name = "CharacterPreviews",
                antiAliasing = 4,
            };

            // Frame the grid: the camera looks along its forward axis at the stage origin.
            Transform view = previewCamera.transform;
            float halfFov = previewCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            cameraDistance = rows * spacing * 0.5f / Mathf.Tan(halfFov);
            view.position = transform.position - view.forward * cameraDistance;
            previewCamera.nearClipPlane = Mathf.Max(0.3f, cameraDistance - 10f);
            previewCamera.farClipPlane = cameraDistance + 10f;
            previewCamera.targetTexture = texture;
            previewCamera.enabled = true;

            turntableTemplate.gameObject.SetActive(false);
            for (int i = 0; i < characters.Count; i++)
            {
                int column = i % columns, row = i / columns;
                Vector3 offset = view.right * ((column - (columns - 1) * 0.5f) * spacing) + view.up * (((rows - 1) * 0.5f - row) * spacing);
                turntables.Add(CreateTurntable(characters[i], transform.position + offset, i));
            }

            RenderPipelineManager.beginCameraRendering += OnBeginCamera;
            RenderPipelineManager.endCameraRendering += OnEndCamera;
        }

        /// <summary>The part of <see cref="Texture"/> showing character <paramref name="index"/> (for RawImage.uvRect).</summary>
        public Rect TileRect(int index)
        {
            int column = index % columns, row = index / columns;
            return new Rect(column / (float)columns, 1f - (row + 1) / (float)rows, 1f / columns, 1f / rows);
        }

        public CharacterTurntable Turntable(int index) => turntables[index];

        CharacterTurntable CreateTurntable(PlayableCharacter character, Vector3 position, int index)
        {
            CharacterTurntable turntable = Instantiate(turntableTemplate, position, Quaternion.Euler(0f, index * 37f - 25f, 0f), transform);
            turntable.name = $"Turntable_{character.id}";
            turntable.gameObject.SetActive(true);

            GameObject model = Instantiate(character.definition.modelPrefab, turntable.transform, false);
            model.name = "Model";
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>())
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            // Normalize: every character fills its tile the same way, centered on the turntable axis.
            Bounds bounds = MeasureLocal(model, turntable.transform);
            float scale = modelFit / Mathf.Max(0.001f, Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z)));
            model.transform.localScale *= scale;
            model.transform.localPosition = -bounds.center * scale;
            return turntable;
        }

        static Bounds MeasureLocal(GameObject model, Transform space)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(Vector3.zero, Vector3.one);
            Bounds world = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) world.Encapsulate(renderers[i].bounds);
            return new Bounds(space.InverseTransformPoint(world.center), world.size);
        }

        // The ink outline fades relative to a focus distance; give this camera its own while it renders.
        void OnBeginCamera(ScriptableRenderContext context, Camera rendering)
        {
            if (rendering != previewCamera) return;
            outlineFocusOutside = Shader.GetGlobalFloat(OutlineFocusId);
            Shader.SetGlobalFloat(OutlineFocusId, cameraDistance);
        }

        void OnEndCamera(ScriptableRenderContext context, Camera rendering)
        {
            if (rendering == previewCamera) Shader.SetGlobalFloat(OutlineFocusId, outlineFocusOutside);
        }

        void OnDestroy()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
            RenderPipelineManager.endCameraRendering -= OnEndCamera;
            if (previewCamera != null) previewCamera.targetTexture = null;
            if (texture == null) return;
            texture.Release();
            Object.Destroy(texture);
        }
    }
}
