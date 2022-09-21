using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using NETCoreBackend.Utility;

namespace NETCoreBackend.Controllers
{
    [ApiController]
    [Route("cpptester")]
    public class CppTesterController : ControllerBase
    {

        private readonly ILogger<CppTesterController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfigurationSection _configurationPaths;
        private readonly IConfigurationSection _credentials;
        private static int _correctCounter = 0;
        private static int _compileCounter = 0;
        private static string? _token = null;
        private static string? _unploadFolderName = null;

        public CppTesterController(ILogger<CppTesterController> logger, IWebHostEnvironment webHostEnvironment)
        {

            _configurationPaths = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("LocalPaths");
            _credentials = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AdminCredentials");
        }

        [HttpPost]
        [Route("postCredentials")]
        public IActionResult PostCredentials([FromBody] Credentials credentials)
        {
            string? username = credentials.username;
            string? password = credentials.password;

            if (_token == null)
            {
                if (username == _credentials["Username"] && password == _credentials["PasswordHash"])
                {
                    do
                    {
                        _token = TokenGenerator.GenerateToken(10);
                        _unploadFolderName = Path.Combine(_configurationPaths["UploadPath"], _token);
                    } while (Directory.Exists(_unploadFolderName));
                    Directory.CreateDirectory(_unploadFolderName);
                    Console.WriteLine("Generated token: {0}", _token);
                    return Ok(JsonSerializer.Serialize(_token));
                }
                else
                {
                    return BadRequest(JsonSerializer.Serialize("Credentials are not correct"));
                }
            }
            return Ok("Already logged in");
        }


        [HttpGet]
        [Route("getProblems")]
        public IActionResult GetProblems()
        {
            var directoryNames = extractProblemNames(Directory.EnumerateDirectories(_configurationPaths["UploadPath"]));
            return Ok(JsonSerializer.Serialize(directoryNames));
        }

        [HttpPost]
        [Route("postSource")]
        public IActionResult PostSource(IFormFile file)
        {

            string cppSourceName = "source.cpp";

            string filePath = Path.Combine(_unploadFolderName, cppSourceName);

            Console.WriteLine("Source file loading at {0}", filePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            _correctCounter = 0;
            _compileCounter = 0;

            return Ok(JsonSerializer.Serialize("Cpp source uploaded"));
        }

        [HttpPost]
        [Route("postDescription")]
        public IActionResult PostDescription(IFormFile file)
        {

            if (file.FileName != _token)
            {
                return Ok(JsonSerializer.Serialize("Invalid credentials"));
            }

            string inputFileName = "description.txt";

            string filePath = Path.Combine(_unploadFolderName, inputFileName);

            Console.WriteLine("Description file loading at {0}", filePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return Ok(JsonSerializer.Serialize("Description file uploaded"));
        }

        [HttpPost]
        [Route("postInput")]
        public IActionResult PostInput(IFormFile file)
        {

            if (file.FileName != _token)
            {
                return Ok(JsonSerializer.Serialize("Invalid credentials"));
            }

            string inputFileName = "input.txt";

            string filePath = Path.Combine(_unploadFolderName, inputFileName);

            Console.WriteLine("Input file loading at {0}", filePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            return Ok(JsonSerializer.Serialize("Input file uploaded"));
        }

        [HttpPost]
        [Route("postOutput")]
        public IActionResult PostOutput(IFormFile file)
        {
            if (file.FileName != _token)
            {
                return Ok(JsonSerializer.Serialize("Invalid credentials"));
            }

            string outputTemplate = "output.txt";

            string filePath = Path.Combine(_unploadFolderName, outputTemplate);

            Console.WriteLine("Output file loading at {0}", filePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return Ok(JsonSerializer.Serialize("Output template uploaded"));
        }

        [HttpGet]
        [Route("getRunCMD")]
        public IActionResult GetRun()
        {
            return runCpp();
        }

        [HttpGet("getProblems/{problemName}")]
        public IActionResult getProblemById([FromRoute] string problemName)
        {
            string directoryName = Path.Combine(_configurationPaths["UploadPath"], problemName);
            string input = System.IO.File.ReadAllText(Path.Combine(directoryName, "input.txt"));
            string output = System.IO.File.ReadAllText(Path.Combine(directoryName, "output.txt"));
            string description = System.IO.File.ReadAllText(Path.Combine(directoryName, "description.txt"));
            return Ok(new string[] { description, input, output });
        }

        private IActionResult runCpp() 
        {
            Process process = processStartup();

            using (StreamWriter sw = process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine("cd " + _unploadFolderName);
                    sw.WriteLine(_configurationPaths["CompilerPath"] + " source.cpp -o source.exe");
                    sw.WriteLine("source.exe input.txt source_output.txt");
                }
            }

            process.WaitForExit();

            _compileCounter++;

            if(process.ExitCode != 0)
            {
                return BadRequest(JsonSerializer.Serialize("Code could not be compiled due to errors"));
            }

            if (compareOutput("source_output.txt", "output.txt"))
            {
                _correctCounter++;
                return Ok(JsonSerializer.Serialize("output is correct " + _correctCounter + "/" + _compileCounter));
            }
            else
            {
                return Ok(JsonSerializer.Serialize("output is not correct " + _correctCounter + "/" + _compileCounter));
            }
        }

        private Process processStartup()
        {
            Process process = new Process();
            var processInfo = new ProcessStartInfo();
            processInfo.WorkingDirectory = _configurationPaths["WorkingDirectoryPath"];
            processInfo.FileName = _configurationPaths["CmdPath"];
            processInfo.Verb = "runas";
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardInput = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;
            processInfo.WindowStyle = ProcessWindowStyle.Maximized;
            process.StartInfo = processInfo;
            process.Start();
            process.OutputDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
            process.ErrorDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return process;
        }
        
        private bool compareOutput(string computedOutput, string actualOutput)
        {
            string computedOutputFilePath = Path.Combine(_unploadFolderName, computedOutput);
            string actualOutputFilePath = Path.Combine(_unploadFolderName, actualOutput);

            return CheckSum.SHA256CheckSum(computedOutputFilePath) == CheckSum.SHA256CheckSum(actualOutputFilePath);

        }

        private IEnumerable<string> extractProblemNames(IEnumerable<string> problems)
        {
            return problems.Select(problem => Path.GetFileName(problem)).ToArray();
        }
    }
}