<template>
    <div>
        <h1>Issa fleet</h1>
        <div class="box" style="width: 80%">
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
                            8,670,000
                        </div>
                        <div class="column">
                            14,330,000
                        </div>
                        <div class="column">
                            9,000,000
                        </div>
                        <div class="column">
                            21,000,000
                        </div>
                    </div>
                    <div class="column">
                        <b-table :columns="columns" :data="fleet.fleetMembers[0].memberMiningLedger"></b-table>
                    </div>
                </div>

            </article>
        </div>
        <b-button type="is-success" @click="endFleet"> End Fleet</b-button>
    </div>
</template>

<script>
    export default {
        name: "Fleet",
        data() {
            return {
                fleet: {},
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
                ]
            }
        },
        mounted: function () {
            this.getFleet()
        },
        methods: {
            async endFleet() {
                try {
                    const response = await fetch('/api/fleet/end', {
                        method: 'POST',
                        credentials: 'include',
                        body: JSON.stringify(this.fleet),
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
                            self.fleet = json;
                            setTimeout(self.getFleet, 5000)
                        })
                } catch (error) {
                    //console.error(error)
                }
            }
        }
    }
</script>

<style scoped>

</style>