FROM nginxinc/nginx-unprivileged:stable-alpine
COPY ./dist /usr/share/nginx/html
USER 101
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s CMD wget -q -O /dev/null http://localhost:8080/ || exit 1
