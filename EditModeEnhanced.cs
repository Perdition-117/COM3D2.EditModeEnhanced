using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace COM3D2.EditModeEnhanced;

[BepInPlugin("net.perdition.com3d2.editmodeenhanced", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
internal partial class EditModeEnhanced : BaseUnityPlugin {
	private const int BaseButtonHeight = 80;

	private static Configuration _config;

	void Awake() {
		_config = new Configuration(Config);

		Harmony.CreateAndPatchAll(typeof(EditModeEnhanced));
	}

	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.HoverOverCallback))]
	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.HoverOverCallbackOnGroup))]
	[HarmonyPostfix]
	private static void OnHoverOverCallback(SceneEdit __instance) {
		var button = UIEventTrigger.current.GetComponentInChildren<ButtonEdit>();
		var sprite = UIEventTrigger.current.GetComponentInChildren<UI2DSprite>();

		var position = new Vector3(-337, UIEventTrigger.current.transform.parent.position.y);
		var offset = new Vector3(0, -(sprite.height - BaseButtonHeight) / 2);

		if (_config["AddTooltipFileName"]) {
			AddItemInfoWindowFileName(__instance.m_info, button.m_MenuItem);
		}
		SetItemInfoWindowPosition(__instance.m_info, position, offset, true);
	}

	// skip set variation panel for single color sets
	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.ClickCallbackFromSetGroup))]
	[HarmonyPrefix]
	static bool SceneEdit_OnClickCallbackFromSetGroup(SceneEdit __instance) {
		if (!_config["SingleColorSetEquip"]) return true;

		var button = UIButton.current.GetComponentInChildren<ButtonEdit>();
		var menuItem = button.m_MenuItem;

		if (menuItem == null || menuItem.m_listMember.Count > 1) {
			return true;
		}

		__instance.m_bOpenSetGroupPanel = false;
		__instance.ClickCallback();
		__instance.m_Panel_GroupSet.SetActive(false);
		return false;
	}
}
