using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO;

public class CreateCurrencyDto
{
    [Required(ErrorMessage = "Поле 'Код валюти' є обов'язковим")]
    [StringLength(10, ErrorMessage = "Код валюти не може перевищувати 10 символів")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле 'Назва' є обов'язковим")]
    [StringLength(255, ErrorMessage = "Назва валюти не може перевищувати 255 символів")]
    public string Name { get; set; } = string.Empty;

    [StringLength(10, ErrorMessage = "Символ валюти не може перевищувати 10 символів")]
    public string? Symbol { get; set; }
}
