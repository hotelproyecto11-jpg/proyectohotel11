import 'zone.js';
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

console.log('Main.ts iniciando...');

bootstrapApplication(App, appConfig)
  .catch((err) => {
    console.error('Error al inicializar Angular:', err);
  });

console.log('Main.ts completado');
