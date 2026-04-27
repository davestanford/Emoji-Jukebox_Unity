using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class EmojiHoverJiggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float hoverScale = 1.12f;
    [SerializeField] private float scaleSpeed = 12f;

    [Header("Jiggle")]
    [SerializeField] private float jiggleAngle = 6f;
    [SerializeField] private float jiggleSpeed = 18f;

    private RectTransform rectTransform;
    private bool isHovered = false;
    private Coroutine hoverRoutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one * normalScale;
            rectTransform.localRotation = Quaternion.identity;
        }

        isHovered = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        if (hoverRoutine != null)
            StopCoroutine(hoverRoutine);

        hoverRoutine = StartCoroutine(HoverRoutine());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;

        if (hoverRoutine != null)
            StopCoroutine(hoverRoutine);

        hoverRoutine = StartCoroutine(ReturnToNormalRoutine());
    }

    private IEnumerator HoverRoutine()
    {
        while (isHovered)
        {
            float angle = Mathf.Sin(Time.unscaledTime * jiggleSpeed) * jiggleAngle;

            rectTransform.localScale = Vector3.Lerp(
                rectTransform.localScale,
                Vector3.one * hoverScale,
                Time.unscaledDeltaTime * scaleSpeed
            );

            rectTransform.localRotation = Quaternion.Lerp(
                rectTransform.localRotation,
                Quaternion.Euler(0f, 0f, angle),
                Time.unscaledDeltaTime * scaleSpeed
            );

            yield return null;
        }
    }

    private IEnumerator ReturnToNormalRoutine()
    {
        while (Vector3.Distance(rectTransform.localScale, Vector3.one * normalScale) > 0.001f ||
               Quaternion.Angle(rectTransform.localRotation, Quaternion.identity) > 0.1f)
        {
            rectTransform.localScale = Vector3.Lerp(
                rectTransform.localScale,
                Vector3.one * normalScale,
                Time.unscaledDeltaTime * scaleSpeed
            );

            rectTransform.localRotation = Quaternion.Lerp(
                rectTransform.localRotation,
                Quaternion.identity,
                Time.unscaledDeltaTime * scaleSpeed
            );

            yield return null;
        }

        rectTransform.localScale = Vector3.one * normalScale;
        rectTransform.localRotation = Quaternion.identity;
        hoverRoutine = null;
    }
}