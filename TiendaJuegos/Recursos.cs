using System.Drawing.Drawing2D;

namespace TiendaJuegos;

/// <summary>Carga imágenes desde la carpeta Imagenes (junto al .exe). Si falta una, devuelve null sin romper la app.</summary>
public static class Recursos
{
    public static Image Cargar(string archivo)
    {
        try
        {
            string ruta = Path.Combine(AppContext.BaseDirectory, "Imagenes", archivo);
            if (!File.Exists(ruta)) return null;
            using var fs = File.OpenRead(ruta);
            using var tmp = Image.FromStream(fs);
            return new Bitmap(tmp);     // copia: no deja el archivo bloqueado
        }
        catch { return null; }
    }
}

/// <summary>Muestra una imagen llenando todo el control (recorta los bordes en lugar de deformar).</summary>
public class CoverPictureBox : Control
{
    public Image Imagen { get; set; }
    public float FocoX { get; set; } = 0.5f;
    public float FocoY { get; set; } = 0.5f;

    public CoverPictureBox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (Imagen == null || Width == 0 || Height == 0) return;
        float escala = Math.Max((float)Width / Imagen.Width, (float)Height / Imagen.Height);
        float w = Imagen.Width * escala, h = Imagen.Height * escala;
        float x = -(w - Width) * FocoX, y = -(h - Height) * FocoY;
        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        e.Graphics.DrawImage(Imagen, x, y, w, h);
    }
}
