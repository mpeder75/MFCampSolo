using Dapr.Workflow;
using Workflow.API.Activities;
using Workflow.API.Models;

namespace Workflow.API.Workflows
{
    public class CreateCustomerWorkflow : Workflow<Customer, Customer>
    {
        public override async Task<Customer> RunAsync(WorkflowContext context, Customer customer)
        {
            // Placeholder implementation to avoid errors
            return await Task.FromResult(customer);
        }
    }
}
