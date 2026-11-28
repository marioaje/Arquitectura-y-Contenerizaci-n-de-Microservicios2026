using System.ComponentModel.DataAnnotations;
namespace SharedContracts;

public sealed class TransferenciaRequest : IValidatableObject
{
    public Guid SolicitudId { get; set; }
    [Range(1, int.MaxValue)] public int CuentaOrigenId { get; set; }
    [Range(1, int.MaxValue)] public int CuentaDestinoId { get; set; }
    [Range(typeof(decimal), "0.01", "999999999999")] public decimal Monto { get; set; }
    [StringLength(250)] public string Descripcion { get; set; } = "";

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (SolicitudId == Guid.Empty)
            yield return new ValidationResult("SolicitudId debe ser un GUID no vacío.", [nameof(SolicitudId)]);
        if (CuentaOrigenId == CuentaDestinoId)
            yield return new ValidationResult("Las cuentas deben ser diferentes.", [nameof(CuentaDestinoId)]);
    }
}
