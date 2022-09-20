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
        private static string? _inputFileName;
        private static string? _cppSourceName;
        private static string? _outputTemplate;
        private static int _correctCounter = 0;
        private static int _compileCounter = 0;

        public CppTesterController(ILogger<CppTesterController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _configurationPaths = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("LocalPaths");
            //_inputFileName = "input" + _uniqueIdentifier + ".txt";
            //_outputTemplate = "expected_output" + _uniqueIdentifier + ".txt";
            //_cppSourceName = "source" + _uniqueIdentifier + ".cpp";
            //_cppSourceName = "source" + _uniqueIdentifier + ".cpp";
        }

        [HttpPost]
        [Route("postSource")]
        public IActionResult PostSource(IFormFile file)
        {

            _cppSourceName = file.FileName;

            string filePath = Path.Combine(_configurationPaths["UploadPath"], _cppSourceName);

            Console.WriteLine("Loading source file: " + file.FileName + " to " + filePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            _correctCounter = 0;
            _compileCounter = 0;

            return Ok(JsonSerializer.Serialize("Cpp source uploaded"));
        } 

        [HttpPost]
        [Route("postInput")]
        public IActionResult PostInput(IFormFile file)
        {
            _inputFileName = file.FileName;

            string filePath = Path.Combine(_configurationPaths["UploadPath"], _inputFileName);

            Console.WriteLine("Loading input file: " + file.FileName + " to " + filePath);

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
            _outputTemplate = file.FileName;

            string filePath = Path.Combine(_configurationPaths["UploadPath"], _outputTemplate);

            Console.WriteLine("Loaded output file: " + file.FileName + " to " + filePath);

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
            if (_inputFileName != null && _cppSourceName != null && _outputTemplate != null) 
            {
                return runCpp();
            }
            return BadRequest(JsonSerializer.Serialize("Enter files before compiling"));
        }

        private IActionResult runCpp() 
        {
            string exeName = _cppSourceName.Split('.')[0] + ".exe";
            //string outputFileName = "computed_output" + _uniqueIdentifier + ".txt";
            string outputFileName = _cppSourceName.Split('.')[0] + "_output.txt";

            Process process = processStartup();

            using (StreamWriter sw = process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine("cd " + _configurationPaths["UploadPath"]);
                    sw.WriteLine(_configurationPaths["CompilerPath"] + _cppSourceName + " -o " + exeName);
                    sw.WriteLine(exeName + " " + _inputFileName + " " + outputFileName);
                }
            }

            process.WaitForExit();

            _compileCounter++;

            if(process.ExitCode != 0)
            {
                return BadRequest(JsonSerializer.Serialize("Code could not be compiled due to errors"));
            }

            if (compareOutput(outputFileName, _outputTemplate))
            {
                _correctCounter++;
                return Ok(JsonSerializer.Serialize("output for " + _cppSourceName + " is correct " + _correctCounter + "/" + _compileCounter));
            }
            else
            {
                return Ok(JsonSerializer.Serialize("output for " + _cppSourceName + " is not correct " + _correctCounter + "/" + _compileCounter));
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
            string computedOutputFilePath = Path.Combine(_configurationPaths["UploadPath"], computedOutput);
            string actualOutputFilePath = Path.Combine(_configurationPaths["UploadPath"], actualOutput);

            return CheckSum.SHA256CheckSum(computedOutputFilePath) == CheckSum.SHA256CheckSum(actualOutputFilePath);

        }
    }
}