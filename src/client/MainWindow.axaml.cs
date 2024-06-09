using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.Controls.Shapes; // Using Shapes for Ellipse
using server;
using IOPath = System.IO.Path; // Alias for System.IO.Path
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
        private DispatcherTimer _animationTimer;
        private RotateTransform _rotateTransform;
        private double _rotationAngle = 0;

        public bool IsKMPAlgorithm { get; set; } = true;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = this;

            var loadingEllipse = this.FindControl<Ellipse>("LoadingEllipse");
            if (loadingEllipse != null)
            {
                _rotateTransform = loadingEllipse.RenderTransform as RotateTransform;
            }
            
            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(30)
            };
            _animationTimer.Tick += (s, e) =>
            {
                _rotationAngle = (_rotationAngle + 5) % 360;
                if (_rotateTransform != null)
                {
                    _rotateTransform.Angle = _rotationAngle;
                }
            };
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
                AllowMultiple = false,
                FileTypeFilter = new [] {FilePickerFileTypes.ImageAll}
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

        private string TruncateText(string text, int lengthLimit)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new string(' ', lengthLimit);
            }

            if (text.Length <= lengthLimit)
            {
                return text;
            }

            string truncatedText = text.Substring(0, lengthLimit - 3);
            return truncatedText + "...";
        }

        private async void SearchButton_Click(object? sender, RoutedEventArgs e)
        {
            var apiDataText = this.FindControl<TextBlock>("ApiDataText");
            var searchTimeText = this.FindControl<TextBlock>("SearchTimeText");
            var accuracyText = this.FindControl<TextBlock>("AccuracyText");
            var biodataPanel = this.FindControl<StackPanel>("BiodataPanel");
            var outputImage = this.FindControl<Image>("OutputImage");
            var loadingOverlay = this.FindControl<Border>("LoadingOverlay");

            if (apiDataText != null)
            {
                apiDataText.IsVisible = false;
            }

            if (_fileData == null)
            {
                if (apiDataText != null)
                {
                    apiDataText.Text = "No image chosen";
                    apiDataText.IsVisible = true;
                }
                return;
            }

            string binaryData = Convert.ToBase64String(_fileData);

            var requestBody = new
            {
                data = binaryData,
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            try
            {
                Dispatcher.UIThread.Post(() =>
                {
                    loadingOverlay?.SetValue(IsVisibleProperty, true);
                    _animationTimer.Start();
                });
                
                var endpoint = IsKMPAlgorithm ? "http://localhost:5099/api/kmp" : "http://localhost:5099/api/bm";
                var response = await _client.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                var searchResult = JsonSerializer.Deserialize<SearchResult>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var realname = searchResult?.SidikJari?.Nama;
                var matchPercentage = searchResult?.MatchPercentage ?? 0;
                var imagePath = searchResult?.SidikJari?.BerkasCitra != null ? IOPath.Combine("..", "..", searchResult.SidikJari.BerkasCitra) : null;
                if (realname != null)
                {
                    var secondRequestBody = new StringRequest
                    {
                        Realname = realname,
                        IsKMP = IsKMPAlgorithm
                    };

                    var secondJson = JsonSerializer.Serialize(secondRequestBody);
                    var secondContent = new StringContent(secondJson);
                    secondContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var secondEndpoint = "http://localhost:5099/api/biosearch";
                    var secondResponse = await _client.PostAsync(secondEndpoint, secondContent);
                    secondResponse.EnsureSuccessStatusCode();
                    var secondResponseBody = await secondResponse.Content.ReadAsStringAsync();

                    var biodataResponse = JsonSerializer.Deserialize<StringResult>(secondResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    stopwatch.Stop();
                    var elapsedMs = stopwatch.ElapsedMilliseconds;

                    Dispatcher.UIThread.Post(() =>
                    {
                        if (searchTimeText != null)
                        {
                            searchTimeText.Text = $"Waktu Pencarian: {elapsedMs} ms";
                        }

                        if (accuracyText != null)
                        {
                            accuracyText.Text = $"Persentase Kecocokan: {matchPercentage}%";
                        }

                        if (biodataPanel != null && biodataResponse != null)
                        {
                            var nikText = this.FindControl<TextBlock>("NikText");
                            var namaText = this.FindControl<TextBlock>("NamaText");
                            var tempatLahirText = this.FindControl<TextBlock>("TempatLahirText");
                            var tanggalLahirText = this.FindControl<TextBlock>("TanggalLahirText");
                            var jenisKelaminText = this.FindControl<TextBlock>("JenisKelaminText");
                            var golonganDarahText = this.FindControl<TextBlock>("GolonganDarahText");
                            var alamatText = this.FindControl<TextBlock>("AlamatText");
                            var agamaText = this.FindControl<TextBlock>("AgamaText");
                            var statusPerkawinanText = this.FindControl<TextBlock>("StatusPerkawinanText");
                            var pekerjaanText = this.FindControl<TextBlock>("PekerjaanText");
                            var kewarganegaraanText = this.FindControl<TextBlock>("KewarganegaraanText");

                            if (nikText != null)
                                nikText.Text = $"{biodataResponse.Biodata?.NIK ?? "N/A"}";
                            if (namaText != null)
                                namaText.Text = $"{realname ?? "N/A"}";
                            if (tempatLahirText != null)
                            {
                                var truncatedTempatLahir = TruncateText(biodataResponse.Biodata?.TempatLahir ?? "N/A", 15);
                                var formattedTanggalLahir = biodataResponse.Biodata?.TanggalLahir.HasValue == true 
                                    ? biodataResponse.Biodata.TanggalLahir.Value.ToString("dd-MM-yyyy") 
                                    : "N/A";
                                tempatLahirText.Text = $"{truncatedTempatLahir}, {formattedTanggalLahir}";
                            }
                            // if (tanggalLahirText != null)
                            //     tanggalLahirText.Text = $"{(biodataResponse.Biodata?.TanggalLahir.HasValue == true ? biodataResponse.Biodata.TanggalLahir.Value.ToString("yyyy-MM-dd") : "N/A")}";
                            if (jenisKelaminText != null)
                                jenisKelaminText.Text = $"{biodataResponse.Biodata?.JenisKelamin ?? "N/A"}";
                            if (golonganDarahText != null)
                                golonganDarahText.Text = $"{biodataResponse.Biodata?.GolonganDarah ?? "N/A"}";
                            if (alamatText != null)
                                alamatText.Text = $"{biodataResponse.Biodata?.Alamat ?? "N/A"}";
                            if (agamaText != null)
                                agamaText.Text = $"{biodataResponse.Biodata?.Agama ?? "N/A"}";
                            if (statusPerkawinanText != null)
                                statusPerkawinanText.Text = $"{biodataResponse.Biodata?.StatusPerkawinan ?? "N/A"}";
                            if (pekerjaanText != null){
                                var pekerjaanTextContent = biodataResponse.Biodata?.Pekerjaan ?? "N/A";
                                pekerjaanText.Text = TruncateText(pekerjaanTextContent, 30);
                            }
                            if (kewarganegaraanText != null)
                                kewarganegaraanText.Text = $"{biodataResponse.Biodata?.Kewarganegaraan ?? "N/A"}";
                        }
                        
                        if (outputImage != null && imagePath != null)
                        {
                            outputImage.Source = new Bitmap(imagePath);
                        }

                        loadingOverlay?.SetValue(IsVisibleProperty, false);
                        _animationTimer.Stop();
                    });
                }
                else
                {
                    stopwatch.Stop();
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (apiDataText != null)
                        {
                            apiDataText.Text = $"Error: sidikjari name not found in the response.";
                            apiDataText.IsVisible = true;
                        }

                        if (searchTimeText != null)
                        {
                            searchTimeText.Text = $"Waktu Pencarian: {stopwatch.ElapsedMilliseconds} ms";
                        }

                        loadingOverlay?.SetValue(IsVisibleProperty, false);
                        _animationTimer.Stop();
                    });
                }
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                Dispatcher.UIThread.Post(() =>
                {
                    if (apiDataText != null)
                    {
                        apiDataText.Text = $"Error: {ex.Message}";
                        apiDataText.IsVisible = true;
                    }

                    if (searchTimeText != null)
                    {
                        searchTimeText.Text = $"Waktu Pencarian: {stopwatch.ElapsedMilliseconds} ms";
                    }

                    loadingOverlay?.SetValue(IsVisibleProperty, false);
                    _animationTimer.Stop();
                });
            }
        }
    }
}
