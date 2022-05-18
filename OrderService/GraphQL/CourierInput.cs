namespace OrderService.GraphQL
{
    public record CourierInput
    (
        int? Id,
        string Name,
        string Phone,
        bool Status
    );
}
