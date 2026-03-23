using UnityEngine;
using UnityEngine.UI;

namespace HappyFamily.UI.Components
{
    public class CardPrefab : MonoBehaviour
    {
        [SerializeField]
        private RectTransform contentRoot;

        [SerializeField]
        private Image backgroundImage;

        public RectTransform ContentRoot => contentRoot;
        public Image BackgroundImage => backgroundImage;

        public bool IsValid => contentRoot != null && backgroundImage != null;

        public void Bind(RectTransform root, Image background)
        {
            contentRoot = root;
            backgroundImage = background;
        }

        public void SetBackgroundColor(Color color)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = color;
            }
        }
    }
}
