using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO;

public class UpdateAddressDto
{
    [Required(ErrorMessage = "Поле 'Область' є обов'язковим")]
    public int RegionId { get; set; }

    [Required(ErrorMessage = "Поле 'Місто' є обов'язковим")]
    [StringLength(100, ErrorMessage = "Назва міста не може перевищувати 100 символів")]
    public required string City { get; set; }

    [Required(ErrorMessage = "Поле 'Вулиця' є обов'язковим")]
    [StringLength(200, ErrorMessage = "Назва вулиці не може перевищувати 200 символів")]
    public required string Street { get; set; }

    [Required(ErrorMessage = "Поле 'Номер будинку' є обов'язковим")]
    [StringLength(20, ErrorMessage = "Номер будинку не може перевищувати 20 символів")]
    public required string BuildingNumber { get; set; }

    [StringLength(20, ErrorMessage = "Номер квартири/офісу не може перевищувати 20 символів")]
    public string? ApartmentNumber { get; set; }

    [Required(ErrorMessage = "Поле 'Поштовий індекс' є обов'язковим")]
    [StringLength(5, MinimumLength = 5, ErrorMessage = "Поштовий індекс має складатися з 5 символів")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "Поштовий індекс має містити тільки 5 цифр")]
    public required string ZipCode { get; set; }

    [StringLength(500, ErrorMessage = "Примітки не можуть перевищувати 500 символів")]
    public string? Notes { get; set; }

    public bool IsPrimary { get; set; }

    [Required(ErrorMessage = "Поле 'Тип адреси' є обов'язковим")]
    public int AddressTypeId { get; set; }
}
