using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ReactNative.Views.Text
{
    /// <summary>
    /// Selectable support for TextBlock
    /// Referenced from https://stackoverflow.com/a/45627524
    /// </summary>
    internal static class TextSelectableExtension
    {
        private static readonly Type TextEditorType = Type.GetType("System.Windows.Documents.TextEditor, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        private static readonly PropertyInfo IsReadOnlyProp = TextEditorType.GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly PropertyInfo TextViewProp = TextEditorType.GetProperty("TextView", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo RegisterMethod = TextEditorType.GetMethod("RegisterCommandHandlers",
            BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Type), typeof(bool), typeof(bool), typeof(bool) }, null);
        private static readonly MethodInfo OnDetachMethod = TextEditorType.GetMethod("OnDetach", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly Type TextContainerType = Type.GetType("System.Windows.Documents.ITextContainer, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        private static readonly PropertyInfo TextContainerTextViewProp = TextContainerType.GetProperty("TextView");

        private static readonly PropertyInfo TextContainerProp = typeof(TextBlock).GetProperty("TextContainer", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void RegisterCommandHandlers(Type controlType, bool acceptsRichContent, bool readOnly, bool registerEventListeners)
        {
            RegisterMethod.Invoke(null, new object[] { controlType, acceptsRichContent, readOnly, registerEventListeners });
        }

        public static readonly DependencyProperty TextEditorProperty =
            DependencyProperty.RegisterAttached(
                "TextEditor",
                TextEditorType,
                typeof(TextSelectableExtension),
                new FrameworkPropertyMetadata(null));

        public static void AttachTextEditor(this TextBlock tb)
        {
            if (tb == null)
                throw new ArgumentNullException(nameof(tb));

            var textContainer = TextContainerProp.GetValue(tb);

            var editor = Activator.CreateInstance(TextEditorType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                null, new[] { textContainer, tb, false }, null);

            IsReadOnlyProp.SetValue(editor, true);
            TextViewProp.SetValue(editor, TextContainerTextViewProp.GetValue(textContainer));

            tb.SetValue(TextEditorProperty, editor);
        }

        public static void DetachTextEditor(this TextBlock tb)
        {
            if (tb == null)
                throw new ArgumentNullException(nameof(tb));

            var editor = tb.GetValue(TextEditorProperty);
            if (editor != null)
            {
                TextViewProp.SetValue(editor, null);
                OnDetachMethod.Invoke(editor, null);
                tb.SetValue(TextEditorProperty, null);
            }
        }
    }
}
