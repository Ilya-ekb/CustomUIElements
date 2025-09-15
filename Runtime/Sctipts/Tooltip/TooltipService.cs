using System;
using System.Collections.Generic;
using System.Linq;
using CustomUIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ILP.Tooltips
{
    public sealed class TooltipService : IDisposable
    {
        private readonly VisualElement root;
        private readonly TooltipSettings settings;
        private readonly VisualElement host;
        private readonly Label title;
        private readonly Label label;
        private readonly Dictionary<VisualElement, TooltipBinding> bindings = new();
        private VisualElement currentTarget;
        private IVisualElementScheduledItem showItem;
        private IVisualElementScheduledItem hideItem;
        private long pointerDownTicks;

        private TooltipService(VisualElement root, TooltipSettings settings)
        {
            this.root = root;
            this.settings = settings ?? ScriptableObject.CreateInstance<TooltipSettings>();

            host = new VisualElement { name = "ilp-tooltip" };
            if (this.settings.styleSheet)
                host.styleSheets.Add(this.settings.styleSheet);
            host.AddToClassList("hidden");
            host.pickingMode = PickingMode.Ignore;
            label = new Label { name = "ilp-tooltip-label" };
            title = new Label { name = "ilp-tooltip-title" };
            label.enableRichText = title.enableRichText = this.settings.enableRichText;
            host.Add(title);
            host.Add(label);
            root.Add(host);

            host.style.maxWidth = this.settings.maxWidth;
        }

        public static TooltipService ForPanel(VisualElement anyElementOnPanel, TooltipSettings settings = null)
        {
            var root = anyElementOnPanel.panel?.visualTree;
            if (root == null) throw new InvalidOperationException("Element is not attached to a panel");
            const string key = "__ilp_tooltip_service";
            if (root.userData is not Dictionary<string, object> data)
            {
                data = new Dictionary<string, object>();
                root.userData = data;
            }

            if (data.TryGetValue(key, out var obj) && obj is TooltipService svc) return svc;
            svc = new TooltipService(root, settings);
            data[key] = svc;

            return svc;
        }

        public void Dispose()
        {
            host.RemoveFromHierarchy();
        }

        public void Attach(VisualElement target, Func<string> textProvider)
        {
            if (target == null) return;
            var binding = new TooltipBinding { Target = target, TextProvider = textProvider };
            bindings[target] = binding;
            target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<FocusInEvent>(OnFocusIn);
            target.RegisterCallback<FocusOutEvent>(OnFocusOut);
            target.RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        public void Detach(VisualElement target)
        {
            if (target == null) return;
            bindings.Remove(target);
            target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<FocusInEvent>(OnFocusIn);
            target.RegisterCallback<FocusOutEvent>(OnFocusOut);
            target.RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        private void OnDetach(DetachFromPanelEvent e)
        {
            var ve = (VisualElement)e.target;
            Detach(ve);
            if (ve == currentTarget) HideImmediate();
        }

        private void OnPointerEnter(PointerEnterEvent e)
        {
            ScheduleShow((VisualElement)e.target);
            UpdatePositionForTarget((VisualElement)e.target);
        }

        private void OnPointerMove(PointerMoveEvent e)
        {
            if (currentTarget != null) UpdatePositionForTarget(currentTarget);
        }

        private void OnPointerLeave(PointerLeaveEvent e)
        {
            ScheduleHide();
        }

        private void OnPointerDown(PointerDownEvent e)
        {
            pointerDownTicks = DateTime.UtcNow.Ticks;
        }

        private void OnPointerUp(PointerUpEvent e)
        {
            var dt = new TimeSpan(DateTime.UtcNow.Ticks - pointerDownTicks).TotalSeconds;
            if (dt >= settings.longPressTime)
            {
                ScheduleShow((VisualElement)e.target);
                UpdatePositionForTarget((VisualElement)e.target);
            }
        }

        private void OnFocusIn(FocusInEvent e)
        {
            var ve = (VisualElement)e.target;
            ScheduleShow(ve);
            UpdatePositionForTarget(ve);
        }

        private void OnFocusOut(FocusOutEvent e)
        {
            ScheduleHide();
        }

        private void ScheduleShow(VisualElement target)
        {
            hideItem?.Pause();
            currentTarget = target;
            showItem ??= root.schedule.Execute(ShowNow);
            showItem.ExecuteLater((long)(settings.showDelay * 1000));
        }

        private void ScheduleHide()
        {
            showItem?.Pause();
            hideItem ??= root.schedule.Execute(HideImmediate);
            hideItem.ExecuteLater((long)(settings.hideDelay * 1000));
        }

        private void ShowNow()
        {
            if (currentTarget is null || !bindings.TryGetValue(currentTarget, out var b)) return;
            var tooltipValue = b.TextProvider?.Invoke() ?? string.Empty;
            if (string.IsNullOrEmpty(tooltipValue)) return;
            var data = tooltipValue.Split('|');
            title.text = data.First();
            label.text = data.Last();
            host.RemoveFromClassList("hidden");
            host.style.display = DisplayStyle.Flex;

            root.schedule.Execute(() => UpdatePositionForTarget(currentTarget));
        }

        private void HideImmediate()
        {
            currentTarget = null;
            host.AddToClassList("hidden");
            host.style.display = DisplayStyle.None;
        }


        private Vector2 GetTooltipSize()
        {
            float w = host.resolvedStyle.width;
            float h = host.resolvedStyle.height;
            if (float.IsNaN(w) || w <= 0f || float.IsNaN(h) || h <= 0f)
            {
                var wb = host.worldBound;
                w = wb.width;
                h = wb.height;
            }

            return new Vector2(w, h);
        }

        private void UpdatePositionForTarget(VisualElement target)
        {
            var panelRect = root.worldBound;
            var tRect = target.worldBound;
            var tipSize = GetTooltipSize();

            var candidates = new (TailedCutoutElement.TailSide corner, Vector2 pos)[]
            {
                (TailedCutoutElement.TailSide.TopRight,
                    new Vector2(tRect.xMax + settings.offset.x, tRect.yMin - tipSize.y - settings.offset.y)),
                (TailedCutoutElement.TailSide.TopLeft,
                    new Vector2(tRect.xMin - tipSize.x - settings.offset.x,
                        tRect.yMin - tipSize.y - settings.offset.y)),
                (TailedCutoutElement.TailSide.BottomRight,
                    new Vector2(tRect.xMax + settings.offset.x, tRect.yMax + settings.offset.y)),
                (TailedCutoutElement.TailSide.BottomLeft,
                    new Vector2(tRect.xMin - tipSize.x - settings.offset.x, tRect.yMax + settings.offset.y)),
            };

            float Score(Vector2 p)
            {
                float overflowX = Math.Max(0, panelRect.xMin - p.x) + Math.Max(0, (p.x + tipSize.x) - panelRect.xMax);
                float overflowY = Math.Max(0, panelRect.yMin - p.y) + Math.Max(0, (p.y + tipSize.y) - panelRect.yMax);
                return overflowX + overflowY;
            }

            var best = candidates[0];
            float bestScore = Score(best.pos);
            for (int i = 1; i < candidates.Length; i++)
            {
                float s = Score(candidates[i].pos);
                if (s < bestScore)
                {
                    best = candidates[i];
                    bestScore = s;
                }
            }

            var clampedX = Mathf.Clamp(best.pos.x, panelRect.xMin, panelRect.xMax - tipSize.x);
            var clampedY = Mathf.Clamp(best.pos.y, panelRect.yMin, panelRect.yMax - tipSize.y);
            var world = new Vector2(clampedX, clampedY);
            var local = root.WorldToLocal(world);
            host.style.left = local.x;
            host.style.top = local.y;
        }

        private class TooltipBinding
        {
            public VisualElement Target;
            public Func<string> TextProvider;
        }
    }
}