using UnityEngine;
using UnityEngine.UI;

namespace BerTaDEV.Toolkit
{
    /// <summary>
    /// Scrolls a RawImage's texture in an endless loop: moving backgrounds, conveyor belts,
    /// loading stripes. The texture repeats every <see cref="tileSize"/> pixels instead of
    /// stretching over the whole image.
    /// </summary>
    [AddComponentMenu("BerTaDEV/UI Scroll UV")]
    [RequireComponent(typeof(RawImage))]
    [DisallowMultipleComponent]
    public class UIScrollUV : MonoBehaviour
    {
        [Tooltip("Tiles per second. Positive X scrolls the image left, positive Y scrolls it down.")]
        public Vector2 speed = new Vector2(0.25f, 0f);

        [Tooltip("Size of one tile in UI pixels. Leave at 0 to use the texture's own size.")]
        public Vector2 tileSize;

        [Tooltip("Keep scrolling while the game is paused (Time.timeScale = 0).")]
        public bool useUnscaledTime;

        RawImage rawImage;
        Vector2 offset;

        void Awake()
        {
            rawImage = GetComponent<RawImage>();
            offset = rawImage.uvRect.position;

            var texture = rawImage.texture;
            if (texture != null && texture.wrapMode != TextureWrapMode.Repeat)
                Debug.LogWarning($"[BerTaDEV] UI Scroll UV: set Wrap Mode to Repeat on '{texture.name}' so it loops instead of smearing.", this);
        }

        void Update() => Advance(useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);

        void Advance(float deltaTime)
        {
            // Wrapping keeps the offset between 0 and 1, so precision doesn't drift after hours of scrolling.
            offset.x = Mathf.Repeat(offset.x + speed.x * deltaTime, 1f);
            offset.y = Mathf.Repeat(offset.y + speed.y * deltaTime, 1f);
            rawImage.uvRect = new Rect(offset, TileCount());
        }

        /// <summary>How many times the texture fits across the image, so it keeps its size when the image is resized.</summary>
        Vector2 TileCount()
        {
            var texture = rawImage.texture;
            Vector2 tile = tileSize;
            if (tile.x <= 0f && texture != null) tile.x = texture.width;
            if (tile.y <= 0f && texture != null) tile.y = texture.height;

            var size = rawImage.rectTransform.rect.size;
            return new Vector2(tile.x > 0f ? size.x / tile.x : 1f, tile.y > 0f ? size.y / tile.y : 1f);
        }
    }
}
