<template>
    <div>
        <h1>Issa fleet</h1>
        <b-button type="is-success" @click="endFleet"> End Fleet</b-button>
    </div>
</template>

<script>
    export default {
        name: "Fleet",
        data() {
            return{
                fleet: {}
            }
        },
        mounted: function(){
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
            async getFleet(){
                try{
                    //We lose the reference to "this" when in the fetch call. So we need to save a reference to it out here to access it
                    //TODO: With Vue you can .bind(this) so that it follows through. Need to do that here
                    let self = this;
                    fetch('/api/fleet/' + this.$route.params.id)
                        .then(function(response){
                            return response.json();
                        })
                        .then(function (json) {
                            // eslint-disable-next-line no-console
                            console.log(json);
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