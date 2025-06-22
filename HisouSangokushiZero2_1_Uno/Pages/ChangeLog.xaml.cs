using Microsoft.UI.Xaml.Controls;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class ChangeLog:UserControl {
  internal ChangeLog() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(ChangeLog page) {
      page.Measure(new(double.PositiveInfinity,double.PositiveInfinity));
      page.Width = 980;
      page.Height = page.DesiredSize.Height;
    }
  }
}