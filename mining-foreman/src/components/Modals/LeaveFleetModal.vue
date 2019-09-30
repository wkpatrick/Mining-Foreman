<template>
    <div class="modal-card">
        <div class="modal-card-head">
            <header class="modal-card-title">
                <h1>Join Fleet</h1>
            </header>
        </div>
        <section class="modal-card-body">
            <div>
                Are you sure you want to leave this fleet? You can always come back later
            </div>
        </section>
        <footer class="modal-card-foot">
            <b-button type="is-danger" @click="$parent.close()">Nope</b-button>
            <b-button type="is-success" @click="leaveFleet"> Hell Yea Brother!</b-button>
        </footer>
    </div>
</template>

<script>
    export default {
        name: "LeaveFleetModal",
        methods: {
            async leaveFleet(){
                let self = this;
                fetch('/api/fleet/' + this.$route.params.id + '/leave' )
                    .then(function () {
                        self.$parent.close()
                    })
            }
        }
    }
</script>

<style scoped>

</style>