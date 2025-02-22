using System;
using System.Collections.Generic;
using HarmonyLib;
using SceneEditWindow;
using UnityEngine;

namespace COM3D2.EditModeEnhanced;

internal partial class EditModeEnhanced {
	private static readonly Dictionary<MPN, string> DefaultItems = new() {
		[MPN.nose] = "nose_del_i_.menu",
		[MPN.facegloss] = "facegloss_del_i_.menu",
	};

	// add tooltips to custom view window
	[HarmonyPatch(typeof(CustomViewItem), nameof(CustomViewItem.Awake))]
	[HarmonyPostfix]
	private static void CustomViewItem_OnAwake(CustomViewItem __instance) {
		var eventTrigger = __instance.button.gameObject.AddComponent<UIEventTrigger>();
		EventDelegate.Add(eventTrigger.onHoverOver, OnHoverOver);
		EventDelegate.Add(eventTrigger.onHoverOut, OnHoverOut);
	}

	private static void OnHoverOver() {
		if (!_config["CustomViewTooltip"] || _config.ItemTooltipStyle == ItemTooltipStyle.None) {
			return;
		}

		var button = UIEventTrigger.current;
		var customViewItem = button.GetComponentInParent<CustomViewItem>();
		var sceneEdit = customViewItem.sceneEdit;
		var menuItem = customViewItem.GetMenuItem(sceneEdit.maid, customViewItem.mpn);
		if (menuItem == null || IsDefaultItem(customViewItem.mpn, menuItem.m_strMenuFileName)) {
			return;
		}

		var position = button.transform.parent.position;
		sceneEdit.m_info.Open(position, menuItem.m_texIconRef, menuItem.menuNameCurrentLanguage, menuItem.infoTextCurrentLanguage);

		var basePosition = new Vector3(sceneEdit.customViewWindow.transform.position.x, position.y);
		var offset = new Vector3(sceneEdit.customViewWindow.WindowSize.x, 0) / 2;

		if (_config["AddTooltipFileName"]) {
			AddItemInfoWindowFileName(sceneEdit.m_info, menuItem.m_strMenuFileName);
		}
		SetItemInfoWindowPosition(sceneEdit.m_info, basePosition, offset);
	}

	private static void OnHoverOut() {
		var customViewItem = UIEventTrigger.current.GetComponentInParent<CustomViewItem>();
		customViewItem.sceneEdit.m_info.Close();
	}

	private static bool IsDefaultItem(MPN mpn, string menuFileName) {
		return TryGetDefaultItem(mpn, out var defaultItem) && menuFileName.Equals(defaultItem, StringComparison.OrdinalIgnoreCase);
	}

	private static bool TryGetDefaultItem(MPN mpn, out string defaultFileName) {
		return CM3.dicDelItem.TryGetValue(mpn, out defaultFileName) || DefaultItems.TryGetValue(mpn, out defaultFileName);
	}

	// add body slot to custom view window
	[HarmonyPostfix]
	[HarmonyPatch(typeof(CustomViewItemData), nameof(CustomViewItemData.Create))]
	private static void CustomViewItemData_OnCreate() {
		if (!_config["CustomViewBodySlot"]) return;

		if (SceneEditInfo.m_dicPartsTypePair.ContainsKey(MPN.body) && !CustomViewItemData.itemList.Exists(e => e.mpn == MPN.body)) {
			CustomViewItemData.itemList.Add(new() {
				page = 1,
				mpn = MPN.body,
				iconTexName = "customview_icon_skin.tex",
				requestNewFace = false,
				requestFBFace = false,
			});
		}
	}

	// delete items by right click in custom view window
	[HarmonyPrefix]
	[HarmonyPatch(typeof(CustomViewItem), nameof(CustomViewItem.OnClickButton))]
	private static bool CustomViewItem_OnClickButton(CustomViewItem __instance) {
		if (!_config["CustomViewRightClickRemove"]) return true;
		if (UICamera.currentTouchID != -2) return true;

		var sceneEdit = __instance.sceneEdit;
		var mpn = __instance.mpn;
		var maid = sceneEdit.maid;
		var isTemp = sceneEdit.modeType == SceneEdit.ModeType.CostumeEdit;

		if (CM3.dicDelItem.ContainsKey(mpn)) {
			maid.DelProp(mpn, isTemp);
			SceneEdit.AllProcPropSeqStart(maid);
			sceneEdit.m_info.Close();
		}

		if (DefaultItems.TryGetValue(mpn, out var defaultItem)) {
			maid.SetProp(mpn, defaultItem, 0, isTemp);
			SceneEdit.AllProcPropSeqStart(maid);
			sceneEdit.m_info.Close();
		}

		if (sceneEdit.NowMPN == mpn) {
			var menuCategory = __instance.GetCategory(mpn);
			var category = sceneEdit.CategoryList.Find(e => e.m_eCategory == menuCategory);
			var partsType = category.m_listPartsType.Find(e => e.m_mpn == mpn);
			sceneEdit.UpdateSelectedMenuItem(partsType);
		}

		return false;
	}
}
