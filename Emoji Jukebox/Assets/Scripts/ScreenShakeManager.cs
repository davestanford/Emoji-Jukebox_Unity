using System.Collections;
using UnityEngine;

public class ScreenShakeManager : MonoBehaviour
{
    public static ScreenShakeManager Instance;

    [Header("Target")]
    [SerializeField] private RectTransform targetToShake;

    private Vector2 originalAnchoredPos;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (targetToShake != null)
            originalAnchoredPos = targetToShake.anchoredPosition;
    }

    public void Shake(float duration = 0.12f, float strength = 20f)
    {
        if (targetToShake == null)
            return;

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, strength));
    }

    private IEnumerator ShakeRoutine(float duration, float strength)
    {
        originalAnchoredPos = targetToShake.anchoredPosition;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float damper = 1f - Mathf.Clamp01(elapsed / duration);

            float offsetX = Random.Range(-strength, strength) * damper;
            float offsetY = Random.Range(-strength, strength) * damper;

            targetToShake.anchoredPosition = originalAnchoredPos + new Vector2(offsetX, offsetY);

            yield return null;
        }

        targetToShake.anchoredPosition = originalAnchoredPos;
        shakeRoutine = null;
    }
}