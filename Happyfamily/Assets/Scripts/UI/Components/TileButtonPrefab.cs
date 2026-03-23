using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HappyFamily.UI.Components
{
    public class TileButtonPrefab : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private TextMeshProUGUI label;

        public Button Button => button;
        public Image BackgroundImage => backgroundImage;
        public TextMeshProUGUI Label => label;

        public bool IsValid => button != null && backgroundImage != null && label != null;

        public void Bind(Button btn, Image background, TextMeshProUGUI text)
        {
            button = btn;
            backgroundImage = background;
            label = text;
        }

        public void Configure(string text, Action onClick, Color backgroundColor, Color textColor)
        {
            if (label != null)
            {
                label.text = text;
                label.color = textColor;
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
                colors.disabledColor = new Color(0.78f, 0.80f, 0.78f, 1f);
                button.colors = colors;
            }
        }

        public void UpdateVisual(string text, bool isInteractable, Color backgroundColor)
        {
            if (label != null)
            {
                label.text = text;
            }

            if (button != null)
            {
                button.interactable = isInteractable;

                var colors = button.colors;
                colors.normalColor = backgroundColor;
                colors.highlightedColor = backgroundColor;
                colors.selectedColor = backgroundColor;
                button.colors = colors;
            }
        }
    }
}
