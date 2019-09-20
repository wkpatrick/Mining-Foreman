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
    import CreateFleetModal from "@/components/CreateFleetModal";

    export default {
        name: "FleetTable",
        props: {
            fleets: Array
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
        computed: {
            fleetSelected(){
                return this.selected !== null;
            }
        },
        methods: {
            clickedFleet() {
                this.$router.push({name: 'fleet', params: { id: this.selected.miningFleetKey, fleet: this.selected}})
            },
            openCreateFleetModal() {
                this.$buefy.modal.open({
                    parent: this,
                    component: CreateFleetModal,
                    hasModalCard: true
                })
            }
        }
    }
</script>

<style scoped>

</style>