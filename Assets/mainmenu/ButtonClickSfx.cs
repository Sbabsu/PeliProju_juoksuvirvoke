using UnityEngine;

public class ButtonClickSfx : MonoBehaviour
{
    public AudioSource uiSfxSource;   // tämä on se UIAudio
    public AudioClip clickClip;

    public void PlayClick()
    {
        if (uiSfxSource == null || clickClip == null) return;
        uiSfxSource.PlayOneShot(clickClip);
    }
}
