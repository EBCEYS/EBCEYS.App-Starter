from docker.io/ebceys/app-starter:1.0.0 as starter

from docker.io/rabbitmq:4.0.5-management

RUN apt-get update && \
    apt-get install -y --no-install-recommends libicu-dev

ENV CONFIGURATION_CONTAINER_TYPE_NAME=rabbitmq
ENV APP_STARTER_EXECUTION_FILE=/docker-entrypoint.sh
ENV APP_STARTER_EXECUTION_ARGS=rabbitmq-server

ARG HEALTHCHECK_PORT=8080

ENV ASPNETCORE_URLS=http://*:$HEALTHCHECK_PORT
ENV HEALTHCHECKS_STARTING_PORT=$HEALTHCHECK_PORT

COPY --from=starter /appstarter /appstarter
COPY ./docker-entrypoint.sh /docker-entrypoint.sh

CMD ["/appstarter/EBCEYS.Container-AppStarter"]