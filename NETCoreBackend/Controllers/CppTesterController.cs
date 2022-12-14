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
        private static string? _uploadFolderName = null;

        public CppTesterController(ILogger<CppTesterController> logger, IWebHostEnvironment webHostEnvironment)
        {

            _configurationPaths = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("LocalPaths");
            _credentials = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AdminCredentials");
        }

        [HttpPost("credentials")]
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
                        _uploadFolderName = Path.Combine(_configurationPaths["UploadPath"], _token);
                    } while (Directory.Exists(_uploadFolderName));
                    Directory.CreateDirectory(_uploadFolderName);
                    Console.WriteLine("Generated token: {0}", _token);
                    return Ok(JsonSerializer.Serialize(_token));
                }
                else
                {
                    return BadRequest(JsonSerializer.Serialize("Credentials are not correct"));
                }
            }
            return Ok(JsonSerializer.Serialize("Already logged in"));
        }

        [HttpDelete("credentials/{token}")]
        public void DeleteCredentials([FromRoute] string token)
        {
            if(_token == token)
            {
                Console.WriteLine(String.Format("TOKEN {0} DELETED", _token));
                _token = null;
            }
            else
            {
                Console.WriteLine("TOKEN DELETION REFUSED");
            }
        }


        [HttpGet("hash/{toBeHashed}")]
        public void ShowHash([FromRoute] string toBeHashed)
        {

            using (System.Security.Cryptography.SHA256 hash = System.Security.Cryptography.SHA256.Create())
            {
                string hashed = String.Concat(hash
                  .ComputeHash(System.Text.Encoding.UTF8.GetBytes(toBeHashed))
                  .Select(item => item.ToString("x2")));
                Console.WriteLine("Hash of {0} is {1}", toBeHashed, hashed);

            }
        }

        [HttpGet("problems")]
        public IActionResult GetProblems()
        {
            var directoryNames = ExtractProblemNames(Directory.EnumerateDirectories(_configurationPaths["UploadPath"]));
            return Ok(JsonSerializer.Serialize(directoryNames));
        }

        [HttpPost("source/{id}/{username}")]
        public IActionResult PostSource(IFormFile file, [FromRoute] string id, [FromRoute] string username)
        {
            if(!UsernameChecks(username))
            {
                return BadRequest(JsonSerializer.Serialize("Useranme is not valid"));
            }
            string cppSourceName = "source.cpp";
            string uploadFolder = Path.Combine(_configurationPaths["UploadPath"], id);
            string userPath = Path.Combine(uploadFolder, username);
            Directory.CreateDirectory(userPath);
            string filePath = Path.Combine(userPath, cppSourceName); 

            Console.WriteLine("Source file loading at {0}", filePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return Ok(JsonSerializer.Serialize("Cpp source uploaded"));
        }

        [HttpPost("description")]
        public IActionResult PostDescription(IFormFile file)
        {

            if (file.FileName != _token)
            {
                return Ok(JsonSerializer.Serialize("Invalid credentials"));
            }

            string inputFileName = "description.txt";
            string filePath = Path.Combine(_uploadFolderName, inputFileName);

            Console.WriteLine("Description file loading at {0}", filePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return Ok(JsonSerializer.Serialize("Description file uploaded"));
        }

        [HttpPost("input/{id}")]
        public IActionResult PostInput(IFormFile file, [FromRoute] int id)
        {

            if (file.FileName != _token)
            {
                return Ok(JsonSerializer.Serialize("Invalid credentials"));
            }

            string inputFileName = "input" + id + ".txt";
            string filePath = Path.Combine(_uploadFolderName, inputFileName);

            Console.WriteLine("Input file loading at {0}", filePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            return Ok(JsonSerializer.Serialize("Input file uploaded"));
        }

        [HttpPost("output/{id}")]
        public IActionResult PostOutput(IFormFile file, [FromRoute] int id)
        {
            if (file.FileName != _token)
            {
                return Ok(JsonSerializer.Serialize("Invalid credentials"));
            }

            string outputTemplate = "output" + id + ".txt";

            string filePath = Path.Combine(_uploadFolderName, outputTemplate);

            Console.WriteLine("Output file loading at {0}", filePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return Ok(JsonSerializer.Serialize("Output template uploaded"));
        }

        [HttpGet("run/{id}/{username}")]
        public IActionResult GetRun([FromRoute] string id, [FromRoute] string username)
        {
            if (!UsernameChecks(username))
            {
                return BadRequest(JsonSerializer.Serialize("Useranme is not valid"));
            }
            return RunCpp(id, username);
        }

        private bool UsernameChecks(string username)
        {
            return Regex.Match(username, @"^[A-Za-z-]+$").Success;
        }

        [HttpGet("problems/{problemName}")]
        public IActionResult GetProblemById([FromRoute] string problemName)
        {
            string directoryName = Path.Combine(_configurationPaths["UploadPath"], problemName);
            string input = System.IO.File.ReadAllText(Path.Combine(directoryName, "input1.txt"));
            string output = System.IO.File.ReadAllText(Path.Combine(directoryName, "output1.txt"));
            string description = System.IO.File.ReadAllText(Path.Combine(directoryName, "description.txt"));
            return Ok(new string[] { description, input, output });
        }

        private IActionResult RunCpp(string id, string username) 
        {
            Process process = ProcessStartup();
            string runPath = Path.Combine(_configurationPaths["UploadPath"], id, username);
            string problemPath = Path.Combine(_configurationPaths["UploadPath"], id);
            int total = 0, correct = 0;
            using (StreamWriter sw = process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine("cd " + runPath);
                    sw.WriteLine(_configurationPaths["CompilerPath"] + " source.cpp -o source.exe");
                    total = Directory.GetFiles(problemPath).Where( path => Regex.Match(path, @".input\d+\.txt$").Success).Count();
                    for (int i=1;i <= total; i++)
                    {
                        sw.WriteLine("source.exe ../input" + i + ".txt source_output" + i +".txt");
                    }
                }
            }

            process.WaitForExit();

            Console.WriteLine(String.Format("Process exit code was {0}", process.ExitCode));

            if(process.ExitCode != 0)
            {
                return BadRequest(JsonSerializer.Serialize("Code could not be compiled due to errors"));
            }

            for(int i=1;i <= total;i++)
            {
                string computedOutputFile = string.Format("source_output{0}.txt", i);
                string actualOutputFile = string.Format("../output{0}.txt", i);
                if (CompareOutput(runPath, computedOutputFile, actualOutputFile))
                {
                    Console.WriteLine(String.Format("Files {0} and {1} are identical", computedOutputFile, actualOutputFile));
                    correct++;
                }
                else
                {
                    Console.WriteLine(String.Format("NOTICE: {0} and {1} are different", computedOutputFile, actualOutputFile));
                }
            }
            return Ok(JsonSerializer.Serialize("Score is " + correct + "/" + total));
        }

        private Process ProcessStartup()
        {
            var processInfo = new ProcessStartInfo
            {
                WorkingDirectory = _configurationPaths["WorkingDirectoryPath"],
                FileName = _configurationPaths["CmdPath"],
                Verb = "runas",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Maximized
            };
            Process process = new Process
            {
                StartInfo = processInfo
            };
            process.Start();
            process.OutputDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
            process.ErrorDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return process;
        }
        
        private bool CompareOutput(string runPath,string computedOutput, string actualOutput)
        {
            string computedOutputFilePath = Path.Combine(runPath, computedOutput);
            string actualOutputFilePath = Path.Combine(runPath, actualOutput);

            return CheckSum.SHA256CheckSum(computedOutputFilePath) == CheckSum.SHA256CheckSum(actualOutputFilePath);
        }

        private IEnumerable<string> ExtractProblemNames(IEnumerable<string> problems)
        {
            return problems.Select(problem => Path.GetFileName(problem)).ToArray();
        }
    }
}