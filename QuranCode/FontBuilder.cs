using System;
using System.Reflection;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;

public class FontBuilder
{
    // Adding a private font (Win2000 and later)
    [DllImport("gdi32.dll", ExactSpelling = true)]
    private static extern IntPtr AddFontMemResourceEx(byte[] pbFont, int cbFont, IntPtr pdv, out uint pcFonts);

    // Cleanup of a private font (Win2000 and later)
    [DllImport("gdi32.dll", ExactSpelling = true)]
    internal static extern bool RemoveFontMemResourceEx(IntPtr fh);

    // Some private holders of font information we are loading
    private static IntPtr s_handler = IntPtr.Zero;
    private static PrivateFontCollection s_fonts = null;

    public static Font Build(string font_filename, float font_size)
    {
        Font font = null;

        InstallFont(font_filename);
        if (s_fonts != null)
        {
            if (s_fonts.Families.Length > 0)
            {
                // Handy how one of the Font constructors takes a FontFamily object, huh? :-)
                font = new Font(s_fonts.Families[0], font_size);
            }
        }

        return font;
    }

    private static void InstallFont(string font_filename)
    {
        if (s_fonts == null)
        {
            // First load the font as a memory stream
            Stream font_stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(font_filename);
            if (font_stream != null)
            {
                // 
                // GDI+ wants a pointer to memory,
                // GDI wants the memory.
                // We will make them both happy.
                // 

                // First read the font into a buffer
                byte[] buffer = new Byte[font_stream.Length];
                font_stream.Read(buffer, 0, buffer.Length);

                // Then do the unmanaged font (Windows 2000 and later)
                // The reason this works is that GDI+ will create a font object for
                // controls like the RichTextBox and this call will make sure that GDI
                // recognizes the font name, later.
                uint font_count;
                AddFontMemResourceEx(buffer, buffer.Length, IntPtr.Zero, out font_count);

                // Now do the managed font
                IntPtr p_buffer = Marshal.AllocCoTaskMem(buffer.Length);
                if (p_buffer != null)
                {
                    Marshal.Copy(buffer, 0, p_buffer, buffer.Length);
                    s_fonts = new PrivateFontCollection();
                    s_fonts.AddMemoryFont(p_buffer, buffer.Length);
                    Marshal.FreeCoTaskMem(p_buffer);
                }
            }
        }
    }
}
