using System;
using Unity.UOS.Encrypt;
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

            var encryptedPayload = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(encryptedPayload))
            {
                return HappyFamilySaveData.CreateDefault();
            }

            try
            {
                var json = EncryptManager.Decrypt(encryptedPayload);
                if (string.IsNullOrWhiteSpace(json))
                {
                    json = encryptedPayload;
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
            var encryptedPayload = EncryptManager.Encrypt(json);
            PlayerPrefs.SetString(SaveKey, encryptedPayload);
            PlayerPrefs.Save();
        }
    }
}
