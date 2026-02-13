using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPulseHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float scaleAmount = 0.08f;   // Kuinka paljon kasvaa
    public float speed = 6f;            // Kuinka nopeasti reagoi

    Vector3 baseScale;
    bool hovering = false;

    void Awake()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        Vector3 targetScale = hovering ? baseScale * (1f + scaleAmount) : baseScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
}
