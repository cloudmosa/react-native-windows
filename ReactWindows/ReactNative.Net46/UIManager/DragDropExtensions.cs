using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ReactNative.UIManager
{
    public static class DragDropExtensions
    {
        public static readonly DependencyProperty DragDropTagProperty =
            DependencyProperty.RegisterAttached(
                "DragDropTag",
                typeof(string),
                typeof(DragDropExtensions),
                new FrameworkPropertyMetadata(""));

        public static readonly DependencyProperty DragDropDataProperty =
            DependencyProperty.RegisterAttached(
                "DragDropData",
                typeof(JObject),
                typeof(DragDropExtensions),
                new FrameworkPropertyMetadata(default(JObject)));

        public static readonly DependencyProperty DraggableProperty =
            DependencyProperty.RegisterAttached(
                "Draggable",
                typeof(bool),
                typeof(DragDropExtensions),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty ShouldSendDragOverProperty =
            DependencyProperty.RegisterAttached(
            "ShouldSendDragOver",
            typeof(bool),
            typeof(DragDropExtensions),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty DisposablesProperty =
            DependencyProperty.RegisterAttached(
                "Disposables",
                typeof(Dictionary<string, IDisposable>),
                typeof(DragDropExtensions),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty MouseDownPointProperty =
            DependencyProperty.RegisterAttached(
                "MouseDownPoint",
                typeof(Point),
                typeof(DragDropExtensions),
                new FrameworkPropertyMetadata(default(Point)));

        public static void SetDragDropTag(this DependencyObject element, string value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            element.SetValue(DragDropTagProperty, value);
        }

        public static string GetDragDropTag(this DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return (string)element.GetValue(DragDropTagProperty);
        }

        public static void SetDragDropData(this DependencyObject element, JObject value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            element.SetValue(DragDropDataProperty, value);
        }

        public static JObject GetDragDropData(this DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return (JObject)element.GetValue(DragDropDataProperty);
        }

        public static void SetDraggable(this DependencyObject element, bool value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            element.SetValue(DraggableProperty, value);
        }

        public static bool GetDraggable(this DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return (bool)element.GetValue(DraggableProperty);
        }

        public static void SetShouldSendDragOver(this DependencyObject element, bool value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            element.SetValue(ShouldSendDragOverProperty, value);
        }

        public static bool GetShouldSendDragOver(this DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return (bool)element.GetValue(ShouldSendDragOverProperty);
        }

        public static void AddDisposable(this DependencyObject element, string key, IDisposable disposable)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var disposables = (Dictionary<string, IDisposable>)element.GetValue(DisposablesProperty);
            if (disposables == null)
            {
                disposables = new Dictionary<string, IDisposable>();
                element.SetValue(DisposablesProperty, disposables);
            }
            disposables[key] = disposable;
        }

        public static void CleanDisposable(this DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var disposables = (Dictionary<string, IDisposable>)element.GetValue(DisposablesProperty);
            if (disposables != null)
            {
                foreach (var disposable in disposables.Values)
                {
                    disposable.Dispose();
                }
                element.SetValue(DisposablesProperty, null);
            }
        }

        public static void CleanDisposable(this DependencyObject element, string key)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var disposables = (Dictionary<string, IDisposable>)element.GetValue(DisposablesProperty);
            if (disposables != null && disposables.ContainsKey(key))
            {
                var disposable = disposables[key];
                disposable.Dispose();
                disposables.Remove(key);
            }
        }

        public static void SetMouseDownPoint(this DependencyObject element, Point value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            element.SetValue(MouseDownPointProperty, value);
        }

        public static Point GetMouseDownPoint(this DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return (Point)element.GetValue(MouseDownPointProperty);
        }
    }
}
