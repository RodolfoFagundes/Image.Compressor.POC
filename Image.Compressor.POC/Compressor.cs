using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Image.Compressor.POC;

class Compressor
{
    private static readonly object LockImage = new();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public static byte[]? Compress(System.Drawing.Image image, long encoderValue)
    {
        byte[]? imageByte = ConvertImage(image);
        if (imageByte == null) return null;

        ImageCodecInfo? jpgEncoder = GetEncoder(ImageFormat.Jpeg);
        if (jpgEncoder == null) return null;

        using MemoryStream msIn = new(imageByte);
        using Bitmap bmp = new(msIn);
        EncoderParameters myEncoderParameters = new(1);
        myEncoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, encoderValue);

        using MemoryStream msOut = new();
        bmp.Save(msOut, jpgEncoder!, myEncoderParameters);

        return msOut.ToArray();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public static System.Drawing.Image ResizeImage(System.Drawing.Image image, int heigth)
    {
        if (image.Height <= heigth)
        {
            return image;
        }

        int widthOriginal;
        int heigthOriginal;
        byte? orientation = null;
        PixelFormat pixelFormat;

        try
        {
            orientation = image.GetPropertyItem(274)?.Value?[0];
        }
        catch (Exception)
        {
        }

        //HACK: Avoid the mistake of InvalidOperationException -
        //      The object is being used elsewhere. Common on desktop caused because the Image class (Gdi+) is not "thread safe" 
        lock (LockImage)
        {
            if (orientation is not null)
            {
                // The picture is rotated 90 or 270
                image.RotateFlip(OrientationToFlipType(orientation!.ToString()!));
            }
            widthOriginal = image.Width;
            heigthOriginal = image.Height;
            pixelFormat = image.PixelFormat;
        }

        double aspectRadio = widthOriginal / (double)heigthOriginal;

        int widthMax = (int)(heigth * aspectRadio);

        double widthFactor = widthOriginal / (double)widthMax;
        double heigthFactor = heigthOriginal / (double)heigth;

        int newWidth, newHeigth;

        if (widthFactor >= heigthFactor)
        {
            newWidth = (int)(widthOriginal / widthFactor);
            newHeigth = (int)(heigthOriginal / widthFactor);
        }
        else
        {
            newWidth = (int)(widthOriginal / heigthFactor);
            newHeigth = (int)(heigthOriginal / heigthFactor);
        }

        Bitmap bitmap = new(widthMax, heigth, pixelFormat);
        bitmap.SetResolution(96, 96);

        Graphics graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.None;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.Clear(Color.White);

        // Want this centred, so must identify where this graphic should be placed on the new bitmap
        int posicaoX = (widthMax - newWidth) / 2;
        int posicaoY = (heigth - newHeigth) / 2;

        lock (LockImage)
        {
            graphics.DrawImage(image, posicaoX, posicaoY, newWidth, newHeigth);
        }

        image.Dispose();

        return bitmap;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public static byte[]? ConvertImage(System.Drawing.Image imagem)
    {
        if (imagem == null)
        {
            return null;
        }

        using Bitmap bitMap = new(original: imagem);

        return ConvertImage(bitMap);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    private static byte[] ConvertImage(Bitmap bmp)
    {
        using MemoryStream ms = new();

        bmp.Save(ms, format: ImageFormat.Bmp);

        return ms.ToArray();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    private static ImageCodecInfo? GetEncoder(ImageFormat format)
    {
        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

        foreach (ImageCodecInfo codec in codecs)
        {
            if (codec.FormatID == format.Guid)
            {
                return codec;
            }
        }

        return null;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    private static RotateFlipType OrientationToFlipType(string orientation)
    {
        return int.Parse(orientation) switch
        {
            1 => RotateFlipType.RotateNoneFlipNone,
            2 => RotateFlipType.RotateNoneFlipX,
            3 => RotateFlipType.Rotate180FlipNone,
            4 => RotateFlipType.Rotate180FlipX,
            5 => RotateFlipType.Rotate90FlipX,
            6 => RotateFlipType.Rotate90FlipNone,
            7 => RotateFlipType.Rotate270FlipX,
            8 => RotateFlipType.Rotate270FlipNone,
            _ => RotateFlipType.RotateNoneFlipNone,
        };
    }
}
