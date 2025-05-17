namespace Workflow.API.Models
{
    public class Customer
    {
        public required string CustomerID { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public string Status { get; set; } = "created";
        public string? Error { get; set; }
    }
}
