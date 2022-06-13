using System.Threading.Tasks;
using FileReaderApplication.BusinessLogic.Dto;

namespace FileReaderApplication.BusinessLogic.Interface
{
    /// <summary>
    /// Сервис чтения файлов
    /// </summary>
    public interface IFileReaderService
    {
        /// <summary>
        /// Прочитать файл
        /// </summary>
        /// <param name="requestId">Id запроса</param>
        /// <param name="fileName">Имя файла</param>
        /// <returns>Результат чтения файла</returns>
        public Task<ReadResultDto> ReadFile(string requestId, string fileName);
    }
}