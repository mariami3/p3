using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace p1_3
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaElement _mediaElement;
        private List<string> _audioFiles;
        private int _currentIndex;
        private bool _isRepeatEnabled;
        private bool _isShuffleEnabled;
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();

            _mediaElement = new MediaElement();
            _mediaElement.MediaOpened += MediaElement_MediaOpened;
            _mediaElement.MediaEnded += MediaElement_MediaEnded;
            _mediaElement.Volume = 0.5;

            AudioGrid.Children.Add(_mediaElement);

            // Инициализация списка аудиофайлов
            _audioFiles = new List<string>();

            // Инициализация текущего индекса
            _currentIndex = 0;

            _isRepeatEnabled = false;
            _isShuffleEnabled = false;

            _cts = new CancellationTokenSource();
            Task.Run(() => UpdatePositionAndTime(_cts.Token));
        }

        private void ChooseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _audioFiles = Directory.GetFiles(dialog.FileName, "C:\\Users\\Дарико\\Music").ToList();
                

                _audioFiles.Sort();

                Play(_audioFiles[0]);
            }
        }
        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            UpdatePositionAndTime(_cts.Token);

            _mediaElement.Play();
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            Next();
        }

        

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaElement.IsLoaded)
            {
                if (_mediaElement.IsPlaying)
                {
                    _mediaElement.Pause();
                    PlayPauseButton.Content = "►";
                }
                else
                {
                    _mediaElement.Play();
                    PlayPauseButton.Content = "❚❚";
                }
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            Previous();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            _isRepeatEnabled = !_isRepeatEnabled;
            RepeatButton.Background = _isRepeatEnabled ? Brushes.Green : Brushes.Gray;
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            _isShuffleEnabled = !_isShuffleEnabled;
            ShuffleButton.Background = _isShuffleEnabled ? Brushes.Green : Brushes.Gray;

            if (_isShuffleEnabled)
            {
                _audioFiles = _audioFiles.OrderBy(a => Guid.NewGuid()).ToList();
            }
            else
            {
                _audioFiles.Sort();
            }

            _currentIndex = _audioFiles.IndexOf(_mediaElement.Source.LocalPath);
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _mediaElement.Position = TimeSpan.FromSeconds(PositionSlider.Value);
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _mediaElement.Volume = VolumeSlider.Value;
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow(_audioFiles);
            historyWindow.ShowDialog();

            if (historyWindow.SelectedFile != null)
            {
                Play(historyWindow.SelectedFile);
            }
        }

        private void Play(string filePath)
        {
            _mediaElement.Source = new Uri(filePath);
            _mediaElement.Play();

            SongTitle.Text = Path.GetFileNameWithoutExtension(filePath);

            _currentIndex = _audioFiles.IndexOf(filePath);
        }

        private void Previous()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
            }
            else
            {
                if (_isRepeatEnabled)
                {
                    _currentIndex = _audioFiles.Count - 1;
                }
            }

            // Воспроизведение выбранной песни
            Play(_audioFiles[_currentIndex]);
        }

        private void Next()
        {
            // Переход к следующей песне
            if (_currentIndex < _audioFiles.Count - 1)
            {
                _currentIndex++;
            }
            else
            {
                if (_isRepeatEnabled)
                {
                    _currentIndex = 0;
                }
            }

            // Воспроизведение выбранной песни
            Play(_audioFiles[_currentIndex]);
        }

        private async Task UpdatePositionAndTime(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Обновление ползунка позиции
                if (_mediaElement.NaturalDuration.HasTimeSpan)
                {
                    PositionSlider.Maximum = _mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                    PositionSlider.Value = _mediaElement.Position.TotalSeconds;
                }

                // Обновление текста с текущей секундой и оставшимся временем
                if (_mediaElement.Source != null && _mediaElement.NaturalDuration.HasTimeSpan)
                {
                    TimeSpan currentTime = _mediaElement.Position;
                    TimeSpan remainingTime = _mediaElement.NaturalDuration.TimeSpan - currentTime;

                    CurrentTimeLabel.Text = currentTime.ToString(@"mm\:ss");
                    RemainingTimeLabel.Text = remainingTime.ToString(@"mm\:ss");
                }

                // Задержка перед следующим обновлением
                await Task.Delay(100, cancellationToken);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Остановка проигрывания и отмена потока
            _mediaElement.Stop();
            _cts.Cancel();
        }
    }

}





