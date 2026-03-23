using UnityEngine;

namespace HappyFamily.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class MobileViewportFitter : MonoBehaviour
    {
        [SerializeField] private Vector2 referenceSize = new Vector2(750f, 1334f);
        [SerializeField] private float margin = 24f;

        private RectTransform targetRect;
        private RectTransform parentRect;

        public void Initialize(RectTransform parent, Vector2 targetReference, float targetMargin)
        {
            parentRect = parent;
            referenceSize = targetReference;
            margin = targetMargin;
            targetRect = GetComponent<RectTransform>();
            Apply();
        }

        private void Awake()
        {
            targetRect = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            Apply();
        }

        private void OnRectTransformDimensionsChange()
        {
            Apply();
        }

        private void Apply()
        {
            if (targetRect == null || parentRect == null)
            {
                return;
            }

            var parentSize = parentRect.rect.size;
            var availableWidth = Mathf.Max(0f, parentSize.x - margin * 2f);
            var availableHeight = Mathf.Max(0f, parentSize.y - margin * 2f);
            var targetAspect = referenceSize.x / referenceSize.y;
            var availableAspect = availableHeight <= 0f ? targetAspect : availableWidth / availableHeight;

            float width;
            float height;
            if (availableAspect > targetAspect)
            {
                height = availableHeight;
                width = height * targetAspect;
            }
            else
            {
                width = availableWidth;
                height = width / targetAspect;
            }

            targetRect.anchorMin = new Vector2(0.5f, 0.5f);
            targetRect.anchorMax = new Vector2(0.5f, 0.5f);
            targetRect.pivot = new Vector2(0.5f, 0.5f);
            targetRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            targetRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            targetRect.anchoredPosition = Vector2.zero;
        }
    }
}
