using System;
using System.Collections.Generic;
using EVEStandard.Models;
using Microsoft.AspNetCore.Mvc;

namespace mining_foreman_backend.Controllers {
    [Route("api/[controller]")]
    public class Fleet : Controller {
        [HttpGet]
        public List<Models.MiningFleet> Index() {
            var user = DataAccess.User.SelectUserByAPIToken(Request.Cookies["APIToken"]);
            var fleets = DataAccess.Fleet.SelectActiveFleets();
            foreach (var fleet in fleets) {
                if (fleet.FleetBossKey == user.UserKey) {
                    fleet.IsFleetBoss = true;
                }
            }

            return fleets;
        }

        [HttpGet("{fleetKey}")]
        public Models.MiningFleetResponse GetFleetInfo(int fleetKey) {
            var user = DataAccess.User.SelectUserByAPIToken(Request.Cookies["APIToken"]);
            var fleet = new Models.MiningFleetResponse {
                FleetInfo = DataAccess.Fleet.SelectFleet(fleetKey),
                MemberInfo = DataAccess.Fleet.SelectFleetMember(fleetKey,user.UserKey),
                FleetTotal = DataAccess.MiningLedger.SelectFleetTotalProduction(fleetKey)
            };
            return fleet;
        }
        
        [HttpPost("start")]
        public void CreateMiningFleet() {
            var user = DataAccess.User.SelectUserByAPIToken(Request.Cookies["APIToken"]);
            var fleet = new Models.MiningFleet {
                FleetBossKey = user.UserKey,
                StartTime = DateTime.Now,
                EndTime = DateTime.MaxValue,
                IsActive = true
            };

            fleet.MiningFleetKey = DataAccess.Fleet.InsertMiningFleet(fleet);
            DataAccess.Fleet.InsertMiningFleetMember(user.UserKey, fleet.MiningFleetKey);
            DataAccess.MiningLedger.InsertStartingFleetMiningLedger(user.UserKey, fleet.MiningFleetKey);
        }

        [HttpPost("end")]
        public void EndMiningFleet([FromBody] Models.MiningFleet fleet) {
            var apiToken = Request.Cookies["APIToken"];
            var user = DataAccess.User.SelectUserByAPIToken(apiToken);

            //Set IsActive to 0 on the mining fleet
            DataAccess.Fleet.EndMiningFleet(fleet.MiningFleetKey);
            //Loop through the mining fleet members and calculate the diffzserences of the mining ledger and calculate the difference and put that as the 'ending' mining fleet ledger
            DataAccess.MiningLedger.InsertEndingFleetMiningLedger(fleet.MiningFleetKey);
        }
    }
}