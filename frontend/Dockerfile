FROM node:22-alpine AS build
WORKDIR /src
COPY package*.json ./
RUN npm ci --no-audit --no-fund
COPY . .
ARG VITE_API_URL=http://localhost:3000
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

FROM nginxinc/nginx-unprivileged:stable-alpine
# LAB-04: cap nhat goi he dieu hanh de va CVE tang OS (vd brotli-libs, curl/libcurl) do Trivy phat hien
USER root
RUN apk upgrade --no-cache
COPY --from=build /src/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
USER 101
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s CMD wget -q -O /dev/null http://127.0.0.1:8080/ || exit 1
