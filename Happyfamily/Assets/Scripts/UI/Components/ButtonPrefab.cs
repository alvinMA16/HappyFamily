using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HappyFamily.UI.Components
{
    public class ButtonPrefab : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private TextMeshProUGUI label;

        [SerializeField]
        private LayoutElement layoutElement;

        public Button Button => button;
        public Image BackgroundImage => backgroundImage;
        public TextMeshProUGUI Label => label;
        public LayoutElement LayoutElement => layoutElement;

        public bool IsValid => button != null && backgroundImage != null && label != null && layoutElement != null;

        public void Bind(Button btn, Image background, TextMeshProUGUI text, LayoutElement layout)
        {
            button = btn;
            backgroundImage = background;
            label = text;
            layoutElement = layout;
        }

        public void Configure(string text, Action onClick, Color backgroundColor, int height, int width = -1)
        {
            if (label != null)
            {
                label.text = text;
                label.font = HappyFamilyUiThemeProvider.GetResolvedPrimaryFont();
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClick?.Invoke());

                var colors = button.colors;
                colors.normalColor = backgroundColor;
                colors.highlightedColor = backgroundColor * 1.05f;
                colors.pressedColor = backgroundColor * 0.92f;
                colors.selectedColor = backgroundColor;
                colors.disabledColor = new Color(0.70f, 0.70f, 0.70f, 1f);
                button.colors = colors;
            }

            if (layoutElement != null)
            {
                layoutElement.preferredHeight = height;
                if (width > 0)
                {
                    layoutElement.preferredWidth = width;
                }
            }
        }
    }
}
