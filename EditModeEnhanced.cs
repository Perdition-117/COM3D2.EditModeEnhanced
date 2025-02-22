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
		ItemInfoWnd_Awake();
	}

	private static void SetItemInfoWindowPosition(ItemInfoWnd itemInfoWindow) {
		var sprite = UIEventTrigger.current.GetComponentInChildren<UI2DSprite>();
		var position = new Vector3(-337, UIEventTrigger.current.transform.parent.position.y);
		var offset = new Vector3(0, -(sprite.height - BaseButtonHeight) / 2);
		SetItemInfoWindowPosition(itemInfoWindow, position, offset, true);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.HoverOverCallback))]
	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.HoverOverCallbackOnGroup))]
	private static void SceneEdit_OnHoverOverCallback(SceneEdit __instance) {
		if (_config["AddTooltipFileName"]) {
			var button = UIEventTrigger.current.GetComponentInChildren<ButtonEdit>();
			AddItemInfoWindowFileName(__instance.m_info, button.m_MenuItem.m_strMenuFileName);
		}
		SetItemInfoWindowPosition(__instance.m_info);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ShopItem), nameof(ShopItem.OnHoverOver))]
	private static void ShopItem_OnHoverOver(ShopItem __instance) {
		if (_config["AddTooltipFileName"] && __instance.item_data.type == Shop.ItemDataBase.Type.Parts && __instance.item_data.trial_wear_item_menu_array.Length > 0) {
			AddItemInfoWindowFileName(__instance.info_window_, string.Join("\n", __instance.item_data.trial_wear_item_menu_array));
		}
		SetItemInfoWindowPosition(__instance.info_window_);
	}

	// skip set variation panel for single color sets
	[HarmonyPrefix]
	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.ClickCallbackFromSetGroup))]
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
