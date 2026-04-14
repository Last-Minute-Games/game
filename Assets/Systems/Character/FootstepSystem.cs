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
    public float stepInterval = 0.4f;
    public float minSpeedForStep = 0.1f;

    private Tilemap _floorTilemap;
    private AudioSource _audioSource;

    // ✅ SUPPORT BOTH CONTROLLERS
    private CharacterMotor2D _playerController;
    private NPCMotor2D _npcController;

    private Rigidbody2D _rb;

    private float _stepTimer;

    private enum SurfaceType
    {
        Wood,
        Concrete,
    }

    private SurfaceType _surfaceType = SurfaceType.Wood;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _rb = GetComponent<Rigidbody2D>();

        // Try both controllers
        _playerController = GetComponent<CharacterMotor2D>();
        _npcController = GetComponent<NPCMotor2D>();

        if (_audioSource == null)
            Debug.LogError("[FootstepSystem] No AudioSource found!");

        if (_playerController == null && _npcController == null)
            Debug.LogError("[FootstepSystem] No movement controller found!");

        if (_rb == null)
            Debug.LogError("[FootstepSystem] No Rigidbody2D found!");

        _floorTilemap = GameObject.Find("Floor")?.GetComponent<Tilemap>();
    }

    void Update()
    {
        if (!_audioSource || !_rb) return;

        // =========================
        // BLOCK STATES (BOTH SYSTEMS)
        // =========================
        bool isBlocked =
            (_playerController != null &&
                (_playerController.IsDialogueActive || _playerController.IsTeleporting))
            ||
            (_npcController != null &&
                (_npcController.IsDialogueActive || _npcController.IsTeleporting));

        if (isBlocked)
            return;

        float speed = _rb.linearVelocity.magnitude;

        if (speed > minSpeedForStep)
        {
            _stepTimer -= Time.deltaTime;

            if (_stepTimer <= 0f)
            {
                PlayStep();

                _stepTimer = Mathf.Lerp(
                    stepInterval,
                    stepInterval * 0.6f,
                    speed / 5f
                );
            }
        }
        else
        {
            _stepTimer = 0f;
        }
    }

    private void PlayStep()
    {
        AudioClip clip = null;

        Vector3Int cellPosition = _floorTilemap.WorldToCell(transform.position);
        TileBase tile = _floorTilemap.GetTile(cellPosition);

        if (tile)
        {
            string name = tile.name.ToLower();

            if (name.Contains("wood"))
                _surfaceType = SurfaceType.Wood;

            else if (name.Contains("concrete") || name.Contains("marble"))
                _surfaceType = SurfaceType.Concrete;
        }

        clip = _surfaceType switch
        {
            SurfaceType.Wood => woodFs[Random.Range(0, woodFs.Count)],
            SurfaceType.Concrete => concreteFs[Random.Range(0, concreteFs.Count)],
            _ => null
        };

        if (clip)
        {
            _audioSource.pitch = Random.Range(0.9f, 1.1f);
            _audioSource.PlayOneShot(clip);
        }
    }
}