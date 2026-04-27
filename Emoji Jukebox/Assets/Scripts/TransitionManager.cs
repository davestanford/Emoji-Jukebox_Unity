using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    [Header("Overlay Images")]
    [SerializeField] private Image blackFadeImage;
    [SerializeField] private Image whiteFlashImage;
    [SerializeField] private Image redFlashImage;

    [Header("Default Black Fade Timing")]
    [SerializeField] private float defaultFadeInDuration = 0.25f;
    [SerializeField] private float defaultHoldDuration = 0.05f;
    [SerializeField] private float defaultFadeOutDuration = 0.25f;

    [Header("Default White Flash Timing")]
    [SerializeField] private float defaultWhiteFlashInDuration = 0.04f;
    [SerializeField] private float defaultWhiteFlashOutDuration = 0.14f;

    [Header("Default Red Flash Timing")]
    [SerializeField] private float defaultRedFlashInDuration = 0.05f;
    [SerializeField] private float defaultRedFlashOutDuration = 0.18f;

    private Coroutine currentBlackFadeRoutine;
    private Coroutine currentWhiteFlashRoutine;
    private Coroutine currentRedFlashRoutine;

    public bool IsTransitioning { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        SetImageAlpha(blackFadeImage, 0f);
        SetImageAlpha(whiteFlashImage, 0f);
        SetImageAlpha(redFlashImage, 0f);
    }

    public void PlayBlackFade(Action midAction)
    {
        PlayBlackFade(midAction, defaultFadeInDuration, defaultHoldDuration, defaultFadeOutDuration);
    }

    public void PlayBlackFade(Action midAction, float fadeInDuration, float holdDuration, float fadeOutDuration)
    {
        if (currentBlackFadeRoutine != null)
            StopCoroutine(currentBlackFadeRoutine);

        currentBlackFadeRoutine = StartCoroutine(
            BlackFadeRoutine(midAction, fadeInDuration, holdDuration, fadeOutDuration)
        );
    }

    public void PlayWhiteFlash()
    {
        PlayWhiteFlash(defaultWhiteFlashInDuration, defaultWhiteFlashOutDuration);
    }

    public void PlayWhiteFlash(float flashInDuration, float flashOutDuration)
    {
        if (currentWhiteFlashRoutine != null)
            StopCoroutine(currentWhiteFlashRoutine);

        currentWhiteFlashRoutine = StartCoroutine(
            FlashRoutine(whiteFlashImage, flashInDuration, flashOutDuration, FlashType.White)
        );
    }

    public void PlayRedFlash()
    {
        PlayRedFlash(defaultRedFlashInDuration, defaultRedFlashOutDuration);
    }

    public void PlayRedFlash(float flashInDuration, float flashOutDuration)
    {
        if (currentRedFlashRoutine != null)
            StopCoroutine(currentRedFlashRoutine);

        currentRedFlashRoutine = StartCoroutine(
            FlashRoutine(redFlashImage, flashInDuration, flashOutDuration, FlashType.Red)
        );
    }

    private IEnumerator BlackFadeRoutine(Action midAction, float fadeInDuration, float holdDuration, float fadeOutDuration)
    {
        IsTransitioning = true;

        yield return FadeImageAlpha(blackFadeImage, 0f, 1f, fadeInDuration);

        if (holdDuration > 0f)
            yield return new WaitForSecondsRealtime(holdDuration);

        midAction?.Invoke();

        yield return FadeImageAlpha(blackFadeImage, 1f, 0f, fadeOutDuration);

        IsTransitioning = false;
        currentBlackFadeRoutine = null;
    }

    private IEnumerator FlashRoutine(Image image, float flashInDuration, float flashOutDuration, FlashType flashType)
    {
        if (image == null)
            yield break;

        SetImageAlpha(image, 0f);

        yield return FadeImageAlpha(image, 0f, 1f, flashInDuration);
        yield return FadeImageAlpha(image, 1f, 0f, flashOutDuration);

        if (flashType == FlashType.White)
            currentWhiteFlashRoutine = null;
        else if (flashType == FlashType.Red)
            currentRedFlashRoutine = null;
    }

    private IEnumerator FadeImageAlpha(Image image, float from, float to, float duration)
    {
        if (image == null)
            yield break;

        if (duration <= 0f)
        {
            SetImageAlpha(image, to);
            yield break;
        }

        float t = 0f;
        SetImageAlpha(image, from);

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            float alpha = Mathf.Lerp(from, to, p);
            SetImageAlpha(image, alpha);
            yield return null;
        }

        SetImageAlpha(image, to);
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
            return;

        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }

    private enum FlashType
    {
        White,
        Red
    }
}