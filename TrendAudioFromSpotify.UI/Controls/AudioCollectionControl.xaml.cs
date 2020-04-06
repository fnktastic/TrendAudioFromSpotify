using GalaSoft.MvvmLight.Messaging;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using TrendAudioFromSpotify.UI.Messaging;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Controls
{
    /// <summary>
    /// Логика взаимодействия для AudioCollectionControl.xaml
    /// </summary>
    public partial class AudioCollectionControl : DataGrid
    {
        public delegate Point GetPosition(IInputElement element);
        int rowIndex = -1;

        public AudioCollectionControl()
        {
            InitializeComponent();
        }

        private void DataGrid_Drop(object sender, DragEventArgs e)
        {
            if (rowIndex < 0)
                return;
            int index = this.GetCurrentRowIndex(e.GetPosition);
            if (index < 0)
                return;
            if (index == rowIndex)
                return;
            if (index == dataGrid.Items.Count)
            {
                MessageBox.Show("This row-index cannot be drop");
                return;
            }
            
            var songsCollection = dataGrid.Items;
            var changedSong = songsCollection[rowIndex] as Audio;

            //messenger part
            Messenger.Default.Send<ChangeSomgPositionMessage>(new ChangeSomgPositionMessage(changedSong, rowIndex, index)); ;
        }

        private void DataGrid_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            rowIndex = GetCurrentRowIndex(e.GetPosition);
            if (rowIndex < 0)
                return;
            dataGrid.SelectedIndex = rowIndex;
            var selectedEmp = dataGrid.Items[rowIndex] as Audio;
            if (selectedEmp == null)
                return;
            DragDropEffects dragdropeffects = DragDropEffects.Move;
            if (DragDrop.DoDragDrop(dataGrid, selectedEmp, dragdropeffects)
                                != DragDropEffects.None)
            {
                dataGrid.SelectedItem = selectedEmp;
            }
        }

        private bool GetMouseTargetRow(Visual theTarget, GetPosition position)
        {
            try
            {
                Rect rect = VisualTreeHelper.GetDescendantBounds(theTarget);
                Point point = position((IInputElement)theTarget);
                return rect.Contains(point);
            }
            catch
            {
                return false;
            }
        }
        private DataGridRow GetRowItem(int index)
        {
            if (dataGrid.ItemContainerGenerator.Status
                    != GeneratorStatus.ContainersGenerated)
                return null;
            return dataGrid.ItemContainerGenerator.ContainerFromIndex(index)
                                                            as DataGridRow;
        }
        private int GetCurrentRowIndex(GetPosition pos)
        {
            int curIndex = -1;
            for (int i = 0; i < dataGrid.Items.Count; i++)
            {
                DataGridRow itm = GetRowItem(i);

                if (itm == null) continue;

                if (GetMouseTargetRow(itm, pos))
                {
                    curIndex = i;
                    break;
                }
            }
            return curIndex;
        }
    }
}
