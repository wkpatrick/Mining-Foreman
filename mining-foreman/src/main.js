import Vue from 'vue'
import VueRouter from "vue-router";
import VueCookies from 'vue-cookies'
import App from './App.vue'
import Buefy from 'buefy'
import 'buefy/dist/buefy.css'
import Fleet from "@/components/Fleet";
import Index from "@/components/Index";

Vue.use(VueCookies);
Vue.use(Buefy);
Vue.use(VueRouter);
Vue.config.productionTip = false;


const routes = [
  { path: '/', component: Index },
  {
    path: '/fleet/:id',
    name: 'fleet',
    component: Fleet,
    props: true
  }
];

const router = new VueRouter({
  mode: 'history',
  routes // short for `routes: routes`
});

new Vue({
  router,
  render: h => h(App),
}).$mount('#app');
