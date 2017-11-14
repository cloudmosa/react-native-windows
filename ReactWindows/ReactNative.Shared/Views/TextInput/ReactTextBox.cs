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
        private bool _avoidTextChanged = false;
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
                this.Text = value;
                this.PlaceholderActive = true;
            }
        }

        public int IncrementEventCount()
        {
            return Interlocked.Increment(ref _eventCount);
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
                this.Select(0, 0);
            }
            else if (SelectTextOnFocus)
            {
                this.Select(0, this.Text.Length);
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!this.IsKeyboardFocusWithin) {
                e.Handled = true;
                this.Focus();
            }
            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // Prevents that the user can go through the placeholder with arrow keys
            if (PlaceholderActive && (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down))
                e.Handled = true;
            base.OnPreviewKeyDown(e);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            // Check flag
            if (_avoidTextChanged) {
                _avoidTextChanged = false;
                return;
            }

            // If the Text is empty, insert placeholder and set cursor to to first position
            if (string.IsNullOrEmpty(this.Text))
            {
                _avoidTextChanged = true;
                this.PlaceholderActive = true;
                this.Text = this.PlaceholderText;
                this.Select(0, 0);
                e.Handled = true;
            }
            else if (this.PlaceholderActive && this.Text != this.PlaceholderText)
            {
                _avoidTextChanged = true;
                this.PlaceholderActive = false;
                if (!string.IsNullOrEmpty(this.PlaceholderText))
                    this.Text = this.Text.Replace(this.PlaceholderText, string.Empty);
                this.Select(this.Text.Length, 0);
                e.Handled = true;
            }

            base.OnTextChanged(e);
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
