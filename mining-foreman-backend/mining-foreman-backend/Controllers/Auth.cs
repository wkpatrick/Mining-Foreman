using EVEStandard.Models.SSO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EVEStandard;
using mining_foreman_backend.Models;

namespace mining_foreman_backend.Controllers {
    [Route("api/[controller]")]
    public class AuthController : Controller {
        private readonly EVEStandardAPI esiClient;

        private static string SSOStateKey = "SSOState";

        public AuthController(EVEStandardAPI esiClient) {
            this.esiClient = esiClient;
        }

        [HttpGet("Login")]
        public IActionResult Login(string returnUrl = null) {
            // Scopes are required for API calls but not for authentication, dummy scope is inserted to workaround an issue in the library
            var scopes = new List<string>() {
                "esi-location.read_location.v1",
                "esi-industry.read_character_mining.v1"
            };

            string state;

            if (!String.IsNullOrEmpty(returnUrl)) {
                state = Base64UrlTextEncoder.Encode(Encoding.ASCII.GetBytes(returnUrl));
            }
            else {
                state = Guid.NewGuid().ToString();
            }

            HttpContext.Session.SetString(SSOStateKey, state);

            var authorization = esiClient.SSO.AuthorizeToEVEUri(scopes, state);
            return Redirect(authorization.SignInURI);
        }

        public async Task<IActionResult> Logout() {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("localhost");
        }

        [HttpGet("Callback")]
        public async Task<IActionResult> Callback(string code, string state) {
            var authorization = new Authorization {
                AuthorizationCode = code,
                ExpectedState = HttpContext.Session.GetString(SSOStateKey),
                ReturnedState = state
            };

            var accessToken = await esiClient.SSO.VerifyAuthorizationAsync(authorization);
            var character = await esiClient.SSO.GetCharacterDetailsAsync(accessToken.AccessToken);

            var dbUser = DataAccess.User.SelectUser(character.CharacterId);

            //The user does not exist in the db
            if (dbUser == null) {
                dbUser = new Models.Database.User {
                    CharacterId = character.CharacterId,
                    CharacterName = character.CharacterName,
                    AccessToken = accessToken.AccessToken,
                    RefreshToken = accessToken.RefreshToken,
                    RefreshTokenExpiresUTC = accessToken.ExpiresUtc,
                    APIToken = Guid.NewGuid().ToString()
                };

                DataAccess.User.InsertUser(dbUser);
            }
            else {
                dbUser.AccessToken = accessToken.AccessToken;
                dbUser.RefreshToken = accessToken.RefreshToken;
                dbUser.RefreshTokenExpiresUTC = accessToken.ExpiresUtc;
                if (dbUser.APIToken == null) {
                    dbUser.APIToken = Guid.NewGuid().ToString();
                }

                dbUser.CharacterName = character.CharacterName;
                DataAccess.User.UpdateUser(dbUser);
            }

            Response.Cookies.Append("APIToken", dbUser.APIToken,
                new CookieOptions {Expires = DateTimeOffset.Now.AddDays(7)});

            await SignInAsync(accessToken, character);

            if (Guid.TryParse(state, out Guid stateGuid)) {
                return Redirect("http://localhost");
            }
            else {
                var returnUrl = Encoding.ASCII.GetString(Base64UrlTextEncoder.Decode(state));
                return Redirect(returnUrl);
            }
        }

        private async Task SignInAsync(AccessTokenDetails accessToken, CharacterDetails character) {
            if (accessToken == null)
                throw new ArgumentNullException(nameof(accessToken));
            if (character == null)
                throw new ArgumentNullException(nameof(character));

            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, character.CharacterId.ToString()),
                new Claim(ClaimTypes.Name, character.CharacterName),
                new Claim("AccessToken", accessToken.AccessToken),
                new Claim("RefreshToken", accessToken.RefreshToken ?? ""),
                new Claim("AccessTokenExpiry", accessToken.ExpiresUtc.ToString()),
                new Claim("Scopes", character.Scopes)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties {IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddHours(24)});
        }
    }
}