namespace Image.Compressor.POC
{
    public class Program
    {
        const int kb = 1024;

        public static void Main(string[] args)
        {
            if (args[0].ToLower() == "help")
            {
                Help();
            }
            else
            {
                if (!ValidateParameters(args))
                {
                    Console.WriteLine("Parâmetros inválidos - Execute o comando 'Help' para verificar a sintaxe");
                    return;
                }

                try
                {
                    if (File.Exists(args[0]))
                    {
                        ProcessFile(new(null,
                                        args[0],
                                        args[1],
                                        int.Parse(args[2]),
                                        int.Parse(args[3]),
                                        int.Parse(args[4])));
                    }
                    else if (Directory.Exists(args[0]))
                    {
                        ProcessDirectory(new(args[0],
                                             null,
                                             args[1],
                                             int.Parse(args[2]),
                                             int.Parse(args[3]),
                                             int.Parse(args[4])));
                    }
                    else
                    {
                        Console.WriteLine("{0} is not a valid file or directory.", args[0]);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("\n Error");
                    Console.WriteLine(e.Message);
                    throw;
                }
            }

        }

        public static void ProcessDirectory(Parameters parameters)
        {
            string[] fileEntries = Directory.GetFiles(parameters.Folder!);
            foreach (string fileName in fileEntries)
                ProcessFile(new(null,
                                fileName,
                                parameters.Extension,
                                parameters.Greater,
                                parameters.Height,
                                parameters.Compression));

            string[] subdirectoryEntries = Directory.GetDirectories(parameters.Folder!);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(new(subdirectory,
                                    null,
                                    parameters.Extension,
                                    parameters.Greater,
                                    parameters.Height,
                                    parameters.Compression));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public static void ProcessFile(Parameters parameters)
        {
            var fileInfo = new FileInfo(parameters.File!);

            string[] extensions = { ".JPEG", ".JPG", ".PNG", ".BMP" };

            if (!extensions.Contains(fileInfo.Extension.ToUpper())) return;

            if (fileInfo.Length / kb < parameters.Greater!) return;

            System.Drawing.Image image = Compressor.ResizeImage(System.Drawing.Image.FromFile(parameters.File!), parameters.Height!);

            File.WriteAllBytes(parameters.File!, Compressor.ConvertImage(image)!);

            var fileInfoNewImage = new FileInfo(parameters.File!);

            if (fileInfoNewImage.Length / kb > parameters.Greater!)
            {
                File.WriteAllBytes(parameters.File!,
                    Compressor.Compress(image, parameters.Compression)!);
            }

            Console.WriteLine("Processed file '{0}'.", parameters.File!);
        }

        private static bool ValidateParameters(string[] args)
        {
            if (args.Length < 4) return false;
            if (string.IsNullOrEmpty(args[0])) return false;
            if (string.IsNullOrEmpty(args[1])) return false;
            if (string.IsNullOrEmpty(args[2])) return false;
            if (string.IsNullOrEmpty(args[3])) return false;
            if (!int.TryParse(args[2], out _)) return false;
            if (!int.TryParse(args[3], out int _)) return false;
            if (!int.TryParse(args[4], out int _)) return false;
            return true;
        }

        private static void Help()
        {
            Console.WriteLine(@"
SYNTAX
    FOLDER: .\CompressorImagem.exe [[-Folder] <string>] [[-Extension] <string>] [[-Greater] <int>] [[-Height] <int>] [[-Compression] <int>]
    FILE:   .\CompressorImagem.exe [[-File] <string>] [[-Greater] <int>] [[-Height] <int>] [[-Compression] <int>]


PARAMETERS
    -Folder <string>

        Required?                    true
        Position?                    0
        Description                  Caminho da pasta

    -Extension <string>

        Required?                    true
        Position?                    1
        Description                  Extensão da imagem para compactação - Aceitas: .JPEG .JPG .PNG .BMP


    -Greater <int>

        Required?                    true
        Position?                    2
        Description                  Arquivos maiores que <valor> serão incluidos na compactação

    -Height <int>

        Required?                    true
        Position?                    3
        Description                  Altura da imagem

    -Compression <int>

        Required?                    true
        Position?                    4
        Description                  Taxa de compressão, quanto maior melhor a qualidade
");
        }
    }
}