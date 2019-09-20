<template>
    <div class="box">
        <h1>Active fleet box</h1>
        <p>This shows the basics of the fleet the user is currently in</p>
        <p>Fleet: {{fleet.miningFleetKey}}</p>
        <p>Fleet Boss: {{fleet.fleetBossKey}}</p>
        <p>Start Time: {{fleet.startTime}}</p>
    </div>
</template>

<script>
    export default {
        name: "SmallFleetBox",
        props: {
            fleet: Object
        }
    }
</script>

<style scoped>

</style>%$