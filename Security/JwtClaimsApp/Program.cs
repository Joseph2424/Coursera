using JwtClaimsApp;

var secretKey = "MySuperSecretKey12345678901234567890";

Console.WriteLine("Generating JWT Token...\n");

string token = JwtTokenHelper.GenerateToken(
    username: "johndoe",
    role: "Admin",
    secretKey: secretKey
);

Console.WriteLine("Token:");
Console.WriteLine(token);
Console.WriteLine();

Console.WriteLine("Validating Token...\n");

var principal = JwtTokenHelper.ValidateToken(token, secretKey);

if (principal != null)
{
    Console.WriteLine("Token Valid");

    foreach (var claim in principal.Claims)
    {
        Console.WriteLine($"{claim.Type}: {claim.Value}");
    }
}
else
{
    Console.WriteLine("Token Invalid");
}
