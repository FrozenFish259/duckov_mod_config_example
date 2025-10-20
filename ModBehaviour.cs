using Duckov.Modding;
using Duckov.Options;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using SodaCraft.Localizations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModConfigExample
{
    [System.Serializable]
    public class DisplayItemValueConfig
    {
        // 是否显示物品价值
        public bool showItemValue = true;

        // 文字大小
        public float fontSize = 20f;

        // 文字颜色
        public string textColor = "#FFFFFF";

        // 价值显示格式：0=原始值，1=除以2，2=除以10
        public int valueFormat = 1;

        // 显示前缀
        public string valuePrefix = "$";

        // 强制更新配置文件token
        public string configToken = "display_item_value_v1";
    }

    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        public static string MOD_NAME = "ModConfigExample";

        DisplayItemValueConfig config = new DisplayItemValueConfig();

        private static string persistentConfigPath => Path.Combine(Application.streamingAssetsPath, "DisplayItemValueConfig.txt");

        TextMeshProUGUI _text = null;
        TextMeshProUGUI Text
        {
            get
            {
                if (_text == null)
                {
                    _text = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI);
                    _text.gameObject.SetActive(false);
                }
                return _text;
            }
        }

        void Awake()
        {
            Debug.Log("DisplayItemValue Loaded!!!");
        }

        void OnDestroy()
        {
            if (_text != null)
                Destroy(_text);
        }

        void OnEnable()
        {
            ItemHoveringUI.onSetupItem += OnSetupItemHoveringUI;
            ModManager.OnModActivated += OnModActivated;

            // 立即检查一次，防止 ModConfig 已经加载但事件错过了
            if (ModConfigAPI.IsAvailable())
            {
                Debug.Log("DisplayItemValue: ModConfig already available!");
                SetupModConfig();
                LoadConfigFromModConfig();
            }
        }

        private void OnModActivated(ModInfo info, Duckov.Modding.ModBehaviour behaviour)
        {
            if (info.name == ModConfigAPI.ModConfigName)
            {
                Debug.Log("DisplayItemValue: ModConfig activated!");
                SetupModConfig();
                LoadConfigFromModConfig();
            }
        }

        void OnDisable()
        {
            ItemHoveringUI.onSetupItem -= OnSetupItemHoveringUI;

            ModManager.OnModActivated -= OnModActivated;
            ModConfigAPI.SafeRemoveOnOptionsChangedDelegate(OnModConfigOptionsChanged);
        }

        private void SaveConfig(DisplayItemValueConfig config)
        {
            try
            {
                string json = JsonUtility.ToJson(config, true);
                File.WriteAllText(persistentConfigPath, json);
                Debug.Log("DisplayItemValue: Config saved");
            }
            catch (Exception e)
            {
                Debug.LogError($"DisplayItemValue: Failed to save config: {e}");
            }
        }

        private void SetupModConfig()
        {
            if (!ModConfigAPI.IsAvailable())
            {
                Debug.LogWarning("DisplayItemValue: ModConfig not available");
                return;
            }

            Debug.Log("准备添加ModConfig配置项");

            // 添加配置变更监听
            ModConfigAPI.SafeAddOnOptionsChangedDelegate(OnModConfigOptionsChanged);

            // 根据当前语言设置描述文字
            SystemLanguage[] chineseLanguages = {
                SystemLanguage.Chinese,
                SystemLanguage.ChineseSimplified,
                SystemLanguage.ChineseTraditional
            };

            bool isChinese = chineseLanguages.Contains(LocalizationManager.CurrentLanguage);

            // 添加配置项
            ModConfigAPI.SafeAddBoolDropdownList(
                MOD_NAME,
                "showItemValue",
                isChinese ? "显示物品价值" : "Show Item Value",
                config.showItemValue
            );

            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "fontSize",
                isChinese ? "文字大小" : "Font Size",
                typeof(float),
                config.fontSize,
                new Vector2(10f, 40f)
            );

            // 价值显示格式下拉菜单
            var formatOptions = new SortedDictionary<string, object>
            {
                { isChinese ? "原始值" : "Raw Value", 0 },
                { isChinese ? "除以2" : "Divided by 2", 1 },
                { isChinese ? "除以10" : "Divided by 10", 2 }
            };

            ModConfigAPI.SafeAddDropdownList(
                MOD_NAME,
                "valueFormat",
                isChinese ? "价值显示格式" : "Value Display Format",
                formatOptions,
                typeof(int),
                config.valueFormat
            );

            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "valuePrefix",
                isChinese ? "价值前缀" : "Value Prefix",
                typeof(string),
                config.valuePrefix,
                null
            );

            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "textColor",
                isChinese ? "文字颜色" : "Text Color",
                typeof(string),
                config.textColor,
                null
            );

            Debug.Log("DisplayItemValue: ModConfig setup completed");
        }

        private void OnModConfigOptionsChanged(string key)
        {
            if (!key.StartsWith(MOD_NAME + "_"))
                return;

            // 使用新的 LoadConfig 方法读取配置
            LoadConfigFromModConfig();

            // 保存到本地配置文件
            SaveConfig(config);

            // 更新当前显示的文本样式（如果正在显示）
            UpdateTextStyle();

            Debug.Log($"DisplayItemValue: ModConfig updated - {key}");
        }

        private void LoadConfigFromModConfig()
        {
            // 使用新的 LoadConfig 方法读取所有配置
            config.showItemValue = ModConfigAPI.SafeLoad<bool>(MOD_NAME, "showItemValue", config.showItemValue);
            config.fontSize = ModConfigAPI.SafeLoad<float>(MOD_NAME, "fontSize", config.fontSize);
            config.valueFormat = ModConfigAPI.SafeLoad<int>(MOD_NAME, "valueFormat", config.valueFormat);
            config.valuePrefix = ModConfigAPI.SafeLoad<string>(MOD_NAME, "valuePrefix", config.valuePrefix);
            config.textColor = ModConfigAPI.SafeLoad<string>(MOD_NAME, "textColor", config.textColor);
        }

        private void UpdateTextStyle()
        {
            if (_text != null)
            {
                _text.fontSize = config.fontSize;

                // 解析颜色
                if (ColorUtility.TryParseHtmlString(config.textColor, out Color color))
                {
                    _text.color = color;
                }
                else
                {
                    _text.color = Color.white; // 默认颜色
                }
            }
        }

        private void OnSetupItemHoveringUI(ItemHoveringUI uiInstance, Item item)
        {
            if (item == null || !config.showItemValue)
            {
                Text.gameObject.SetActive(false);
                return;
            }

            // 计算价值
            float rawValue = item.GetTotalRawValue();
            float displayValue = CalculateDisplayValue(rawValue);

            Text.gameObject.SetActive(true);
            Text.transform.SetParent(uiInstance.LayoutParent);
            Text.transform.localScale = Vector3.one;
            Text.text = $"{config.valuePrefix}{displayValue:F0}";
            Text.fontSize = config.fontSize;

            // 设置颜色
            if (ColorUtility.TryParseHtmlString(config.textColor, out Color color))
            {
                Text.color = color;
            }

            // 确保文本在布局中正确显示
            LayoutRebuilder.ForceRebuildLayoutImmediate(uiInstance.LayoutParent as RectTransform);
        }

        private float CalculateDisplayValue(float rawValue)
        {
            return config.valueFormat switch
            {
                0 => rawValue,           // 原始值
                1 => rawValue / 2f,      // 除以2
                2 => rawValue / 10f,     // 除以10
                _ => rawValue / 2f       // 默认除以2
            };
        }
    }
}