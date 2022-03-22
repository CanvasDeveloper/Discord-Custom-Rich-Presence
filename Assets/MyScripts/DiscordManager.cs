using System;
using Discord;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct ButtonSettings
{
    public string buttonText;
    public bool imageFill;
}

namespace MyScripts
{
    public class DiscordManager : MonoBehaviour
    {
        private Discord.Discord _discord;
        private ActivityManager _activityManager;

        [Header("UI")]
        [SerializeField] private TMP_InputField appIdInputField;
        [SerializeField] private TMP_InputField statusInputField;
        [SerializeField] private TMP_InputField detailsInputField;
        [SerializeField] private TMP_InputField largeImageInputField;
        [SerializeField] private TMP_InputField largeImageDescInputField;
        [SerializeField] private TMP_InputField smallImageInputField;
        [SerializeField] private TMP_InputField smallImageDescInputField;
        
        [SerializeField] private Toggle largeImageToggle;
        [SerializeField] private Toggle smallImageToggle;
        [SerializeField] private Toggle timerToggle;
        
        [SerializeField] private Button startPresenceButton;
        [SerializeField] private TextMeshProUGUI startPresenceText;
        [SerializeField] private Button stopPresenceButton;
        
        [SerializeField] private ButtonSettings defaultStartSettings;
        [SerializeField] private ButtonSettings updateSettings;

        private bool _isPresenceOn;

        private void Awake()
        {
            CheckSave();
            
            appIdInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            
            startPresenceButton.onClick.AddListener(StartPresence);
            stopPresenceButton.onClick.AddListener(StopPresence);
            largeImageToggle.onValueChanged.AddListener(LargeImageEnable);
            smallImageToggle.onValueChanged.AddListener(SmallImageEnable);
            timerToggle.onValueChanged.AddListener(TimerEnable);
        }

        private void StartPresence()
        {
            if(string.IsNullOrEmpty(appIdInputField.text))
                return;

            startPresenceButton.image.fillCenter = updateSettings.imageFill;
            startPresenceText.text = updateSettings.buttonText;
            
            var newAppID = appIdInputField.text;
            var newLargeDesc = largeImageDescInputField.text;
            var newSmallDesc = smallImageDescInputField.text;

            if (!_isPresenceOn)
            {
                PlayerPrefs.SetString(PlayerPrefsConst.AppKey, newAppID);
                PlayerPrefs.SetString(PlayerPrefsConst.LargeImageDescKey, newLargeDesc);
                PlayerPrefs.SetString(PlayerPrefsConst.SmallImageDescKey, newSmallDesc);

                _discord = new Discord.Discord(Convert.ToInt64(newAppID), (ulong) CreateFlags.Default);
                _activityManager = _discord.GetActivityManager();
                _activityManager.ClearActivity(ClearActivity);
                
                UpdatePresence(statusInputField.text, detailsInputField.text, GetAssets());

                _isPresenceOn = true;
                return;
            }

            UpdatePresence(statusInputField.text, detailsInputField.text, GetAssets());
        }

        private void OnDisable()
        {
            if(_isPresenceOn)
                StopPresence();
        }
        
        private void Update()
        {
            if(_isPresenceOn)
                _discord.RunCallbacks();
        }
        
        private void CheckSave()
        {
            if (PlayerPrefs.HasKey(PlayerPrefsConst.AppKey))
                appIdInputField.text = PlayerPrefs.GetString(PlayerPrefsConst.AppKey);

            if (PlayerPrefs.HasKey(PlayerPrefsConst.StatusKey))
                statusInputField.text = PlayerPrefs.GetString(PlayerPrefsConst.StatusKey);
            
            if (PlayerPrefs.HasKey(PlayerPrefsConst.DetailsKey))
                detailsInputField.text = PlayerPrefs.GetString(PlayerPrefsConst.DetailsKey);
            
            if (PlayerPrefs.HasKey(PlayerPrefsConst.LargeImageKey))
                largeImageInputField.text = PlayerPrefs.GetString(PlayerPrefsConst.LargeImageKey);
            
            if (PlayerPrefs.HasKey(PlayerPrefsConst.LargeImageDescKey))
                largeImageDescInputField.text = PlayerPrefs.GetString(PlayerPrefsConst.LargeImageDescKey);
            
            if (PlayerPrefs.HasKey(PlayerPrefsConst.LargeImageEnableKey))
                largeImageToggle.isOn = IntToBool(PlayerPrefs.GetInt(PlayerPrefsConst.LargeImageEnableKey));
            
            if (PlayerPrefs.HasKey(PlayerPrefsConst.SmallImageKey))
                smallImageInputField.text = PlayerPrefs.GetString(PlayerPrefsConst.SmallImageKey);
            
            if (PlayerPrefs.HasKey(PlayerPrefsConst.SmallImageDescKey))
                smallImageDescInputField.text = PlayerPrefs.GetString(PlayerPrefsConst.SmallImageDescKey);
            
            if (PlayerPrefs.HasKey(PlayerPrefsConst.SmallImageEnableKey))
                smallImageToggle.isOn = IntToBool(PlayerPrefs.GetInt(PlayerPrefsConst.SmallImageEnableKey));
            
            if (PlayerPrefs.HasKey(PlayerPrefsConst.TimerEnableKey))
                timerToggle.isOn = IntToBool(PlayerPrefs.GetInt(PlayerPrefsConst.TimerEnableKey));
        }

        private bool IntToBool(int intToConvert) => intToConvert != 0;

        private void TimerEnable(bool value)
        {
            var enableStatus = value ? 1 : 0;
            PlayerPrefs.SetInt(PlayerPrefsConst.TimerEnableKey, enableStatus);
        }
        
        private void LargeImageEnable(bool value)
        {
            var enableStatus = value ? 1 : 0;
            PlayerPrefs.SetInt(PlayerPrefsConst.LargeImageEnableKey, enableStatus);
        }
        
        private void SmallImageEnable(bool value)
        {
            var enableStatus = value ? 1 : 0;
            PlayerPrefs.SetInt(PlayerPrefsConst.SmallImageEnableKey, enableStatus);
        }

        private ActivityAssets GetAssets()
        {
            var newAssets = new ActivityAssets();

            if (largeImageToggle.isOn)
            {
                newAssets.LargeImage = largeImageInputField.text;
                newAssets.LargeText = largeImageDescInputField.text;
            }

            if (smallImageToggle.isOn)
            {     
                newAssets.SmallImage = smallImageInputField.text;
                newAssets.SmallText = smallImageDescInputField.text;
            }
           
            return newAssets;
        }

        private void UpdatePresence(string newState, string newDetails, ActivityAssets assets = new())
        {
            var active = new Activity
            {
                State = newState,
                Details = newDetails,
                Assets = assets,
            };

            if (timerToggle.isOn)
            {   active.Timestamps = new ActivityTimestamps()
                {
                    Start = DateTimeOffset.Now.ToUnixTimeSeconds(),
                };
            }

            PlayerPrefs.SetString(PlayerPrefsConst.StatusKey, newState);
            PlayerPrefs.SetString(PlayerPrefsConst.DetailsKey, newDetails);
            PlayerPrefs.SetString(PlayerPrefsConst.LargeImageKey, assets.LargeImage);
            PlayerPrefs.SetString(PlayerPrefsConst.SmallImageKey, assets.SmallImage);

            _activityManager.UpdateActivity(active, ActivityCallBack);
        }

        private void StopPresence()
        {
            if(!_isPresenceOn)
                return;
            
            _isPresenceOn = false;
            _activityManager.ClearActivity(ClearActivity);
            _discord.Dispose();

            if (!startPresenceButton || !startPresenceButton.image)
                return;
            
            startPresenceButton.image.fillCenter = defaultStartSettings.imageFill;
            startPresenceText.text = defaultStartSettings.buttonText;
        }

        private void ClearActivity(Result result)
        {
            Debug.Log("Clear Callback: " + result);
        }

        private void ActivityCallBack(Result result)
        {
            Debug.Log("Status Callback: " + result);
        }
    }
}