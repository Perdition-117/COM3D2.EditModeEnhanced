using System.Collections.Generic;
using BepInEx.Configuration;

namespace COM3D2.EditModeEnhanced;

internal class Configuration {
	private readonly ConfigFile _config;

	private readonly Dictionary<string, ConfigEntry<bool>> _configEntries = new();

	private readonly Option[] _options = new Option[] {
		new() {
			Key = "CustomViewRightClickRemove",
			Description = "Enable right clicking to remove items in custom view",
		},
		new() {
			Key = "CustomViewTooltip",
			Description = "Show tooltip for items in custom view",
		},
		new() {
			Key = "CustomViewBodySlot",
			Description = "Adds body slot to custom view",
		},
		new() {
			Key = "SingleColorSetEquip",
			Description = "Equip single color sets without showing the color panel",
		},
		new() {
			Key = "AddTooltipFileName",
			Description = "Add menu file name to item tooltip",
		},
	};

	public Configuration(ConfigFile config) {
		_config = config;

		foreach (var option in _options) {
			_configEntries.Add(option.Key, _config.Bind("General", option.Key, true, option.Description));
		}
	}

	public bool this[string option] { get => _configEntries[option].Value; }

	private class Option {
		public string Key { get; set; }
		public string Description { get; set; }
	}
}
