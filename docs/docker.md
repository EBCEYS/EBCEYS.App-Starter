# *Docker*

## Сборка образа

Пример сборки контейнера с использованием данного сервиса.

```dockerfile
from docker.io/ebceys/app-starter:1.0.0 as starter

from docker.io/rabbitmq:4.0.5-management


ENV CONFIGURATION_CONTAINER_TYPE_NAME=rabbitmq
ENV APP_STARTER_EXECUTION_FILE=/docker-entrypoint.sh
ENV APP_STARTER_EXECUTION_ARGS=rabbitmq-server
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

COPY --from=starter /appstarter /appstarter
COPY ./docker-entrypoint.sh /docker-entrypoint.sh

CMD ["/appstarter/EBCEYS.Container-AppStarter"]
```

## *Docker-compose*

Пример *docker-compose.yaml* файла:

```yaml
services:
  ebceys.server-configuration:
    container_name: ebceys.server-configuration
    hostname: ebceys.server-configuration
    environment:
      - DOCKER_CONNECTION_USE_DEFAULT=true
      - CONFIG_PROCESSOR_ENABLE=false
      - CONFIG_PROCESSOR_CONFIGS_PATH=/storage/configs
      - KEYS_STORAGE_PATH=/storage/keys
      - KEYS_STORAGE_FORGET_OLD_KEYS=true
    image: docker.io/ebceys/serverconfiguration:1.0.0
    volumes:
      - C:\\storage:/storage:rw
      - C:\\server-configuration\data:/data:rw
      - /var/run/docker.sock:/var/run/docker.sock
    ports:
      - "5007:3000"
      - "5008:8080"
    networks:
      - "testnet"
  ebceys.container-appstarter:
    container_name: appstarter
    # image: ${DOCKER_REGISTRY-}ebceyscontainerappstarter
    image: docker.io/ebceys/rabbitmq:1.0.0
    environment:
      - CONFIGURATION_CONTAINER_TYPE_NAME=rabbitmq
      - CONFIGURATION_SERVER_URI=http://ebceys.server-configuration:3000
      - CONFIGURATION_SAVE_DIRECTORY=/configs
      - CONFIGURATION_BREAK_START_IF_NO_CONFIGS=true
      - CONFIGURATION_REQUEST_PERIOD=00:00:10
      - RABBITMQ_CONFIG_FILE=/configs/rabbitmq.conf
    ports:
      - "5675:5672"
      - "15675:15672"
    volumes:
      - C:\\TestAppStarter:/configs:rw
    networks:
      - "testnet"
    depends_on:
      - ebceys.server-configuration
networks:
  testnet:
    name: testnet
    external: true
```