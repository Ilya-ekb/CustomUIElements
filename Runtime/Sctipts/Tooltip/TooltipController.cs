using System;
using UnityEngine.UIElements;

namespace ILP.Tooltips {
    public static class TooltipController {
        private static TooltipService _service;

        public static void Attach(VisualElement element, string text, TooltipSettings settings = null) {
            Attach(element, () => text, settings);
        }
        public static void Attach(VisualElement element, Func<string> textProvider, TooltipSettings settings = null) {
            _service = TooltipService.ForPanel(element, settings);
            _service.Attach(element, textProvider);
        }

        public static void Detach<T>(T element) where T : VisualElement
        {
            _service.Detach(element);
        }
    }
}