using System;
using System.Collections.Generic;
using EVEStandard.Models;
using EVEStandard.Models.API;

namespace mining_foreman_backend.Models {
    public class User {
        public int UserKey { get; set; }
        public int CharacterId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiresUTC { get; set; }
        public string APIToken { get; set; }
        public int ActiveFleetKey { get; set; }
        public string CharacterName { get; set; }
        public string CorporationName { get; set; }
        public string CharacterLocation { get; set; }
        public ESIModelDTO<List<CharacterMining>> CharacterMining { get; set; }
    }
}