using System;

namespace FileReaderApplication.BusinessLogic.Dto
{
    /// <summary>
    /// Результат чтения файла
    /// </summary>
    public class ReadResultDto
    {
        /// <summary>
        /// Возникла ли ошибка при чтении файла
        /// </summary>
        public bool IsError { get; set; }
        
        /// <summary>
        /// Результат чтения файла
        /// </summary>
        public string Result { get; set; }
        
        /// <summary>
        /// Ошибка, возникшая при чтении файла
        /// </summary>
        public Exception Exception { get; set; }
    }
}