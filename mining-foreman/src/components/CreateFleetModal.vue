<template>
    <div class="modal-card">
        <div class="modal-card-head">
            <header class="modal-card-title">
                <h1>Create Fleet Modal</h1>
            </header>
        </div>
        <section class="modal-card-body">
            <div>
                Do you want to filter by ore mined?
                Do you want to filter by location mined?
            </div>
        </section>
        <footer class="modal-card-foot">
            <b-button type="is-success" @click="createNewFleet"> Hell Yea Brother!</b-button>
        </footer>
    </div>
</template>

<script>
    export default {
        name: "CreateFleetModal",
        methods: {
            async createNewFleet() {
                try {
                    const response = await fetch('/api/fleet/start', {
                        method: 'POST',
                        credentials: 'include'
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