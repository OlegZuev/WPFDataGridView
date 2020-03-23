using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using WPFDataGridView.Models;

namespace WPFDataGridView.ViewModels {
    public class PublishersPageViewModel : BaseViewModel {
        public ObservableCollection<Publisher> Publishers { get; set; }

        private readonly TableInteraction<Publisher> _tableInteraction;

        public PublishersPageViewModel() {
            Publishers = new ObservableCollection<Publisher>();
            _tableInteraction = new TableInteraction<Publisher>("publishers",
                                                                new[] {
                                                                    new Column("id", true, true),
                                                                    new Column("name"),
                                                                    new Column("address"),
                                                                });
            _tableInteraction.InitializeToTable(Publishers);

            DataGridAddingNewItemCommand = new DelegateCommand<AddingNewItemEventArgs>(DataGridAddingNewItem);

            DataGridRowEditEndingCommand = new DelegateCommand<DataGridRowEditEndingEventArgs>(DataGridRowEditEnding);

            PreviewKeyDownCommand = new DelegateCommand<object>(PreviewKeyDown);

            var semaphore = new Semaphore(0, 1, "PublishersInitialion");
            semaphore.WaitOne();
            PublishersInitialized?.Invoke(Publishers);
        }

        public ICommand DataGridAddingNewItemCommand { get; set; }

        public ICommand PreviewKeyDownCommand { get; set; }

        public ICommand DataGridRowEditEndingCommand { get; set; }


        public static event Action<ObservableCollection<Publisher>> PublishersInitialized;

        private static void DataGridAddingNewItem(AddingNewItemEventArgs e) {
            e.NewItem = new Publisher();
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
                        .Where(item => item is Publisher).ToList()
                        .ForEach(book => _tableInteraction.DeleteRowFromTable((Publisher) book));
                    return;
            }
        }

        private void DataGridRowEditEnding(DataGridRowEditEndingEventArgs e) {
            if (e.EditAction == DataGridEditAction.Cancel || !(e.Row.Item is Publisher publisher) ||
                !publisher.Validator.IsValid)
                return;

            if (e.Row.IsNewItem) {
                _tableInteraction.AddRowToTable(publisher);
            } else {
                _tableInteraction.UpdateRowToTable(publisher);
            }
        }
    }
}