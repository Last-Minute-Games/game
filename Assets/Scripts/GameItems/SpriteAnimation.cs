using System.Collections.Generic;
using UnityEngine;

namespace GameItems
{
    /// <summary>
    /// A ScriptableObject to hold the frames for a sprite-based animation.
    /// This allows you to create animation assets in the editor.
    /// </summary>
    [CreateAssetMenu(fileName = "New SpriteAnimation", menuName = "Game/Sprite Animation")]
    public class SpriteAnimation : ScriptableObject
    {
        [Tooltip("The sequence of sprites that make up the animation.")]
        public List<Sprite> frames = new List<Sprite>();

        [Tooltip("The number of frames to display per second.")]
        public float frameRate = 12f;

        [Tooltip("Should this animation loop?")]
        public bool loop = true;
    }
}

