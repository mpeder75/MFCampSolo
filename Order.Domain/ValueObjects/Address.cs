namespace Order.Domain.ValueObjects;

public record Address
{
    private readonly string _street;
    private readonly string _zipCode;
    private readonly string _city;
    public string Street => _street;
    public string ZipCode => _zipCode;
    public string City => _city;

    private Address(string street, string zipcode, string city)
    {
        _street = street;
        _zipCode = zipcode;
        _city = city;
    }

    //protected Address() { }

    public static Address Create(string street, string zipCode, string city)
    {
        if (string.IsNullOrWhiteSpace(street))
        {
            throw new ArgumentException("Street cannot be empty", nameof(street));
        }
    
        if (string.IsNullOrWhiteSpace(zipCode))
        {
            throw new ArgumentException("Zip code cannot be empty", nameof(zipCode));
        }
    
        if (!zipCode.All(char.IsDigit) || zipCode.Length != 4)
        {
            throw new ArgumentException("Danish zip code must be exactly 4 digits", nameof(zipCode));
        }
    
        int zipCodeValue = int.Parse(zipCode);
        if (zipCodeValue < 1000 || zipCodeValue > 9999)
        {
            throw new ArgumentException("Invalid Danish zip code range", nameof(zipCode));
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("City cannot be empty", nameof(city));
        }

        return new Address(street, zipCode, city);
    }

    public override string ToString()
    {
        return $"Street:{Street}, Zipcode:{ZipCode}, City:{City}";
    }
}