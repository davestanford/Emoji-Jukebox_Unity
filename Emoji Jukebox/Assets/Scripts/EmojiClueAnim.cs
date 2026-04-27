using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class EmojiClueAnim : MonoBehaviour
{
    [Header("Reveal")]
    [SerializeField] private float hiddenScale = 0.6f;
    [SerializeField] private float overshootScale = 1.12f;
    [SerializeField] private float finalScale = 1f;
    [SerializeField] private float revealDuration = 0.35f;

    [Range(0.1f, 0.9f)]
    [SerializeField] private float popPhasePercent = 0.7f;

    [Header("Idle Bob")]
    [SerializeField] private float bobAmount = 4f;
    [SerializeField] private float bobSpeed = 2f;

    [Header("Rotation Wiggle")]
    [SerializeField] private float rotateAmount = 2.5f;
    [SerializeField] private float rotateSpeed = 1.6f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 baseAnchoredPos;
    private float randomOffset;
    private float randomRotateOffset;

    public void Play(float revealDelay = 0f)
    {
        rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        randomOffset = Random.Range(0f, 10f);
        randomRotateOffset = Random.Range(0f, 10f);

        StopAllCoroutines();
        StartCoroutine(PlaySequence(revealDelay));
    }

    private IEnumerator PlaySequence(float revealDelay)
    {
        // Hide immediately
        canvasGroup.alpha = 0f;
        rectTransform.localScale = Vector3.one * hiddenScale;
        rectTransform.localRotation = Quaternion.identity;

        // Let Unity layout groups finish positioning this object
        yield return null;
        yield return new WaitForEndOfFrame();

        baseAnchoredPos = rectTransform.anchoredPosition;

        if (revealDelay > 0f)
            yield return new WaitForSeconds(revealDelay);

        // Reveal now
        canvasGroup.alpha = 1f;

        float popTime = revealDuration * popPhasePercent;
        float settleTime = revealDuration - popTime;

        float t = 0f;
        while (t < popTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / popTime);
            float eased = EaseOutBack(p);

            rectTransform.localScale = Vector3.LerpUnclamped(
                Vector3.one * hiddenScale,
                Vector3.one * overshootScale,
                eased
            );

            yield return null;
        }

        t = 0f;
        while (t < settleTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / settleTime);

            rectTransform.localScale = Vector3.Lerp(
                Vector3.one * overshootScale,
                Vector3.one * finalScale,
                p
            );

            yield return null;
        }

        rectTransform.localScale = Vector3.one * finalScale;

        StartCoroutine(IdleMotion());
    }

    private IEnumerator IdleMotion()
    {
        while (true)
        {
            float time = Time.unscaledTime;

            float bob = Mathf.Sin((time + randomOffset) * bobSpeed) * bobAmount;
            float rot = Mathf.Sin((time + randomRotateOffset) * rotateSpeed) * rotateAmount;

            rectTransform.anchoredPosition = baseAnchoredPos + new Vector2(0f, bob);
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, rot);

            yield return null;
        }
    }

    private float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}