namespace ERP.Domain.ValueObjects
{
    public class Address : ValueObject
    {
        public string Street { get; private set; } = string.Empty;
        public string City { get; private set; } = string.Empty;
        public string State { get; private set; } = string.Empty;
        public string Country { get; private set; } = string.Empty;
        public string PostalCode { get; private set; } = string.Empty;

        private Address() { }

        public Address(string street, string city, string state, string country, string postalCode)
        {
            Street = street;
            City = city;
            State = state;
            Country = country;
            PostalCode = postalCode;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return State;
            yield return Country;
            yield return PostalCode;
        }
    }
}
