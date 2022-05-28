using System;
using HarmonyLib;
using UnityEngine;

namespace COM3D2.EditModeEnhanced;

internal partial class EditModeEnhanced {
	private const int BaseTooltipHeight = 87;
	private const int MinTotalTooltipHeight = 128;

	// set internal tooltip anchor to top (instead of center) in order to facilitate easier resizing later
	[HarmonyPatch(typeof(ItemInfoWnd), nameof(ItemInfoWnd.Start))]
	[HarmonyPostfix]
	private static void ItemInfoWnd_OnStart(ItemInfoWnd __instance) {
		__instance.m_uiBase.pivot = UIWidget.Pivot.Top;
		var offset = __instance.m_uiBase.height / 2f;
		var offsetVector = new Vector3(0, offset);
		var line = UTY.GetChildObject(__instance.m_uiBase.gameObject, "Line", false);
		line.GetComponent<UISprite>().transform.localPosition -= offsetVector;
		__instance.m_uiTitle.transform.localPosition -= offsetVector;
		__instance.m_uiInfo.transform.localPosition -= offsetVector;
		__instance.m_uiIcon.transform.localPosition -= offsetVector;
		__instance.m_vecOffsetPos.y += offset;
		__instance.m_uiInfo.overflowMethod = UILabel.Overflow.ResizeHeight;
	}

	private static void AddItemInfoWindowFileName(ItemInfoWnd itemInfoWindow, SceneEdit.SMenuItem menuItem) {
		itemInfoWindow.m_uiInfo.text += "\n\n" + menuItem.m_strMenuFileName;
	}

	private static void SetItemInfoWindowPosition(ItemInfoWnd itemInfoWindow, Vector3 basePosition, Vector3 offset, bool setLocalX = false) {
		var uiBase = itemInfoWindow.m_uiBase;
		uiBase.height = Math.Max(MinTotalTooltipHeight, BaseTooltipHeight + (int)itemInfoWindow.m_uiInfo.printedSize.y);

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
		}

		transform.localPosition = position + vecOffsetPos;
	}
}
