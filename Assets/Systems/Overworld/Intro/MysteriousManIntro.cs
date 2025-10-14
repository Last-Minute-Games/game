using System.Collections;
using UnityEngine;

namespace Systems.Overworld.Intro
{
    public class MysteriousManIntro : MonoBehaviour
    {
        private SpriteRenderer _mysteriousSpriteRenderer;
        
        private Animator _mysteriousAnimator;
        public AnimationClip mysteriousAnimationClip;
    
        public IEnumerator FadeOut()
        {
            float alphaVal = _mysteriousSpriteRenderer.color.a;
            Color tmp = _mysteriousSpriteRenderer.color;

            while (_mysteriousSpriteRenderer.color.a > 0)
            {
                alphaVal -= 0.01f;
                tmp.a = alphaVal;
                _mysteriousSpriteRenderer.color = tmp;

                yield return new WaitForSeconds(0.015f); // update interval
            }
        }

        public IEnumerator FadeIn()
        {
            float alphaVal = _mysteriousSpriteRenderer.color.a;
            Color tmp = _mysteriousSpriteRenderer.color;

            while (_mysteriousSpriteRenderer.color.a < 1)
            {
                alphaVal += 0.01f;
                tmp.a = alphaVal;
                _mysteriousSpriteRenderer.color = tmp;

                yield return new WaitForSeconds(0.015f); // update interval
            }
        }
        
        private IEnumerator FreezeAnimationAfterDelay(Animator animator, float delay)
        {
            yield return new WaitForSeconds(delay);
            animator.speed = 0; // Freeze the animation
        }
        
        public IEnumerator PlayAnimationOnce()
        {
            _mysteriousAnimator.speed = 1; // Resume the animation
            yield return new WaitForSeconds(mysteriousAnimationClip.length);
            _mysteriousAnimator.speed = 0; // Freeze the animation again
        }
    
        void Start()
        {
            _mysteriousSpriteRenderer = GetComponent<SpriteRenderer>();
            _mysteriousAnimator = GetComponent<Animator>();
            
            // freeze animator at start
            _mysteriousAnimator.speed = 0;
            
            // Start fully invisible
            Color tmp = _mysteriousSpriteRenderer.color;
            tmp.a = 0f;
            _mysteriousSpriteRenderer.color = tmp;
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
