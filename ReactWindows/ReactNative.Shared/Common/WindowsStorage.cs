using PCLStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace ReactNative.Common
{
    /// <summary>
    /// Helper to get Windows system storage
    /// </summary>
    public static class WindowsStorage
    {
        /// <summary>
        /// Get the LocalStorage path
        /// </summary>
        public static string LocalStoragePath
        {
            get
            {
#if WINDOWS_UWP
                return FileSystem.Current.LocalStorage.Path;
#else
                var appDataFolderPath = Application.Current.Properties["AppDataFolder"] as string;
                if (appDataFolderPath != null && Directory.Exists(appDataFolderPath))
                    return appDataFolderPath;
                return FileSystem.Current.LocalStorage.Path;
#endif
            }
        }

        /// <summary>
        /// Get the LocalStorage as PCLStorage IFolder instance
        /// </summary>
        public static IFolder LocalStorage
        {
            get
            {
#if WINDOWS_UWP
                return FileSystem.Current.LocalStorage;
#else
                return new FileSystemFolder(LocalStoragePath);
#endif
            }
        }
    }
}
