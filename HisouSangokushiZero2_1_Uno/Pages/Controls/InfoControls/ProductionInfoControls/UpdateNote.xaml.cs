using Microsoft.UI.Xaml.Controls;
using System;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class UpdateNote:UserControl {
  private static Action close = MyUtil.MyUtil.nothing;
  internal UpdateNote() {
    InitializeComponent();
    MyInit();
    void MyInit() {
      CloseButton.Click += (_, _) => close();
    }
  }
  internal static void Init(Action closeAction) => close = closeAction;
}