using ReactNative.UIManager;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
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

        public ReactTextBox()
        {
            SizeChanged += OnSizeChanged;
            PlaceholderActive = false;
            TextColor = Brushes.Black;
            PlaceholderColor = Brushes.LightGray;
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
                if (this.PlaceholderActive && !this.IsFocused) {
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
            Focus();
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

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.GetReactContext()
                .GetNativeModule<UIManagerModule>()
                .EventDispatcher
                .DispatchEvent(
                    new ReactTextChangedEvent(
                        this.GetTag(),
                        Text,
                        e.NewSize.Width,
                        e.NewSize.Height,
                        IncrementEventCount(),
                        ReactTextChangedEvent.Reason.SizeChanged));
        }
    }
}
