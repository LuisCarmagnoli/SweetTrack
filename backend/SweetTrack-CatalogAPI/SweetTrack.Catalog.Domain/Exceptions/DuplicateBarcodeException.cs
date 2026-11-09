namespace SweetTrack.Catalog.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando se intenta crear o actualizar un producto con un código de barras ya registrado.
/// </summary>
public class DuplicateBarcodeException : Exception
{
    public string Barcode { get; }

    public DuplicateBarcodeException(string barcode)
        : base($"El código de barras '{barcode}' ya está registrado para otro producto en el catálogo.")
    {
        Barcode = barcode;
    }
}
