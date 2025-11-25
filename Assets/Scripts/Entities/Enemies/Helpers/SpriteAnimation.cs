using System.Collections.Generic;
using UnityEngine;

namespace Entities.Enemies.Helpers
{
    /// <summary>
    /// Lightweight sprite-based animation data that can be played manually without AnimationClips or Controllers.
    /// More efficient for simple sprite frame sequences.
    /// </summary>
    [CreateAssetMenu(menuName = "Animation/Sprite Animation", fileName = "NewSpriteAnimation")]
    public class SpriteAnimation : ScriptableObject
    {
        [Tooltip("Array of sprite frames to play in sequence.")]
        public List<Sprite> frames = new List<Sprite>();

        [Tooltip("Frames per second (e.g., 12 for classic pixel art feel).")]
        public float frameRate = 12f;

        [Tooltip("Should this animation loop when it reaches the end?")]
        public bool loop = true;

        /// <summary>
        /// Gets the total duration of this animation in seconds.
        /// </summary>
        public float Duration => frames.Count > 0 && frameRate > 0 ? frames.Count / frameRate : 0f;

        /// <summary>
        /// Validates that the animation has at least one frame.
        /// </summary>
        public bool IsValid => frames != null && frames.Count > 0;

        private void OnValidate()
        {
            if (frameRate <= 0f)
            {
                frameRate = 12f;
                Debug.LogWarning($"SpriteAnimation '{name}': frameRate must be > 0. Reset to 12.");
            }

            if (frames == null)
            {
                frames = new List<Sprite>();
            }
        }
    }
}

