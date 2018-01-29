using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Storage;
#else
using PCLStorage;
#endif

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
                return ApplicationData.Current.LocalFolder.Path;
#else
                if (_localStoragePath != null)
                    return _localStoragePath;
                return FileSystem.Current.LocalStorage.Path;
#endif
            }
        }

        /// <summary>
        /// Get given specific filePath in local storage
        /// </summary>
        /// <param name="filePath">Relative file path in local storage</param>
        /// <param name="token">cancel token</param>
        /// <returns>full path of |filePath| in local storage</returns>
        public static async Task<string> GetLocalStorageFilePathAsync(string filePath, CancellationToken token)
        {
#if WINDOWS_UWP
            var localFolder = ApplicationData.Current.LocalFolder;
            var storageFile = await localFolder.GetFileAsync(filePath).AsTask(token).ConfigureAwait(false);
            return storageFile.Path;
#else
            return await Task.FromResult(Path.Combine(LocalStoragePath, filePath));
#endif
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
