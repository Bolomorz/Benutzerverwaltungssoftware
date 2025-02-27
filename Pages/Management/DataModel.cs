using Benutzerverwaltungssoftware.Data;

namespace Benutzerverwaltungssoftware.Pages.Management;

public class CustomerModel
{
    public required string GivenName { get; set; }
    public required string Surname { get; set; }
    public required string Street { get; set; }
    public required string StreetNumber { get; set; }
    public required string PostalCode { get; set; }
    public required string City { get; set; }
    public required string Birthday { get; set; }
    public required string JoinDate { get; set; }
    public required string Book { get; set; }

    public ReturnDialog ValidateBooking()
    {
        if(!DataModelValidation.ValidateDecimal(Book)) return new(new(MID.FailedValidation, false, $"Geben Sie einen korrekten Buchungswert ein. Zeichen: {DataModelValidation.DecimalChars}"));
        return new(Message.ValidationSucccessful);
    }
    public ReturnDialog ValidateSaving()
    {
        if(!DataModelValidation.ValidateString(GivenName)) return new(new(MID.FailedValidation, false, $"Geben Sie einen Vornamen ein."));
        if(!DataModelValidation.ValidateString(Surname)) return new(new(MID.FailedValidation, false, $"Geben Sie einen Nachnamen ein."));
        if(!DataModelValidation.ValidateString(Street)) return new(new(MID.FailedValidation, false, $"Geben Sie eine Strasse ein."));
        if(!DataModelValidation.ValidateInteger(StreetNumber)) return new(new(MID.FailedValidation, false, $"Geben Sie eine korrekte Strassennummer ein. Zeichen: {DataModelValidation.IntegerChars}"));
        if(!DataModelValidation.ValidatePostalCode(PostalCode)) return new(new(MID.FailedValidation, false, $"Geben Sie eine korrekte PLZ ein. Format: {DataModelValidation.PostalCodeFormat}"));
        if(!DataModelValidation.ValidateString(City)) return new(new(MID.FailedValidation, false, $"Geben Sie eine Stadt ein."));
        if(!DataModelValidation.ValidateDate(Birthday)) return new(new(MID.FailedValidation, false, $"Geben Sie ein korrektes Geburtsdatum ein. Format: {DataModelValidation.DateFormat}"));
        if(!DataModelValidation.ValidateDate(JoinDate)) return new(new(MID.FailedValidation, false, $"Geben Sie ein korrektes Eintrittsdatum ein. Format: {DataModelValidation.DateFormat}"));
        return new(Message.ValidationSucccessful);
    }
}
public class InvoiceItemModel
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string DefaultValue { get; set; }
    public required string TransformFormula { get; set; }

    public ReturnDialog Validate()
    {
        if(!DataModelValidation.ValidateString(Name)) return new(new(MID.FailedValidation, false, $"Geben Sie einen Namen ein."));
        if(!DataModelValidation.ValidateString(Description)) return new(new(MID.FailedValidation, false, $"Geben Sie eine Beschreibung ein."));
        if(!DataModelValidation.ValidateDecimal(DefaultValue)) return new(new(MID.FailedValidation, false, $"Geben Sie einen korrekten Standardwert ein. Zeichen: {DataModelValidation.DecimalChars}"));
        if(!DataModelValidation.ValidateFormula(TransformFormula)) return new(new(MID.FailedValidation, false, $"Geben Sie eine korrekte Formel ein. Zeichen: {DataModelValidation.FormulaChars}"));
        return new(Message.ValidationSucccessful);
    }
}