using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;
using FileReaderApplication.BusinessLogic.Dto;
using FileReaderApplication.BusinessLogic.Interface;
using Microsoft.Extensions.Logging;

namespace FileReaderApplication.BusinessLogic.Impl
{
    /// <summary>
    /// Сервис чтения файлов
    /// </summary>
    public class FileReaderService : IFileReaderService
    {
        private readonly ILogger<FileReaderService> _logger;
        private readonly IFileReaderFactory _fileReaderFactory;
        private readonly ConcurrentDictionary<string, FileReader> _filesDictionary;

        /// <summary>
        /// Кончтруктор
        /// </summary>
        /// <param name="logger">Логгер</param>
        /// <param name="fileReaderFactory">Фабрика создания "Читателей" фалов</param>
        public FileReaderService(ILogger<FileReaderService> logger, IFileReaderFactory fileReaderFactory)
        {
            _logger = logger;
            _fileReaderFactory = fileReaderFactory;
            _filesDictionary = new ConcurrentDictionary<string, FileReader>();
        }
        
        
        /// <summary>
        /// Прочитать файл
        /// </summary>
        public async Task<ReadResultDto> ReadFile(string requestId, string fileName)
        {
            using (_logger.BeginScope(new {requestId, fileName}))
            {
                _logger.LogInformation("Получение \"читателя файла\"");
                var fileReader = _filesDictionary.GetOrAdd(fileName, _fileReaderFactory.CreateFileReader);
                _logger.LogInformation("Начало чтения файла");
                var (isReadingCompleted, readingResult) = await fileReader.ReadFile(requestId);
                _logger.LogInformation("Файл прочтён. Результат: {Result}", JsonSerializer.Serialize(readingResult));
                if (isReadingCompleted)
                {
                    _logger.LogInformation("Удаление \"Читателя\" из словаря");
                    _filesDictionary.TryRemove(fileName, out _);
                }

                return readingResult;
            }
        }
    }
}