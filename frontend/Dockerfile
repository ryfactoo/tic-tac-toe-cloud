FROM node:21.7.1-alpine

WORKDIR /app

COPY package.json .
COPY package-lock.json .

RUN npm install

COPY . .

ARG DEFAULT_API_DOMAIN="http://localhost"
ENV REACT_APP_API_DOMAIN=${DEFAULT_API_DOMAIN}

ARG DEFAULT_API_PORT=8080
ENV REACT_APP_API_PORT=${DEFAULT_API_PORT}

EXPOSE 3000

CMD ["npm", "start"]
