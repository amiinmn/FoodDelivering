namespace UserService.GraphQL
{
    public record CourierStatusInput
    (
        int? Id,
        string Status,
        int UserId,
        double? Latitude,
        double? Longitude
    );
}
