namespace UserService.GraphQL
{
    public record UserRoleInput
     (
         int? Id,
         int UserId,
         int RoleId
     );
}
