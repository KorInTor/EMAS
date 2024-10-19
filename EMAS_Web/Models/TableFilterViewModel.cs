using DocumentFormat.OpenXml.Wordprocessing;

namespace EMAS_Web.Models
{
    public class TableFilterViewModel
    {
        public string TableId { get; set; }
        public string ColumnIndex { get; set; }
		public string TableFilterObject { get; set; }
        public string ColumnHeader { get; set; }
		public bool IsSorting {  get; set; }
        public bool IsNumericRange { get; set; }
        public bool IsFiltering { get; set; }
        public bool IsDateRange { get; set; }

        public IEnumerable<string>? uniqueValues;

	}

    public class TableFilterViewModelBuilder
    {
        private TableFilterViewModel _tableFilterMenu;
        private string _tableId;
		private string _tableFilterObjectName;

		public TableFilterViewModelBuilder(string tableId, string tableFilterObjectName, int columnIndex, string columnHeader) 
        {
            _tableFilterMenu = new();
            _tableFilterMenu.IsSorting = false;
            _tableFilterMenu.IsNumericRange = false;
            _tableFilterMenu.IsFiltering = false;
            _tableFilterMenu.IsDateRange = false;

            _tableId = tableId;
            _tableFilterObjectName = tableFilterObjectName;

            _tableFilterMenu.TableId = tableId;
            _tableFilterMenu.TableFilterObject = tableFilterObjectName;
			_tableFilterMenu.ColumnIndex = columnIndex.ToString();
			_tableFilterMenu.ColumnHeader = columnHeader;
		}

		public TableFilterViewModelBuilder Clear(string columnIndex, string columnHeader)
        {
			_tableFilterMenu = new();
			_tableFilterMenu.IsSorting = false;
			_tableFilterMenu.IsNumericRange = false;
			_tableFilterMenu.IsFiltering = false;
			_tableFilterMenu.IsDateRange = false;

			_tableFilterMenu.TableId = _tableId;
			_tableFilterMenu.TableFilterObject = _tableFilterObjectName;
			_tableFilterMenu.ColumnIndex = columnIndex.ToString();
			_tableFilterMenu.ColumnHeader = columnHeader;
			return this;
		}

        public TableFilterViewModel Build()
        {
            if(_tableFilterMenu.ColumnIndex == null)
            {
                throw new ArgumentException("Column Index is not set");
            }
			if (_tableFilterMenu.ColumnHeader == string.Empty)
            {
                _tableFilterMenu.ColumnHeader = $"{_tableFilterMenu.TableId}.{_tableFilterMenu.ColumnIndex}";
			}

			return _tableFilterMenu;
        }

        public TableFilterViewModelBuilder Sortable()
        {
            _tableFilterMenu.IsSorting = true;
            return this;
        }

        public TableFilterViewModelBuilder NumericRange()
        {
            if (_tableFilterMenu.IsDateRange)
            {
                throw new ArgumentException("Menu already set to Date");
            }

            _tableFilterMenu.IsNumericRange = true;
            return this;
        }

        public TableFilterViewModelBuilder Filtering(IEnumerable<string> uniqueValues)
        {
            _tableFilterMenu.IsFiltering = true;
            _tableFilterMenu.uniqueValues = uniqueValues;
            return this;
        }

        public TableFilterViewModelBuilder DateRange()
        {
            if (_tableFilterMenu.IsNumericRange)
            {
                throw new ArgumentException("Menu already set to Numeric");
            }

            _tableFilterMenu.IsDateRange = true;
            return this;
        }
    }
}
