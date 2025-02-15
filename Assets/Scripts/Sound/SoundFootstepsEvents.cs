
using UnityEngine;

public class SoundFootstepsEvents : MonoBehaviour
{
    [SerializeField] private Transform origin = null;
    [SerializeField] private float screenShakeStrength = 0;
    [SerializeField] private float screenShakeDuration = .5f;
    [SerializeField] private SoundVariant sound;

    void Start()
    {
        if (origin == null) origin = transform;
    }
    void FootstepEvent()
    {
        if (screenShakeStrength > 0)
            ScreenShake.ShakeStart(origin.position, screenShakeStrength, screenShakeDuration);
        if (sound != null)
            sound.Play(origin.position);
    }
}
