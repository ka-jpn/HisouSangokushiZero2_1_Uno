using HisouSangokushiZero2_1_Uno.MyUtil;

namespace HisouSangokushiZero2_1_Uno.Code {
	internal static class UIUtil {

	}
	internal static class 汎用拡張メソッド {
		internal static T MySetChildren<T>(this T panel,List<UIElement> elements) where T : Panel => panel.MyApplyA(v => v.Children.Clear()).MyApplyA(v => elements.ForEach(v.Children.Add));
		internal static T MySetChild<T>(this T control,UIElement element) where T : ContentControl => control.MyApplyA(v => v.Content=element);
		internal static Border MySetChild(this Border border,UIElement element) => border.MyApplyA(v => v.Child=element);
	}
}
