using System;
using System.Text;
using UnityEngine;

namespace HappyFamily.Save
{
    public class PlayerProgressService
    {
        private const string SaveKey = "happy_family_progress";

        public HappyFamilySaveData Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                return HappyFamilySaveData.CreateDefault();
            }

            var encodedPayload = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(encodedPayload))
            {
                return HappyFamilySaveData.CreateDefault();
            }

            try
            {
                var json = Decode(encodedPayload);
                if (string.IsNullOrWhiteSpace(json))
                {
                    json = encodedPayload;
                }

                var data = JsonUtility.FromJson<HappyFamilySaveData>(json) ?? HappyFamilySaveData.CreateDefault();
                data.Normalize();
                return data;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to load progress, fallback to default save. {exception.Message}");
                return HappyFamilySaveData.CreateDefault();
            }
        }

        public void Save(HappyFamilySaveData data)
        {
            data.Normalize();
            var json = JsonUtility.ToJson(data);
            var encodedPayload = Encode(json);
            PlayerPrefs.SetString(SaveKey, encodedPayload);
            PlayerPrefs.Save();
        }

        private static string Encode(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }

            var bytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(bytes);
        }

        private static string Decode(string encodedText)
        {
            if (string.IsNullOrEmpty(encodedText))
            {
                return string.Empty;
            }

            try
            {
                var bytes = Convert.FromBase64String(encodedText);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                // Fallback: assume it's plain JSON (for backward compatibility)
                return encodedText;
            }
        }
    }
}
