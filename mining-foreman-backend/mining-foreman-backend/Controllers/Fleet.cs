using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace mining_foreman_backend.Controllers {
    [Route("api/[controller]")]
    public class Fleet : Controller {
        [HttpGet]
        public List<Models.MiningFleet> Index() {
            return DataAccess.Fleet.SelectActiveFleets();
        }

        [HttpPost("start")]
        public void CreateMiningFleet() {
            
            var characterId = int.Parse(Request.Cookies["CharacterId"]);
            var userKey = DataAccess.User.SelectUserKeyByCharacterId(characterId);
            var fleet = new Models.MiningFleet {
                FleetBossKey = userKey,
                StartTime = DateTime.Now,
                EndTime = DateTime.MaxValue,
                IsActive = true
            };
            
            fleet.MiningFleetKey = DataAccess.Fleet.InsertMiningFleet(fleet);
            DataAccess.Fleet.InsertMiningFleetMember(userKey, fleet.MiningFleetKey);
            DataAccess.MiningLedger.InsertStartingFleetMiningLedger(userKey);
        }

        [HttpPost("end")]
        public void EndMiningFleet() {
            var characterId = int.Parse(Request.Cookies["CharacterId"]);
            var userKey = DataAccess.User.SelectUserKeyByCharacterId(characterId);
            
            //Set IsActive to 0 on the mining fleet
            //Loop through the mining fleet members and set the end fleet ledger
            //Calculate the fleets totals and save that in a seperatez table.
        }
    }
}