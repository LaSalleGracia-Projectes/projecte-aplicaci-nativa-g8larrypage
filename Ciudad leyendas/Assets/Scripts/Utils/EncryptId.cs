using System;
using System.Security.Cryptography;
using System.Text;
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

        public string EncryptAndroidId(string androidId)
        {
            int keyLength = 256 / 8;
            int iteraciones = 50000;
            byte[] fixedBytes = Encoding.UTF8.GetBytes("CiudadLeyendas2025");

            using (var deriveBytes = new Rfc2898DeriveBytes(
                       androidId,
                       fixedBytes,
                       iteraciones,
                       HashAlgorithmName.SHA256))
            {
                byte[] key = deriveBytes.GetBytes(keyLength);

                byte[] iv = new byte[16];
                Array.Copy(fixedBytes, iv, Math.Min(fixedBytes.Length, 16));

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform encryptor = aes.CreateEncryptor())
                    {
                        byte[] dataBytes = Encoding.UTF8.GetBytes(androidId);
                        byte[] bytesEncriptados = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);

                        return Convert.ToBase64String(bytesEncriptados);
                    }
                }
            }
        }
    }
}