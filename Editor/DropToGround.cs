using UnityEditor;
using UnityEngine;

namespace BerTaDEV.Toolkit
{
    /// <summary>
    /// Drops the selected objects onto the first collider below them (End),
    /// optionally rotating them to match the surface (Shift+End).
    /// </summary>
    internal static class DropToGround
    {
        const string DropPath = "Tools/BerTaDEV/Drop To Ground _END";
        const string AlignPath = "Tools/BerTaDEV/Drop To Ground And Align #END";
        static readonly RaycastHit[] hits = new RaycastHit[64];

        [MenuItem(DropPath, priority = 200)]
        static void Drop() => Run(false);

        [MenuItem(AlignPath, priority = 201)]
        static void DropAndAlign() => Run(true);

        [MenuItem(DropPath, true)]
        [MenuItem(AlignPath, true)]
        static bool Validate() => Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable).Length > 0;

        static void Run(bool alignToSurface)
        {
            var targets = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            int missed = 0;

            Physics.SyncTransforms();
            foreach (var t in targets)
            {
                var bounds = GetBounds(t);
                if (!TryFindGround(t, bounds.center, out var ground))
                {
                    missed++;
                    continue;
                }

                Undo.RecordObject(t, "Drop To Ground");
                if (alignToSurface)
                {
                    t.rotation = Quaternion.FromToRotation(t.up, ground.normal) * t.rotation;
                    Physics.SyncTransforms();
                    bounds = GetBounds(t);
                }
                t.position += Vector3.up * (ground.point.y - bounds.min.y);

                // Later objects in the selection can land on this one.
                Physics.SyncTransforms();
            }

            Undo.SetCurrentGroupName(alignToSurface ? "Drop To Ground And Align" : "Drop To Ground");
            Undo.CollapseUndoOperations(group);

            if (missed > 0)
                Debug.Log($"[BerTaDEV] Drop To Ground: {missed} object(s) had no collider below them.");
        }

        static bool TryFindGround(Transform t, Vector3 origin, out RaycastHit ground)
        {
            ground = default;
            var physics = t.gameObject.scene.GetPhysicsScene();
            int count = physics.Raycast(origin, Vector3.down, hits, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            float closest = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                if (hits[i].collider.transform.IsChildOf(t)) continue;
                if (hits[i].distance < closest)
                {
                    closest = hits[i].distance;
                    ground = hits[i];
                }
            }
            return closest < float.PositiveInfinity;
        }

        /// <summary>World bounds of the visible meshes/sprites, falling back to colliders, then to the pivot.</summary>
        static Bounds GetBounds(Transform t)
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
