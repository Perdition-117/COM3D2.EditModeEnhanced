using System;
using HarmonyLib;
using UnityEngine;

namespace COM3D2.EditModeEnhanced;

internal partial class EditModeEnhanced {
	private const int BaseTooltipHeight = 81;
	private const int MinTotalTooltipHeight = 128;
	private const float MinimalPosition = 20.5f;

	private static int _initialWidth;
	private static int _infoOffset;
	private static float _titleInitialPos;
	private static float _infoInitialPos;

	private static ItemInfoWnd _itemInfoWindow;

	private void ItemInfoWnd_Awake() {
		_config.ItemTooltipStyleChange += (o, e) => SetItemInfoWindowLayout();
	}

	// set internal tooltip anchor to top left (instead of center) in order to facilitate easier resizing later
	[HarmonyPostfix]
	[HarmonyPatch(typeof(ItemInfoWnd), nameof(ItemInfoWnd.Start))]
	private static void ItemInfoWnd_OnStart(ItemInfoWnd __instance) {
		__instance.m_uiBase.pivot = UIWidget.Pivot.TopLeft;
		var offsetVector = new Vector3(-__instance.m_uiBase.width, __instance.m_uiBase.height) / 2f;
		var line = UTY.GetChildObject(__instance.m_uiBase.gameObject, "Line", false);
		line.GetComponent<UISprite>().transform.localPosition -= offsetVector;
		__instance.m_uiTitle.transform.localPosition -= offsetVector;
		__instance.m_uiInfo.transform.localPosition -= offsetVector;
		__instance.m_uiIcon.transform.localPosition -= offsetVector;
		__instance.m_vecOffsetPos.y += offsetVector.y;
		__instance.m_uiInfo.overflowMethod = UILabel.Overflow.ResizeHeight;
		__instance.m_uiInfo.supportEncoding = true;

		_itemInfoWindow = __instance;

		_initialWidth = __instance.m_uiBase.width;
		_titleInitialPos = __instance.m_uiTitle.transform.localPosition.x;
		_infoInitialPos = __instance.m_uiInfo.transform.localPosition.x;

		var infoOffset = __instance.m_uiTitle.transform.localPosition - __instance.m_uiInfo.transform.localPosition;
		_infoOffset = Math.Abs((int)infoOffset.y);

		SetItemInfoWindowLayout();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.HoverOverCallback))]
	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.HoverOverCallbackOnGroup))]
	[HarmonyPatch(typeof(ShopItem), nameof(ShopItem.OnHoverOver))]
	private static bool PreHoverOver() {
		return _config.ItemTooltipStyle != ItemTooltipStyle.None;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ItemInfoWnd), nameof(ItemInfoWnd.Open))]
	private static void ItemInfoWnd_Open(ref string f_strInfo, ref int group_num) {
		if (_config.ItemTooltipStyle == ItemTooltipStyle.Minimal) {
			// omit item description in minimal tooltip
			f_strInfo = string.Empty;
		} else {
			f_strInfo = f_strInfo.TrimEnd();
		}
		// omit All N Color tooltip line
		group_num = 0;
	}

	private static void AddItemInfoWindowFileName(ItemInfoWnd itemInfoWindow, string menuFileName) {
		var descriptionLabel = itemInfoWindow.m_uiInfo;
		if (!string.IsNullOrEmpty(descriptionLabel.text)) {
			descriptionLabel.text += "\n\n";
		}
		descriptionLabel.text += $"[c][606060]{menuFileName}[-][/c]";
		itemInfoWindow.m_uiBase.height += _infoOffset;
	}

	private static void SetItemInfoWindowLayout() {
		if (_itemInfoWindow == null) return;

		var isMinimalTooltip = _config.ItemTooltipStyle == ItemTooltipStyle.Minimal;
		var titlePos = _itemInfoWindow.m_uiTitle.transform.localPosition;
		var infoPos = _itemInfoWindow.m_uiInfo.transform.localPosition;
		if (isMinimalTooltip) {
			titlePos.x = MinimalPosition;
			infoPos.x = MinimalPosition;
		} else {
			titlePos.x = _titleInitialPos;
			infoPos.x = _infoInitialPos;
			_itemInfoWindow.m_uiBase.width = _initialWidth;
		}
		_itemInfoWindow.m_uiTitle.transform.localPosition = titlePos;
		_itemInfoWindow.m_uiInfo.transform.localPosition = infoPos;

		var line = UTY.GetChildObject(_itemInfoWindow.gameObject, "Base/Line");
		line.SetActive(!isMinimalTooltip);
		_itemInfoWindow.m_uiIcon.gameObject.SetActive(!isMinimalTooltip);
	}

	private static void SetItemInfoWindowPosition(ItemInfoWnd itemInfoWindow, Vector3 basePosition, Vector3 offset, bool setLocalX = false) {
		var uiBase = itemInfoWindow.m_uiBase;

		// resize tooltip to fit contents
		if (_config.ItemTooltipStyle == ItemTooltipStyle.Minimal) {
			var width = Math.Max((int)itemInfoWindow.m_uiTitle.printedSize.x, (int)itemInfoWindow.m_uiInfo.printedSize.x) + (int)MinimalPosition * 2;
			uiBase.width = Math.Min(width, _initialWidth);
			uiBase.height = (int)(itemInfoWindow.m_uiInfo.printedSize.y + MinimalPosition * 2);
			// description text will be empty for shop upgrade items
			if (_config["AddTooltipFileName"] && itemInfoWindow.m_uiInfo.text != string.Empty) {
				uiBase.height += itemInfoWindow.m_uiInfo.fontSize * 2;
			}
		} else {
			uiBase.height = Math.Max(MinTotalTooltipHeight, BaseTooltipHeight + (int)itemInfoWindow.m_uiInfo.printedSize.y);
		}

		if (setLocalX) {
			offset.x -= _initialWidth / 2;
		} else {
			offset.x -= uiBase.width;
		}

		var transform = uiBase.gameObject.transform;

		transform.position = basePosition;

		var position = new Vector3(setLocalX ? basePosition.x : transform.localPosition.x, transform.localPosition.y);
		var vecOffsetPos = itemInfoWindow.m_vecOffsetPos + offset;

		transform.localPosition = position + vecOffsetPos;

		if (transform.localPosition.y - uiBase.height < -512) {
			vecOffsetPos.y *= -1;
			vecOffsetPos.y += uiBase.height;
		}

		if (transform.localPosition.x - uiBase.width < -1180) {
			vecOffsetPos.x *= -1;
			vecOffsetPos.x += 58;
			vecOffsetPos.x -= uiBase.width;
		}

		transform.localPosition = position + vecOffsetPos;
	}
}
