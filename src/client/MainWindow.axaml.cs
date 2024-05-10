using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Net.Http;
using Avalonia.Threading;

namespace client;

public partial class MainWindow : Window
{
    private HttpClient _client = new HttpClient();

    public MainWindow()
    {
        InitializeComponent();
        FetchData();
    }

    private async void FetchData()
    {
        try
        {
            string url = "http://localhost:5099/tester";
            HttpResponseMessage response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            // Assume you have a TextBlock named ApiDataText in your XAML
            Dispatcher.UIThread.Post(() =>
            {
                ApiDataText.Text = responseBody;
            });
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }
}
