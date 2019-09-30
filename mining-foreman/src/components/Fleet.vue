<template>
    <div>
        <h1>Issa fleet</h1>
        <div class="box" style="width: 80%" v-if="fleetLoaded">
            <article class="media">
                <div class="media-left">
                    <figure class="media-left image is-128x128">
                        <img src="https://bulma.io/images/placeholders/128x128.png">
                        Fleet leader
                    </figure>
                </div>
                <div class="media-content">
                    <div class="level">
                        <div class="level-left">
                            <div class="level-item" v-for="member in fleet.fleetInfo.fleetMembers"
                                 v-bind:key="member.userKey">
                                <figure class="image is-128x128">
                                    <img :src="member.portraitUrl">
                                    {{member.characterName}}
                                </figure>
                            </div>
                        </div>
                        <div class="level-right">
                            <div class="level-item">
                                <b-button type="is-info" v-if="showJoinFleetButton" @click="openJoinFleetModal">Join Fleet</b-button>
                                <b-button type="is-info" v-if="showLeaveFleetButton" @click="openLeaveFleetModal">Leave Fleet</b-button>
                            </div>
                        </div>
                    </div>
                </div>
            </article>
            <b-table :data="fleet.fleetTotal">
                <template slot-scope="props">
                    <b-table-column><img :src="props.row.imgUrl"></b-table-column>
                    <b-table-column label="Ore">{{props.row.typeId}}</b-table-column>
                    <b-table-column label="Quantity" field="quantity"> {{props.row.quantity}}</b-table-column>
                </template>
            </b-table>
        </div>
        <div class="box" style="width: 80%" v-if="isUsersActiveFleet">
            <h1>Your Output</h1>
            <article class="media">
                <div class="media-left">
                    <figure class="media-left image is-128x128">
                        <img src="https://bulma.io/images/placeholders/128x128.png">
                    </figure>
                </div>
                <div class="media-content">
                    <div class="columns">
                        <div class="column">
                            Total M3 Mined
                        </div>
                        <div class="column">
                            Total Isk Earned
                        </div>
                        <div class="column">
                            M3 / Hour
                        </div>
                        <div class="column">
                            Isk /hr
                        </div>
                    </div>
                    <div class="columns">
                        <div class="column">
                            -----
                        </div>
                        <div class="column">
                            -----
                        </div>
                        <div class="column">
                            -----
                        </div>
                        <div class="column">
                            -----
                        </div>
                    </div>
                    <div class="column">
                        <b-table v-if="fleetLoaded" :columns="columns"
                                 :data="fleet.memberInfo.memberMiningLedger"></b-table>
                    </div>
                </div>
            </article>
        </div>
        <div class="box" style="width: 80%">
            <h1>Total Detailed Fleet Output</h1>
            <b-table v-if="fleetLoaded"
                     :data="fleet.fleetInfo.fleetMembers"
                     :narrowed="true">
                <template slot-scope="props">
                    <b-table-column field="characterName" label="Character">
                        {{props.row.characterName}}
                    </b-table-column>
                    <b-table-column label="Ore">
                        <div v-for="ledger in props.row.memberMiningLedger" v-bind:key="ledger.miningFleetKedgerKey">
                            {{ledger.typeId}}
                        </div>
                    </b-table-column>
                    <b-table-column label="Quantity">
                        <div v-for="ledger in props.row.memberMiningLedger" v-bind:key="ledger.miningFleetKedgerKey">
                            {{ledger.quantity}}
                        </div>
                    </b-table-column>
                    <b-table-column label="Copy">
                        <a @click="test">
                            <b-icon
                                    icon="clipboard-arrow-down-outline"
                                    size="is-small">
                            </b-icon>
                        </a>
                    </b-table-column>
                </template>
            </b-table>
        </div>
        <b-button type="is-success" v-if="fleetLoaded && isFleetBoss" @click="endFleet"> End Fleet</b-button>
    </div>
</template>

<script>
    import JoinFleetModal from "@/components/Modals/JoinFleetModal";
    import LeaveFleetModal from "@/components/Modals/LeaveFleetModal";

    export default {
        name: "Fleet",
        data() {
            return {
                fleet: {},
                fleetLoaded: false,
                columns: [
                    {
                        field: 'typeId',
                        label: 'Ore'
                    },
                    {
                        field: 'solarSystemId',
                        label: 'Solar System'
                    },
                    {
                        field: 'quantity',
                        label: 'Quantity'
                    }
                ],
                fleetColumns: [
                    {
                        field: 'characterName',
                        label: 'Name'
                    },
                    {
                        field: 'userKey',
                        label: 'User Key'
                    },
                    {
                        field: 'memberMiningLedger',
                        label: 'test'
                    }
                ],
                fleetTimer: {}
            }
        },
        created: function () {
            this.getFleet()
        },
        destroyed: function () {
            window.clearTimeout(this.fleetTimer);
        },
        computed: {
            isUsersActiveFleet: function () {
                return typeof (this.fleet.memberInfo) !== 'undefined' && this.fleet.memberInfo !== null && this.fleet.memberInfo.memberIsActive;
            },
            isFleetBoss: function () {
                if (this.fleet.memberInfo !== null) {
                    return this.fleet.fleetInfo.fleetBoss.userKey === this.fleet.memberInfo.userKey;
                }
                return false;
            },
            showJoinFleetButton: function(){
                return !this.isUsersActiveFleet && this.fleet.fleetInfo.isActive
            },
            showLeaveFleetButton: function () {
                return this.isUsersActiveFleet && this.fleet.memberInfo.memberIsActive
            }
        },
        methods: {
            async endFleet() {
                try {
                    const response = await fetch('/api/fleet/end', {
                        method: 'POST',
                        credentials: 'include',
                        body: JSON.stringify(this.fleet.fleetInfo),
                        headers: {'Content-type': 'application/json'}
                    });
                    const data = await response.json();
                    // eslint-disable-next-line no-console
                    console.log(data);
                } catch (error) {
                    //console.error(error)
                }
            },
            async getFleet() {
                try {
                    //We lose the reference to "this" when in the fetch call. So we need to save a reference to it out here to access it
                    //TODO: With Vue you can .bind(this) so that it follows through. Need to do that here
                    let self = this;
                    fetch('/api/fleet/' + this.$route.params.id)
                        .then(function (response) {
                            return response.json();
                        })
                        .then(function (json) {
                            self.fleet = self.modifyData(json);
                            self.fleetLoaded = true;
                            self.fleetTimer = setTimeout(self.getFleet, 5000)
                        })
                } catch (error) {
                    //console.error(error)
                }
            },
            modifyData(fleet) {
                fleet.fleetTotal.forEach(function (element) {
                    element.imgUrl = 'https://imageserver.eveonline.com/Type/' + element.typeId + '_32.png';
                });
                fleet.fleetInfo.fleetMembers.forEach(function (element) {
                    element.portraitUrl = 'https://imageserver.eveonline.com/Character/' + element.characterId + '_128.jpg';
                });
                return fleet;
            },
            openJoinFleetModal() {
                this.$buefy.modal.open({
                    parent: this,
                    component: JoinFleetModal,
                    hasModalCard: true
                })
            },
            openLeaveFleetModal(){
                this.$buefy.modal.open({
                    parent: this,
                    component: LeaveFleetModal,
                    hasModalCard: true
                })
            },
            test() {
                // eslint-disable-next-line no-console
                console.log('test')
            }
        }
    }
</script>

<style scoped>

</style>