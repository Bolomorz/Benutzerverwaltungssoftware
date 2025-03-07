using System.Globalization;
using Benutzerverwaltungssoftware.Data;

namespace Benutzerverwaltungssoftware.Pages;

internal static class DataModelValidation
{
    internal static string DecimalChars = "0123456789,.-+";
    internal static string FormulaChars = "W0123456789+-*/()";
    internal static string IntegerChars = "0123456789";
    internal static string DateFormat = "yyyy-MM-dd .,-/";
    internal static string PostalCodeFormat = "#####";

    internal static bool ValidateString(string value) => value is not null && value.Length > 0;
    internal static bool ValidateDecimal(string value) 
    {
        if(value is null || value.Length == 0) return false;
        bool separator = false;
        for(int i = 0; i < value.Length; i++) 
        {
            var isnmb = IsNumber(value[i]);
            var isspr = IsSeparator(value[i]);
            var issgn = IsSign(value[i]);
            if(!isnmb && !isspr && !issgn) return false;
            if(issgn && i > 0) return false; 
            if(isspr) { if(!separator) separator = true; else return false; }
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

        for(int i = 0; i < value.Length; i++)
        {
            if(IsNumber(value[i])) result += value[i];
            if(IsSeparator(value[i]) && !separator) {result += '.'; separator = true;}
            if(IsSign(value[i]) && i == 0) result += value[i];
        }
        return decimal.Parse(result, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, new CultureInfo("en-US"));
    }
    internal static DateOnly StringToDate(string value)
    {
        string result = "";
        foreach(var c in value)
        {
            if(DateLengthNumber(result.Length) && IsNumber(c)) result += c;
            if(DateLengthSeparator(result.Length) && IsDateSeparator(c)) result += '-';
        }
        return DateOnly.ParseExact(result, "yyyy-MM-dd", null);
    }
    internal static string DateToString(DateOnly date)
    {
        return $"{date.Year}-{date.Month}-{date.Day}";
    }

    private static bool IsSign(char c) => c == '-' || c == '+';
    private static bool IsNumber(char c) => c == '0' || c == '1' || c == '2' || c == '3' || c == '4' || c == '5' || c == '6' || c == '7' || c == '8' || c == '9';
    private static bool IsSeparator(char c) => c == '.' || c == ',';
    private static bool IsFormulaSign(char c) => c == 'W' || c == '+' || c == '*' || c == '/' || c == '-' || c == '(' || c == ')';
    private static bool IsDateSeparator(char c) => c == '.' || c == ',' || c == '-' || c == '/';
    private static bool DateLengthNumber(int length) => length == 0 || length == 1 || length == 2 || length == 3 || length == 5 || length == 6 || length == 8 || length == 9;
    private static bool DateLengthSeparator(int length) => length == 4 || length == 7;
}

