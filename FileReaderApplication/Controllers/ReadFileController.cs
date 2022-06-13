using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using FileReaderApplication.BusinessLogic.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FileReaderApplication.Controllers
{
    /// <summary>
    /// Контроллер методов чтения файла
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ReadFileController : ControllerBase
    {
        /// <summary>
        /// Расширение файла для обработки (необходимо, если не задано в конфигурации)
        /// </summary>
        private const string FILE_EXTENSION = ".txt";
        private readonly IFileReaderService _fileReaderService;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="fileReaderService">Сервис чтения файлов</param>
        /// <param name="configuration">Параметры конфигурации приложения</param>
        public ReadFileController(IFileReaderService fileReaderService, IConfiguration configuration)
        {
            _fileReaderService = fileReaderService;
            _configuration = configuration;
        }

        /// <summary>
        /// Чтение файла
        /// </summary>
        /// <param name="requestId">Id запроса</param>
        /// <param name="filename">Имя файла</param>
        /// <returns>результат чтения файла</returns>
        [HttpGet]
        public async Task<IActionResult> ReadFile([FromHeader] string requestId, [Required] string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return BadRequest("В качестве имени файла была подана пустая строка");
            }
            var extension = Path.GetExtension(filename);
            var expectedExtension = _configuration.GetValue<string>("FileReading:FileExtension") ?? FILE_EXTENSION;
            if (extension != expectedExtension)
            {
                return BadRequest($"В параметре запроса должен быть указан файл с расширением {expectedExtension}");
            }

            requestId ??= Guid.NewGuid().ToString();
            var result = await _fileReaderService.ReadFile(requestId, Path.GetFileName(filename));

            if (result.IsError)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }
    }
}