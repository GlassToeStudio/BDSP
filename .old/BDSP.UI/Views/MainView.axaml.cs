using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using BDSP.UI.ViewModels;


namespace BDSP.UI.Views
{

    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }

        private Point? _dragStart;
        private BerryViewModel? _dragBerry;
        private bool _dragStarted;


        //private void BerryClicked(object? sender, RoutedEventArgs e)
        //{
        //    Debug.WriteLine("BerryClicked fired");

        //    if (sender is not Button btn)
        //        return;

        //    if (btn.DataContext is not BerryViewModel berry)
        //        return;

        //    if (DataContext is not MainViewModel vm)
        //        return;

        //    vm.SelectedBerry = berry.Berry;
        //}








        private void BerryPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            Debug.WriteLine("BerryPointerPressed");

            if (sender is not Control c ||
                c.DataContext is not BerryViewModel berry)
                return;

            _dragStart = e.GetPosition(this);
            _dragBerry = berry;
            _dragStarted = false;


            e.Pointer.Capture(c);

            e.Handled = true;
        }

        private async void BerryPointerMoved(object? sender, PointerEventArgs e)
        {

            if (_dragStart is null || _dragBerry is null || _dragStarted)
                return;

            var pos = e.GetPosition(this);
            var dx = pos.X - _dragStart.Value.X;
            var dy = pos.Y - _dragStart.Value.Y;

            if (dx * dx + dy * dy < 64)
                return;

            _dragStarted = true;

            var data = new DataObject();
            data.Set("berry", _dragBerry);

            await DragDrop.DoDragDrop(e, data, DragDropEffects.Copy);
        }


        private void BerryPointerReleased(object? sender, PointerReleasedEventArgs e)
        {

            Debug.WriteLine("BerryPointerReleased");

            if (sender is not Control c)
                return;

            e.Pointer.Capture(null);

            if (!_dragStarted &&
                _dragBerry is not null &&
                DataContext is MainViewModel vm)
            {
                vm.SelectedBerry = _dragBerry.Berry;
                vm.SelectedBerryInfo = new BerryViewModel(_dragBerry.Berry);
            }

            _dragStart = null;
            _dragBerry = null;
            _dragStarted = false;

            e.Handled = true;
        }



        private async void BerryDragStart(object? sender, PointerPressedEventArgs e)
        {
            Debug.WriteLine("BerryDragStart");

            if (sender is not Button btn ||
                btn.DataContext is not BerryViewModel berry)
                return;

            var data = new DataObject();
            data.Set("berry", berry);

            await DragDrop.DoDragDrop(e, data, DragDropEffects.Copy);
        }


        private void IngredientDragOver(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains("berry"))
                e.DragEffects = DragDropEffects.Copy;
            else
                e.DragEffects = DragDropEffects.None;

            e.Handled = true;
        }

        private void IngredientDrop(object? sender, DragEventArgs e)
        {

            if (sender is not Border border ||
                border.DataContext is not IngredientSlotViewModel slot ||
                DataContext is not MainViewModel vm ||
                !e.Data.Contains("berry"))
                return;

            if (e.Data.Get("berry") is not BerryViewModel berry)
                return;

            System.Diagnostics.Debug.WriteLine($"Dropped into slot {slot.Index}, berry={berry.Name}");

            vm.SetIngredient(slot.Index, berry);
            e.Handled = true;
        }



    }
}