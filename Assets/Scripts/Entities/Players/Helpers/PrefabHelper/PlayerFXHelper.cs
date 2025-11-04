using UnityEngine;

public class PlayerFXHelper : MonoBehaviour
{
    [Header("Sub-Helpers")]
    public PlayerAnimationHelper animHelper;
    public PlayerSFXHelper sfxHelper;

    public void PlaySpawnFeedback()
    {
        animHelper?.PlaySpawnAnimation();
        sfxHelper?.PlaySpawn();
    }

    public void PlayDamageFeedback()
    {
        animHelper?.PlayDamageFlash();
        sfxHelper?.PlayDamage();
    }

    public void PlayHealFeedback()
    {
        animHelper?.PlayHealGlow();
        sfxHelper?.PlayHeal();
    }

    public void PlayShieldFeedback()
    {
        animHelper?.PlayShieldPulse();
        sfxHelper?.PlayShield();
    }

    public void PlayEnergyFeedback()
    {
        animHelper?.PlayEnergyPulse();
        sfxHelper?.PlayEnergyGain();
    }
}
