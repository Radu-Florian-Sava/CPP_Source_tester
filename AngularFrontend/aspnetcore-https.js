// This script sets up HTTPS for the application using the ASP.NET Core HTTPS certificate
const fs = require('fs');
const spawn = require('child_process').spawn;
const path = require('path');

const baseFolder =
  process.env.APPDATA !== undefined && process.env.APPDATA !== ''
    ? `${process.env.APPDATA}/ASP.NET/https`
    : `${process.env.HOME}/.aspnet/https`;

const certificateArg = process.argv.map(arg => arg.match(/--name=(?<value>.+)/i)).filter(Boolean)[0];
const certificateName = certificateArg ? certificateArg.groups.value : process.env.npm_package_name;

if (!certificateName) {
  console.error('Invalid certificate name. Run this script in the context of an npm/yarn script or pass --name=<<app>> explicitly.')
  process.exit(-1);
}


const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);


if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
  spawn('dotnet', [
    'dev-certs',
    'https',
    '--export-path',
    certFilePath,
    '--format',
    'Pem',
    '--no-password',
  ], {stdio: 'inherit',})
    .on('exit', (code) => process.exit(code));
}

if (!fs.existsSync('./certificate.pem')) {
  fs.copyFile(certFilePath, './certificate.pem', (err) => {
    if (err) {
      console.log(err);
    }
  });
}

if (!fs.existsSync('./certificate.key')) {
  fs.copyFile(certFilePath, './certificate.key', (err) => {
    if (err) {
      console.log(err);
    }
  });
}

console.log("SSL certificate data can be found in the certificate.pem and certificate.key files in the parent folder");
console.log("To change this setting, check out the package.json file and the angular.json options attribute")
