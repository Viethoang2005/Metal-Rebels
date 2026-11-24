using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MetalRebels.UI
{
    public class MenuButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color normalColor = new Color(0.09f, 0.13f, 0.20f, 1f);
        [SerializeField] private Color hoverColor = new Color(0.15f, 0.22f, 0.34f, 1f);
        [SerializeField] private float hoverScale = 1.03f;
        [SerializeField] private float shiftX = 12f;

        private RectTransform rectTransform;
        private Vector3 targetScale = Vector3.one;
        private Vector2 baseAnchoredPos;
        private Vector2 targetAnchoredPos;
        private Color targetColor;
        private bool initialized;

        public void SetupColors(Image bg, Color normal, Color hover)
        {
            backgroundImage = bg;
            normalColor = normal;
            hoverColor = hover;
            targetColor = normal;
            if (backgroundImage != null) backgroundImage.color = normal;
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (backgroundImage == null) backgroundImage = GetComponent<Image>();
            targetColor = normalColor;
        }

        private void Start()
        {
            baseAnchoredPos = rectTransform.anchoredPosition;
            targetAnchoredPos = baseAnchoredPos;
            initialized = true;
        }

        private void Update()
        {
            if (!initialized) return;
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.unscaledDeltaTime * 14f);
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetAnchoredPos, Time.unscaledDeltaTime * 14f);
            if (backgroundImage != null)
            {
                backgroundImage.color = Color.Lerp(backgroundImage.color, targetColor, Time.unscaledDeltaTime * 14f);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            targetScale = Vector3.one * hoverScale;
            targetAnchoredPos = baseAnchoredPos + new Vector2(shiftX, 0f);
            targetColor = hoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetScale = Vector3.one;
            targetAnchoredPos = baseAnchoredPos;
            targetColor = normalColor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            targetScale = Vector3.one * 0.98f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            targetScale = Vector3.one * hoverScale;
        }
    }
}