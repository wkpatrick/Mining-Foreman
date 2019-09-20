<template>
    <div>
        <h1>Issa fleet</h1>
        <b-button type="is-success" @click="endFleet"> End Fleet</b-button>
    </div>
</template>

<script>
    export default {
        name: "Fleet",
        props: {
            fleet: Object
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
            }
        }
    }
</script>

<style scoped>

</style>