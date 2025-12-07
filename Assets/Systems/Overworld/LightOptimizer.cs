using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Systems.Overworld
{
    public class LightOptimizer : MonoBehaviour
    {
        public GameObject roomsGroup;
    
        public void EnableLight(string roomName)
        {
            DisableAllLights();
            
            Transform roomTransform = roomsGroup.transform.Find(roomName);
            Light2D[] lights = roomTransform.GetComponentsInChildren<Light2D>();

            if (roomTransform == null) return;
        
            foreach (Light2D light2D in lights)
            {
                light2D.enabled = true;
            }
        }
    
        public void DisableLight(string roomName) 
        {
            Transform roomTransform = roomsGroup.transform.Find(roomName);
            Light2D[] lights = roomTransform.GetComponentsInChildren<Light2D>();

            if (roomTransform == null) return;
        
            foreach (Light2D light2D in lights)
            {
                light2D.enabled = false;
            }
        }
    
        public void EnableAllLights() 
        {
            Light2D[] lights = roomsGroup.GetComponentsInChildren<Light2D>();

            foreach (Light2D light2D in lights)
            {
                light2D.enabled = true;
            }
        }
    
        public void DisableAllLights() 
        {
            Light2D[] lights = roomsGroup.GetComponentsInChildren<Light2D>();

            foreach (Light2D light2D in lights)
            {
                light2D.enabled = false;
            }
        }
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            DisableAllLights();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
