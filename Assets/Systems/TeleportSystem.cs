using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Systems
{
    public class TeleportSystem : MonoBehaviour
    {
        public GameObject tptTo;
        public bool isRight = false;
        
        public float fadeDuration = 1f; // Duration of the fade effect in seconds
        
        private CanvasGroup _fadeCanvasGroup;
        
        private BoxCollider2D _tptCollider;
        private BoxCollider2D _newCollider;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _fadeCanvasGroup = GameObject.Find("FadeCanvasGroup").GetComponent<CanvasGroup>();
            _fadeCanvasGroup.blocksRaycasts = false;
            
            _tptCollider = transform.gameObject.GetComponent<BoxCollider2D>();
            _tptCollider.isTrigger = true;
            
            // make the new collider
            BoxCollider2D newCollider = gameObject.AddComponent<BoxCollider2D>();
            newCollider.isTrigger = false;
            newCollider.size = Vector2.one * 0.99f;
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        
        private IEnumerator FadeOut()
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                _fadeCanvasGroup.alpha = 1f - (timer / fadeDuration); // Decrease alpha
                yield return null; // Wait for the next frame
            }
            _fadeCanvasGroup.alpha = 0f; // Ensure it's fully transparent
            
            _fadeCanvasGroup.blocksRaycasts = false;
        }

        private IEnumerator FadeIn()
        {
            _fadeCanvasGroup.blocksRaycasts = true;
            
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                _fadeCanvasGroup.alpha = timer / fadeDuration; // Increase alpha
                yield return null; // Wait for the next frame
            }
            _fadeCanvasGroup.alpha = 1f; // Ensure it's fully opaque
        }
        
        private IEnumerator TeleportWithFade(Collider2D other, Vector3 offset)
        {
            // Start fade-in
            yield return StartCoroutine(FadeIn());

            // Teleport the object
            
            other.transform.position = tptTo.transform.position + offset * 1.5f;

            yield return new WaitForSeconds(0.5f); // Adjust the wait time as needed
            
            // Start fade-out
            yield return StartCoroutine(FadeOut());
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Vector3 offset = Vector3.right;
            
            if (isRight)
            {
                offset = Vector3.left;
            }
            
            StartCoroutine(TeleportWithFade(other, offset));
            
            // wait for fade in to finish
            
        }
    }
}
