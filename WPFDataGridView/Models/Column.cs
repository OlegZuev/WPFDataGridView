namespace WPFDataGridView.Models {
    public class Column {
        public string Name { get; }
        
        public bool IsSerial { get; }

        public bool IsPrimaryKeyPart { get; }

        public Column(string name, bool isSerial = false, bool isPrimaryKeyPart = false) {
            Name = name;
            IsSerial = isSerial;
            IsPrimaryKeyPart = isPrimaryKeyPart;
        }
    }
}