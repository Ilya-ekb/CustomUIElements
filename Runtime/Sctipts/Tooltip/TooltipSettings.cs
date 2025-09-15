using System.Collections.Generic;
using EditorExtensions;
using TriInspector;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace ILP.Tooltips
{
    [CreateAssetMenu(fileName = "TooltipSettings", menuName = "ILP/Tooltips/Settings")]
    public class TooltipSettings : ScriptableObject
    {
        [Header("Timing (seconds)")] [Min(0f)] public float showDelay = 0.1f;
        [Min(0f)] public float hideDelay = 0.00f;
        [Min(0f)] public float longPressTime = 0.5f;
        
        [Title(nameof(StyleSheet))]
        [DropdownWithSearch(nameof(GetAllStyleSheets))]
        public StyleSheet styleSheet;
        public Vector2 offset = new(12, 12);
        public int fontSize = 12;
        public float maxWidth = 220;
        public bool enableRichText = true;

        protected virtual IEnumerable<TriDropdownItem<StyleSheet>> GetAllStyleSheets()
        {
            var list = new TriDropdownList<StyleSheet>();
#if UNITY_EDITOR
            var sheets = Utils.Resources.FindObjectsOfType<StyleSheet>(ResourceType.ScriptableObject);
            foreach (var sheet in sheets)
            {
                list.Add(sheet.name, sheet);    
            }
#endif
            return list;
        }
    }
}