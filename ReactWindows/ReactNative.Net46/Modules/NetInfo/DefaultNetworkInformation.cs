using NETWORKLIST;
using System;

namespace ReactNative.Modules.NetInfo
{
    class DefaultNetworkInformation : INetworkInformation
    {
        public event EventHandler<NetworkConnectivityChangedEventArgs> NetworkConnectivityChanged;
        private NetworkListManager _networkListManager;

        public void Start()
        {
            _networkListManager = new NetworkListManager();
            try
            {
                // NOTE: It might throw Exception on some computers.
                //   The specified service does not exist as an installed service. (Exception from HRESULT: 0x80070424)
                _networkListManager.NetworkConnectivityChanged += OnNetworkConnectivityChanged;
            }
            catch (Exception)
            {
            }
        }

        public void Stop()
        {
            try
            {
                _networkListManager.NetworkConnectivityChanged -= OnNetworkConnectivityChanged;
            }
            catch (Exception)
            {
            }
            _networkListManager = null;
        }

        public string GetInternetStatus()
        {
            return _networkListManager.IsConnectedToInternet ? "InternetAccess" : "None";
        }

        private void OnNetworkConnectivityChanged(Guid guid, NLM_CONNECTIVITY connectivity)
        {
            NetworkConnectivityChangedEventArgs e = new NetworkConnectivityChangedEventArgs()
            {
                IsAvailable = _networkListManager.IsConnectedToInternet,
                ConnectionStatus = GetInternetStatus()
            };
            NetworkConnectivityChanged?.Invoke(new object(), e);
        }
    }
}
