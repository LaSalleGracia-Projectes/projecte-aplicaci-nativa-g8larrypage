using UnityEngine;

namespace Utils
{
    public class EncryptId
    {
        private string _androidId;

        public string GetAndroidId()
        {
            if (!string.IsNullOrEmpty(_androidId))
                return _androidId;

            #if UNITY_ANDROID && !UNITY_EDITOR
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        using (var contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver"))
                        {
                            using (var secure = new AndroidJavaClass("android.provider.Settings$Secure"))
                            {
                                _androidId = secure.CallStatic<string>("getString", contentResolver, "android_id");
                            }
                        }
                    }
                }
            #else
            _androidId = SystemInfo.deviceUniqueIdentifier;
            #endif

            return _androidId;
        }
    }
}