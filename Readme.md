## Тестовое задание на бэк разработчика .NET
### Описание
Представим себе систему, обрабатывающую входящий http трафик, запрашивая бэкэнд и отдающая его результат в ответе. Бэкэндом у нас являются файлы в каком-то каталоге (content root например).
API выглядит так: GET /?filename=xxx.txt ,где xxx.txt - произвольное имя файла, которое и выдается в Reponse.
Чтение файла у нас “долгое” - т.е. вычитка файла занимает продолжительное время.

### Задача
Реализовать корректную работу сценария: 
при запросе файла: 
- если файл уже читается другим “вызывающим”, то ожидаем пока тот зачитает файл и отдаем результат чтения активного читателя (двойной “перечитки” файла НЕ производим).
- если же файл не читается никем иным, то читаем его и эмулируем задержку в 2 секунды (эмуляция “долгого чтения”).

### Будет плюсом
1. Если есть сценарии нагрузочного тестирования
2. Метрики
3. Документация

***
## Реализация

Создан микросервис с одним end-point'ом, который принимает на вход имя файла и возвращает результат его чтения.\
Более подробную информацию можно найти в swagger (GET /swagger/index.html)

Приложение состоит из трёх основных вещей:
1. Контроллер
2. Сервис чтения файлов
3. "Читатель" файла

#### Контроллер
Получает название файла. Делает базовые првоерки и вызывает чтение файла в Сервисе чтения файлов.

#### Сервис чтения фалов
Сервис, хранящий в себе "Читателей" файлов.
Когда приходит запрос на чтение файла и в данный момент "Читатель" данного файла уже есть, то вызывается метод чтения файла в этом "Читателе".
Если такой "Читатель" не зарегистрирован, то он создаётся, сохраняется в сервисе, и вызывается метод чтения файла.
Синхронизация потоков организована с использованием ConcurrentDictionary<TKey,TValue>, который позволяет атомарно выполнять операции добавления/взятия/удаления объектов в/из словаря.

#### "Читатель" файла
Производит чтение файла и сохраняет в себе результат чтения.\
Когда несколько поток одновременно вызывают метод чтения файла, только один из потоков производит чтение файла, остальный потоки ждут завершения операции чтения и возвращают результат чтения первого потока.\
Данная синхронизация потоков осуществима благодяря наличию двух событий синхронизации потоков: AutoResetEvent и ManualREsetEvent.
Изначально, AutoResetEvent находится во включённом состоянии, а ManualREsetEvent - во выключенном состоянии.
Когда первый поток начинает процесс чтения файла, происходит проверка, что хотябы одно из событий синхронизации потоков находится во включённом состоянии. После того, как один первый поток проходит эту проверку, AutoResetEvent автоматически переходит во выключенное состояние, поэтому все последующие потоки становятся в ожидание того, что одно из событий перейдёт во включённое состояние. Первый поток видит, что чтение файла ещё не было произведено, читает его и сохраняет результат. Следующим шагом он переводит ManualREsetEvent во включённое состояние, сигнализируя о том, что остальные потоки могут идти дальше. Остальные потоки, видя что результат чтения уже получен, возвращают его, а первый поток возвращает результат в Сервис чтения файлов и уведомляет его, что данный читатель можно удалить из словаря, чтобы следующие запросы заново производили чтение файла.

***
## Запуск

Для запуска дополнительных сервисов необходимо выполнить следующие команды в папке с файлом docker-compose.yml

	docker-compose build
	docker-compose up

которые запустят Postgres для сбора логов и Prometheus для метрик.

Prometheus -> localhost:9090\
Postgres -> User ID=user;Password=password;Host=localhost;Port=5432;Database=postgres\
PatronApp -> localhost:5000

Сам проект необходимо собрать и запустить. Его я оставил вне docker-compose, чтобы можно было в режиме debug посмотреть как ведут себя потоки при чтении одного файла.

### Нагрузочное тестирование

В проекте находится файл Patron_Plan.jmx, для проведения нагрузочного тестирования в JMeter. \
В нём находятся 3 ThreadGroup для отправки запросов на прочтение разного наборы файлов:
1. singe.txt
2. (0-9).txt
3. (10-999).txt
