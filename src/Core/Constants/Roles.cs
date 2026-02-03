namespace DirectPayGateway.Core.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Merchant = "Merchant";

    public static readonly string[] All = { Admin, User, Merchant };
}
