﻿using PropertyChanged;
using SprayMaster.Helpers;
using SprayMaster.Services;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static SprayMaster.Models.Tool;

namespace SprayMaster.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class MainViewModel
    {
        public ICommand LoadImageCommand { get; private set; }
        public ICommand SaveImageAsCommand { get; private set; }
        public ICommand SaveImageCommand { get; private set; }
        public ICommand ClearAllCommand { get; private set; }
        public ICommand SaveStrokeDataCommand { get; set; }
        public ICommand ActivateSprayCommand { get; set; }
        public ICommand UseEraserCommand { get; set; }

        public ToolManager toolManager { get; set; }
        public ImageService imageService { get; set; }
        public SprayCanService sprayCanService { get; set; }

        public MainViewModel()
        {
            var inkCanvas = (Application.Current.MainWindow as MainWindow)?.canvasPanel;
            toolManager = new ToolManager();
            imageService = new ImageService();
            sprayCanService = new SprayCanService(toolManager.BrushSize, toolManager.DrawingAttributes);
            LoadImageCommand = new RelayCommand(imageService.LoadImage);
            SaveImageAsCommand = new RelayCommand(imageService.SaveAs);
            SaveImageCommand = new RelayCommand(imageService.Save);
            ClearAllCommand = new RelayCommand(imageService.ClearAll);
            ActivateSprayCommand = new RelayCommand(() => {
                toolManager.isSprayCanActive = !toolManager.isSprayCanActive;
                if (toolManager.CurrentTool == ToolType.Spray && toolManager.isSprayCanActive)
                {
                    sprayCanService.StartSpraying();
                    inkCanvas.EditingMode = InkCanvasEditingMode.None;
                    toolManager.SprayCan();
                }
                else
                {
                    sprayCanService.StopSpraying();
                    inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                }
            });
            UseEraserCommand = new RelayCommand(() =>
            {
                toolManager.isUseEraser = !toolManager.isUseEraser;
                toolManager.UseEraser(toolManager.isUseEraser);

            });
            SaveStrokeDataCommand = new RelayCommand(SaveStrokesData);
        }

        public void SaveStrokesData()
        {
            var inkCanvas = (Application.Current.MainWindow as MainWindow)?.canvasPanel;
            if (inkCanvas == null || imageService.CurrentImage == null) return;

            try
            {
                var strokesPath = Path.ChangeExtension(imageService.CurrentImage.Name, ".isf");
                using (var fs = new FileStream(strokesPath, FileMode.Create))
                {
                    inkCanvas.Strokes.Save(fs);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving strokes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
