using UnityEngine;

namespace BerTaDEV.Toolkit
{
    /// <summary>
    /// Shows a serialized field in the Inspector but greys it out so it can't be edited.
    /// Handy for values your code sets that you still want to watch.
    /// </summary>
    /// <example><code>[SerializeField, ReadOnlyInspector] int currentHealth;</code></example>
    public sealed class ReadOnlyInspectorAttribute : PropertyAttribute
    {
    }
}
