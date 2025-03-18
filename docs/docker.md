# *Docker*

## Сборка образа

Пример сборки контейнера с использованием данного сервиса.

```dockerfile
from docker.io/ebceys/app-starter:1.0.0 as starter

from docker.io/rabbitmq:4.0.5-management

ENV CONFIGURATION_CONTAINER_TYPE_NAME=rabbitmq
ENV APP_STARTER_EXECUTION_FILE=/docker-entrypoint.sh
ENV APP_STARTER_EXECUTION_ARGS=rabbitmq-server

ARG HEALTHCHECK_PORT=8080

ENV ASPNETCORE_URLS=http://*:$HEALTHCHECK_PORT
ENV HEALTHCHECKS_STARTING_PORT=$HEALTHCHECK_PORT

COPY --from=starter /appstarter /appstarter
COPY ./docker-entrypoint.sh /docker-entrypoint.sh

CMD ["/appstarter/EBCEYS.Container-AppStarter"]
```

## *Docker-compose*

Пример *docker-compose.yaml* файла:

```yaml
services:
  server-configuration:
    container_name: server-configuration
    hostname: server-configuration
    environment:
      - DOCKER_CONNECTION_USE_DEFAULT=true
      - CONFIG_PROCESSOR_ENABLE=false
      - CONFIG_PROCESSOR_CONFIGS_PATH=/storage/configs
      - KEYS_STORAGE_PATH=/storage/keys
      - KEYS_STORAGE_FORGET_OLD_KEYS=true
    image: ebceys/conf-serv-slim:1.0.0
    volumes:
      - C:\\storage:/storage:rw
      - C:\\server-configuration\data:/data:rw
      - /var/run/docker.sock:/var/run/docker.sock
    labels:
      - healthchecks.enabled=true 
      - healthchecks.port=8080 
      - healthchecks.restart=true
      - healthchecks.isebceys=true
      - healthchecks.hostname=server-configuration
    ports:
      - "5007:3000"
      - "5008:8080"
    networks:
      - "testnet"
  ebceys.container-appstarter.rabbitmq:
    container_name: rabbitmq
    hostname: rabbitmq
    image: docker.io/ebceys/rabbitmq:1.0.0
    environment:
      - CONFIGURATION_CONTAINER_TYPE_NAME=rabbitmq
      - CONFIGURATION_SERVER_URI=http://server-configuration:3000
      - CONFIGURATION_SAVE_DIRECTORY=/configs
      - CONFIGURATION_BREAK_START_IF_NO_CONFIGS=true
      - CONFIGURATION_REQUEST_PERIOD=00:00:10
      - RABBITMQ_CONFIG_FILE=/configs/rabbitmq.conf
      - HEALTHCHECKS_ENABLE=true
      - HEALTHCHECKS_STARTING_PORT=8080
      - APP_STARTER_RESTART_APP_ON_CONFIG_UPDATE=true
      - CONFIGURATION_REQUEST_RETRIES=3
      - CONFIGURATION_REQUEST_DELAY=00:00:05
    labels: # labels с информацией для хелсчеков
      - healthchecks.enabled=true 
      - healthchecks.port=8080 
      - healthchecks.restart=true
      - healthchecks.isebceys=true
      - healthchecks.hostname=rabbitmq
    ports:
      - "5675:5672"
      - "15675:15672"
      - "0:8080"
    volumes:
      - C:\\TestAppStarter:/configs:rw # конфиги пробрасываются для rabbitmq
    networks:
      - "testnet"
    depends_on:
      - server-configuration
networks:
  testnet:
    name: testnet
    external: true
```