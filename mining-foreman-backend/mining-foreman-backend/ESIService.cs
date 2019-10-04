using System;
using System.Threading;
using System.Threading.Tasks;
using EVEStandard;
using EVEStandard.Models.API;
using EVEStandard.Models.SSO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal class ESIService : IHostedService, IDisposable {
    private readonly ILogger _logger;
    private readonly EVEStandardAPI esiClient;
    private Timer _timer;
    private DateTime _miningExpiration = DateTime.MinValue;

    public ESIService(ILogger<ESIService> logger, EVEStandardAPI esiClient) {
        _logger = logger;
        this.esiClient = esiClient;
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("ESI Background Service is starting.");
        _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(15));

        return Task.CompletedTask;
    }

    private void DoWork(object state) {
        //Instead of fiddling with the timer and setting it to reset at the expiration, im doing this for two reasons
        //1: I cant figure out how have the timer switch end times without it running twice
        //2: This way we can have different expirations for various endpoints without getting too complicated.
        if (DateTime.Now >= _miningExpiration) {
            _logger.LogDebug("Checking for registered users");
            try {
                var users = mining_foreman_backend.DataAccess.User.SelectAllUsers();
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
                    var corporationInfo = esiClient.Corporation
                        .GetCorporationInfoV4Async(characterInfo.Model.CorporationId)
                        .Result;
                    var locationInfo = esiClient.Location.GetCharacterLocationV1Async(userAuth).Result;
                    var location = esiClient.Universe.GetSolarSystemInfoV4Async(locationInfo.Model.SolarSystemId)
                        .Result;
                    var mining = esiClient.Industry.CharacterMiningLedgerV1Async(userAuth).Result;

                    var character = new mining_foreman_backend.Models.Database.User {
                        UserKey = user.UserKey,
                        CharacterName = characterInfo.Model.Name,
                        CorporationName = corporationInfo.Model.Name,
                        CharacterLocation = location.Model.Name,
                        CharacterMining = mining
                    };

                    mining_foreman_backend.DataAccess.User.UpdateCharacter(character);

                    _logger.LogDebug("Found User: {0}", user.CharacterId);
                    _miningExpiration = mining.Expires.Value.LocalDateTime;
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
                //throw;
            }
        }
        else {
            _logger.LogInformation("Waiting for the mining ledger to expire");
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