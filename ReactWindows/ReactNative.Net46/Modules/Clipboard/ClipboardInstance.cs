﻿using System.Windows;

namespace ReactNative.Modules.Clipboard
{
    class ClipboardInstance : IClipboardInstance
    {
        public void Clear()
        {
            System.Windows.Clipboard.Clear();
        }

        public IDataObject GetContent()
        {
            return System.Windows.Clipboard.GetDataObject();
        }

        public void SetContent(IDataObject package)
        {
            System.Windows.Clipboard.SetDataObject(package);
        }

        public bool ContainsText()
        {
            return System.Windows.Clipboard.ContainsText();
        }

        public string GetText()
        {
            return System.Windows.Clipboard.GetText();
        }

        public void SetText(string str)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    System.Windows.Clipboard.SetText(str);
                    return;
                }
                catch { }
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}
