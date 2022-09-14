using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using System.Security.Cryptography;
using NETCoreBackend.Utility;

namespace NETCoreBackend.Controllers
{
    [ApiController]
    [Route("cpptester")]
    public class CppTesterController : ControllerBase
    {

        private readonly ILogger<CppTesterController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private static string? inputFileName;
        private static string? cppSourceName;
        private static string outputTemplate;
        private static int correctCounter = 0;
        private static int compileCounter = 0;
        private static IConfigurationSection configurationPaths = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()
            .GetSection("LocalPaths");

        public CppTesterController(ILogger<CppTesterController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost]
        [Route("postSource")]
        public IActionResult PostSource(IFormFile file)
        {
            Console.WriteLine("Loaded source file: " + file.FileName);
            string filePath = Path.Combine(configurationPaths["UploadPath"], file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            cppSourceName = file.FileName;
            correctCounter = 0;
            compileCounter = 0;
            return Ok(JsonSerializer.Serialize("Cpp source uploaded"));
        } 

        [HttpPost]
        [Route("postInput")]
        public IActionResult PostInput(IFormFile file)
        {
            Console.WriteLine("Loaded input file: " + file.FileName);
            string filePath = Path.Combine(configurationPaths["UploadPath"], file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            inputFileName = file.FileName;
            return Ok(JsonSerializer.Serialize("Input file uploaded"));
        }

        [HttpPost]
        [Route("postOutput")]
        public IActionResult PostOutput(IFormFile file)
        {
            Console.WriteLine("Loaded output file: " + file.FileName);
            string filePath = Path.Combine(configurationPaths["UploadPath"], file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            outputTemplate = file.FileName;
            return Ok(JsonSerializer.Serialize("Output template uploaded"));
        }

        [HttpGet]
        [Route("getRunCMD")]
        public IActionResult GetRun()
        {
            if (inputFileName != null && cppSourceName != null && outputTemplate != null) 
            {
                return runCpp();
            }
            return BadRequest(JsonSerializer.Serialize("Enter files before compiling"));
        }

        public IActionResult runCpp() 
        {
            string exeName = cppSourceName.Split('.')[0] + ".exe";
            string outputFileName = cppSourceName.Split('.')[0] + "Output.txt";
           
            Process process = processStartup();

            using (StreamWriter sw = process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine("cd " + configurationPaths["UploadPath"]);
                    sw.WriteLine(configurationPaths["CompilerPath"] + cppSourceName + " -o " + exeName);
                    sw.WriteLine(exeName + " " + inputFileName + " " + outputFileName);
                }
            }

            process.WaitForExit();

            compileCounter++;

            if(process.ExitCode != 0)
            {
                return BadRequest(JsonSerializer.Serialize("Code could not be compiled due to errors"));
            }

            if (compareOutput(outputFileName, outputTemplate))
            {
                correctCounter++;
                return Ok(JsonSerializer.Serialize("output for " + cppSourceName + " is correct " + correctCounter + "/" + compileCounter));
            }
            else
            {
                return Ok(JsonSerializer.Serialize("output for " + cppSourceName + " is not correct " + correctCounter + "/" + compileCounter));
            }
        }

        public Process processStartup()
        {
            Process process = new Process();
            var processInfo = new ProcessStartInfo();
            processInfo.WorkingDirectory = configurationPaths["WorkingDirectoryPath"];
            processInfo.FileName = configurationPaths["CmdPath"];
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

        public bool compareOutput(string computedOutput, string actualOutput)//compar outputFileName cu outputTemplate
        {

            string computedOutputFilePath = Path.Combine(configurationPaths["UploadPath"], computedOutput);
            string actualOutputFilePath = Path.Combine(configurationPaths["UploadPath"], actualOutput);

            return CheckSum.SHA256CheckSum(computedOutputFilePath) == CheckSum.SHA256CheckSum(actualOutputFilePath);

        }
    }
}