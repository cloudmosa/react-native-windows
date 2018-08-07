using Newtonsoft.Json.Linq;
using ReactNative.Touch;
using ReactNative.UIManager.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;

namespace ReactNative.UIManager
{
    /// <summary>
    /// Base class that should be suitable for the majority of subclasses of <see cref="IViewManager"/>.
    /// It provides support for base view properties such as opacity, etc.
    /// </summary>
    /// <typeparam name="TFrameworkElement">Type of framework element.</typeparam>
    /// <typeparam name="TLayoutShadowNode">Type of shadow node.</typeparam>
    public abstract class BaseViewManager<TFrameworkElement, TLayoutShadowNode> :
            ViewManager<TFrameworkElement, TLayoutShadowNode>
        where TFrameworkElement : FrameworkElement
        where TLayoutShadowNode : LayoutShadowNode
    {
        private readonly IDictionary<TFrameworkElement, Action<TFrameworkElement, Dimensions>> _transforms =
            new Dictionary<TFrameworkElement, Action<TFrameworkElement, Dimensions>>();

        private static readonly string kDragdropTagName = "dragdropTag";
        private static readonly string kDragdropDataName = "dragdropData";
        private static readonly string kDragEnterDisposableKey = "dragEnterSubscriber";
        private static readonly string kDragOverDisposableKey = "dragOverSubscriber";
        private static readonly string kDragLeaveDisposableKey = "dragLeaveSubscriber";
        private static readonly string kDragdropDataFormat = "Newtonsoft.Json.Linq.JObject";

        /// <summary>
        /// Set's the  <typeparamref name="TFrameworkElement"/> styling layout
        /// properties, based on the <see cref="JObject"/> map.
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <param name="transforms">The list of transforms.</param>
        [ReactProp("transform")]
        public void SetTransform(TFrameworkElement view, JArray transforms)
        {
            if (transforms == null)
            {
                if (_transforms.Remove(view))
                    ResetProjectionMatrix(view);
            }
            else
            {
                _transforms[view] = (v, d) => SetProjectionMatrix(v, d, transforms);
                var dimensions = GetDimensions(view);
                SetProjectionMatrix(view, dimensions, transforms);
            }
        }

        /// <summary>
        /// Sets the opacity of the <typeparamref name="TFrameworkElement"/>.
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <param name="opacity">The opacity value.</param>
        [ReactProp("opacity", DefaultDouble = 1.0)]
        public void SetOpacity(TFrameworkElement view, double opacity)
        {
            view.Opacity = opacity;
        }

        /// <summary>
        /// Sets the overflow property for the <typeparamref name="TFrameworkElement"/>.
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <param name="overflow">The overflow value.</param>
        [ReactProp("overflow")]
        public void SetOverflow(TFrameworkElement view, string overflow)
        {
            if (overflow == "hidden")
            {
                view.ClipToBounds = true;
            }
            else
            {
                view.ClipToBounds = false;
            }
        }

        /// <summary>
        /// Sets the z-index of the element.
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <param name="zIndex">The z-index.</param>
        [ReactProp("zIndex")]
        public void SetZIndex(TFrameworkElement view, int zIndex)
        {
            Canvas.SetZIndex(view, zIndex);
        }

        /// <summary>
        /// Sets the accessibility label of the element.
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <param name="label">The label.</param>
        [ReactProp("accessibilityLabel")]
        public void SetAccessibilityLabel(TFrameworkElement view, string label)
        {
            AutomationProperties.SetName(view, label ?? "");
        }

        // ToDo: SetAccessibilityLiveRegion - ReactProp("accessibilityLiveRegion")

        /// <summary>
        /// Sets the test ID, i.e., the automation ID.
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <param name="testId">The test ID.</param>
        [ReactProp("testID")]
        public void SetTestId(TFrameworkElement view, string testId)
        {
            AutomationProperties.SetAutomationId(view, testId ?? "");
        }

        /// <summary>
        /// Sets a tooltip for the view.
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <param name="tooltip">String to display in the tooltip.</param>
        [ReactProp("tooltip")]
        public void SetTooltip(TFrameworkElement view, string tooltip)
        {
            ToolTipService.SetToolTip(view, tooltip);
        }

        /// <summary>
        /// Sets if the target view is able to drag during drag-n-drop
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <param name="draggable">true to support drag-n-drop</param>
        [ReactProp("draggable")]
        public void SetDraggable(TFrameworkElement view, bool draggable)
        {
            if (view.GetDraggable() == draggable)
                return;

            if (draggable)
            {
                view.MouseDown += OnPointerPressed;
                view.MouseMove += OnPointerMove;
            }
            else
            {
                view.MouseDown -= OnPointerPressed;
                view.MouseMove -= OnPointerMove;
            }

            view.SetDraggable(draggable);
        }

        /// <summary>
        /// Sets if the target view supports for drop during drag-n-drop
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <param name="draggable">true to support drag-n-drop</param>
        [ReactProp("droppable")]
        public void SetDroppable(TFrameworkElement view, bool droppable)
        {
            if (view.AllowDrop == droppable)
                return;

            if (droppable)
            {
                view.Drop += OnDrop;
                view.AddDisposable(kDragEnterDisposableKey, RegisterDragEnterHandler(view));
                view.AddDisposable(kDragOverDisposableKey, RegisterDragOverHandler(view));
                view.AddDisposable(kDragLeaveDisposableKey, RegisterDragLeaveHandler(view));
            }
            else
            {
                view.Drop -= OnDrop;
                view.CleanDisposable(kDragEnterDisposableKey);
                view.CleanDisposable(kDragOverDisposableKey);
                view.CleanDisposable(kDragLeaveDisposableKey);
            }

            view.AllowDrop = droppable;
        }

        /// <summary>
        /// Sets the custom tag for view to support drag-n-drop
        /// Only if target view's dragdropTag and the dragging view's dragdropTag,
        /// the target view accepts to drop.
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <param name="dragdropTag">Custom meaningful string</param>
        [ReactProp("dragdropTag")]
        public void SetDragDropTag(TFrameworkElement view, string dragdropTag)
        {
            view.SetDragDropTag(dragdropTag);
        }

        /// <summary>
        /// Sets the custom data passing from drag item into drop item
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <param name="data">Custom data will pass from drag item during drag-n-drop</param>
        [ReactProp("dragdropData")]
        public void SetDragDropData(TFrameworkElement view, JObject data)
        {
            view.SetDragDropData(data);
        }

        /// <summary>
        /// Called when view is detached from view hierarchy and allows for
        /// additional cleanup by the <see cref="IViewManager"/> subclass.
        /// </summary>
        /// <param name="reactContext">The React context.</param>
        /// <param name="view">The view.</param>
        /// <remarks>
        /// Be sure to call this base class method to register for pointer
        /// entered and pointer exited events.
        /// </remarks>
        public override void OnDropViewInstance(ThemedReactContext reactContext, TFrameworkElement view)
        {
            view.Drop -= OnDrop;
            view.MouseEnter -= OnPointerEntered;
            view.MouseDown -= OnPointerPressed;
            view.MouseMove -= OnPointerMove;
            view.MouseLeave -= OnPointerExited;
            view.CleanDisposable();
            _transforms.Remove(view);
        }

        /// <summary>
        /// Subclasses can override this method to install custom event
        /// emitters on the given view.
        /// </summary>
        /// <param name="reactContext">The React context.</param>
        /// <param name="view">The view instance.</param>
        /// <remarks>
        /// Consider overriding this method if your view needs to emit events
        /// besides basic touch events to JavaScript (e.g., scroll events).
        ///
        /// Make sure you call the base implementation to ensure base pointer
        /// event handlers are subscribed.
        /// </remarks>
        protected override void AddEventEmitters(ThemedReactContext reactContext, TFrameworkElement view)
        {
            view.MouseEnter += OnPointerEntered;
            view.MouseLeave += OnPointerExited;
        }

        private void OnPointerEntered(object sender, MouseEventArgs e)
        {
            var view = (TFrameworkElement)sender;
            TouchHandler.OnPointerEntered(view, e);
        }

        private void OnPointerPressed(object sender, MouseEventArgs e)
        {
            var view = (TFrameworkElement)sender;
            view.SetMouseDownPoint(e.GetPosition(view));
        }

        private void OnPointerMove(object sender, MouseEventArgs e)
        {
            var view = (TFrameworkElement)sender;
            if (view != null && e.LeftButton == MouseButtonState.Pressed)
            {
                // [0] Only start drap-n-drop if moving exceed threshold
                var mouseDownPoint = view.GetMouseDownPoint();
                var currentPoint = e.GetPosition(view);
                if (Math.Abs(currentPoint.X - mouseDownPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(currentPoint.Y - mouseDownPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
                {
                    return;
                }

                // [1] Setup dragdropData
                var data = new JObject
                {
                    { kDragdropTagName, view.GetDragDropTag() },
                };
                var dragdropData = view.GetDragDropData();
                if (dragdropData != null)
                {
                    data[kDragdropDataName] = dragdropData;
                }

                // [2] Start drag-n-drop
                try
                {
                    var viewTag = view.GetTag();

                    var eventData = new JObject
                    {
                        { "timestamp",  Environment.TickCount },
                    };
                    view.GetReactContext()
                        .GetNativeModule<UIManagerModule>()
                        .EventDispatcher
                        .DispatchEvent(new DragDropEvent(viewTag, "topDragStart", eventData));

                    var dragdropEffects = DragDrop.DoDragDrop(view, data, DragDropEffects.Move);

                    eventData = new JObject
                    {
                        { "timestamp",  Environment.TickCount },
                        { "dropEffect", DragDropEffectsToString(dragdropEffects) },
                    };
                    view.GetReactContext()
                        .GetNativeModule<UIManagerModule>()
                        .EventDispatcher
                        .DispatchEvent(new DragDropEvent(viewTag, "topDragEnd", eventData));
                }
                catch (Exception ex)
                {
                    Debug.Print("DoDragDrop exception: " + ex.Message);
                }
            }
        }

        private void OnPointerExited(object sender, MouseEventArgs e)
        {
            var view = (TFrameworkElement)sender;
            TouchHandler.OnPointerExited(view, e);
        }

        private bool MakeDragData(TFrameworkElement view, DragEventArgs args, out JObject data)
        {
            if (args.Data.GetDataPresent(kDragdropDataFormat))
            {
                var dragData = (JObject)args.Data.GetData(kDragdropDataFormat);
                var locationPos = args.GetPosition(view);
                var timestamp = Environment.TickCount;
                // Simulate TouchHandler data structure, but without pageX/pageY.
                // We don't have ReactRootView here.
                // TODO(kudo): Consider move the logic into TouchHandler and we can then support pageX/pageY.
                data = new JObject
                {
                    { "locationX",  locationPos.X },
                    { "locationY",  locationPos.Y },
                    { "timestamp",  timestamp },
                };
                data.Merge(dragData);
                return true;
            }
            data = default(JObject);
            return false;
        }

        private bool IsTargetViewDroppable(TFrameworkElement view, JObject dragData)
        {
            var tag = dragData.Value<string>(kDragdropTagName);
            if (tag != null && tag != "" && tag == view.GetDragDropTag())
            {
                return true;
            }
            return false;
        }

        private IDisposable RegisterDragEnterHandler(TFrameworkElement view)
        {
            return Observable.FromEventPattern<DragEventHandler, DragEventArgs>(
                h => view.DragEnter += h,
                h => view.DragEnter -= h)
                .Subscribe(e =>
                {
                    var args = e.EventArgs;
                    view.SetShouldSendDragOver(true);

                    JObject data;
                    if (!MakeDragData(view, args, out data))
                        return;

                    var viewTag = view.GetTag();

                    view.GetReactContext()
                        .GetNativeModule<UIManagerModule>()
                        .EventDispatcher
                        .DispatchEvent(new DragDropEvent(viewTag, "topDragEnter", data));
                });
        }

        /// <summary>
        /// Setup DragOver handler for RN.
        ///
        /// We need special handler that the WPF's DragOver will send events frequently,
        /// even to block RN bridge even we don't move mouse.
        /// This handler will only send event to RN if
        ///     1. There is mouse moving.
        ///     2. Throttle control for 100ms
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <returns>Resource disposable for cleanup</returns>
        private IDisposable RegisterDragOverHandler(TFrameworkElement view)
        {
            return Observable.FromEventPattern<DragEventHandler, DragEventArgs>(
                h => view.DragOver += h,
                h => view.DragOver -= h)
                // [0] Handle the DragEventArgs to setup if target view is droppable
                .Select(e =>
                {

                    var args = e.EventArgs;
                    args.Effects = DragDropEffects.None;
                    args.Handled = true;

                    JObject data;
                    if (!MakeDragData(view, args, out data))
                        return null;
                    if (IsTargetViewDroppable(view, data))
                    {
                        args.Effects = DragDropEffects.Move;
                    }
                    return Tuple.Create<EventPattern<DragEventArgs>, JObject>(e, data);
                })
                .Where(tuple => tuple != null)
                // [1] Send event only if position has changed
                .DistinctUntilChanged(tuple => tuple.Item1.EventArgs.GetPosition(view))
                // [2] Throttle for 100ms
                .Sample(TimeSpan.FromMilliseconds(100), DispatcherScheduler.Instance)
                // [3] Send event to JS
                .Subscribe(tuple =>
                {
                    var data = tuple.Item2;
                    if (!view.GetShouldSendDragOver())
                        return;

                    view.GetReactContext()
                        .GetNativeModule<UIManagerModule>()
                        .EventDispatcher
                        .DispatchEvent(new DragDropEvent(view.GetTag(), "topDragOver", data));
                });
        }

        private void OnDrop(object sender, DragEventArgs args)
        {
            var view = (TFrameworkElement)sender;
            view.SetShouldSendDragOver(false);

            JObject data;
            if (!MakeDragData(view, args, out data))
                return;

            view.GetReactContext()
                .GetNativeModule<UIManagerModule>()
                .EventDispatcher
                .DispatchEvent(new DragDropEvent(view.GetTag(), "topDrop", data));
        }

        private IDisposable RegisterDragLeaveHandler(TFrameworkElement view)
        {
            return Observable.FromEventPattern<DragEventHandler, DragEventArgs>(
                h => view.DragLeave += h,
                h => view.DragLeave -= h)
                .Subscribe(e =>
                {
                    var args = e.EventArgs;
                    view.SetShouldSendDragOver(false);

                    JObject data;
                    if (!MakeDragData(view, args, out data))
                        return;

                    var viewTag = view.GetTag();

                    view.GetReactContext()
                        .GetNativeModule<UIManagerModule>()
                        .EventDispatcher
                        .DispatchEvent(new DragDropEvent(viewTag, "topDragLeave", data));
                });
        }

        /// <summary>
        /// Sets the dimensions of the view.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="dimensions">The dimensions.</param>
        public override void SetDimensions(TFrameworkElement view, Dimensions dimensions)
        {
            Action<TFrameworkElement, Dimensions> applyTransform;
            if (_transforms.TryGetValue(view, out applyTransform))
            {
                applyTransform(view, dimensions);
            }
            base.SetDimensions(view, dimensions);
        }

        /// <summary>
        /// Sets the shadow color of the <typeparamref name="TFrameworkElement"/>.
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <param name="color">The shadow color.</param>
        [ReactProp("shadowColor", CustomType = "Color")]
        public void SetShadowColor(TFrameworkElement view, uint? color)
        {
            var effect = (DropShadowEffect)view.Effect ?? new DropShadowEffect();
            effect.Color = ColorHelpers.Parse(color.Value);
            view.Effect = effect;
        }

        /// <summary>
        /// Sets the shadow offset of the <typeparamref name="TFrameworkElement"/>.
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <param name="offset">The shadow offset.</param>
        [ReactProp("shadowOffset")]
        public void SetShadowOffset(TFrameworkElement view, JObject offset)
        {
            var effect = (DropShadowEffect)view.Effect ?? new DropShadowEffect();
            var deltaX = offset.Value<double>("width");
            var deltaY = offset.Value<double>("height");
            var angle = Math.Atan2(deltaY, deltaX) * (180 / Math.PI);
            var distance = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
            effect.Direction = angle;
            effect.ShadowDepth = distance;
            view.Effect = effect;
        }

        /// <summary>
        /// Sets the shadow opacity of the <typeparamref name="TFrameworkElement"/>.
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <param name="opacity">The shadow opacity.</param>
        [ReactProp("shadowOpacity")]
        public void SetShadowOpacity(TFrameworkElement view, double opacity)
        {
            var effect = (DropShadowEffect)view.Effect ?? new DropShadowEffect();
            effect.Opacity = opacity;
            view.Effect = effect;
        }

        /// <summary>
        /// Sets the shadow radius of the <typeparamref name="TFrameworkElement"/>.
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <param name="radius">The shadow radius.</param>
        [ReactProp("shadowRadius")]
        public void SetShadowRadius(TFrameworkElement view, double radius)
        {
            var effect = (DropShadowEffect)view.Effect ?? new DropShadowEffect();
            effect.BlurRadius = radius;
            view.Effect = effect;
        }

        private static void SetProjectionMatrix(TFrameworkElement view, Dimensions dimensions, JArray transforms)
        {
            var transformMatrix = TransformHelper.ProcessTransform(transforms);

            var translateMatrix = Matrix3D.Identity;
            var translateBackMatrix = Matrix3D.Identity;
            if (!double.IsNaN(dimensions.Width))
            {
                translateMatrix.OffsetX = -dimensions.Width / 2;
                translateBackMatrix.OffsetX = dimensions.Width / 2;
            }

            if (!double.IsNaN(dimensions.Height))
            {
                translateMatrix.OffsetY = -dimensions.Height / 2;
                translateBackMatrix.OffsetY = dimensions.Height / 2;
            }

            var projectionMatrix = translateMatrix * transformMatrix * translateBackMatrix;
            ApplyProjection(view, projectionMatrix);
        }

        private static void ApplyProjection(TFrameworkElement view, Matrix3D projectionMatrix)
        {
            if (!projectionMatrix.IsAffine)
            {
                throw new NotImplementedException("ReactNative.Net46 does not support non-affine transformations");
            }

            if (IsSimpleTranslationOnly(projectionMatrix))
            {
                ResetProjectionMatrix(view);
                var transform = new MatrixTransform();
                var matrix = transform.Matrix;
                matrix.OffsetX = projectionMatrix.OffsetX;
                matrix.OffsetY = projectionMatrix.OffsetY;
                transform.Matrix = matrix;
                view.RenderTransform = transform;
            }
            else
            {
                var transform = new MatrixTransform(projectionMatrix.M11,
                    projectionMatrix.M12,
                    projectionMatrix.M21,
                    projectionMatrix.M22,
                    projectionMatrix.OffsetX,
                    projectionMatrix.OffsetY);

                view.RenderTransform = transform;
            }
        }

        private static bool IsSimpleTranslationOnly(Matrix3D matrix)
        {
            // Matrix3D is a struct and passed-by-value. As such, we can modify
            // the values in the matrix without affecting the caller.
            matrix.OffsetX = matrix.OffsetY = 0;
            return matrix.IsIdentity;
        }

        private static void ResetProjectionMatrix(TFrameworkElement view)
        {
            var transform = view.RenderTransform;
            var matrixTransform = transform as MatrixTransform;
            if (transform != null && matrixTransform == null)
            {
                throw new InvalidOperationException("Unknown projection set on framework element.");
            }

            view.RenderTransform = null;
        }

        private static string DragDropEffectsToString(DragDropEffects dragDropEffects)
        {
            if (dragDropEffects == DragDropEffects.Copy)
            {
                return "copy";
            }
            if (dragDropEffects == DragDropEffects.Move)
            {
                return "move";
            }
            if (dragDropEffects == DragDropEffects.Link)
            {
                return "link";
            }
            return "none";
        }
    }
}
