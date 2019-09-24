<template>
    <div>
        //TODO: Move IsLoggedIn to the main app
        <div class="hello" v-if="!isLoggedIn">
            <h1>{{msgText}}</h1>
            <a href="api/auth/login"><img
                    src="https://web.ccpgamescdn.com/eveonlineassets/developers/eve-sso-login-white-large.png"></a>
        </div>
        <div v-if="isLoggedIn">
            <h1>{{msgText}}</h1>
            <fleet-table :fleets="fleets"></fleet-table>
        </div>
    </div>
</template>

<script>
    import FleetTable from "@/components/FleetTable";

    export default {
        name: 'Index',
        components: { FleetTable},
        props: {
            msg: String,
            loggedIn: Boolean
        },
        data() {
            return {
                fleets: []
            }
        },
        mounted() {
            this.getFleets()
        },
        computed: {
            isLoggedIn() {
                return this.$cookies.isKey('APIToken');
            },
            msgText() {
                if (this.isLoggedIn) {
                    return 'Is logged in'
                }
                return 'Not logged in'
            },
            test() {
                return this.$cookies.keys()
            }
        },
        methods: {
            async getFleets() {
                try {
                    const response = await fetch('/api/fleet');
                    const data = await response.json();
                    this.fleets = data;
                } catch (error) {
                    //console.error(error)
                }
            }
        }
    }
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
    h3 {
        margin: 40px 0 0;
    }

    ul {
        list-style-type: none;
        padding: 0;
    }

    li {
        display: inline-block;
        margin: 0 10px;
    }

    a {
        color: #42b983;
    }
</style>
