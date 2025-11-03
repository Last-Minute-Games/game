using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

namespace Systems.Overworld.Intro
{
    public class TutorialTrigger : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private BoxCollider2D _boxCollider;
        private TutorialScene _tutorialScene;

        // create enum
        public enum TriggerType
        {
            Unknown,
            Journal,
            Lighting
        }

        public UnityEvent OnTriggerEnter;

        [Header("Trigger Settings")] public TriggerType currentType = TriggerType.Unknown;
        public bool onlyTriggerOnce = true;

        [Header("Lighting Settings")] public Light2D selectedLight2D;
        public bool setLightActive = false;

        void Start()
        {
            _boxCollider = gameObject.AddComponent<BoxCollider2D>();
            _boxCollider.isTrigger = true;

            _tutorialScene = FindFirstObjectByType<TutorialScene>();
        }

        // Update is called once per frame
        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            Debug.Log("Player entered the tutorial trigger area.");

            if (OnTriggerEnter != null && OnTriggerEnter.GetPersistentEventCount() > 0)
            {
                OnTriggerEnter.Invoke();
            }
            else
            {
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
                            selectedLight2D.enabled = setLightActive;
                            var lightAudioSource = selectedLight2D.GetComponent<AudioSource>();
                            if (lightAudioSource) lightAudioSource.Play();
                        }

                        break;
                    }
                }
            }

            if (onlyTriggerOnce) transform.gameObject.SetActive(false);
        }
    }
}