using System.Data;

namespace Benutzerverwaltungssoftware.Data;

internal static class TransformFormula
{
    internal static decimal Transform(string formula, decimal input)
    {
        if(formula != "W" && formula != string.Empty)
        {
            var result = new DataTable().Compute(string.Format(Replace(formula), input), "").ToString();
            return result is not null ? decimal.Parse(result) : input;
        }
        return input;
    }
    private static string Replace(string formula)
    {
        string output = "";
        foreach(var c in formula) output += c == 'W' ? "{0}" : c != ' ' ? c : "";
        return output;
    }
}