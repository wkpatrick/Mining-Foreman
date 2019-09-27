using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using EVEStandard;
using EVEStandard.Models.API;
using EVEStandard.Models.SSO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mining_foreman_backend.Models;

namespace mining_foreman_backend.Controllers {
    [Route("api/[controller]")]
    [Authorize]
    //TODO: I think I can just delete this. Though it looks like a good example of how to use the proper cookie auth
    public class SSOController : Controller {
        // GET
        private readonly EVEStandardAPI esiClient;

        public SSOController(EVEStandardAPI esiClient) {
            this.esiClient = esiClient;
        }

        public async Task<Models.Database.User> Index() {
            var characterId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var characterInfo = await esiClient.Character.GetCharacterPublicInfoV4Async(characterId);
            var corporationInfo =
                await esiClient.Corporation.GetCorporationInfoV4Async((int) characterInfo.Model.CorporationId);

            var auth = new AuthDTO {
                AccessToken = new AccessTokenDetails {
                    AccessToken = User.FindFirst("AccessToken").Value,
                    ExpiresUtc = DateTime.Parse(User.FindFirst("AccessTokenExpiry").Value),
                    RefreshToken = User.FindFirst("RefreshToken").Value
                },
                CharacterId = characterId,
                Scopes = User.FindFirst("Scopes").Value
            };

            var locationInfo = await esiClient.Location.GetCharacterLocationV1Async(auth);
            var location = await esiClient.Universe.GetSolarSystemInfoV4Async(locationInfo.Model.SolarSystemId);
            var mining = await esiClient.Industry.CharacterMiningLedgerV1Async(auth);

            var model = new Models.Database.User {
                CharacterName = characterInfo.Model.Name,
                CorporationName = corporationInfo.Model.Name,
                CharacterLocation = location.Model.Name,
                CharacterMining = mining
            };

            return model;
        }
    }
}