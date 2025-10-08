using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(AudioSource))]
public class FootstepSystem : MonoBehaviour
{
    [Header("Footstep Sounds")]
    public List<AudioClip> woodFs;
    public List<AudioClip> concreteFs;

    [Header("Settings")]
    public float stepInterval = 0.4f;   // seconds between steps while moving
    public float minSpeedForStep = 0.1f; // how fast the player must move to count as walking

    private Tilemap _floorTilemap;
    private AudioSource _audioSource;
    private CharacterMotor2D _controller;
    private float _stepTimer;

    private enum SurfaceType
    {
        Wood,
        Concrete,
        // Add more surface types here
    }

    private SurfaceType _surfaceType = SurfaceType.Wood;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _controller = GetComponent<CharacterMotor2D>(); // same GameObject

        if (_audioSource == null)
            Debug.LogError("[FootstepSystem] No AudioSource component found!");
        if (_controller == null)
            Debug.LogError("[FootstepSystem] No CharacterController2D found!");
        
        // Find the ground tilemap in the scene
        _floorTilemap = GameObject.Find("Floor")?.GetComponent<Tilemap>();
    }

    void Update()
    {
        if (_controller == null || _audioSource == null) return;

        // Skip if in dialogue/teleport or not moving
        if (_controller.IsDialogueActive || _controller.IsTeleporting) return;

        Vector2 velocity = _controller.GetComponent<Rigidbody2D>().linearVelocity;
        float speed = velocity.magnitude;

        // Only step if moving
        if (speed > minSpeedForStep)
        {
            _stepTimer -= Time.deltaTime;
            if (_stepTimer <= 0f)
            {
                PlayStep();
                // Reset step timer; shorter interval for faster movement
                _stepTimer = Mathf.Lerp(stepInterval, stepInterval * 0.6f, speed / 5f);
            }
        }
        else
        {
            _stepTimer = 0f; // reset so next time you start moving, step instantly
        }
    }

    private void PlayStep()
    {
        AudioClip clip = null;

        Vector3Int cellPosition = _floorTilemap.WorldToCell(transform.position);
        TileBase tile = _floorTilemap.GetTile(cellPosition);
        if (tile)
        {
            // Here you could check tile properties to determine surface type
            // For simplicity, we assume all tiles in this tilemap are wood
                
            // print tile name
            Debug.Log("[FootstepSystem] Stepping on tile: " + tile.name);

            var loweredName = tile.name.ToLower();
            
            if (loweredName.Contains("wood"))
            {
                _surfaceType = SurfaceType.Wood;
            } else if (loweredName.Contains("concrete") || loweredName.Contains("marble"))
            {
                _surfaceType = SurfaceType.Concrete;
            }
        }

        clip = _surfaceType switch
        {   
            SurfaceType.Wood => woodFs[Random.Range(0, woodFs.Count)],
            SurfaceType.Concrete => concreteFs[Random.Range(0, concreteFs.Count)],
            _ => null
        };

        if (clip)
        {
            // RANDOMIZE PITCH
            _audioSource.pitch = Random.Range(0.9f, 1.1f); 
            _audioSource.PlayOneShot(clip);
        }
    }
}