import { mount } from 'svelte'
import './app.css'
import App from './App.svelte'
import { Router } from "svelte-routing";
import { setupInterceptors } from './services/interceptor';


const app = mount(App, {
  target: document.getElementById('app'),
})

export default app
