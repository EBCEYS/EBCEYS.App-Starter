from docker.io/ebceys/app-starter:1.0.0 as starter

from docker.io/rabbitmq:4.0.5-management


ENV CONFIGURATION_CONTAINER_TYPE_NAME=rabbitmq
ENV APP_STARTER_EXECUTION_FILE=/docker-entrypoint.sh
ENV APP_STARTER_EXECUTION_ARGS=rabbitmq-server
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

COPY --from=starter /appstarter /appstarter
COPY ./docker-entrypoint.sh /docker-entrypoint.sh

CMD ["/appstarter/EBCEYS.Container-AppStarter"]