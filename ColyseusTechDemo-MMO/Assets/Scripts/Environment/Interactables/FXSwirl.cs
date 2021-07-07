using System.Collections;
using UnityEngine;

public class FXSwirl : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem particles = null;

    [SerializeField]
    private ParticleSystemForceField forceField = null;

    [Header("Expansion Settings")]
    [SerializeField]
    private Vector3 volumeStartSize = new Vector3();

    [SerializeField]
    private Vector3 volumeEndSize = new Vector3();

    [SerializeField]
    private float expansionTime = 0.5f;

    [SerializeField]
    private Easings.EaseType easeType = Easings.EaseType.None;

    [Header("Swirl Settings")]
    [SerializeField]
    private float swirlTime = 1f;

    [SerializeField]
    private float maxSwirlSpeed = 1f;

    [SerializeField]
    private float gravityTime = 1f;

    [SerializeField]
    private float maxGravity = 1f;

    [SerializeField]
    private Easings.EaseType swirlEaseType = Easings.EaseType.None;

    public void PlaySwirlFx()
    {
        ResetFX();

        StartCoroutine(Co_AnimateFX());
    }

    private void ResetFX()
    {
        particles.Stop();
        forceField.gravity = 0;
        forceField.rotationSpeed = 0;
    }

    private IEnumerator Co_AnimateFX()
    {
        yield return Co_StartAndExpand();

        yield return Co_Swirl();

        yield return Co_Gravitate();
    }

    private IEnumerator Co_StartAndExpand()
    {
        float currentTime = 0;

        transform.localScale = volumeStartSize;

        particles.Play();

        yield return new WaitForSeconds(1);

        while (currentTime < expansionTime)
        {
            transform.localScale = Vector3.Lerp(volumeStartSize, volumeEndSize, Easings.Ease(Mathf.Clamp01(currentTime / expansionTime), easeType));

            currentTime += Time.deltaTime;

            yield return null;
        }

        transform.localScale = volumeEndSize;
    }

    private IEnumerator Co_Swirl()
    {
        float currentTime = 0;
        float rotSpeed = 0;

        ParticleSystem.MinMaxCurve rotationSpeed = forceField.rotationSpeed;
        rotationSpeed.mode = ParticleSystemCurveMode.Constant;
        
        while (currentTime < swirlTime)
        {
            rotSpeed = Mathf.Lerp(0, maxSwirlSpeed, Easings.Ease(Mathf.Clamp01(currentTime / swirlTime), swirlEaseType));

            rotationSpeed.constant = rotSpeed;
            forceField.rotationSpeed = rotationSpeed;

            currentTime += Time.deltaTime;

            yield return null;
        }

        rotationSpeed.constant = maxSwirlSpeed;

        forceField.rotationSpeed = rotationSpeed;

    }

    private IEnumerator Co_Gravitate()
    {
        float currentTime = 0;
        float gravitateStrength = 0;

        ParticleSystem.MinMaxCurve gravityStrength = forceField.gravity;
        gravityStrength.mode = ParticleSystemCurveMode.Constant;

        while (currentTime < gravityTime)
        {
            gravitateStrength = Mathf.Lerp(0, maxGravity, Easings.Ease(Mathf.Clamp01(currentTime / gravityTime), swirlEaseType));

            gravityStrength.constant = gravitateStrength;
            forceField.gravity = gravityStrength;

            currentTime += Time.deltaTime;

            yield return null;
        }

        gravityStrength.constant = maxGravity;

        forceField.gravity = gravityStrength;
    }
}
