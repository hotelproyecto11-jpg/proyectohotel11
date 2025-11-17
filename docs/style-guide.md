# Guía rápida de estilo — Pricing MVP

## Paleta de colores
- Primary: `#0B5A5A` (verde azulado oscuro)
- Accent: `#FFB400` (ámbar/dorado)
- Background: `#F6F7F9` (gris claro)
- Card / Surface: `#FFFFFF` (blanco)
- Text primary: `#0F1724` (gris oscuro)
- Muted text: `#6B7280` (gris medio)
- Success: `#10B981` (verde)
- Danger: `#EF4444` (rojo)

## Tipografía
- Fuente principal: `Inter` (importada desde Google Fonts en `styles.css`)
- Tamaños base: 14px (cuerpo), 16px (base real), encabezados escalados (18px, 20px, 24px...)

## Variables CSS definidas (en `frontend/src/styles.css`)
- `--color-primary`, `--color-accent`, `--bg`, `--card-bg`, `--text`, `--muted`, `--radius`, `--shadow-soft`.

## Componentes y clases utilitarias
- `.container` — contenedor centrado con `max-width`.
- `.card` — tarjeta con padding, border-radius y sombra suave.
- `.btn`, `.btn-primary`, `.btn-accent` — botones estándar.
- `.input` — inputs con padding y bordes suaves.
- `.auth-card` — tarjeta para login/register.
- `.data-table` — tablas de datos.

## Reglas de diseño
- Espaciado consistente: `8px` / `12px` / `16px` según contexto.
- Radio de bordes: `8px`-`12px` para elementos interactivos.
- Contraste: mantener text contrast >= 4.5:1 para texto normal.

## Recomendaciones de implementación
- Preferir componentes Material para inputs, tablas y diálogos en próximas iteraciones (`@angular/material`).
- Mantener la lógica en los componentes `.ts`; solo cambiar HTML/CSS para estilos.
- Para producción: añadir tokens en un sistema de diseño y versionarlos.

## Comandos útiles
- Instalar dependencias de Material (opcional):
```powershell
cd frontend
npm install @angular/material @angular/cdk @angular/animations
```

- Levantar frontend (desarrollo):
```powershell
cd frontend
npm install
npm run start
```

---
Esta guía es mínima y pensada para el manual técnico. Puedes ampliarla con ejemplos visuales y snapshots de las pantallas.
