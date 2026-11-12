using UnityEngine;

namespace RelayYard
{
    // Two distinct asset instances share one data type, as in Week 7.
    [CreateAssetMenu(menuName = "Relay Yard/Switch Style", fileName = "SwitchStyle")]
    public sealed class SwitchStyle : ScriptableObject
    {
        [SerializeField] private string styleName = "Cyan circuit";
        [SerializeField] private Color offColor = new Color(0.13f, 0.24f, 0.33f);
        [SerializeField] private Color onColor = new Color(0.42f, 0.95f, 0.88f);
        public string StyleName => styleName;
        public Color OffColor => offColor;
        public Color OnColor => onColor;

#if UNITY_EDITOR
        public void EditorConfigure(string label, Color off, Color on)
        {
            styleName = label;
            offColor = off;
            onColor = on;
        }
#endif
    }
}
