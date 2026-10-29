using UnityEngine;

namespace BerTaDEV.Toolkit
{
    internal static class BoundsUtility
    {
        /// <summary>World bounds of the visible meshes/sprites, falling back to colliders, then to the pivot.</summary>
        public static Bounds GetWorldBounds(Transform t)
        {
            var bounds = new Bounds(t.position, Vector3.zero);
            bool found = false;

            foreach (var renderer in t.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.enabled) continue;
                if (!(renderer is MeshRenderer || renderer is SkinnedMeshRenderer || renderer is SpriteRenderer)) continue;
                if (found) bounds.Encapsulate(renderer.bounds);
                else { bounds = renderer.bounds; found = true; }
            }
            if (found) return bounds;

            foreach (var collider in t.GetComponentsInChildren<Collider>())
            {
                if (!collider.enabled || collider.isTrigger) continue;
                if (found) bounds.Encapsulate(collider.bounds);
                else { bounds = collider.bounds; found = true; }
            }
            return bounds;
        }
    }
}
