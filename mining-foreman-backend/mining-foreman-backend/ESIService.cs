using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EVEStandard;
using EVEStandard.Models.API;
using EVEStandard.Models.SSO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using mining_foreman_backend.DataAccess;

internal class ESIService : IHostedService, IDisposable {
    private readonly ILogger _logger;
    private readonly EVEStandardAPI esiClient;
    private Timer _timer;

    public ESIService(ILogger<ESIService> logger, EVEStandardAPI esiClient) {
        _logger = logger;
        this.esiClient = esiClient;
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("ESI Background Serviec is starting.");

        _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(60));

        return Task.CompletedTask;
    }

    private void DoWork(object state) {
        _logger.LogInformation("Checking for registered users");
        try {
            var users = User.SelectAllUsers();
            foreach (var user in users) {
                var userAuth = new AuthDTO {
                    AccessToken = new AccessTokenDetails() {
                        AccessToken = user.AccessToken,
                        ExpiresUtc = user.RefreshTokenExpiresUTC,
                        RefreshToken = user.RefreshToken
                    },
                    CharacterId = user.CharacterId,
                    Scopes = "esi-location.read_location.v1 esi-industry.read_character_mining.v1"
                };

                var test = esiClient.SSO.GetRefreshTokenAsync(user.RefreshToken).Result;
                userAuth.AccessToken = test;

                var characterInfo = esiClient.Character.GetCharacterPublicInfoV4Async(user.CharacterId).Result;
                var corporationInfo = esiClient.Corporation.GetCorporationInfoV4Async(characterInfo.Model.CorporationId)
                    .Result;
                var locationInfo = esiClient.Location.GetCharacterLocationV1Async(userAuth).Result;
                var location = esiClient.Universe.GetSolarSystemInfoV4Async(locationInfo.Model.SolarSystemId).Result;
                var mining = esiClient.Industry.CharacterMiningLedgerV1Async(userAuth).Result;

                var character = new mining_foreman_backend.Models.User {
                    UserKey = user.UserKey,
                    CharacterName = characterInfo.Model.Name,
                    CorporationName = corporationInfo.Model.Name,
                    CharacterLocation = location.Model.Name,
                    CharacterMining = mining
                };

                User.UpdateCharacter(character);

                _logger.LogInformation("Found User: {0}", user.CharacterId);
            }
        }
        catch (Exception e) {
            Console.WriteLine(e);
            //throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Timed Background Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose() {
        _timer?.Dispose();
    }
}