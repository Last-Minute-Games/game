using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepSystem : MonoBehaviour
{
    [Header("Footstep Sounds")]
    public List<AudioClip> woodFs;

    [Header("Settings")]
    public float stepInterval = 0.4f;   // seconds between steps while moving
    public float minSpeedForStep = 0.1f; // how fast the player must move to count as walking

    private AudioSource _audioSource;
    private CharacterController2D _controller;
    private float _stepTimer;

    private enum SurfaceType
    {
        Wood,
        // Add more surface types here
    }

    private SurfaceType _surfaceType = SurfaceType.Wood;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _controller = GetComponent<CharacterController2D>(); // same GameObject

        if (_audioSource == null)
            Debug.LogError("[FootstepSystem] No AudioSource component found!");
        if (_controller == null)
            Debug.LogError("[FootstepSystem] No CharacterController2D found!");
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

        switch (_surfaceType)
        {
            case SurfaceType.Wood:
                if (woodFs.Count > 0)
                {
                    int index = Random.Range(0, woodFs.Count);
                    clip = woodFs[index];
                }
                break;
        }

        if (clip != null)
            _audioSource.PlayOneShot(clip);
    }
}