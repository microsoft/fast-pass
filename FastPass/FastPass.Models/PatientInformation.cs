using Hl7.Fhir.Model;

namespace FastPass.Models;

public class PatientInformation
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Gender { get; set; } = default!;
    public string StreetAddress { get; set; } = default!;
    public string City { get; set; } = default!;
    public string State { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public string Country { get; set; } = default!;

    public PatientInformation() { }
    public PatientInformation(Patient patient)
    {
        FirstName = patient?.Name.FirstOrDefault()?.Given?.FirstOrDefault() ?? string.Empty;
        LastName = patient?.Name.FirstOrDefault()?.Family ?? string.Empty;
        Gender = patient?.Gender?.ToString() ?? string.Empty;

        var address = patient?.Address?.FirstOrDefault();

        StreetAddress = address?.Line?.FirstOrDefault() ?? string.Empty;
        City = address?.City ?? string.Empty;
        State = address?.State ?? string.Empty;
        PostalCode = address?.PostalCode ?? string.Empty;
        Country = address?.Country ?? string.Empty;
    }

    public Patient ToPatient()
    {
        /// https://docs.fire.ly/projects/Firely-NET-SDK/model/patient-example.html
        // example Patient setup, fictional data only
        var pat = new Patient();

        // todo: find if exists already using another component
        //var id = new Identifier();
        //id.System = "http://hl7.org/fhir/sid/us-ssn";
        //id.Value = "000-12-3456";
        //pat.Identifier.Add(id);

        var name = new HumanName().WithGiven(FirstName).AndFamily(LastName);
        //name.Prefix = new string[] { "Mr." };
        //name.Use = HumanName.NameUse.Official;

        //var nickname = new HumanName();
        //nickname.Use = HumanName.NameUse.Nickname;
        //nickname.GivenElement.Add(new FhirString("Chris"));

        pat.Name.Add(name);
        //pat.Name.Add(nickname);


        switch (Gender.ToLower())
        {
            case "male":
                pat.Gender = AdministrativeGender.Male;
                break;
            case "female":
                pat.Gender = AdministrativeGender.Female;
                break;
            case "other":
                pat.Gender = AdministrativeGender.Other;
                break;
            default:
                pat.Gender = AdministrativeGender.Unknown;
                break;
        };

        //pat.BirthDate = "1983-04-23";

        //var birthplace = new Extension();
        //birthplace.Url = "http://hl7.org/fhir/StructureDefinition/birthPlace";
        //birthplace.Value = new Address() { City = "Seattle" };
        //pat.Extension.Add(birthplace);

        //var birthtime = new Extension("http://hl7.org/fhir/StructureDefinition/patient-birthTime",
        //                               new FhirDateTime(1983, 4, 23, 7, 44));
        //pat.BirthDateElement.Extension.Add(birthtime);

        var address = new Address()
        {
            Line = new string[] { StreetAddress },
            City = City,
            State = State,
            PostalCode = PostalCode,
            Country = Country
        };
        pat.Address.Add(address);

        //var contact = new Patient.ContactComponent();
        //contact.Name = new HumanName();
        //contact.Name.Given = new string[] { "Susan" };
        //contact.Name.Family = "Parks";
        //contact.Gender = AdministrativeGender.Female;
        //contact.Relationship.Add(new CodeableConcept("http://hl7.org/fhir/v2/0131", "N"));
        //contact.Telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Phone, null, ""));
        //pat.Contact.Add(contact);

        //pat.Deceased = new FhirBoolean(false);

        return pat;
    }
}

