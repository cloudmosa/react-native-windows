using PCLStorage;
using System.IO;

namespace ReactNative.Common
{
    /// <summary>
    /// Helper to get Windows system storage
    /// </summary>
    public static class WindowsStorage
    {
        private static string _localStoragePath = null;

        /// <summary>
        /// Setup WindowsStorage settings
        /// </summary>
        /// <param name="localStoragePath">Setup LocalStorage path</param>
        public static void Initialize(string localStoragePath)
        {
#if !WINDOWS_UWP
            _localStoragePath = EnsureFolder(localStoragePath);
#endif
        }

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
                if (_localStoragePath != null)
                    return _localStoragePath;
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

        /// <summary>
        /// Ensure folder is existed, otherwise will create the folder.
        /// </summary>
        /// <param name="folderPath">Target folder path</param>
        /// <returns>Existed folder path</returns>
        private static string EnsureFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            return folderPath;
        }
    }
}
