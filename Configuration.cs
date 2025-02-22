using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace COM3D2.EditModeEnhanced;

enum ItemTooltipStyle {
	None,
	Minimal,
	Default,
}

internal class Configuration {
	private readonly Dictionary<string, ConfigEntry<bool>> _configEntries = new();
	private readonly ConfigEntry<ItemTooltipStyle> _configItemTooltipStyle;

	private readonly Option[] _options = {
		new() {
			Key = "CustomViewRightClickRemove",
			Description = "Enables right clicking to remove items in custom view",
		},
		new() {
			Key = "CustomViewTooltip",
			Description = "Shows tooltip for items in custom view",
		},
		new() {
			Key = "CustomViewBodySlot",
			Description = "Adds body slot to custom view",
		},
		new() {
			Key = "SingleColorSetEquip",
			Description = "Equips single color sets without showing the color panel",
		},
		new() {
			Key = "AddTooltipFileName",
			Description = "Adds menu file name to item tooltip",
		},
	};

	public Configuration(ConfigFile config) {
		foreach (var option in _options) {
			_configEntries.Add(option.Key, config.Bind("General", option.Key, true, option.Description));
		}

		_configItemTooltipStyle = config.Bind("General", "ItemTooltipStyle", ItemTooltipStyle.Default, "Sets the item tooltip style");
	}

	public event EventHandler ItemTooltipStyleChange {
		add => _configItemTooltipStyle.SettingChanged += value;
		remove => _configItemTooltipStyle.SettingChanged -= value;
	}

	public ItemTooltipStyle ItemTooltipStyle => _configItemTooltipStyle.Value;

	public bool this[string key] => _configEntries[key].Value;

	private class Option {
		public string Key { get; set; }
		public string Description { get; set; }
	}
}
