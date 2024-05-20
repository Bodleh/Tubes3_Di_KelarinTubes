using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace client
{
    public partial class MainWindow : Window
    {
        private HttpClient _client = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.AddHandler(DragDrop.DropEvent, Window_Drop);
            this.AddHandler(DragDrop.DragOverEvent, Window_DragOver);
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.DragEffects = DragDropEffects.Copy;
        }

        private async void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            var result = await ((TopLevel)this).StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select a file",
                AllowMultiple = false
            });

            if (result != null && result.Any())
            {
                var file = result.First();
                HandleFile(file.Path.LocalPath);
            }
        }

        private void Window_Drop(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.Files))
            {
                var files = e.Data.GetFiles();
                var file = files?.FirstOrDefault();
                if (file != null)
                {
                    HandleFile(file.Path.LocalPath);
                }
            }
        }

        private void HandleFile(string filePath)
        {
            var apiDataText = this.FindControl<TextBlock>("ApiDataText");
            if (apiDataText != null)
            {
                apiDataText.Text = $"File selected: {Path.GetFileName(filePath)}";
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var searchAlgorithmSwitch = this.FindControl<ToggleSwitch>("SearchAlgorithmSwitch");
            var apiDataText = this.FindControl<TextBlock>("ApiDataText");
            if (searchAlgorithmSwitch != null && apiDataText != null)
            {
                var algorithm = searchAlgorithmSwitch.IsChecked == true ? "BM" : "KMP";
                apiDataText.Text = $"Search using {algorithm} algorithm";
            }
        }

        private async void FetchData()
        {
            try
            {
                string url = "http://localhost:5099/api/sidikjari";
                HttpResponseMessage response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                Dispatcher.UIThread.Post(() =>
                {
                    var apiDataText = this.FindControl<TextBlock>("ApiDataText");
                    if (apiDataText != null)
                    {
                        apiDataText.Text = responseBody;
                    }
                });
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }
    }
}
