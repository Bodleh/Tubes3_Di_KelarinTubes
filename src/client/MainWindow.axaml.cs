using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace client
{
    public partial class MainWindow : Window
    {
        private HttpClient _client = new HttpClient();
        private byte[]? _fileData;
        private Bitmap? _selectedImage;

        public bool IsBMAlgorithm { get; set; } = true;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.AddHandler(DragDrop.DropEvent, Window_Drop);
            this.AddHandler(DragDrop.DragOverEvent, Window_DragOver);
        }

        private void Window_DragOver(object? sender, DragEventArgs e)
        {
            e.DragEffects = DragDropEffects.Copy;
        }

        private async void ChooseFileButton_Click(object? sender, RoutedEventArgs e)
        {
            var result = await ((TopLevel)this).StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select a file",
                AllowMultiple = false
            });

            if (result != null && result.Any())
            {
                var file = result.First();
                await HandleFile(file.Path.LocalPath);
            }
        }

        private async void Window_Drop(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.Files))
            {
                var files = e.Data.GetFiles();
                var file = files?.FirstOrDefault();
                if (file != null)
                {
                    await HandleFile(file.Path.LocalPath);
                }
            }
        }

        private async Task HandleFile(string filePath)
        {
            var apiDataText = this.FindControl<TextBlock>("ApiDataText");
            if (apiDataText != null)
            {
                apiDataText.Text = $"File selected: {Path.GetFileName(filePath)}";
            }

            // Read the file as a byte array
            _fileData = await File.ReadAllBytesAsync(filePath);

            // Update the selected image
            _selectedImage = new Bitmap(filePath);

            // Update the Image control
            var inputImage = this.FindControl<Image>("InputImage");
            if (inputImage != null)
            {
                inputImage.Source = _selectedImage;
            }
        }

        private string ConvertToBinaryString(byte[] data)
        {
            StringBuilder binaryString = new StringBuilder();
            foreach (byte b in data)
            {
                binaryString.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }
            return binaryString.ToString();
        }

        private async void SearchButton_Click(object? sender, RoutedEventArgs e)
        {
            var apiDataText = this.FindControl<TextBlock>("ApiDataText");

            if (_fileData == null)
            {
                if (apiDataText != null)
                {
                    apiDataText.Text = "No image chosen";
                }
                return;
            }

            string binaryData = ConvertToBinaryString(_fileData);

            var requestBody = new
            {
                data = binaryData,
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            try
            {
                var endpoint = IsBMAlgorithm ? "http://localhost:5099/api/bm" : "http://localhost:5099/api/kmp";
                var response = await _client.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                Dispatcher.UIThread.Post(() =>
                {
                    if (apiDataText != null)
                    {
                        apiDataText.Text = $"Response: {responseBody}";
                    }
                });
            }
            catch (HttpRequestException ex)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (apiDataText != null)
                    {
                        apiDataText.Text = $"Error: {ex.Message}";
                    }
                });
            }
        }
    }
}
