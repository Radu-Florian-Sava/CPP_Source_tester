using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using NETCoreBackend.Utility;
using System.Text.RegularExpressions;

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

        [HttpPost("postSource/{id}")]
        public IActionResult PostSource(IFormFile file, [FromRoute] string id)
        {

            string cppSourceName = "source.cpp";

            string uploadFolder = Path.Combine(_configurationPaths["UploadPath"], id);

            string filePath = Path.Combine(uploadFolder, cppSourceName);

            Console.WriteLine("Source file loading at {0}", filePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

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

        [HttpPost("postInput/{id}")]
        public IActionResult PostInput(IFormFile file, [FromRoute] int id)
        {

            if (file.FileName != _token)
            {
                return Ok(JsonSerializer.Serialize("Invalid credentials"));
            }

            string inputFileName = "input" + id + ".txt";

            string filePath = Path.Combine(_unploadFolderName, inputFileName);

            Console.WriteLine("Input file loading at {0}", filePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            return Ok(JsonSerializer.Serialize("Input file uploaded"));
        }

        [HttpPost("postOutput/{id}")]
        public IActionResult PostOutput(IFormFile file, [FromRoute] int id)
        {
            if (file.FileName != _token)
            {
                return Ok(JsonSerializer.Serialize("Invalid credentials"));
            }

            string outputTemplate = "output" + id + ".txt";

            string filePath = Path.Combine(_unploadFolderName, outputTemplate);

            Console.WriteLine("Output file loading at {0}", filePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return Ok(JsonSerializer.Serialize("Output template uploaded"));
        }

        [HttpGet("getRunCMD/{id}")]
        public IActionResult GetRun([FromRoute] string id)
        {
            return runCpp(id);
        }

        [HttpGet("getProblems/{problemName}")]
        public IActionResult getProblemById([FromRoute] string problemName)
        {
            string directoryName = Path.Combine(_configurationPaths["UploadPath"], problemName);
            string input = System.IO.File.ReadAllText(Path.Combine(directoryName, "input1.txt"));
            string output = System.IO.File.ReadAllText(Path.Combine(directoryName, "output1.txt"));
            string description = System.IO.File.ReadAllText(Path.Combine(directoryName, "description.txt"));
            return Ok(new string[] { description, input, output });
        }

        private IActionResult runCpp(string id) 
        {
            Process process = processStartup();
            string runPath = Path.Combine(_configurationPaths["UploadPath"], id);
            int total = 0, correct = 0;
            using (StreamWriter sw = process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine("cd " + runPath);
                    sw.WriteLine(_configurationPaths["CompilerPath"] + " source.cpp -o source.exe");
                    total = Directory.GetFiles(runPath).Where( path => Regex.Match(path, @".input\d+\.txt$").Success).Count();
                    for (int i=1;i <= total; i++)
                    {
                        sw.WriteLine("source.exe input" + i + ".txt source_output" + i +".txt");
                    }
                }
            }

            process.WaitForExit();


            if(process.ExitCode != 0)
            {
                return BadRequest(JsonSerializer.Serialize("Code could not be compiled due to errors"));
            }

            for(int i=1;i < total;i++)
            {
                if(compareOutput(runPath, "source_output" + i +".txt", "output"+ i + ".txt"))
                {
                    correct++;
                }
            }
            Directory.GetFiles(runPath).Where(path => Regex.Match(path, @".source_output\d+\.txt$").Success)
                .ToList().ForEach(path => System.IO.File.Delete(path));
            return Ok(JsonSerializer.Serialize("Score is " + correct + "/" + total));
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
        
        private bool compareOutput(string runPath,string computedOutput, string actualOutput)
        {
            string computedOutputFilePath = Path.Combine(runPath, computedOutput);
            string actualOutputFilePath = Path.Combine(runPath, actualOutput);

            return CheckSum.SHA256CheckSum(computedOutputFilePath) == CheckSum.SHA256CheckSum(actualOutputFilePath);
        }

        private IEnumerable<string> extractProblemNames(IEnumerable<string> problems)
        {
            return problems.Select(problem => Path.GetFileName(problem)).ToArray();
        }
    }
}