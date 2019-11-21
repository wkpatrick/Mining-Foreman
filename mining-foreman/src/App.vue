<template>
    <div id="app">
        <nav class="level" style="background: black; color: white">
            <div class="level-left">
                <div class="level-item">
                    <a class="is-size-2" @click="clickedHomeButton">Eve Mining Foreman</a>
                </div>
            </div>
        </nav>
        <router-view></router-view>
    </div>
</template>

<script>
    export default {
        name: 'app',
        components: {},
        methods: {
            clickedHomeButton() {
                // eslint-disable-next-line no-unused-vars
                this.$router.push({name: 'home'}).catch(err => {})
            }
        }
    }
</script>
<style lang="scss">
    @import "~bulma/sass/utilities/_all";

    // Set your colors
    $primary: #8c67ef;
    $primary-invert: findColorInvert($primary);

    // Setup $colors to use as bulma classes (e.g. 'is-twitter')
    $colors: (
            "white": ($white, $black),
            "black": ($black, $white),
            "light": ($light, $light-invert),
            "dark": ($dark, $dark-invert),
            "primary": ($primary, $primary-invert),
            "info": ($info, $info-invert),
            "success": ($success, $success-invert),
            "warning": ($warning, $warning-invert),
            "danger": ($danger, $danger-invert)
    );

    $family-primary: 'Exo 2', Helvetica, Arial, sans-serif;
    $family-primary-sans-serif: 'Exo 2', Helvetica, Arial, sans-serif;

    // Links
    $link: $primary;
    $link-invert: $primary-invert;
    $link-focus-border: $primary;

    @import "~bulma";
    @import "~buefy/src/scss/buefy";
</style>

<style>
    @import "~typeface-exo-2";

    #app {
        font-family: 'Exo 2', Helvetica, Arial, sans-serif;
        -webkit-font-smoothing: antialiased;
        -moz-osx-font-smoothing: grayscale;
        text-align: center;
        color: #2c3e50;
    }
    a {
        color: white;
    }
    a:visited {
        color: white;
    }

    /* mouse over link */
    a:hover {
        color: white;
    }

    /* selected link */
    a:active {
        color: white;
    }

</style>
