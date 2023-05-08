using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWT.Models
{
    public class TableList
    {
        public string TableName { get; set; }
        public string Type { get; set; }
    }
    public class FieldType
    {
        public string DisplayType { get; set; }
        public string Type { get; set; }
    }
    public class TicketDataTemplate
    {
        public TicketDataTemplate()
        {
            this.IsSelected = false;
        }
        public int ControlID { get; set; }
        public string ControlType { get; set; }
        public string ControlTable { get; set; }
        public string ControlTableRef { get; set; }
        public bool ControlDisableFirst { get; set; }
        public bool ControlDisableSecond { get; set; }
        public bool ControlDisableSingle { get; set; }
        public string ControlLoadStatusDisable { get; set; }
        public bool Dependent { get; set; }
        public string F_Table { get; set; }
        public string F_FieldName { get; set; }
        public string F_Caption { get; set; }
        public string F_Type { get; set; }
        public string F_Size { get; set; }
        public string SelectionBasis { get; set; }
        public bool Mandatory { get; set; }
        public string MandatoryStatus { get; set; }
        public bool IsSelected { get; set; }
    }
    public class FormulaField
    {
        public string FormulaName { get; set; }
        public string FieldName { get; set; }
        public string Operator { get; set; }
        public string Constant { get; set; }
        public string Formula { get; set; }
    }
    public class TabelColumns
    {
        public string column_name { get; set; }
    }
    public class CustomMastereBuilder
    {
        public string TableName { get; set; }
        public string TableImage { get; set; }
        public string TableText { get; set; }
        public List<CustomFieldBuilder> Fields { get; set; }
    }
    public class CustomFieldBuilder
    {
        public CustomFieldBuilder()
        {
            this.IsEnabled = true;
        }
        public string FieldTable { get; set; }
        public string FieldName { get; set; }
        public string FieldCaption { get; set; }
        public string FieldType { get; set; }
        public int FieldSize { get; set; }
        public string ControlType { get; set; }
        public string ControlTable { get; set; }
        public string ControlTableRef { get; set; }
        public string SelectionBasis { get; set; }
        public string RegName { get; set; }
        public string FieldImage { get; set; }
        public object Value { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsUsed { get; set; }
        public string[] Formula { get; set; }
        public ControlStatus MandatoryStatus { get; set; }
        public ControlStatus DisableStatus { get; set; }
    }
    public class FormulaTemplate
    {
        public int FormulaID { get; set; }
        public string FormulaName { get; set; }
        public string FormulaList { get; set; }
    }
    public class FieldDependency
    {
        public int ID { get; set; }
        public string LinkedName { get; set; }
        public string LinkedType { get; set; }
        public string LinkedEvent { get; set; }
        public string CustomName { get; set; }
        public string CustomcType { get; set; }
        public string ControlTable { get; set; }
        public string ControlTableRef { get; set; }
        public string SelectionBasis { get; set; }
    }
    public class ControlStatus
    {
        public bool SGT { get; set; }
        public bool FTE { get; set; }
        public bool FTL { get; set; }
        public bool STE { get; set; }
        public bool STL { get; set; }
        public ControlStatus(bool sgt=true,bool fte=true,bool ftl=true,bool ste=true,bool stl=true)
        {
            this.SGT = sgt;
            this.FTE = fte;
            this.FTL = ftl;
            this.STE = ste;
            this.STL = stl;
        }
    }
}
