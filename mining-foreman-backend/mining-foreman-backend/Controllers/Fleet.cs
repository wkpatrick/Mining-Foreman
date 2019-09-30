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
        public Models.Network.MiningFleetResponse GetFleetInfo(int fleetKey) {
            var user = DataAccess.User.SelectUserByAPIToken(Request.Cookies["APIToken"]);

            var fleet = new Models.Network.MiningFleetResponse {
                FleetInfo = DataAccess.Fleet.SelectFleet(fleetKey),
                FleetTotal = DataAccess.MiningLedger.SelectFleetTotalProduction(fleetKey)
            };

            if (user.ActiveFleetKey == fleetKey) {
                fleet.MemberInfo = DataAccess.Fleet.SelectFleetMember(fleetKey, user.UserKey);
            }

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
            //var user = DataAccess.User.SelectUserByAPIToken(apiToken);

            //Set IsActive to 0 on the mining fleet
            DataAccess.Fleet.EndMiningFleet(fleet.MiningFleetKey);
            //Loop through the mining fleet members and calculate the differences of the mining ledger and calculate the difference and put that as the 'ending' mining fleet ledger
            DataAccess.MiningLedger.InsertEndingFleetMiningLedger(fleet.MiningFleetKey);
        }

        //This differs from end since a single user can leave and join mining fleets
        [HttpGet("{fleetKey}/leave")]
        public void LeaveMiningFleets(int fleetKey) {
            var apiToken = Request.Cookies["APIToken"];
            var user = DataAccess.User.SelectUserByAPIToken(apiToken);

            DataAccess.MiningLedger.InsertEndingFleetMiningLedger(fleetKey, user.UserKey);
            DataAccess.Fleet.LeaveMiningFleet(user.UserKey, fleetKey);
        }

        [HttpGet("{fleetKey}/join")]
        public void JoinMiningFleet(int fleetKey) {
            var apiToken = Request.Cookies["APIToken"];
            var user = DataAccess.User.SelectUserByAPIToken(apiToken);

            //check to see if user is eligible to join
            if (DataAccess.User.SelectIsUserValidForFleet(user.UserKey, fleetKey)) {
                if (DataAccess.Fleet.SelectIfUserHasBeenInFleetBefore(user.UserKey, fleetKey)) {
                    DataAccess.Fleet.InsertReturningMiningFleetMember(user.UserKey, fleetKey);
                }
                else {
                    DataAccess.Fleet.InsertMiningFleetMember(user.UserKey, fleetKey);
                }

                DataAccess.MiningLedger.InsertStartingFleetMiningLedger(user.UserKey, fleetKey);
            }
        }
    }
}