/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
      colors: {
        // Dorado bronce del sello de la cooperativa — color dominante de acento.
        // Escala invertida respecto al diseño anterior en negro: los tonos que
        // antes servían como acento claro sobre fondo oscuro (300/400) ahora son
        // bronce oscuro con buen contraste sobre el fondo marfil.
        gold: {
          50: '#2b2010',
          100: '#453419',
          200: '#644c22',
          300: '#82642c',
          400: '#a17d3a',
          500: '#b58e4a',
          600: '#c9a665',
          700: '#ddc48c',
          800: '#eee0c1',
          900: '#f8f2e4',
        },
        // Gris grafito con leve tinte azulado. Escala invertida: 100/200/300
        // (antes texto claro sobre negro) ahora son tinta oscura para leer
        // sobre el fondo marfil; 900/950 (antes fondo casi negro de paneles)
        // ahora son gris muy claro para tarjetas sobre el marfil.
        graphite: {
          100: '#1d212a',
          200: '#282d36',
          300: '#333944',
          400: '#414852',
          500: '#525a66',
          600: '#68707b',
          700: '#848b96',
          800: '#a2a8b1',
          900: '#c7cbd1',
          950: '#e7e9ec',
        },
        // Azul petróleo profundo — segundo color corporativo, ahora usado en
        // trazos/detalles sobre el fondo marfil en vez de mezclarse con negro.
        petrol: {
          300: '#101f27',
          400: '#152a35',
          500: '#1a3644',
          600: '#204357',
          700: '#2c5670',
          800: '#3d6b86',
          900: '#5a8ba8',
        },
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0', transform: 'translateY(6px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        zoomIn: {
          '0%': { opacity: '0', transform: 'scale(0.94)' },
          '100%': { opacity: '1', transform: 'scale(1)' },
        },
      },
      animation: {
        'fade-in': 'fadeIn 0.4s var(--ease-spring) both',
        'zoom-in': 'zoomIn 0.4s var(--ease-spring) both',
      },
    },
  },
  plugins: [],
};
