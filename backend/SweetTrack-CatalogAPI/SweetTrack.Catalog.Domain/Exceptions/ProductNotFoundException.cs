namespace SweetTrack.Catalog.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando un producto no existe en el catálogo por su identificador o código de barras.
/// </summary>
public class ProductNotFoundException : Exception
{
    public ProductNotFoundException(int id)
        : base($"El producto con ID '{id}' no fue encontrado en el catálogo.")
    {
    }

    public ProductNotFoundException(string barcode)
        : base($"No se encontró ningún producto con el código de barras '{barcode}'.")
    {
    }
}

