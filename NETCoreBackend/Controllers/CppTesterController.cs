using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

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

        public CppTesterController(ILogger<CppTesterController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }


        [HttpPost]
        [Route("postText")]
        public void Post([FromBody] string sentText)
        {
            Console.WriteLine(sentText);
        }

        [HttpPost]
        [Route("postFile")]
        public IActionResult PostFile(IFormFile file)
        {
            Console.WriteLine("field " + file.Name + " = " + file.FileName);

            string directoryPath = Path.Combine(_webHostEnvironment.ContentRootPath, "UploadedFiles");            
            string filePath = Path.Combine(directoryPath, file.FileName);

            if (file.FileName.Split('.')[1] == "cpp")
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                cppSourceName = file.FileName;
                correctCounter = 0;
                compileCounter = 0;
                return Ok(JsonSerializer.Serialize("Cpp source uploaded"));
            }
            if (file.FileName.ToLower().Contains("input"))
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                inputFileName = file.FileName;
                return Ok(JsonSerializer.Serialize("Input file uploaded"));
            }
            if(file.FileName.ToLower().Contains("output"))
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                outputTemplate = file.FileName;
                return Ok(JsonSerializer.Serialize("Output template uploaded"));
            }

            return Ok(JsonSerializer.Serialize("Enter valid files"));
        }

        [HttpPost]
        [Route("postRunCMD")]
        public IActionResult PostRun()
        {
            if (inputFileName != null && cppSourceName != null && outputTemplate != null) 
            {
                return runCpp();
            }
            return Ok(JsonSerializer.Serialize("Enter files before compiling"));
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
                    sw.WriteLine("cd C:\\Users\\Radu\\source\\repos\\CppTester\\NETCoreBackend\\UploadedFiles\\");
                    sw.WriteLine("g++.exe " + cppSourceName + " -o " + exeName);
                    sw.WriteLine(exeName + " " + inputFileName + " " + outputFileName);
                }
            }
            process.WaitForExit();
            compileCounter++;

            if (compareOutput(outputFileName,outputTemplate))
            {
                correctCounter++;
                return Ok(JsonSerializer.Serialize("output for " + cppSourceName + " is correct " + correctCounter + "/" + compileCounter));
            }               
            else 
                return Ok(JsonSerializer.Serialize("output for " + cppSourceName + " is not correct " + correctCounter + "/" + compileCounter));            
        }

        public Process processStartup()
        {
            Process process = new Process();
            var processInfo = new ProcessStartInfo();
            processInfo.WorkingDirectory = @"C:\WINDOWS\system32";
            processInfo.FileName = @"C:\WINDOWS\system32\cmd.exe";
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

        public bool compareOutput(string file1, string file2)//compar outputFileName cu outputTemplate
        {
            string directoryPath = Path.Combine(_webHostEnvironment.ContentRootPath, "UploadedFiles");
            string file1Path = Path.Combine(directoryPath, file1);
            string file2Path = Path.Combine(directoryPath, file2);
            int file1byte;
            int file2byte;
            if (file1 == file2)
                return true;

            FileStream fs1 = new FileStream(file1Path, FileMode.Open);
            FileStream fs2 = new FileStream(file2Path, FileMode.Open);

            if(fs1.Length!= fs2.Length)
            {
                fs1.Close();
                fs2.Close();
                return false;
            }

            do
            {
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            }
            while ((file1byte == file2byte) && (file1byte != -1) && (file2byte != -1));

            fs1.Close();
            fs2.Close();

            return ((file1byte - file2byte) == 0);
        }
    }
}