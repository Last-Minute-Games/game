using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterMotor2D))]
public class PlayerInput2D : MonoBehaviour
{
    private CharacterMotor2D _motor;
    
    public bool isInputEnabled = true;

    void Awake() => _motor = GetComponent<CharacterMotor2D>();

    void Update()
    {
        if (_motor.IsDialogueActive || _motor.IsTeleporting || !isInputEnabled)
        {
            _motor.SetMoveInput(Vector2.zero);
            return;
        }

        var move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _motor.SetMoveInput(move);
    }
}