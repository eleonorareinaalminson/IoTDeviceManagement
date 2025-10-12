using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HMI.Views;
public partial class DeviceCardView : UserControl
{
    public DeviceCardView()
    {
        InitializeComponent();
    }

    private void SpeedSlider_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is Slider slider && DataContext is ViewModels.DeviceCardViewModel vm)
        {
            System.Diagnostics.Debug.WriteLine($"Slider MouseUp: {slider.Value}");
            vm.SetValueCommand.Execute(slider.Value);
        }
    }
}