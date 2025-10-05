using UnityEngine;

public class AttackCard : CardBase
{
    public int damage = 25;
    private Enemy target;

    private void Start()
    {
        cardName = "Strike";
    }

    private void OnMouseDown()
    {
        // Find the first enemy for demo (can expand later)
        // Find the first Enemy instance
        target = Object.FindFirstObjectByType<Enemy>();

        // Find the player
        Protagonist player = Object.FindFirstObjectByType<Protagonist>();

        // Use the card
        if (target != null && player != null)
        {
            Use(player, target);
            gameObject.SetActive(false); // hide card instead of destroy
        }
    }

    public override void Use(CharacterBase user, CharacterBase target)
    {
        Debug.Log($"{user.characterName} uses {cardName} on {target.characterName}!");
        target.TakeDamage(damage + user.attack);
    }
}
