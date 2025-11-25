using System.Collections.Generic;
using UnityEngine;

namespace GameItems
{
    /// <summary>
    /// Holds the sprite frames and frame rate for a single animation.
    /// This version is designed for runtime use and does not depend on UnityEditor.
    /// </summary>
    public class AnimationData
    {
        public List<Sprite> Frames { get; }
        public float FrameRate { get; }

        public AnimationData(List<Sprite> frames, float frameRate = 12f)
        {
            Frames = frames ?? new List<Sprite>();
            FrameRate = frameRate > 0 ? frameRate : 12f;
        }
    }
}
