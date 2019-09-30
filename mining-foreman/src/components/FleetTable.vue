<template>
    <div class="container">
        <h1>Active Fleets</h1>
        <p v-if="fleets.length <= 0">Ain't any active fleets dog</p>
        <b-table v-else :columns="columns" :data="fleets" :selected.sync="selected" focusable></b-table>
        <b-button v-if="fleetSelected" @click="clickedFleet">View Fleet</b-button>
        <b-button type="is-success" @click="openCreateFleetModal">Create New Fleet</b-button>
    </div>
</template>

<script>
    import CreateFleetModal from "@/components/Modals/CreateFleetModal";

    export default {
        name: "FleetTable",
        props: {
            fleets: Array,
            activeFleetKey: Number
        },
        data() {
            return {
                columns: [
                    {
                        field: 'miningFleetKey',
                        label: 'Fleet Key'
                    },
                    {
                        field: 'fleetBossKey',
                        label: 'Fleet Boss'
                    },
                    {
                        field: 'startTime',
                        label: 'Start Time'
                    },
                    {
                        field: 'endTime',
                        label: 'End Time'
                    }
                ],
                selected: null
            }
        },
        created: function () {
            this.selectActiveFleet();
        },
        computed: {
            fleetSelected() {
                return this.selected !== null;
            }
        },
        watch: {
            //This way we can pre-select the active fleet of the table
            //Since we load the fleets async, we need to watch the prop instead
            fleets: function () {
                if(this.fleetSelected === false){
                    this.selectActiveFleet()
                }
            }
        },
        methods: {
            clickedFleet() {
                this.$router.push({name: 'fleet', params: {id: this.selected.miningFleetKey}})
            },
            openCreateFleetModal() {
                this.$buefy.modal.open({
                    parent: this,
                    component: CreateFleetModal,
                    hasModalCard: true
                })
            },
            selectActiveFleet(){
                let self = this;
                this.fleets.forEach(function(fleet){
                    if(fleet.miningFleetKey === self.activeFleetKey){
                        self.selected = fleet;
                    }
                })
            }
        }
    }
</script>

<style scoped>
</style>