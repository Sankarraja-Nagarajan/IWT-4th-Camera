using IWT.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IWT.ViewModel
{
    public class FormulaFieldDialogViewModel:ViewBaseModel
    {
        public FormulaFieldDialogViewModel(List<TabelColumns> tabelColumns)
        {
            _trTableColumns = tabelColumns;
        }
        private List<string> _operators=new List<string> { "*","+","-","/","%","(",")"};
        private List<TabelColumns> _trTableColumns;
        //public FormulaField FormulaField { get => _formulaField; set => SetProperty(ref _formulaField, value); }
        public List<TabelColumns> TRTableColumns { get => _trTableColumns; set => _trTableColumns = value; }
        public ICommand FormulaBtnCommand => new AnotherCommandImplementation(FormulaBtnClick);
        public ICommand FieldBtnCommand => new AnotherCommandImplementation(FieldBtnClick);
        public ICommand OperatorBtnCommand => new AnotherCommandImplementation(OperatorBtnClick);
        public ICommand ConstantBtnCommand => new AnotherCommandImplementation(ConstantBtnClick);
        public ICommand ClearBtnCommand => new AnotherCommandImplementation(ClearBtnClick);
        private string _formulaName;
        private string _fieldName;
        private string _operator;
        private string _constant;
        private string _formula;

        public List<string> Operators { get => _operators; set => _operators = value; }
        public string FormulaName { get => _formulaName; set => SetProperty(ref _formulaName, value); }
        public string FieldName { get => _fieldName; set => SetProperty(ref _fieldName, value); }
        public string Operator { get => _operator; set => SetProperty(ref _operator, value); }
        public string Constant { get => _constant; set => SetProperty(ref _constant, value); }
        public string Formula { get => _formula; set => SetProperty(ref _formula, value); }

        private void FormulaBtnClick(object _)
        {
            // Needs to be implemented
        }
        private void FieldBtnClick(object _)
        {
            Formula += FieldName;
            FieldName = "";
        }
        private void OperatorBtnClick(object _)
        {
            Formula += Operator;
            Operator = "";
        }
        private void ConstantBtnClick(object _)
        {
            Formula += Constant;
            Constant = "";
        }
        private void ClearBtnClick(object _)
        {
            Formula = "";
            FieldName = "";
            Operator = "";
            Constant = "";
        }
    }
}
