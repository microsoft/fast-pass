using System;
namespace FastPass.Models
{
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
	}
}

