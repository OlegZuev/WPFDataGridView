using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using WPFDataGridView.Models;

namespace WPFDataGridView.ViewModels {
    public class BooksPageViewModel : BaseViewModel {
        public ObservableCollection<Book> Books { get; set; }

        private ObservableCollection<Publisher> _publishers;

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
                _publishers = publishers;
                foreach (Book book in Books) {
                    book.InitializeValidator(Books, _publishers);
                }
            };
            semaphore.Release();

            DataGridAddingNewItemCommand = new DelegateCommand<AddingNewItemEventArgs>(DataGridAddingNewItem);

            PreviewKeyDownCommand = new DelegateCommand<object>(PreviewKeyDown);

            DataGridRowEditEndingCommand = new DelegateCommand<DataGridRowEditEndingEventArgs>(DataGridRowEditEnding);
        }

        public ICommand DataGridAddingNewItemCommand { get; set; }

        public ICommand PreviewKeyDownCommand { get; set; }

        public ICommand DataGridRowEditEndingCommand { get; set; }

        private void DataGridAddingNewItem(AddingNewItemEventArgs e) {
            e.NewItem = new Book {ReleaseDate = DateTime.Now};
            ((Book) e.NewItem).InitializeValidator(Books, _publishers);
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
                    ((IList<object>) sender.SelectedItems)
                        .Where(item => item is Book).ToList()
                        .ForEach(book => _tableInteraction.DeleteRowFromTable((Book) book));
                    return;
            }
        }

        private void DataGridRowEditEnding(DataGridRowEditEndingEventArgs e) {
            if (e.EditAction == DataGridEditAction.Cancel || !(e.Row.Item is Book book) || !book.Validator.IsValid)
                return;

            book.PublisherId = ISBNUtils.GetPublisherId(book.ISBN);
            if (e.Row.IsNewItem) {
                _tableInteraction.AddRowToTable(book);
            } else {
                _tableInteraction.UpdateRowToTable(book);
            }
        }
    }
}