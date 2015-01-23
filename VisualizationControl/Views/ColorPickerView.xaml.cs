using Microsoft.Data.Visualization.Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Forms;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class ColorPickerView : System.Windows.Controls.UserControl
    {

        public ColorPickerView()
        {
            this.InitializeComponent();
        }

        private void RibbonButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerViewModel colorPickerViewModel = this.DataContext as ColorPickerViewModel;
            if (colorPickerViewModel == null)
                return;
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.FullOpen = true;
            colorDialog.CustomColors = Enumerable.ToArray<int>(Enumerable.Take<int>(Enumerable.Select<Color4F, int>((IEnumerable<Color4F>)colorPickerViewModel.CustomColors, new Func<Color4F, int>(ColorPickerView.Color4FToWindowsDialogCustomColor)), 16));
            if (colorDialog.ShowDialog() != DialogResult.OK)
                return;
            System.Drawing.Color color = colorDialog.Color;
            colorPickerViewModel.SelectedColor = new Color4F(1f, (float)color.R / (float)byte.MaxValue, (float)color.G / (float)byte.MaxValue, (float)color.B / (float)byte.MaxValue);
            colorPickerViewModel.CustomColors.Clear();
            colorPickerViewModel.CustomColors.AddRange(Enumerable.Select<int, Color4F>((IEnumerable<int>)colorDialog.CustomColors, new Func<int, Color4F>(ColorPickerView.WindowsDialogCustomColorToColor4F)));
        }

        private static int Color4FToWindowsDialogCustomColor(Color4F color)
        {
            return ((int)((double)color.B * (double)byte.MaxValue) << 16) + ((int)((double)color.G * (double)byte.MaxValue) << 8) + (int)((double)color.R * (double)byte.MaxValue);
        }

        private static Color4F WindowsDialogCustomColorToColor4F(int customColorIndex)
        {
            return new Color4F(1f, (float)(customColorIndex & (int)byte.MaxValue) / (float)byte.MaxValue, (float)(customColorIndex >> 8 & (int)byte.MaxValue) / (float)byte.MaxValue, (float)(customColorIndex >> 16 & (int)byte.MaxValue) / (float)byte.MaxValue);
        }

        private void ColorPickerButton_Initialized(object sender, RoutedEventArgs e)
        {
            DependencyObject focusScope = FocusManager.GetFocusScope((DependencyObject)this);
            IInputElement focusedElement = FocusManager.GetFocusedElement(focusScope);
            FocusManager.SetFocusedElement(focusScope, (IInputElement)this.ColorPickerButton);
            this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            FocusManager.SetFocusedElement(focusScope, focusedElement);
        }

        private void ColorPickerDropdownGallery_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.ColorPickerButton.InvalidateProperty(RibbonMenuButton.SmallImageSourceProperty);
        }

        private void ColorPickerDropdownGallery_Loaded(object sender, RoutedEventArgs e)
        {
            ColorPickerViewModel viewModel = this.DataContext as ColorPickerViewModel;
            if (viewModel == null)
                return;
            this.ColorPickerDropdownGallery.SelectedItem = (object)Enumerable.FirstOrDefault<Tuple<Color4F, string>>((IEnumerable<Tuple<Color4F, string>>)ColorPickerViewModel.Colors, (Func<Tuple<Color4F, string>, bool>)(n => (int)n.Item1.ToUint() == (int)viewModel.SelectedColor.ToUint()));
        }
    }
}
