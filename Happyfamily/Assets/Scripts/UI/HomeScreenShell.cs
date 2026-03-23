using UnityEngine;

namespace HappyFamily.UI
{
    public class HomeScreenShell : MonoBehaviour
    {
        [field: SerializeField] public RectTransform ContentRoot { get; private set; }
        [field: SerializeField] public RectTransform TitleRoot { get; private set; }
        [field: SerializeField] public RectTransform ProgressRoot { get; private set; }
        [field: SerializeField] public RectTransform ActionRoot { get; private set; }

        public bool IsValid =>
            ContentRoot != null &&
            TitleRoot != null &&
            ProgressRoot != null &&
            ActionRoot != null;

        public void Bind(RectTransform contentRoot, RectTransform titleRoot, RectTransform progressRoot, RectTransform actionRoot)
        {
            ContentRoot = contentRoot;
            TitleRoot = titleRoot;
            ProgressRoot = progressRoot;
            ActionRoot = actionRoot;
        }
    }
}
