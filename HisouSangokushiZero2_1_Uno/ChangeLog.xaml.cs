using Microsoft.UI.Xaml.Controls;
namespace HisouSangokushiZero2_1_Uno;
public sealed partial class ChangeLog:UserControl {
  public ChangeLog(){
    InitializeComponent();
    MyInit(this);
  }
  private static void MyInit(ChangeLog page) {
    page.Measure(new(double.PositiveInfinity,double.PositiveInfinity));
    page.Width = 980;
    page.Height = page.DesiredSize.Height;
  }
}