# Переменные окружения

## *App-Starter*:

1. *CONFIGURATION_BREAK_START_IF_NO_CONFIGS* - *true/false*. Указывает падать сервису, если не удалось получить конфигурацию для данного приложения. По умолчанию *true*.
1. *CONFIGURATION_HTTP_TIMEOUT* - *TimeSpan*. Время ожидания *http* запросов на [сервер конфигурации](https://github.com/EBCEYS/EBCEYS.Server-Configuration). По умолчанию *00:00:30*.
1. *CONFIGURATION_REQUEST_PERIOD* - *TimeSpan*. Период опроса сервера конфигурации после запуска приложения. По умолчанию *00:00:10*.
1. *CONFIGURATION_REQUEST_RETRIES* - *int*. Кол-во повторов опроса сервера конфигурации в случае неудачного ответа. По умолчанию *3*.
1. *CONFIGURATION_REQUEST_DELAY* - *TimeSpan*. Время задержки перед повторами опроса конфигурации. По умолчанию *00:00:05*.
1. *APP_STARTER_DELAY_BEFORE_START* - *TimeSpan*. Задержка перед запуском сервиса. По умолчанию *00:00:00*.
1. *APP_STARTER_EXECUTION_FILE* - *PathString*. Абсолютный путь до исполняемого файла приложения. **Обязательный параметр**.
1. *APP_STARTER_EXECUTION_ARGS* - *string*. Аргументы для исполняемого файла приложения. По умолчанию *string.empty*.
1. *APP_STARTER_WORKING_DIRECTORY* - *DirectoryString*. Рабочая директория исполняемого файла. По умолчанию берется директория исполняемого файла.
1. *APP_STARTER_RESTART_APP_ON_CONFIG_UPDATE* - *true/false*. Перезапускать испольняемое приложение в случае обновления конфигурации. По умолчанию *true*.
1. *APP_STARTER_ENABLE_HEALTHCHECKS* - *true/false*. Включать *HealthChecks* *App-Starter-a*. Можно выключить проверки здоровья стартера если запускаемое приложение уже их включает. По умолчанию *true*.

## *Configuration*:

Берутся из [библиотеки](https://github.com/EBCEYS/EBCEYS.ContainersEnvironment). А именно [тут](https://github.com/EBCEYS/EBCEYS.ContainersEnvironment/tree/master#%D0%BA%D0%BE%D0%BD%D1%84%D0%B8%D0%B3%D1%83%D1%80%D0%B0%D1%86%D0%B8%D1%8F-%D1%81%D0%B5%D1%80%D0%B2%D0%B8%D1%81%D0%BE%D0%B2).

## *HealthChecks*:

Берутся из [библиотеки](https://github.com/EBCEYS/EBCEYS.ContainersEnvironment). А имено [тут](https://github.com/EBCEYS/EBCEYS.ContainersEnvironment/tree/master#healthchecks).