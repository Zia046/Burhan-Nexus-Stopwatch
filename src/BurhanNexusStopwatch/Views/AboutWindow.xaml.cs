using System.Windows;
using BurhanNexusStopwatch.Models;

namespace BurhanNexusStopwatch.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();

        ProductText.Text = AppInfo.ProductName;
        DeveloperRun.Text = AppInfo.Developer;
        DescriptionText.Text = AppInfo.Description;
        DeveloperValue.Text = AppInfo.Developer;
        MobileValue.Text = AppInfo.Mobile;
        EmailValue.Text = AppInfo.Email;
        VersionValue.Text = AppInfo.Version;
        CopyrightText.Text = AppInfo.Copyright;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
