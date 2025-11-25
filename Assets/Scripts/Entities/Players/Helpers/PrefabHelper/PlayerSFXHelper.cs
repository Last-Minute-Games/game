using UnityEngine;

public class PlayerSFXHelper : MonoBehaviour
{
    public SFXCueData spawnCue;
    public SFXCueData damageCue;
    public SFXCueData healCue;
    public SFXCueData shieldCue;
    public SFXCueData energyGainCue;

    public void PlaySpawn()      => SFXManager.Instance?.Play(spawnCue);
    public void PlayDamage()     => SFXManager.Instance?.Play(damageCue);
    public void PlayHeal()       => SFXManager.Instance?.Play(healCue);
    public void PlayShield()     => SFXManager.Instance?.Play(shieldCue);
    public void PlayEnergyGain() => SFXManager.Instance?.Play(energyGainCue);
}
