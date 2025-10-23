using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Systems.Overworld.Intro
{
    public class TutorialTrigger : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private BoxCollider2D _boxCollider;
        private TutorialScene _tutorialScene;
        
        // create enum
        public enum TriggerType { Unknown, Journal, Lighting }
        
        [Header("Trigger Settings")]
        public TriggerType currentType = TriggerType.Unknown;
        public bool onlyTriggerOnce = true;
        
        [Header("Lighting Settings")]
        public Light2D selectedLight2D;
        public bool setLightActive = false;
        private float lastLightIntensity;
    
        void Start()
        {
            _boxCollider = gameObject.AddComponent<BoxCollider2D>();
            _boxCollider.isTrigger = true;
        
            _tutorialScene = FindFirstObjectByType<TutorialScene>();
            
            lastLightIntensity  = selectedLight2D.intensity;
        }
        

        // Update is called once per frame
        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
        
            // Trigger the tutorial event here
            Debug.Log("Player entered the tutorial trigger area.");
            // You can add your tutorial logic here, such as displaying UI elements or starting a tutorial sequence.
            
            // Optionally, disable the trigger after activation to prevent repeated triggers

            switch (currentType)
            {
                case TriggerType.Journal:
                    _tutorialScene.SwitchJournalPage(transform.name);
                    StartCoroutine(_tutorialScene.OpenJournal());
                    break;
                case TriggerType.Lighting:
                {
                    if (selectedLight2D)
                    {
                        selectedLight2D.intensity = setLightActive  ? lastLightIntensity : 0;
                        var audioSource = selectedLight2D.transform.GetComponent<AudioSource>();
                        if (audioSource) audioSource.Play();
                    }

                    break;
                }
            }
            
            if (onlyTriggerOnce) _boxCollider.enabled = false;
        }
    }
}
