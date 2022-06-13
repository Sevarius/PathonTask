using System;
using FileReaderApplication.BusinessLogic.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileReaderApplication.BusinessLogic.Impl
{
    /// <summary>
    /// Фабрика создания "читателей" файлов
    /// </summary>
    public class FileReaderFactory : IFileReaderFactory
    {
        /// <summary>
        /// Длительность чтения файла (в случае, если в конфигурации данный параметр не задан)
        /// </summary>
        private const int HARD_WORK_DURATION = 3;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="serviceProvider">Провайдер сервисов</param>
        /// <param name="configuration">Параметры конфигурации приложения</param>
        public FileReaderFactory(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        /// <summary>
        /// Создать "читателя" фала
        /// </summary>
        public FileReader CreateFileReader(string fileName)
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<FileReader>>();
            var fileReader =  new FileReader(fileName, logger);

            var duration = _configuration.GetValue<int?>("FileReading:HardWorkDuration") ?? HARD_WORK_DURATION;
            fileReader.HardWorkDuration = duration;

            return fileReader;
        }
    }
}