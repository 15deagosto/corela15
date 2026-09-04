# Build de producción del frontend de Corela15 (React + Vite) + nginx
# sirviendo el estático y proxeando /api/ al backend -- mismo origen,
# mismo dominio/puerto que la página: sin CORS y sin mixed-content real
# en producción, sin importar si se sirve por HTTPS.
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
# VITE_API_URL="" (relativo) -- las rutas de la app ya incluyen /api/... completo
# (ver src/lib/api.ts), así que un baseURL vacío hace que axios pegue al mismo
# origen desde el que se sirvió la página, y nginx se encarga de reenviarlo.
ARG VITE_API_URL=""
ENV VITE_API_URL=${VITE_API_URL}
RUN npm run build

FROM nginx:1.27-alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
