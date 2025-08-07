using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO;

public class CreateCurrencyDto
{
    [Required(ErrorMessage = "Поле 'Назва' є обов'язковим")]
    [StringLength(50, ErrorMessage = "Назва валюти не може перевищувати 50 символів")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Поле 'Символ' є обов'язковим")]
    [StringLength(10, ErrorMessage = "Символ валюти не може перевищувати 10 символів")]
    public string Symbol { get; set; } = string.Empty;
}
