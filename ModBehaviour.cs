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

            // 加载配置
            TryLoadingConfig();

            // 尝试初始化 ModConfig
            if (ModConfigAPI.IsAvailable())
            {
                SetupModConfig();
            }
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
        }

        void OnDisable()
        {
            ItemHoveringUI.onSetupItem -= OnSetupItemHoveringUI;

            ModManager.OnModActivated -= OnModActivated;
            ModConfigAPI.SafeRemoveOnOptionsChangedDelegate(OnOptionsChanged);
        }

        private void OnModActivated(ModInfo info, Duckov.Modding.ModBehaviour behaviour)
        {
            if (info.name == ModConfigAPI.ModConfigName)
            {
                Debug.Log("DisplayItemValue: ModConfig activated!");
                SetupModConfig();
            }
        }

        private void TryLoadingConfig()
        {
            try
            {
                if (File.Exists(persistentConfigPath))
                {
                    string json = File.ReadAllText(persistentConfigPath);
                    DisplayItemValueConfig loadedConfig = JsonUtility.FromJson<DisplayItemValueConfig>(json);

                    // 检查配置版本
                    if (loadedConfig.configToken == config.configToken)
                    {
                        config = loadedConfig;
                        Debug.Log("DisplayItemValue: Config loaded successfully");
                    }
                    else
                    {
                        Debug.LogWarning("DisplayItemValue: Config version mismatch, using default config");
                        SaveConfig(config);
                    }
                }
                else
                {
                    SaveConfig(config);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"DisplayItemValue: Failed to load config: {e}");
                SaveConfig(config);
            }
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

            // 添加配置变更监听
            ModConfigAPI.SafeAddOnOptionsChangedDelegate(OnOptionsChanged);

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
                $"{MOD_NAME}_showItemValue",
                isChinese ? "显示物品价值" : "Show Item Value",
                config.showItemValue
            );

            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                $"{MOD_NAME}_fontSize",
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
                $"{MOD_NAME}_valueFormat",
                isChinese ? "价值显示格式" : "Value Display Format",
                formatOptions,
                typeof(int),
                config.valueFormat
            );

            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                $"{MOD_NAME}_valuePrefix",
                isChinese ? "价值前缀" : "Value Prefix",
                typeof(string),
                config.valuePrefix,
                null
            );

            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                $"{MOD_NAME}_textColor",
                isChinese ? "文字颜色" : "Text Color",
                typeof(string),
                config.textColor,
                null
            );

            Debug.Log("DisplayItemValue: ModConfig setup completed");
        }

        private void OnOptionsChanged(string key)
        {
            if (!ModConfigAPI.IsAvailable())
                return;

            // 读取更新后的配置值
            config.showItemValue = OptionsManager.Load<bool>($"{MOD_NAME}_showItemValue", config.showItemValue);
            config.fontSize = OptionsManager.Load<float>($"{MOD_NAME}_fontSize", config.fontSize);
            config.valueFormat = OptionsManager.Load<int>($"{MOD_NAME}_valueFormat", config.valueFormat);
            config.valuePrefix = OptionsManager.Load<string>($"{MOD_NAME}_valuePrefix", config.valuePrefix);
            config.textColor = OptionsManager.Load<string>($"{MOD_NAME}_textColor", config.textColor);

            // 保存配置
            SaveConfig(config);

            // 更新当前显示的文本样式（如果正在显示）
            UpdateTextStyle();

            Debug.Log($"DisplayItemValue: Config updated - {key}");
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
            Text.text = $"{config.valuePrefix}{displayValue}";
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