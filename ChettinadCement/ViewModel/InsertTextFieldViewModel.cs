using IWT.DBCall;
using IWT.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWT.ViewModel
{
    public class InsertTextFieldViewModel : ViewBaseModel
    {
        private string _controlType="";
        public InsertTextFieldViewModel(TableList table,int controlType, ObservableCollection<TableList> tableLists)
        {
            _tableName = table.TableName;
            //if (table.Type == "1")
            //{
            //    _isMandatoryField = "Visible";
            //}
            _isMandatoryField = "Visible";
            if (controlType == 1)
            {
                _fieldTypes = new ObservableCollection<FieldType>() { new FieldType { DisplayType = "CHARACTER", Type = "NVARCHAR" }, new FieldType { DisplayType = "NUMERIC", Type = "FLOAT" } };
            }
            if (controlType == 0)
            {
                _controlType = "TextBox";
            }
            else if(controlType == 1)
            {
                _controlType = "Dropdown";
                IsDropdown = "Visible";
            }
            else if (controlType == 2)
            {
                _controlType = "DataDependancy";
                IsDataDependency = "Visible";
                IsDropdown = "Visible";
                _isMandatoryField = "Collapsed";
                _fieldTypes = new ObservableCollection<FieldType>() { new FieldType { DisplayType = "CHARACTER", Type = "NVARCHAR" }, new FieldType { DisplayType = "NUMERIC", Type = "INT" } };
            }
            else
            {
                _controlType = "Formula";
            }
            _tableList = tableLists;
        }
        private string _tableName = "";

        public string TableName { get => _tableName; set => _tableName = value; }
        public string IsMandatoryField { get => _isMandatoryField; set => _isMandatoryField = value; }
        public ObservableCollection<FieldType> FieldTypes { get => _fieldTypes; set => _fieldTypes = value; }
        public string ControlType { get => _controlType; set => _controlType = value; }
        public string IsDropdown { get => _isDropdown; set => _isDropdown = value; }
        public ObservableCollection<TableList> TableList { get => _tableList; set => _tableList = value; }
        public string SelectionTable { get => _selectionTable; set => _selectionTable = value; }
        public List<TabelColumns> TabelColumns { get => tabelColumns; set => SetProperty(ref tabelColumns, value); }
        public string IsDataDependency { get => _isDataDependency; set => _isDataDependency = value; }

        private string _isMandatoryField = "Collapsed";

        private string _isDropdown = "Collapsed";
        private string _isDataDependency = "Collapsed";
        private ObservableCollection<TableList> _tableList = new ObservableCollection<TableList>();
        private string _selectionTable;

        private ObservableCollection<FieldType> _fieldTypes = new ObservableCollection<FieldType>() { new FieldType { DisplayType="CHARACTER",Type="NVARCHAR"}, new FieldType { DisplayType = "DATE", Type = "DATETIME" }, new FieldType { DisplayType = "NUMERIC", Type = "FLOAT" } };
        private List<TabelColumns> tabelColumns = new List<TabelColumns>();
    }
}
