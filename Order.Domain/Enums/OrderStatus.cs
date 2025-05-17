namespace Order.Domain.Enums;

public enum OrderStatus
{
    Created,        
    Placed,         
    PaymentPending, 
    PaymentApproved,
    PaymentFailed,  
    Processing,     
    Shipped,        
    Delivered,      
    Cancelled    
}