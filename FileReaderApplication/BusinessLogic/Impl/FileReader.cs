using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FileReaderApplication.BusinessLogic.Dto;
using Microsoft.Extensions.Logging;

namespace FileReaderApplication.BusinessLogic.Impl
{
    /// <summary>
    /// Читатель файлов
    /// </summary>
    public class FileReader
    {
        private readonly ILogger<FileReader> _logger;
        private readonly ManualResetEvent _manualResetEvent;
        private readonly AutoResetEvent _autoResetEvent;
        
        /// <summary>
        /// Имя файла
        /// </summary>
        private string FileName { get; }

        /// <summary>
        /// Id "Читателя"
        /// </summary>
        private Guid ReaderId { get; }

        /// <summary>
        /// Длительность чтения файла (в секундах)
        /// </summary>
        public int HardWorkDuration { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="logger">Логгер</param>
        public FileReader(string fileName, ILogger<FileReader> logger)
        {
            _logger = logger;
            FileName = fileName;
            _manualResetEvent = new ManualResetEvent(false);
            _autoResetEvent = new AutoResetEvent(true);
            ReaderId = Guid.NewGuid();
        }
        
        private ReadResultDto Result { get; set; }

        /// <summary>
        /// Прочитать файли
        /// </summary>
        /// <param name="requestId">Id запроса</param>
        /// <returns>(Завершилось ли чтение, Результат чтения)</returns>
        public async Task<(bool, ReadResultDto)> ReadFile(string requestId)
        {
            using (_logger.BeginScope(new {requestId, FileName, ReaderId}))
            {
                _logger.LogInformation("Старт попытки чтения файла");
                WaitHandle.WaitAny(new WaitHandle[] {_manualResetEvent, _autoResetEvent});

                if (Result == null)
                {
                    _logger.LogInformation("Результат чтения ещё не определён. Начало чтения файла");
                    try
                    {
                        var readingResult = await DoHardWork();
                        Result = new ReadResultDto
                        {
                            IsError = false,
                            Result =  readingResult
                        };
                        _logger.LogInformation("Чтение прошло успешно. Результат: {Result}", JsonSerializer.Serialize(Result));
                    }
                    catch (Exception e)
                    {
                        Result = new ReadResultDto
                        {
                            IsError = true,
                            Exception = e
                        };
                        _logger.LogError(e, "Возникла ошибка чтения файла");
                    }
                    finally
                    {
                        _logger.LogInformation("Отпускаем все остальные потоки на получение результата");
                        _manualResetEvent.Set();
                    }
                
                    return (true, Result);
                }

                _logger.LogInformation("Результат уже был получен. Результат: {Result}", JsonSerializer.Serialize(Result));
                return (false, Result);
            }
            
        }

        /// <summary>
        /// Выполнить сложную работы и прочитать файл
        /// </summary>
        /// <returns>Результат чтения файла</returns>
        private async Task<string> DoHardWork()
        {
            await Task.Delay(TimeSpan.FromSeconds(HardWorkDuration));
            if (FileName == "seva_bad.txt")
            {
                throw new ApplicationException($"Произошла ошибка чтения файла {FileName}");
            }
            return FileName;
        }
    }
}