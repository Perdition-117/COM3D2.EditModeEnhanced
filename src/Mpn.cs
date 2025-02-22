using System;

namespace COM3D2.EditModeEnhanced;

internal static class Mpn {
	public static readonly MPN body = (MPN)Enum.Parse(typeof(MPN), "body");
	public static readonly MPN nose = (MPN)Enum.Parse(typeof(MPN), "nose");
	public static readonly MPN facegloss = (MPN)Enum.Parse(typeof(MPN), "facegloss");
}
