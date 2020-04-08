using GalaSoft.MvvmLight.Messaging;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
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
            int index = 0;

            if (rowIndex < 0)
            {
                index = this.GetCurrentRowIndex(e.GetPosition);

                var audioObject = e.Data.GetData(typeof(Audio));

                if (audioObject is Audio audio)
                {
                    Messenger.Default.Send<SendSongToPlaylistMessage>(new SendSongToPlaylistMessage(audio, index));
                }

                return;
            }

            index = this.GetCurrentRowIndex(e.GetPosition);

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
            Messenger.Default.Send<ChangeSomgPositionMessage>(new ChangeSomgPositionMessage(changedSong, rowIndex, index));

            rowIndex = -1;
        }

        private void DataGrid_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Ellipse || e.OriginalSource is Path)
            {
                //e.Handled = false;
            }
            else
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

                if (itm == null)
                {
                    curIndex = -1;
                    continue;
                }

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
