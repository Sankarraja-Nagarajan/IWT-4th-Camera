using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace IWT.Validators
{
    public class RequiredValidationRule : ValidationRule
    {
        public string FieldName { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string strValue = Convert.ToString(value);
            //if (string.IsNullOrEmpty(strValue))
            //{
                return string.IsNullOrWhiteSpace((value ?? "").ToString())? new ValidationResult(false, $"{FieldName} is mandatory") : ValidationResult.ValidResult;
            //}
            //else
            //{
            //    return new ValidationResult(true,null);
            //}
        }
    }
    public class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return string.IsNullOrWhiteSpace((value ?? "").ToString())
                ? new ValidationResult(false, "Field is required.")
                : ValidationResult.ValidResult;
        }
    }
    public class NumericValidationRule : ValidationRule
    {
        public Type ValidationType { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string strValue = Convert.ToString(value);

            if (string.IsNullOrEmpty(strValue)) 
            return string.IsNullOrWhiteSpace((value ?? "").ToString()) ? new ValidationResult(false, $"Value cannot be coverted to string.") : ValidationResult.ValidResult;
            bool canConvert = false;
            switch (ValidationType.Name)
            {

                case "Boolean":
                    bool boolVal = false;
                    canConvert = bool.TryParse(strValue, out boolVal);
                    return canConvert ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of boolean");
                case "Int32":
                    int intVal = 0;
                    canConvert = int.TryParse(strValue, out intVal);
                    return canConvert ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of Int32");
                case "Double":
                    double doubleVal = 0;
                    canConvert = double.TryParse(strValue, out doubleVal);
                    return canConvert ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of Double");
                case "Int64":
                    long longVal = 0;
                    canConvert = long.TryParse(strValue, out longVal);
                    return canConvert ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of Int64");
                default:
                    throw new InvalidCastException($"{ValidationType.Name} is not supported");
            }
        }
    }
}
