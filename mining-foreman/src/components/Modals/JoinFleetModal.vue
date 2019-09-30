<template>
    <div class="modal-card">
        <div class="modal-card-head">
            <header class="modal-card-title">
                <h1>Join Fleet</h1>
            </header>
        </div>
        <section class="modal-card-body">
            <div>
                Would you like to join the fleet?
            </div>
        </section>
        <footer class="modal-card-foot">
            <b-button type="is-danger" @click="$parent.close()">Nope</b-button>
            <b-button type="is-success" @click="joinFleet"> Hell Yea Brother!</b-button>
        </footer>
    </div>
</template>

<script>
    export default {
        name: "JoinFleetModal",
        methods: {
            async joinFleet(){
                let self = this;
                fetch('/api/fleet/' + this.$route.params.id + '/join' )
                    .then(function () {
                        self.$parent.close()
                    })
            }
        }
    }
</script>

<style scoped>

</style>