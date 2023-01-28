using System;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TimeKeep.Authentication;

public class BearerAuthentication : AuthenticationHandler<BearerAuthenticationOptions>
{
	public new const string Scheme = "Bearer";
	private const string User = "User";

	private readonly BearerAuthenticationSettings settings;
	private readonly UTF8Encoding utf8ValidatingEncoding = new UTF8Encoding(false, true);

	public BearerAuthentication(
		IOptionsMonitor<BearerAuthenticationOptions> options,
		ILoggerFactory logger,
		UrlEncoder encoder,
		ISystemClock clock,
		IOptions<BearerAuthenticationSettings> settings)
	: base(options, logger, encoder, clock) =>
		this.settings = settings.Value;

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		string? authorizationHeader = Request.Headers["Authorization"];
		if (String.IsNullOrEmpty(authorizationHeader))
		{
			return Task.FromResult(AuthenticateResult.NoResult());
		}

		if (Scheme == authorizationHeader)
		{
			string noCredentialsMessage = $"Authorization scheme was {Scheme} but the header had no token.";
			Logger.LogInformation(noCredentialsMessage);
			return Task.FromResult(AuthenticateResult.Fail(noCredentialsMessage));
		}

		if (!authorizationHeader.StartsWith(Scheme + ' ', StringComparison.OrdinalIgnoreCase))
		{
			return Task.FromResult(AuthenticateResult.NoResult());
		}

		string encodedToken = authorizationHeader.Substring(Scheme.Length).Trim();
		string decodedToken = String.Empty;
		byte[] base64DecodedToken;
		try
		{
			base64DecodedToken = Convert.FromBase64String(encodedToken);
		}
		catch (FormatException)
		{
			const string failedToDecodeToken = "Cannot convert token from base64.";
			Logger.LogInformation(failedToDecodeToken);
			return Task.FromResult(AuthenticateResult.Fail(failedToDecodeToken));
		}
		try
		{
			decodedToken = utf8ValidatingEncoding.GetString(base64DecodedToken);
		}
		catch (Exception ex)
		{
			string failedToDecodeToken = $"Cannot build token from decoded base64 value, exception {ex.Message} encountered.";
			Logger.LogInformation(failedToDecodeToken);
			return Task.FromResult(AuthenticateResult.Fail(failedToDecodeToken));
		}

		if (decodedToken == settings.Token)
		{
			var claims = new Claim[]
			{
				new(ClaimTypes.NameIdentifier, User, ClaimValueTypes.String, Options.ClaimsIssuer),
				new(ClaimTypes.Name, User, ClaimValueTypes.String, Options.ClaimsIssuer)
			};
			var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme));
			return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme)));
		}
		else
		{
			const string wrongToken = "The wrong token was provided.";
			Logger.LogWarning(wrongToken);
			return Task.FromResult(AuthenticateResult.Fail(wrongToken));
		}
	}

}

public class BearerAuthenticationOptions : AuthenticationSchemeOptions { }
public class BearerAuthenticationSettings
{
	public const string Section = "BearerAuthentication";

	public required string Token { get; init; }
}
