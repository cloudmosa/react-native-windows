using ReactNative.UIManager;
using System.Threading;
#if WINDOWS_UWP
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
#endif

namespace ReactNative.Views.TextInput
{
    class ReactTextBox : TextBox
    {
        private int _eventCount;
        private string _placeholderText = string.Empty;
        private Brush _textColor;
        private Brush _placeholderColor;
        private bool _placeholderActive;
        private bool _selectionChangedSubscribed;
        private bool _sizeChangedSubscribed;

        public ReactTextBox()
        {
            PlaceholderActive = false;
            TextColor = new SolidColorBrush(Colors.Black);
            PlaceholderColor = new SolidColorBrush(Colors.LightGray);
        }

        public int CurrentEventCount
        {
            get
            {
                return _eventCount;
            }
        }

        public bool ClearTextOnFocus
        {
            get;
            set;
        }

        public bool SelectTextOnFocus
        {
            get;
            set;
        }

        public bool SelectTextOnUserFocus
        {
            get;
            set;
        }

        public bool PlaceholderActive
        {
            get { return _placeholderActive; }
            set {
                _placeholderActive = value;
                if (_placeholderActive)
                    this.Foreground = PlaceholderColor;
                else
                    this.Foreground = TextColor;
            }
        }

        public Brush TextColor
        {
            get { return _textColor; }
            set
            {
                _textColor = value;
                if (!PlaceholderActive)
                    this.Foreground = _textColor;
            }
        }

        public Brush PlaceholderColor
        {
            get { return _placeholderColor; }
            set {
                _placeholderColor = value;
                if (PlaceholderActive)
                    this.Foreground = _placeholderColor;
            }
        }

        public string PlaceholderText
        {
            get { return _placeholderText; }
            set
            {
                _placeholderText = value;
                if (!string.IsNullOrEmpty(_placeholderText))
                {
                    this.PlaceholderActive = true;
                    this.Text = value;
                }
            }
        }

        public bool OnSelectionChange
        {
            get
            {
                return _selectionChangedSubscribed;
            }
            set
            {
                if (value != _selectionChangedSubscribed)
                {
                    _selectionChangedSubscribed = value;
                    if (_selectionChangedSubscribed)
                    {
                        SelectionChanged += OnSelectionChanged;
                    }
                    else
                    {
                        SelectionChanged -= OnSelectionChanged;
                    }
                }
            }
        }

        public bool OnContentSizeChange
        {
            get
            {
                return _sizeChangedSubscribed;
            }
            set
            {
                if (value != _sizeChangedSubscribed)
                {
                    _sizeChangedSubscribed = value;
                    if (_sizeChangedSubscribed)
                    {
                        SizeChanged += OnSizeChanged;
                    }
                    else
                    {
                        SizeChanged -= OnSizeChanged;
                    }
                }
            }
        }

        public bool AutoGrow
        {
            get;
            set;
        }

        public bool DimensionsUpdated
        {
            get;
            set;
        }

        public int IncrementEventCount()
        {
            return Interlocked.Increment(ref _eventCount);
        }

        public string Text {
            get
            {
                if (this.PlaceholderActive && base.Text == this.PlaceholderText)
                    return string.Empty;
                return base.Text;
            }
            set {
                bool isFocused = false;
#if WINDOWS_UWP
                isFocused = this.FocusState != FocusState.Unfocused;
#else
                isFocused = this.IsFocused;
#endif
                if (this.PlaceholderActive && !isFocused) {
                    if (string.IsNullOrEmpty(value))
                        return;
                    else if (value != this.PlaceholderText)
                        PlaceholderActive = false;
                }
                base.Text = value;
            }
        }

        private bool _setFocusByCommand = false;

        public void FocusByCommand()
        {
            _setFocusByCommand = true;
#if WINDOWS_UWP
            Focus(FocusState.Programmatic);
#else
            Focus();
#endif
            _setFocusByCommand = false;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (this.ClearTextOnFocus)
            {
                this.Text = "";
            }

            if (this.PlaceholderActive)
            {
                this.PlaceholderActive = false;
                this.Text = string.Empty;
                this.Select(0, 0);
            }
            else if (SelectTextOnFocus ||
                (!_setFocusByCommand && SelectTextOnUserFocus))
            {
                this.Select(0, this.Text.Length);
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            if (string.IsNullOrEmpty(this.Text) && !string.IsNullOrEmpty(this.PlaceholderText))
            {
                this.PlaceholderActive = true;
                this.Text = this.PlaceholderText;
            }
        }

#if !WINDOWS_UWP
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (!this.IsKeyboardFocusWithin && (SelectTextOnFocus || SelectTextOnUserFocus))
            {
                e.Handled = true;
                this.Focus();
            }
        }

        protected override void OnPreviewTouchDown(TouchEventArgs e)
        {
            base.OnPreviewTouchDown(e);
            if (!this.IsKeyboardFocusWithin && (SelectTextOnFocus || SelectTextOnUserFocus))
            {
                e.Handled = true;
                this.Focus();
            }
        }
#endif

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DimensionsUpdated)
            {
                DimensionsUpdated = false;
                return;
            }

            this.GetReactContext()
                .GetNativeModule<UIManagerModule>()
                .EventDispatcher
                .DispatchEvent(
                    new ReactTextInputContentSizeChangedEvent(
                        this.GetTag(),
                        e.NewSize.Width,
                        e.NewSize.Height));
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            var start = this.SelectionStart;
            var length = this.SelectionLength;
            this.GetReactContext()
                .GetNativeModule<UIManagerModule>()
                .EventDispatcher
                .DispatchEvent(
                    new ReactTextInputSelectionEvent(
                        this.GetTag(),
                        start,
                        start + length));
        }
    }
}
