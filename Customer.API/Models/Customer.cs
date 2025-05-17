using System.Runtime.InteropServices;

namespace Customer.API.Models
{
    public class Customer
    {
        public Guid ID { get; private set; }
        public string? Name { get; protected set; }
        public string? Email { get; private set; }
        public string? Phone { get; private set; }

        private Customer(Guid id, string? name, string? email, string? phone)
        {
            ID = id;
            Name = name;
            Email = email;
            Phone = phone;
        }

        public static Customer Create(Guid ID, string? name, string? email, string? phone)
        {
            return new Customer(ID, name, email, phone);
        }
        public void Update(string? name, string? email, string? phone)
        {
            this.Name = name ?? Name;
            this.Email = email ?? Email;
            this.Phone = phone ?? Phone;
        }
    }
}