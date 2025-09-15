using System;
using UnityEngine.UIElements;

namespace ILP.Tooltips {
    public static class TooltipExtensions {
        public static T WithTooltip<T>(this T element, string text, TooltipSettings settings) where T : VisualElement {
            TooltipController.Attach(element, text, settings);
            return element;
        }
        public static T WithTooltip<T>(this T element, Func<string> textProvider, TooltipSettings settings) where T : VisualElement {
            TooltipController.Attach(element, textProvider, settings);
            return element;
        }

        public static void RemoveTooltip<T>(this T element) where T : VisualElement
        {
            TooltipController.Detach(element);
        }
    }
}