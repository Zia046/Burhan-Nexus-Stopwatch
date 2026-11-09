using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BurhanNexusStopwatch.Models;

namespace BurhanNexusStopwatch.Views;

public partial class AboutWindow : Window
{
    private const string CopyGlyph = "";
    private const string CheckGlyph = "";

    private readonly DispatcherTimer _feedbackTimer = new() { Interval = TimeSpan.FromSeconds(2) };

    public AboutWindow()
    {
        InitializeComponent();

        ProductText.Text = AppInfo.ProductName;
        VersionText.Text = $"Version {AppInfo.Version}";
        DescriptionText.Text = AppInfo.Description;
        DeveloperText.Text = AppInfo.Developer;
        MobileLink.Content = AppInfo.Mobile;
        EmailLink.Content = AppInfo.Email;
        CopyrightText.Text = AppInfo.Copyright;

        _feedbackTimer.Tick += (_, _) => ClearFeedback();
        Closed += (_, _) => _feedbackTimer.Stop();
    }

    private void MobileLink_Click(object sender, RoutedEventArgs e)
    {
        if (!TryOpen($"tel:{AppInfo.Mobile}"))
        {
            CopyToClipboard(AppInfo.Mobile, CopyMobileButton, "No calling app found. Number copied.");
        }
    }

    private void EmailLink_Click(object sender, RoutedEventArgs e)
    {
        if (!TryOpen($"mailto:{AppInfo.Email}?subject={Uri.EscapeDataString(AppInfo.ProductName)}"))
        {
            CopyToClipboard(AppInfo.Email, CopyEmailButton, "No email app found. Address copied.");
        }
    }

    private void CopyMobile_Click(object sender, RoutedEventArgs e)
        => CopyToClipboard(AppInfo.Mobile, CopyMobileButton, "Mobile number copied to clipboard");

    private void CopyEmail_Click(object sender, RoutedEventArgs e)
        => CopyToClipboard(AppInfo.Email, CopyEmailButton, "Email address copied to clipboard");

    private static bool TryOpen(string uri)
    {
        try
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
            return true;
        }
        catch
        {
            return false; // No app registered for tel: / mailto:
        }
    }

    private void CopyToClipboard(string text, Button button, string message)
    {
        // The clipboard can be briefly locked by another app; retry a few times.
        for (int attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                Clipboard.SetText(text);
                ShowFeedback(button, message);
                return;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                Thread.Sleep(40);
            }
        }
        ShowFeedback(null, "Could not access the clipboard. Please try again.");
    }

    private void ShowFeedback(Button? button, string message)
    {
        ClearFeedback();
        if (button is not null) button.Content = CheckGlyph;
        StatusText.Text = message;
        _feedbackTimer.Stop();
        _feedbackTimer.Start();
    }

    private void ClearFeedback()
    {
        _feedbackTimer.Stop();
        CopyMobileButton.Content = CopyGlyph;
        CopyEmailButton.Content = CopyGlyph;
        StatusText.Text = string.Empty;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
