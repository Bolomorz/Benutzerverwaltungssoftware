using System.Globalization;
using Benutzerverwaltungssoftware.Data;

namespace Benutzerverwaltungssoftware.Pages;

internal static class DataModelValidation
{
    internal static string DecimalChars = "0123456789,.";
    internal static string FormulaChars = "W0123456789+-*/()";
    internal static string IntegerChars = "0123456789";
    internal static string DateFormat = "yyyy-MM-dd .,-/";
    internal static string PostalCodeFormat = "#####";

    internal static bool ValidateString(string value) => value is not null && value.Length > 0;
    internal static bool ValidateDecimal(string value) 
    {
        if(value is null || value.Length == 0) return false;
        bool separator = false;
        int decimalplaces = 0;
        foreach(var c in value) 
        {
            var isn = IsNumber(c);
            var iss = IsSeparator(c);
            if(!isn && !iss) return false;
            if(iss) { if(!separator) separator = true; else return false; }
            if(isn) { if(separator) { if(decimalplaces < 2) decimalplaces++; else return false; } }
        }
        return true;
    }
    internal static bool ValidateFormula(string formula)
    {
        if(formula is null || formula.Length == 0) return false;
        foreach(var c in formula) if(!IsNumber(c) && !IsFormulaSign(c)) return false;
        return true;
    }
    internal static bool ValidateInteger(string integer)
    {
        if(integer is null || integer.Length == 0) return false;
        foreach(var c in integer) if(!IsNumber(c)) return false;
        return true;
    }
    internal static bool ValidatePostalCode(string postalcode)
    {
        if(postalcode is null || postalcode.Length != 5) return false;
        foreach(var c in postalcode) if(!IsNumber(c)) return false;
        return true;
    }
    internal static bool ValidateDate(string date)
    {
        if(date is null || date.Length != 10) return false;
        if(!IsNumber(date[0])) return false;
        if(!IsNumber(date[1])) return false;
        if(!IsNumber(date[2])) return false;
        if(!IsNumber(date[3])) return false;
        if(!IsDateSeparator(date[4])) return false;
        if(!IsNumber(date[5])) return false;
        if(!IsNumber(date[6])) return false;
        if(!IsDateSeparator(date[7])) return false;
        if(!IsNumber(date[8])) return false;
        if(!IsNumber(date[9])) return false;
        return true;
    }

    internal static decimal StringToDecimal(string value)
    {
        string result = "";
        bool separator = false;
        int decimalplaces = 0;
        foreach(var c in value)
        {
            if(IsNumber(c)) if((separator && decimalplaces < 2) || !separator) {result += c; decimalplaces += separator ? 1 : 0;}
            if(IsSeparator(c)) if(!separator) {result += '.'; separator = true;}
        }
        return decimal.Parse(result, NumberStyles.AllowDecimalPoint, new CultureInfo("en-US"));
    }
    internal static DateTime StringToDate(string value)
    {
        string result = "";
        foreach(var c in value)
        {
            if(DateLengthNumber(result.Length) && IsNumber(c)) result += c;
            if(DateLengthSeparator(result.Length) && IsDateSeparator(c)) result += '-';
        }
        return DateTime.ParseExact(result, "yyyy-MM-dd", null);
    }
    internal static string DateToString(DateTime date)
    {
        return $"{date.Year}-{date.Month}-{date.Day}";
    }

    private static bool IsNumber(char c) => c == '0' || c == '1' || c == '2' || c == '3' || c == '4' || c == '5' || c == '6' || c == '7' || c == '8' || c == '9';
    private static bool IsSeparator(char c) => c == '.' || c == ',';
    private static bool IsFormulaSign(char c) => c == 'W' || c == '+' || c == '*' || c == '/' || c == '-' || c == '(' || c == ')';
    private static bool IsDateSeparator(char c) => c == '.' || c == ',' || c == '-' || c == '/';
    private static bool DateLengthNumber(int length) => length == 0 || length == 1 || length == 2 || length == 3 || length == 5 || length == 6 || length == 8 || length == 9;
    private static bool DateLengthSeparator(int length) => length == 4 || length == 7;
}

