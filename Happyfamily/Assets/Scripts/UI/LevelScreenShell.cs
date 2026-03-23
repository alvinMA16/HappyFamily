using UnityEngine;

namespace HappyFamily.UI
{
    public class LevelScreenShell : MonoBehaviour
    {
        [field: SerializeField] public RectTransform Root { get; private set; }
        [field: SerializeField] public RectTransform HeaderRoot { get; private set; }
        [field: SerializeField] public RectTransform BoardRoot { get; private set; }
        [field: SerializeField] public RectTransform ToolbarRoot { get; private set; }

        public bool IsValid =>
            Root != null &&
            HeaderRoot != null &&
            BoardRoot != null &&
            ToolbarRoot != null;

        public void Bind(RectTransform root, RectTransform headerRoot, RectTransform boardRoot, RectTransform toolbarRoot)
        {
            Root = root;
            HeaderRoot = headerRoot;
            BoardRoot = boardRoot;
            ToolbarRoot = toolbarRoot;
        }
    }
}
