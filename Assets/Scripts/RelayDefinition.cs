using UnityEngine;

namespace RelayYard
{
    [CreateAssetMenu(menuName = "Relay Yard/Relay Definition", fileName = "RelayDefinition")]
    public sealed class RelayDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Power Relay";
        [SerializeField] private Color activeColor = new Color(0.22f, 0.95f, 0.78f);

        public string DisplayName => displayName;
        public Color ActiveColor => activeColor;

#if UNITY_EDITOR
        public void EditorConfigure(string nameToShow, Color color)
        {
            displayName = nameToShow;
            activeColor = color;
        }
#endif
    }
}
