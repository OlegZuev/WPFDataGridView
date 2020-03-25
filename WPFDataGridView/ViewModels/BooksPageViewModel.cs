using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WPFDataGridView.Models;

namespace WPFDataGridView.ViewModels {
    public class BooksPageViewModel : BaseViewModel {
        public ObservableCollection<Book> Books { get; set; }

        public ObservableCollection<Publisher> Publishers { get; set; }

        private readonly TableInteraction<Book> _tableInteraction;

        public BooksPageViewModel() {
            Books = new ObservableCollection<Book>();
            _tableInteraction = new TableInteraction<Book>("books",
                                                           new[] {
                                                               new Column("isbn", isPrimaryKeyPart: true),
                                                               new Column("publisher_id"),
                                                               new Column("name"),
                                                               new Column("release_date"),
                                                               new Column("age_restriction"),
                                                           });
            _tableInteraction.InitializeToTable(Books);

            var semaphore = new Semaphore(0, 1, "PublishersInitialion");
            PublishersPageViewModel.PublishersInitialized += publishers => {
                Publishers = publishers;
                foreach (Book book in Books) {
                    book.InitializeValidator(Books, Publishers);
                }
            };
            semaphore.Release();

            DataGridAddingNewItemCommand = new DelegateCommand<AddingNewItemEventArgs>(DataGridAddingNewItem);

            PreviewKeyDownCommand = new DelegateCommand<object>(PreviewKeyDown);

            DataGridRowEditEndingCommand = new DelegateCommand<DataGridRowEditEndingEventArgs>(DataGridRowEditEnding);

            DataGridCellEditEndingCommand =
                new DelegateCommand<DataGridCellEditEndingEventArgs>(DataGridCellEditEnding);
        }

        public ICommand DataGridAddingNewItemCommand { get; set; }

        public ICommand PreviewKeyDownCommand { get; set; }

        public ICommand DataGridRowEditEndingCommand { get; set; }

        public ICommand DataGridCellEditEndingCommand { get; set; }

        private void DataGridAddingNewItem(AddingNewItemEventArgs e) {
            e.NewItem = new Book {ReleaseDate = DateTime.Now};
            ((Book) e.NewItem).InitializeValidator(Books, Publishers);
        }

        private void PreviewKeyDown(object eventArgs) {
            var tempEventArgs = (object[]) eventArgs;
            if (!(tempEventArgs[0] is KeyEventArgs e && tempEventArgs[1] is DataGrid sender)) {
                throw new ArgumentException("Bad arguments in PreviewKeyDown");
            }

            switch (e.Key) {
                case Key.Escape:
                    sender.CancelEdit();
                    sender.CancelEdit();
                    return;
                case Key.Delete:
                    if (sender.SelectedItem is Book book) {
                        _tableInteraction.DeleteRowFromTable(book);
                    }

                    return;
            }
        }

        private void DataGridRowEditEnding(DataGridRowEditEndingEventArgs e) {
            if (e.EditAction == DataGridEditAction.Cancel || !(e.Row.Item is Book book) || !book.Validator.IsValid)
                return;

            if (e.Row.IsNewItem) {
                _tableInteraction.AddRowToTable(book);
            } else {
                _tableInteraction.UpdateRowToTable(book);
            }
        }

        private static void DataGridCellEditEnding(DataGridCellEditEndingEventArgs e) {
            if ((string) e.Column.Header != "Publisher") return;
            var comboBox = (ComboBox) e.EditingElement;
            double contentWidth = MeasureString(((Publisher) comboBox.SelectedItem).Name, comboBox).Width;
            double headerWidth = MeasureString(e.Column.Header.ToString(), comboBox).Width;
            e.Column.Width = (contentWidth > headerWidth ? contentWidth : headerWidth) + 12;
        }
        private static Size MeasureString(string candidate, Control element) {
            var formattedText = new FormattedText(candidate,
                                                  CultureInfo.CurrentCulture,
                                                  FlowDirection.LeftToRight,
                                                  new Typeface(element.FontFamily, element.FontStyle,
                                                               element.FontWeight, element.FontStretch),
                                                  element.FontSize,
                                                  Brushes.Black,
                                                  new NumberSubstitution(),
                                                  1);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}