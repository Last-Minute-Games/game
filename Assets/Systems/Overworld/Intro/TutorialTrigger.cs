using UnityEngine;

namespace Systems.Overworld.Intro
{
    public class TutorialTrigger : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private BoxCollider2D _boxCollider;
        private TutorialScene _tutorialScene;
    
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
        
            // Trigger the tutorial event here
            Debug.Log("Player entered the tutorial trigger area.");
            // You can add your tutorial logic here, such as displaying UI elements or starting a tutorial sequence.
            
            // Optionally, disable the trigger after activation to prevent repeated triggers

            _tutorialScene.SwitchJournalPage(transform.name);
            StartCoroutine(_tutorialScene.OpenJournal());
            
            transform.gameObject.SetActive(false);
        }
    }
}
