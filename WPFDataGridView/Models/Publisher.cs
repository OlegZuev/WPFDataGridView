using System;
using System.ComponentModel;
using PropertyChanged;
using ReactiveValidation;
using ReactiveValidation.Extensions;

namespace WPFDataGridView.Models {
    [AddINotifyPropertyChangedInterface]
    public class Publisher : ValidatableObject, IEditableObject, ITable {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        private Publisher _backupCopy;

        private bool _inEdit;

        public Publisher() {
            Validator = GetValidator();
        }

        private IObjectValidator GetValidator() {
            var builder = new ValidationBuilder<Publisher>();

            builder.RuleFor(publisher => publisher.Name)
                   .NotEmpty()
                   .WithMessage("Name can't be empty");
            builder.RuleFor(publisher => publisher.Address)
                   .NotEmpty()
                   .WithMessage("Address can't be empty");

            return builder.Build(this);
        }

        public void BeginEdit() {
            if (_inEdit) return;
            _inEdit = true;
            _backupCopy = MemberwiseClone() as Publisher;
        }

        public void CancelEdit() {
            if (!_inEdit) return;
            _inEdit = false;
            Id = _backupCopy.Id;
            Name = _backupCopy.Name;
            Address = _backupCopy.Address;
        }

        public void EndEdit() {
            if (!_inEdit) return;
            _inEdit = false;
            _backupCopy = null;
        }

        [SuppressPropertyChangedWarnings]
        public object this[string key, bool getOldValue] {
            get {
                switch (key) {
                    case "id":
                        return !getOldValue ? Id : _backupCopy.Id;
                    case "name":
                        return !getOldValue ? Name : _backupCopy.Name;
                    case "address":
                        return !getOldValue ? Address : _backupCopy.Address;
                }

                throw new ArgumentException($"Key {key} not found");
            }
            set {
                switch (key) {
                    case "id":
                        Id = (int) value;
                        return;
                    case "name":
                        Name = (string) value;
                        return;
                    case "address":
                        Address = (string) value;
                        return;
                }

                throw new ArgumentException($"Key {key} not found");
            }
        }
    }
}